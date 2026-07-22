using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using ProsocAPI.Extensions;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.DTOs.FlexPay;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services.Repositories;
using ProsocAPI.Services;
using Prosoc.Data;
using ProsocAPI.Exceptions;
using Prosoc.Utilities;
using ProsocAPI.Utilities;
using System.Text.Json;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdhesionController : ControllerBase
    {
        private readonly IAdhesionRepository _repo;
        private readonly IAffilieRepository _affilieRepo;
        private readonly ProsocDbContext _db;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly ICotisationAffilieMetierService _cotisationMetier;
        private readonly IFlexPayAdhesionService _flexPayAdhesionService;
        private readonly ICollecteMultideviseService _multidevise;
        private readonly IWalletVirtuelPaymentService _walletVirtuelPayment;
        private readonly ITypeAdhesionDependantsValidationService _typeAdhesionDependantsValidation;

        public AdhesionController(
            IAdhesionRepository repo, 
            IAffilieRepository affilieRepo,
            ProsocDbContext db,
            IEmailService emailService,
            INotificationService notificationService,
            ICotisationAffilieMetierService cotisationMetier,
            IFlexPayAdhesionService flexPayAdhesionService,
            ICollecteMultideviseService multidevise,
            IWalletVirtuelPaymentService walletVirtuelPayment,
            ITypeAdhesionDependantsValidationService typeAdhesionDependantsValidation,
            ErrorService errorService,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<AdhesionController> logger)
        {
            _repo = repo;
            _affilieRepo = affilieRepo;
            _db = db;
            _emailService = emailService;
            _notificationService = notificationService;
            _cotisationMetier = cotisationMetier;
            _flexPayAdhesionService = flexPayAdhesionService;
            _multidevise = multidevise;
            _walletVirtuelPayment = walletVirtuelPayment;
            _typeAdhesionDependantsValidation = typeAdhesionDependantsValidation;
            _errorService = errorService;
            _paginationService = paginationService;
            _paginationOptions = paginationOptions.Value;
            _logger = logger;
        }

        private readonly IPaginationService _paginationService;
        private readonly PaginationOptions _paginationOptions;
        private readonly ILogger<AdhesionController> _logger;
        private readonly ErrorService _errorService;

        private ObjectResult TechnicalErrorResponse(string message, Exception ex, string code = ErrorCodes.TECHNICAL_INTERNAL_ERROR)
        {
            var errorResponse = _errorService.CreateTechnicalError(code, message, ex);
            _logger.LogError(ex, "{Message}", message);
            return StatusCode(500, errorResponse);
        }

        private int? TryGetCurrentUserId()
        {
            try
            {
                return GetCurrentUserId();
            }
            catch
            {
                return null;
            }
        }

        private int GetCurrentUserId()
        {
            // Récupérer l'ID utilisateur depuis le token JWT
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                              ?? User.FindFirst("uid")?.Value
                              ?? User.FindFirst("UserId")?.Value;
            
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("Utilisateur non identifié");
        }

        private bool HasPermission(string permission)
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
                return true;

            return User.HasClaim("permission", permission);
        }

        private ObjectResult ForbiddenPermission(string permission) =>
            StatusCode(StatusCodes.Status403Forbidden, new { message = $"Permission requise : {permission}" });

        private static string BuildNomComplet(string prenom, string nom, string? postnom)
        {
            var p = string.IsNullOrWhiteSpace(postnom) ? null : postnom.Trim();
            return p == null ? $"{prenom} {nom}" : $"{prenom} {p} {nom}";
        }

        // Remarque 4 : 18–54 ans pour adhérer en titulaire ; 55+ = personne à charge uniquement
        private void ValidateAffilieAge(AdhesionWithAffilieCreateDto input)
        {
            var errors = PersonneEnChargeRegles.ValiderAgeAdherent(input.DateNaissance);
            if (!errors.Any())
            {
                var age = PersonneEnChargeRegles.CalculerAge(input.DateNaissance);
                _logger.LogInformation(
                    "Validation âge affilié : {Age} ans (DateNaissance: {DateNaissance})",
                    age, input.DateNaissance.ToShortDateString());
                return;
            }

            var ageActuel = input.DateNaissance == default
                ? (int?)null
                : PersonneEnChargeRegles.CalculerAge(input.DateNaissance);

            var code = ageActuel.HasValue && ageActuel > PersonneEnChargeRegles.AgeMaxAdherent
                ? ErrorCodes.VALIDATION_AGE_MAXIMUM_ADHERENT
                : ErrorCodes.VALIDATION_AGE_MINIMUM;

            var errorResponse = _errorService.CreateValidationError(
                code,
                string.Join(" ", errors),
                errors.Select(msg => new ValidationError
                {
                    Field = "DateNaissance",
                    Message = msg,
                    AttemptedValue = input.DateNaissance,
                    ErrorCode = code
                }).ToList());

            throw new ArgumentException(errorResponse.Error.Message);
        }

        private void ValidateNiveau1At(AdhesionWithAffilieCreateDto input)
        {
            var errors = AdhesionNiveau1Regles.ValiderChampsObligatoires(input);
            if (!errors.Any())
                return;

            var validationErrors = errors.Select(msg => new ValidationError
            {
                Field = "Niveau1",
                Message = msg,
                ErrorCode = ErrorCodes.VALIDATION_NIVEAU1_AT
            }).ToList();

            var errorResponse = _errorService.CreateValidationError(
                ErrorCodes.VALIDATION_NIVEAU1_AT,
                string.Join(" ", errors),
                validationErrors);

            throw new ArgumentException(errorResponse.Error.Message);
        }

        private static int CalculateAge(DateTime dateNaissance) =>
            PersonneEnChargeRegles.CalculerAge(dateNaissance);

        // ✅ VALIDATION 1 : Validation complète des collectes
        private async Task ValidateCollectesAsync(IEnumerable<CollecteCreateDto> collectes, int agentId, CancellationToken ct)
        {
            _logger.LogInformation("Début de ValidateCollectesAsync avec {Count} collectes pour l'agent {AgentId}", collectes?.Count() ?? 0, agentId);
            
            if (collectes == null || !collectes.Any())
            {
                throw new ArgumentException("Au moins une collecte est requise");
            }

            var errors = new List<string>();
            var referencePaiements = new HashSet<string>();

            foreach (var collecte in collectes)
            {
                _logger.LogInformation("Validation de la collecte #{Index}", errors.Count + 1);
                
                // ✅ SÉCURITÉ : Vérifier si la collecte est null
                if (collecte == null)
                {
                    errors.Add("Une collecte ne peut pas être null");
                    continue;
                }

                // Validation du montant
                _logger.LogInformation("Validation du montant: {Montant}", collecte.Montant);
                if (collecte.Montant <= 0)
                {
                    errors.Add($"Le montant de la collecte ne peut pas être négatif ou nul (Montant: {collecte.Montant})");
                }

                // Validation du mode de paiement
                if (string.IsNullOrWhiteSpace(collecte.ModePaiement))
                {
                    errors.Add("Le mode de paiement est obligatoire");
                }
                else
                {
                    var modesValides = new[] { "ESPECE", "MOBILE_MONEY", "CARTE_BANCAIRE", "VIREMENT_BANCAIRE", "CHEQUE", "VIRTUAL_ACCOUNT" };
                    if (!modesValides.Contains(collecte.ModePaiement.ToUpperInvariant()))
                    {
                        errors.Add($"Mode de paiement invalide: {collecte.ModePaiement}. Modes valides: {string.Join(", ", modesValides)}");
                    }
                }

                // Validation de la date de collecte (ne peut pas être dans le futur)
                if (collecte.Mois > 0 && collecte.Mois <= 12 && collecte.Annee >= 2020 && collecte.Annee <= 2100)
                {
                    var dateCollecte = new DateTime(collecte.Annee, collecte.Mois, 1);
                    if (dateCollecte > DateTime.Today.AddMonths(1)) // Permettre le mois en cours et le mois suivant
                    {
                        errors.Add($"La date de collecte ne peut pas être dans le futur (Mois: {collecte.Mois}, Année: {collecte.Annee})");
                    }
                }

                // Validation des montants attendus vs reçus
                if (collecte.MontantAttendu.HasValue && collecte.MontantRecu.HasValue)
                {
                    if (collecte.MontantRecu.Value > collecte.MontantAttendu.Value)
                    {
                        errors.Add($"Le montant reçu ({collecte.MontantRecu}) ne peut pas dépasser le montant attendu ({collecte.MontantAttendu})");
                    }
                }

                // Validation des références de paiement uniques
                var modesSansReference = new[] { "VIRTUAL_ACCOUNT", "MOBILE_MONEY", "CARTE_BANCAIRE" };
                
                if (string.IsNullOrWhiteSpace(collecte.ReferencePaiement))
                {
                    // ✅ RÈGLE : La référence de paiement est obligatoire sauf pour VIRTUAL_ACCOUNT
                    if (!modesSansReference.Contains(collecte.ModePaiement.ToUpperInvariant()))
                    {
                        errors.Add("La référence de paiement est obligatoire pour ce mode de paiement");
                    }
                }
                else
                {
                    // ✅ VALIDATION : Pour les modes qui nécessitent une référence, vérifier l'unicité
                    if (!modesSansReference.Contains(collecte.ModePaiement.ToUpperInvariant()))
                    {
                        var refNormalisee = collecte.ReferencePaiement!.Trim().ToUpperInvariant();
                        if (referencePaiements.Contains(refNormalisee))
                        {
                            errors.Add($"La référence de paiement '{collecte.ReferencePaiement}' est dupliquée");
                        }
                        referencePaiements.Add(refNormalisee);
                    }
                }
            }

            if (errors.Any())
            {
                throw new ArgumentException($"Erreurs de validation des collectes:\n{string.Join("\n", errors)}");
            }

            _logger.LogInformation("Validation collectes réussie pour {Count} collecte(s)", collectes.Count());
        }

        // ✅ VALIDATION 1 (Legacy) : Validation complète des collectes (version synchrone pour compatibilité)
        private void ValidateCollectes(IEnumerable<CollecteCreateDto> collectes)
        {
            _logger.LogInformation("Début de ValidateCollectes avec {Count} collectes", collectes?.Count() ?? 0);
            
            if (collectes == null || !collectes.Any())
            {
                throw new ArgumentException("Au moins une collecte est requise");
            }

            var errors = new List<string>();
            var referencePaiements = new HashSet<string>();

            foreach (var collecte in collectes)
            {
                _logger.LogInformation("Validation de la collecte #{Index}", errors.Count + 1);
                
                // ✅ SÉCURITÉ : Vérifier si la collecte est null
                if (collecte == null)
                {
                    errors.Add("Une collecte ne peut pas être null");
                    continue;
                }

                // Validation du montant
                _logger.LogInformation("Validation du montant: {Montant}", collecte.Montant);
                if (collecte.Montant <= 0)
                {
                    errors.Add($"Le montant de la collecte ne peut pas être négatif ou nul (Montant: {collecte.Montant})");
                }

                // Validation du mode de paiement
                if (string.IsNullOrWhiteSpace(collecte.ModePaiement))
                {
                    errors.Add("Le mode de paiement est obligatoire");
                }
                else
                {
                    var modesValides = new[] { "ESPECE", "MOBILE_MONEY", "CARTE_BANCAIRE", "VIREMENT_BANCAIRE", "CHEQUE", "VIRTUAL_ACCOUNT" };
                    if (!modesValides.Contains(collecte.ModePaiement.ToUpperInvariant()))
                    {
                        errors.Add($"Mode de paiement invalide: {collecte.ModePaiement}. Modes valides: {string.Join(", ", modesValides)}");
                    }
                }

                // Validation de la date de collecte (ne peut pas être dans le futur)
                if (collecte.Mois > 0 && collecte.Mois <= 12 && collecte.Annee >= 2020 && collecte.Annee <= 2100)
                {
                    var dateCollecte = new DateTime(collecte.Annee, collecte.Mois, 1);
                    if (dateCollecte > DateTime.Today.AddMonths(1)) // Permettre le mois en cours et le mois suivant
                    {
                        errors.Add($"La date de collecte ne peut pas être dans le futur (Mois: {collecte.Mois}, Année: {collecte.Annee})");
                    }
                }

                // Validation des montants attendus vs reçus
                if (collecte.MontantAttendu.HasValue && collecte.MontantRecu.HasValue)
                {
                    if (collecte.MontantRecu.Value > collecte.MontantAttendu.Value)
                    {
                        errors.Add($"Le montant reçu ({collecte.MontantRecu}) ne peut pas dépasser le montant attendu ({collecte.MontantAttendu})");
                    }
                }

                // Validation des références de paiement uniques
                var modesSansReference = new[] { "VIRTUAL_ACCOUNT", "MOBILE_MONEY", "CARTE_BANCAIRE" };
                
                if (string.IsNullOrWhiteSpace(collecte.ReferencePaiement))
                {
                    // ✅ RÈGLE : La référence de paiement est obligatoire sauf pour VIRTUAL_ACCOUNT
                    if (!modesSansReference.Contains(collecte.ModePaiement.ToUpperInvariant()))
                    {
                        errors.Add("La référence de paiement est obligatoire pour ce mode de paiement");
                    }
                }
                else
                {
                    // ✅ VALIDATION : Pour les modes qui nécessitent une référence, vérifier l'unicité
                    if (!modesSansReference.Contains(collecte.ModePaiement.ToUpperInvariant()))
                    {
                        var refNormalisee = collecte.ReferencePaiement!.Trim().ToUpperInvariant();
                        if (referencePaiements.Contains(refNormalisee))
                        {
                            errors.Add($"La référence de paiement '{collecte.ReferencePaiement}' est dupliquée");
                        }
                        referencePaiements.Add(refNormalisee);
                    }
                }
            }

            if (errors.Any())
            {
                throw new ArgumentException($"Erreurs de validation des collectes:\n{string.Join("\n", errors)}");
            }

            _logger.LogInformation("Validation collectes réussie pour {Count} collecte(s)", collectes.Count());
        }

        private async Task ValidateAdhesionCollectesMultideviseAsync(
            IEnumerable<CollecteCreateDto> collectes,
            int agentId,
            int nombreDependants,
            CancellationToken ct)
        {
            var collectesList = collectes as IList<CollecteCreateDto> ?? collectes.ToList();
            if (collectesList.Any(c => CollecteAdhesionHelper.IsVirtualAccountPayment(c.ModePaiement)))
            {
                WalletVirtuelPaiementAutorisation.EnsureCallerPeutPayerEnWalletVirtuel(User);
            }

            WalletVirtuelAgent? walletVirtuel = null;
            decimal cumulDebitVirtuel = 0m;

            foreach (var collecte in collectesList)
            {
                var tempCollecte = CollecteAdhesionHelper.ToTempCollecte(collecte);
                var dateConversion = CollecteAdhesionHelper.ResolveDateConversionPaiement(
                    collecte.ModePaiement, tempCollecte.DateCollecte);
                await _multidevise.ValidateAndApplySnapshotAsync(
                    tempCollecte, nombreDependants, ct, dateConversion);

                if (!CollecteAdhesionHelper.IsVirtualAccountPayment(collecte.ModePaiement))
                    continue;

                walletVirtuel ??= await _db.WalletsVirtuelsAgents
                    .AsNoTracking()
                    .Include(w => w.Devise)
                    .FirstOrDefaultAsync(w => w.AgentId == agentId && w.Statut, ct);

                if (walletVirtuel == null)
                {
                    var errorResponse = _errorService.CreateBusinessError(
                        ErrorCodes.BUSINESS_WALLET_VIRTUEL_INEXISTANT,
                        $"Aucun wallet virtuel actif trouvé pour l'agent {agentId}",
                        new List<ErrorDetail>
                        {
                            new ErrorDetail
                            {
                                Field = "ModePaiement",
                                Value = "VIRTUAL_ACCOUNT",
                                Issue = "Wallet virtuel non trouvé",
                                Expected = "Wallet virtuel actif pour cet agent"
                            }
                        });

                    throw new ArgumentException(errorResponse.Error.Message);
                }

                cumulDebitVirtuel += await _walletVirtuelPayment.ComputeMontantDebitAsync(
                    tempCollecte, walletVirtuel, tempCollecte.DateCollecte, ct);
            }

            if (walletVirtuel != null && cumulDebitVirtuel > 0)
            {
                try
                {
                    await _walletVirtuelPayment.ValidateSoldeCumulSuffisantAsync(
                        walletVirtuel, cumulDebitVirtuel, ct);
                }
                catch (InvalidOperationException ex)
                {
                    var errorResponse = _errorService.CreateBusinessError(
                        ErrorCodes.BUSINESS_SOLDE_INSUFFISANT,
                        ex.Message,
                        new List<ErrorDetail>
                        {
                            new ErrorDetail
                            {
                                Field = "Montant",
                                Value = cumulDebitVirtuel,
                                Issue = "Solde virtuel insuffisant après conversion multidevise",
                                Expected = $"Montant total <= {walletVirtuel.SoldeVirtuel:F2} {walletVirtuel.Devise?.Code}"
                            }
                        });

                    throw new ArgumentException(errorResponse.Error.Message);
                }
            }
        }

        // Remarque 4 — personnes à charge (enfants 0–18, 18–25 avec certificat scolarité)
        private void ValidateDependants(IEnumerable<DependantCreateDto>? dependants, DateTime dateNaissanceAffilie)
        {
            if (dependants == null || !dependants.Any())
                return;

            var errors = PersonneEnChargeRegles.ValiderDependants(
                dependants.Select(DependantValidationInput.FromCreate),
                dateNaissanceAffilie);

            if (errors.Any())
                throw new ArgumentException($"Erreurs de validation des personnes à charge:\n{string.Join("\n", errors)}");

            _logger.LogInformation("Validation personnes à charge réussie pour {Count} dépendant(s)", dependants.Count());
        }

        private static Dependant MapDependantFromCreate(DependantCreateDto d, int affilieId)
        {
            var dependant = new Dependant
            {
                Nom = d.Nom.Trim(),
                Adresse = d.Adresse,
                LienParente = LienParenteRegles.Normaliser(d.LienParente),
                DateNaissance = d.DateNaissance,
                AffilieId = affilieId,
                DateCreation = DateTime.Now,
                Statut = true
            };

            DependantCertificatApplicator.Appliquer(
                dependant, d.CertificatScolariteBase64, d.CertificatScolariteContentType);

            return dependant;
        }

        private static Dependant MapDependantFromNiveau2(DependantNiveau2Dto d)
        {
            var dependant = new Dependant
            {
                IdDependant = d.IdDependant ?? 0,
                Nom = d.NomComplet.Trim(),
                Adresse = d.Adresse.Trim(),
                LienParente = LienParenteRegles.Normaliser(d.LienParente),
                DateNaissance = d.DateNaissance,
                Statut = d.Statut
            };

            DependantCertificatApplicator.Appliquer(
                dependant, d.CertificatScolariteBase64, d.CertificatScolariteContentType);

            return dependant;
        }

        // ✅ VALIDATION 4 : Validation croisée entre collectes et leurs références
        private void ValidateCrossReferences(IEnumerable<CollecteCreateDto> collectes)
        {
            if (collectes == null || !collectes.Any())
            {
                return; // Les collectes sont obligatoires, déjà validé dans ValidateCollectes
            }

            var errors = new List<string>();
            var souscriptionIds = new HashSet<int>();
            var fraisIds = new HashSet<int>();

            // Collecter tous les IDs référencés
            foreach (var collecte in collectes)
            {
                if (collecte.SouscriptionPrestationId.HasValue)
                {
                    souscriptionIds.Add(collecte.SouscriptionPrestationId.Value);
                }

                if (collecte.FraisId.HasValue)
                {
                    fraisIds.Add(collecte.FraisId.Value);
                }
            }

            // Validation croisée pour chaque collecte
            foreach (var collecte in collectes)
            {
                switch (collecte.TypeCollecte)
                {
                    case TypeCollecte.Souscription:
                        if (!collecte.SouscriptionPrestationId.HasValue)
                        {
                            errors.Add($"Collecte de type SOUSCRIPTION : souscription.prestationId requis (Montant: {collecte.Montant})");
                        }
                        else if (!souscriptionIds.Contains(collecte.SouscriptionPrestationId.Value))
                        {
                            errors.Add($"prestationId {collecte.SouscriptionPrestationId.Value} non trouvé dans les souscriptions de la demande");
                        }
                        break;

                    case TypeCollecte.Frais:
                        if (!collecte.FraisId.HasValue)
                        {
                            errors.Add($"Collecte de type FRAIS doit avoir un FraisId (Montant: {collecte.Montant})");
                        }
                        else if (!fraisIds.Contains(collecte.FraisId.Value))
                        {
                            errors.Add($"FraisId {collecte.FraisId.Value} non trouvé dans les frais de la demande");
                        }
                        break;

                    case TypeCollecte.Cotisation:
                        if (!collecte.CotisationAffilieId.HasValue)
                        {
                            errors.Add($"Collecte de type COTISATION doit avoir un CotisationAffilieId (Montant: {collecte.Montant})");
                        }
                        break;

                    default:
                        errors.Add($"Type de collecte non supporté: {collecte.TypeCollecte}");
                        break;
                }

                var referenceCount =
                    (collecte.SouscriptionPrestationId.HasValue ? 1 : 0)
                    + (collecte.FraisId.HasValue ? 1 : 0)
                    + (collecte.CotisationAffilieId.HasValue ? 1 : 0);

                if (referenceCount > 1)
                {
                    errors.Add($"Collecte ne peut avoir qu'une seule référence (Frais, Souscription ou Cotisation). Montant: {collecte.Montant}");
                }
            }

            // Validation de cohérence globale
            var collectesSouscription = collectes.Count(c => c.TypeCollecte == TypeCollecte.Souscription);
            var collectesFrais = collectes.Count(c => c.TypeCollecte == TypeCollecte.Frais);
            var collectesCotisation = collectes.Count(c => c.TypeCollecte == TypeCollecte.Cotisation);

            if (collectesSouscription > 0 || collectesFrais > 0 || collectesCotisation > 0)
            {
                _logger.LogInformation(
                    "Validation croisée : {SouscriptionCount} souscription(s), {FraisCount} frais, {CotisationCount} cotisation(s)",
                    collectesSouscription, collectesFrais, collectesCotisation);
            }

            if (errors.Any())
            {
                throw new ArgumentException($"Erreurs de validation croisée:\n{string.Join("\n", errors)}");
            }

            _logger.LogInformation("Validation croisée réussie pour {Count} collecte(s)", collectes.Count());
        }

        private Task ValidateDependantsCountForTypeAdhesionAsync(
            int typeAdhesionId,
            int nombreDependants,
            CancellationToken ct) =>
            _typeAdhesionDependantsValidation.ValidateDependantsCountAsync(typeAdhesionId, nombreDependants, ct);

        private async Task ValidateCotisationCollectesForAdhesionAsync(
            IEnumerable<CollecteCreateDto> collectes,
            int typeAdhesionId,
            int nombreDependants,
            CancellationToken ct)
        {
            foreach (var collecte in collectes.Where(c => c.TypeCollecte == TypeCollecte.Cotisation))
            {
                if (!collecte.CotisationAffilieId.HasValue)
                {
                    throw new ArgumentException(
                        $"Collecte de type COTISATION : CotisationAffilieId est requis (montant saisi : {collecte.Montant}).");
                }

                await _cotisationMetier.ValidateCollecteCotisationAsync(
                    collecte.CotisationAffilieId.Value,
                    typeAdhesionId,
                    collecte.Montant,
                    nombreDependants,
                    ct);
            }
        }

        // ✅ VALIDATION 6 : Validation des données existantes (unicité, conflits)
        private async Task ValidateExistingDataAsync(AdhesionWithAffilieCreateDto input, CancellationToken ct)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            // Validation 1 : Email unique
            if (!string.IsNullOrWhiteSpace(input.EmailAffilie))
            {
                var existingEmail = await _db.Affilies
                    .AsNoTracking()
                    .Where(a => a.EmailAffilie == input.EmailAffilie.Trim())
                    .Select(a => new { a.IdAffilie, a.Nom, a.Prenom, a.EmailAffilie })
                    .FirstOrDefaultAsync(ct);

                if (existingEmail != null)
                {
                    errors.Add($"L'email '{input.EmailAffilie}' est déjà utilisé par {existingEmail.Nom} {existingEmail.Prenom} (ID: {existingEmail.IdAffilie})");
                }
            }

            // Validation 2 : Téléphone unique
            if (!string.IsNullOrWhiteSpace(input.Telephone))
            {
                var existingTelephone = await _db.Affilies
                    .AsNoTracking()
                    .Where(a => a.Telephone == input.Telephone.Trim())
                    .Select(a => new { a.IdAffilie, a.Nom, a.Prenom, a.Telephone })
                    .FirstOrDefaultAsync(ct);

                if (existingTelephone != null)
                {
                    errors.Add($"Le téléphone '{input.Telephone}' est déjà utilisé par {existingTelephone.Nom} {existingTelephone.Prenom} (ID: {existingTelephone.IdAffilie})");
                }
            }

            // Validation 3 : Conflits de noms/prénoms similaires
            var inputNom = input.Nom?.Trim().ToUpper();
            var inputPrenom = input.Prenom?.Trim().ToUpper();
            
            var similarAffilies = await _db.Affilies
                .AsNoTracking()
                .Where(a => 
                    ((a.Nom == null ? "" : a.Nom.Trim().ToUpper()) == inputNom ||
                    (a.Prenom == null ? "" : a.Prenom.Trim().ToUpper()) == inputPrenom) &&
                    a.DateNaissance.Date == input.DateNaissance.Date)
                .Select(a => new { a.IdAffilie, a.Nom, a.Prenom, a.DateNaissance, a.EmailAffilie, a.Telephone })
                .ToListAsync(ct);

            if (similarAffilies.Any())
            {
                var similarList = similarAffilies.Select(a => 
                    $"- {a.Nom} {a.Prenom} (ID: {a.IdAffilie}, Email: {a.EmailAffilie ?? "N/A"}, Téléphone: {a.Telephone ?? "N/A"})");
                
                warnings.Add($"Affiliés similaires trouvés avec la même date de naissance :\n{string.Join("\n", similarList)}");
            }

            // Validation 4 : Vérification des codes adhésion potentiels
            if (!string.IsNullOrWhiteSpace(input.ProvinceResidence))
            {
                // Simuler la génération du code adhésion pour vérifier les doublons potentiels
                var typeAdhesion = await _db.TypeAdhesions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.IdTypeAdhesion == input.TypeAdhesionId, ct);

                if (typeAdhesion != null)
                {
                    var typePrefix = typeAdhesion.Libelle.Length >= 2
                        ? typeAdhesion.Libelle.Substring(0, 2).ToUpperInvariant()
                        : typeAdhesion.Libelle.ToUpperInvariant().PadRight(2, 'X');

                    var year2 = (DateTime.Now.Year % 100).ToString("00");
                    var prov = input.ProvinceResidence.Trim();
                    prov = prov.Length >= 3 ? prov.Substring(0, 3).ToUpperInvariant() : prov.ToUpperInvariant().PadRight(3, 'X');
                    var prefix = $"{typePrefix}-{year2}-{prov}-";

                    // Vérifier s'il y a déjà beaucoup de codes avec ce préfixe
                    var existingCodesCount = await _db.Affilies
                        .AsNoTracking()
                        .CountAsync(a => a.CodeAdhesion != null && a.CodeAdhesion.StartsWith(prefix), ct);

                    if (existingCodesCount > 900) // Approche de la limite 999
                    {
                        warnings.Add($"Attention : Il y a déjà {existingCodesCount} codes d'adhésion avec le préfixe '{prefix}'. Le système pourrait bientôt atteindre la limite.");
                    }
                }
            }

            // Validation 5 : Vérification des dépendants similaires
            if (input.Dependants != null && input.Dependants.Any())
            {
                var dependantNames = input.Dependants
                    .Select(d => d.Nom?.Trim().ToUpperInvariant())
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList();

                if (dependantNames.Any())
                {
                    var existingDependants = await _db.Dependants
                        .AsNoTracking()
                        .Where(d => dependantNames.Contains(d.Nom))
                        .Select(d => new { d.Nom, d.LienParente, d.AffilieId })
                        .ToListAsync(ct);

                    if (existingDependants.Any())
                    {
                        var dependantList = existingDependants.Select(d => 
                            $"- {d.Nom} ({d.LienParente}) - Affilié ID: {d.AffilieId}");
                        
                        warnings.Add($"Dépendants avec des noms similaires existent déjà :\n{string.Join("\n", dependantList)}");
                    }
                }
            }

            // Gérer les erreurs et warnings
            if (errors.Any())
            {
                var errorResponse = _errorService.CreateBusinessError(
                    "VALIDATION_EXISTING_DATA_CONFLICT",
                    "Conflits de données existantes détectés",
                    errors.Select(e => new ErrorDetail
                    {
                        Field = "ExistingData",
                        Value = e,
                        Issue = "Conflit de données",
                        Expected = "Données uniques"
                    }).ToList());
                
                throw new ArgumentException(errorResponse.Error.Message);
            }

            if (warnings.Any())
            {
                _logger.LogWarning("Validation des données existantes - Avertissements :\n{Warnings}", string.Join("\n", warnings));
            }

            _logger.LogInformation("Validation des données existantes réussie");
        }

        /// <summary>Dossier d'adhésion de l'affilié connecté (sans liste globale).</summary>
        [HttpGet("mon-adhesion")]
        public async Task<ActionResult<AdhesionWithAffilieReadDto>> GetMonAdhesion(CancellationToken ct)
        {
            var (affilieId, error) = await AffilieMemberScopeHelper.RequireOwnAffilieIdAsync(User, _db, ct);
            if (error != null)
                return error;

            var adhesionId = await _db.Adhesions.AsNoTracking()
                .Where(a => a.AffilieId == affilieId)
                .Select(a => a.IdAdhesion)
                .FirstOrDefaultAsync(ct);

            if (adhesionId <= 0)
                return NotFound("Aucune adhésion trouvée pour votre compte.");

            return await GetById(adhesionId, ct);
        }

        [HttpGet("en-ligne-sans-gestionnaire")]
        [Authorize(Roles = "Admin,Superviseur")]
        public async Task<ActionResult<PaginatedResponse<AdhesionEnLigneSansGestionnaireDto>>> GetEnLigneSansGestionnaire(
            [FromQuery] PaginationRequest request,
            CancellationToken ct = default)
        {
            try
            {
                IQueryable<Adhesion> query = _db.Adhesions
                    .AsNoTracking()
                    .Include(a => a.Affilie)
                    .Include(a => a.TypeAdhesion)
                    .Where(a => a.AgentId == null && a.Statut && a.Affilie.Statut)
                    .OrderByDescending(a => a.DateCreation);

                query = query.ApplyAdhesionSearch(request.Search);
                request.Search = null;

                var paginated = await _paginationService.CreatePaginatedResponseAsync(query, request, ct);

                var affilieIds = paginated.Data.Select(a => a.AffilieId).Distinct().ToList();
                var modesPaiement = await _db.Collectes
                    .AsNoTracking()
                    .Where(c => affilieIds.Contains(c.AffilieId))
                    .GroupBy(c => c.AffilieId)
                    .Select(g => new
                    {
                        AffilieId = g.Key,
                        ModePaiement = g.OrderBy(c => c.DateCreation).Select(c => c.ModePaiement).FirstOrDefault()
                    })
                    .ToDictionaryAsync(x => x.AffilieId, x => x.ModePaiement, ct);

                var dtos = paginated.Data.Select(a => new AdhesionEnLigneSansGestionnaireDto
                {
                    IdAdhesion = a.IdAdhesion,
                    IdAffilie = a.AffilieId,
                    CodeAdhesion = a.Affilie.CodeAdhesion ?? string.Empty,
                    NomComplet = a.Affilie.NomComplet,
                    Telephone = a.Affilie.Telephone,
                    EmailAffilie = a.Affilie.EmailAffilie,
                    ProvinceResidence = a.Affilie.ProvinceResidence,
                    TypeAdhesion = a.TypeAdhesion?.Libelle ?? string.Empty,
                    StatutDossier = a.StatutDossier,
                    DateAdhesion = a.DateCreation,
                    ModePaiementAdhesion = modesPaiement.GetValueOrDefault(a.AffilieId)
                }).ToList();

                return Ok(new PaginatedResponse<AdhesionEnLigneSansGestionnaireDto>
                {
                    Data = dtos,
                    CurrentPage = paginated.CurrentPage,
                    PageSize = paginated.PageSize,
                    TotalItems = paginated.TotalItems,
                    TotalPages = paginated.TotalPages,
                    HasNextPage = paginated.HasNextPage,
                    HasPreviousPage = paginated.HasPreviousPage
                });
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des adhésions en ligne",
                    ex);
            }
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<AdhesionReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var deny = AffilieMemberScopeHelper.DenyListAccessForMembre(User, "des adhésions");
                if (deny != null)
                    return deny;

                if (!HasPermission("READ_ADHESION"))
                    return ForbiddenPermission("READ_ADHESION");

                var query = _db.Adhesions
                    .Include(a => a.Affilie)
                    .Include(a => a.TypeAdhesion)
                    .AsQueryable();

                query = query.ApplyAdhesionSearch(request.Search);
                request.Search = null;

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(ToReadDto).ToList();

                var paginatedDtos = new PaginatedResponse<AdhesionReadDto>
                {
                    Data = dtos,
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasNextPage = result.HasNextPage,
                    HasPreviousPage = result.HasPreviousPage
                };

                return Ok(paginatedDtos);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des adhésions paginées",
                    ex);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdhesionWithAffilieReadDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var scopeError = await AffilieMemberScopeHelper.EnsureOwnAdhesionScopeAsync(User, _db, id, ct);
            if (scopeError != null)
                return scopeError;

            if (!AffilieMemberScopeHelper.IsMembreAffilie(User) && !HasPermission("READ_ADHESION"))
                return ForbiddenPermission("READ_ADHESION");

            var adhesion = await _db.Adhesions
                .Include(a => a.Affilie)
                    .ThenInclude(aff => aff.Dependants)
                        .ThenInclude(d => d.Antecedants)
                            .ThenInclude(an => an.Affilie)
                .Include(a => a.Affilie)
                    .ThenInclude(aff => aff.Souscriptions)
                        .ThenInclude(sp => sp.Prestation)
                .Include(a => a.Affilie)
                    .ThenInclude(aff => aff.Souscriptions)
                        .ThenInclude(sp => sp.Collectes)
                            .ThenInclude(c => c.Devise)
                // ✅ AJOUT : Include des antécédents
                .Include(a => a.Affilie)
                    .ThenInclude(aff => aff.Antecedants)
                .Include(a => a.TypeAdhesion)
                .Include(a => a.AgentCreateur)
                .FirstOrDefaultAsync(a => a.IdAdhesion == id, ct);

            if (adhesion == null)
                return NotFound();

            var dto = new AdhesionWithAffilieReadDto
            {
                Id = adhesion.IdAdhesion,
                StatutDossier = adhesion.StatutDossier,
                DateCreation = adhesion.DateCreation,
                DateModification = adhesion.DateModification,
                Statut = adhesion.Statut,
                AffilieId = adhesion.AffilieId,
                TypeAdhesionId = adhesion.TypeAdhesionId,
                AgentId = adhesion.AgentId,
                CodeAdhesion = adhesion.Affilie.CodeAdhesion ?? "",
                
                Affilie = AffilieDtoMapper.ToReadDto(adhesion.Affilie),

                // Souscriptions
                Souscriptions = adhesion.Affilie.Souscriptions.Select(sp => new SouscriptionPrestationReadDto
                {
                    Id = sp.IdSouscriptionPrestation,
                    AffilieId = sp.AffilieId,
                    AffilieNom = adhesion.Affilie.Nom,
                    AffiliePrenom = adhesion.Affilie.Prenom,
                    PrestationId = sp.PrestationId,
                    PrestationNom = sp.Prestation.NomPrestation,
                    PrestationDescription = sp.Prestation.Description,
                    DateSouscription = sp.DateSouscription,
                    DateCreation = sp.DateCreation,
                    DateModification = sp.DateModification,
                    Statut = sp.Statut,
                    NombreCollectes = sp.Collectes.Count,
                    TotalCollectes = sp.Collectes.Sum(c => c.Montant)
                }).ToList(),

                // Dépendants
                Dependants = adhesion.Affilie.Dependants.Select(DependantDtoMapper.ToReadDto).ToList(),

                // ✅ AJOUT : Antécédents
                Antecedants = adhesion.Affilie.Antecedants.Select(a => new AntecedantReadDto
                {
                    IdAntecedant = a.IdAntecedant,
                    Description = a.Description,
                    AffilieId = a.AffilieId,
                    DateCreation = a.DateCreation,
                    DateModification = a.DateModification,
                    Statut = a.Statut
                }).ToList(),

                // Collectes (toutes les collectes de l'adhésion)
                Collectes = MapCollectesToDto(await _db.Collectes
                    .Where(c => c.AffilieId == adhesion.AffilieId)
                    .Include(c => c.Devise)
                    .Include(c => c.Frais)
                    .Include(c => c.SouscriptionPrestationRef)
                    .ToListAsync(ct)),

                // Type d'adhésion
                TypeAdhesionLibelle = adhesion.TypeAdhesion?.Libelle ?? "",
                
                // Agent
                AgentNom = adhesion.AgentCreateur?.NomComplet ?? ""
            };

            return Ok(dto);
        }

        private static CollecteReadDto GetFirstCollecte(Affilie affilie, Agent? agent)
        {
            var allCollectes = affilie.Souscriptions
                .SelectMany(sp => sp.Collectes)
                .OrderByDescending(c => c.DateCreation)
                .ToList();
            
            if (!allCollectes.Any())
                return new CollecteReadDto();
            
            var firstCollecte = allCollectes.First();
            return new CollecteReadDto
            {
                IdCollecte = firstCollecte.IdCollecte,
                AffilieId = firstCollecte.AffilieId,
                AffilieNom = affilie.Nom,
                AgentId = firstCollecte.AgentId,
                AgentNom = agent?.NomComplet ?? "",
                Montant = firstCollecte.Montant,
                ModePaiement = firstCollecte.ModePaiement,
                ReferencePaiement = firstCollecte.ReferencePaiement,
                StatutPaiement = firstCollecte.StatutPaiement,
                DateCreation = firstCollecte.DateCreation,
                DeviseId = firstCollecte.DeviseId,
                DeviseNom = firstCollecte.Devise?.Nom ?? "",
                DeviseCode = firstCollecte.Devise?.Code ?? ""
            };
        }

        [HttpGet("{id:int}/affilie")]
        public async Task<ActionResult<AffilieReadDto>> GetAffilie([FromRoute] int id, CancellationToken ct)
        {
            var scopeError = await AffilieMemberScopeHelper.EnsureOwnAdhesionScopeAsync(User, _db, id, ct);
            if (scopeError != null)
                return scopeError;

            if (!AffilieMemberScopeHelper.IsMembreAffilie(User) && !HasPermission("READ_ADHESION"))
                return ForbiddenPermission("READ_ADHESION");

            var adhesion = await _repo.GetByIdAsync(id, ct);
            if (adhesion == null)
                return NotFound();

            var affilie = await _affilieRepo.GetByIdAsync(adhesion.AffilieId, ct);
            return affilie == null ? NotFound() : Ok(AffilieDtoMapper.ToReadDto(affilie));
        }

        /// <summary>
        /// Création adhésion niveau 1 : JSON métier dans <c>payload</c> + fichiers <c>photo</c> et <c>carteIdentite</c> (stockage BLOB en base).
        /// </summary>
        [HttpPost("with-affilie-multipart")]
        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(2_200_000)]
        public async Task<ActionResult<AdhesionWithAffilieReadDto>> CreateWithAffilieMultipart(
            [FromForm] AdhesionWithAffilieMultipartRequest form,
            IFormFile? photo,
            IFormFile? carteIdentite,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(form.Payload))
                return BadRequest("Le champ form.payload (JSON) est obligatoire.");

            AdhesionWithAffilieCreateDto? input;
            try
            {
                input = JsonSerializer.Deserialize<AdhesionWithAffilieCreateDto>(
                    form.Payload,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                return BadRequest($"JSON payload invalide : {ex.Message}");
            }

            if (input == null)
                return BadRequest("Le payload JSON est vide.");

            try
            {
                var photoBin = await AffilieFichierHelper.DepuisFormFileOptionnelAsync(photo, "photo", ct: ct);
                if (photoBin != null)
                {
                    input.PhotoBase64 = Convert.ToBase64String(photoBin.Data);
                    input.PhotoContentType = photoBin.ContentType;
                }

                var carteBin = await AffilieFichierHelper.DepuisFormFileOptionnelAsync(
                    carteIdentite, "carteIdentite", autoriserPdf: true, ct: ct);
                if (carteBin != null)
                {
                    input.CarteIdentiteBase64 = Convert.ToBase64String(carteBin.Data);
                    input.CarteIdentiteContentType = carteBin.ContentType;
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            return await CreateWithAffilie(input, ct);
        }

        [HttpPost("with-affilie")]
        [AllowAnonymous]
        public async Task<ActionResult<AdhesionWithAffilieReadDto>> CreateWithAffilie([FromBody] AdhesionWithAffilieCreateDto input, CancellationToken ct)
        {
            _logger.LogInformation("Début de CreateWithAffilie");
            
            // ✅ VALIDATION 0 : Vérifier l'input
            if (input == null)
            {
                _logger.LogError("Input null reçu");
                return BadRequest("L'objet d'entrée ne peut pas être null");
            }
            
            if (input.Collectes == null || !input.Collectes.Any())
            {
                _logger.LogError("Aucune collecte fournie");
                return BadRequest("Au moins une collecte est requise");
            }
            
            _logger.LogInformation("Reçu {Count} collecte(s)", input.Collectes.Count);

            if (!AdhesionAgentIdHelper.IsTerrainAgentRequired(input.AgentId))
                return BadRequest("AgentId obligatoire pour une adhésion terrain.");

            var terrainAgentId = input.AgentId!.Value;
            
            // ✅ VALIDATION 1 : Vérifier l'âge minimum de l'affilié (18 ans)
            // ✅ VALIDATION niveau 1 AT : photo, pièce d'identité, adresse, souscription/collecte confirmée
            try
            {
                ValidateAffilieAge(input);
                ValidateNiveau1At(input);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(_errorService.CreateValidationError(
                    ErrorCodes.VALIDATION_NIVEAU1_AT,
                    ex.Message));
            }

            // ✅ VALIDATION 1 : Validation complète des collectes
            _logger.LogInformation("Début du mapping des collectes avec {Count} éléments", input.Collectes?.Count ?? 0);
            
            var collectesCreateDtos = input.Collectes.Select(c => 
            {
                // ✅ SÉCURITÉ : Vérifier si la collecte source est null
                if (c == null)
                {
                    _logger.LogWarning("Collecte source null détectée");
                    throw new ArgumentException("Une collecte ne peut pas être null");
                }
                
                _logger.LogInformation("Mapping de la collecte avec TypeCollecte: {TypeCollecte}", c.TypeCollecte);
                
                if (c.TypeCollecte == TypeCollecte.Souscription)
                {
                    _logger.LogInformation("Souscription trouvée: {SouscriptionPrestationId}", c.Souscription?.PrestationId);
                }
                
                return new CollecteCreateDto
                {
                    TypeCollecte = c.TypeCollecte,
                    FraisId = c.FraisId,
                    CotisationAffilieId = c.CotisationAffilieId,
                    Montant = c.Montant,
                    Mois = c.Mois,
                    Annee = c.Annee,
                    ReferencePaiement = c.ReferencePaiement,
                    ModePaiement = c.ModePaiement,
                    Operateur = c.Operateur,
                    StatutPaiement = c.StatutPaiement,
                    MontantRecu = c.MontantRecu,
                    MontantAttendu = c.MontantAttendu,
                    DeviseId = c.DeviseId,
                    Observation = c.Observation,
                    Statut = c.Statut,
                    // ✅ CONSERVER : Mapping de la souscription
                    SouscriptionPrestationId = c.Souscription?.PrestationId, // Utiliser l'ID de la souscription imbriquée
                    
                    // ✅ AJOUT : Propriétés requises pour CollecteCreateDto
                    AffilieId = 0, // Sera assigné après création de l'affilié
                    AgentId = terrainAgentId // Utiliser l'agent de l'adhésion
                };
            }).ToList();
            
            await ValidateCollectesAsync(collectesCreateDtos, terrainAgentId, ct);

            // ✅ VALIDATION 4 : Validation croisée entre collectes et références
            ValidateCrossReferences(collectesCreateDtos);

            // ✅ VALIDATION 6 : Validation des données existantes (unicité, conflits)
            await ValidateExistingDataAsync(input, ct);

            // ✅ VALIDATION 2 : Validation des dépendants (âge, liens parentaux valides)
            ValidateDependants(input.Dependants, input.DateNaissance);

            var nombreDependants = input.Dependants?.Count ?? 0;
            await ValidateDependantsCountForTypeAdhesionAsync(input.TypeAdhesionId, nombreDependants, ct);
            await ValidateCotisationCollectesForAdhesionAsync(
                collectesCreateDtos, input.TypeAdhesionId, nombreDependants, ct);

            await ValidateAdhesionCollectesMultideviseAsync(
                collectesCreateDtos, terrainAgentId, nombreDependants, ct);

            var flexPayCollectes = collectesCreateDtos
                .Where(c => MethodePaiementHelper.IsFlexPay(c.ModePaiement))
                .ToList();
            if (flexPayCollectes.Count > 0)
            {
                return BadRequest(
                    "Les paiements électroniques (MOBILE_MONEY/CARTE_BANCAIRE) doivent utiliser " +
                    "POST /api/Adhesion/with-affilie-paiement-electronique.");
            }

            _logger.LogInformation("Création d'adhésion avec {Count} dépendants", nombreDependants);

            var affilie = new Affilie
            {
                Nom = input.Nom,
                Prenom = input.Prenom,
                Postnom = input.Postnom,
                DateNaissance = input.DateNaissance,
                Telephone = input.Telephone,
                EmailAffilie = input.EmailAffilie,
                ProvinceResidence = input.ProvinceResidence,
                CommuneResidence = input.CommuneResidence,
                QuartierResidence = input.QuartierResidence,
                AvenueResidence = input.AvenueResidence,
                NumeroResidence = input.NumeroResidence,
                CommuneActivite = input.CommuneActivite,
                QuartierActivite = input.QuartierActivite,
                AvenueActivite = input.AvenueActivite,
                NumeroActivite = input.NumeroActivite,
                Statut = input.AffilieStatut,
                DateCreation = DateTime.Now
            };

            try
            {
                AffilieFichierApplicator.AppliquerCreation(affilie, input);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            var adhesion = new Adhesion
            {
                StatutDossier = input.StatutDossier,
                TypeAdhesionId = input.TypeAdhesionId,
                AgentId = terrainAgentId,
                UtilisateurId = GetCurrentUserId(), // 🆕 Récupérer l'ID utilisateur connecté
                Statut = input.AdhesionStatut,
                DateCreation = DateTime.Now
            };

            // 🆕 NOUVELLE APPROCHE : Extraire les souscriptions des collectes
            var allSouscriptions = new List<SouscriptionPrestation>();
            var allCollectes = new List<Collecte>();

            // ✅ VALIDATION 2 : Vérifier les doublons de souscriptions (PrestationId, AffilieId)
            // Note : On ne peut pas vérifier avant la création de l'affilié, donc on le fera dans le service
            var prestationIds = collectesCreateDtos
                .Where(c => c.SouscriptionPrestationId.HasValue) // ✅ CORRIGÉ : Utiliser la propriété mappée
                .Select(c => c.SouscriptionPrestationId.Value)
                .Distinct()
                .ToList();

            if (prestationIds.Any())
            {
                var prestationsExistantes = await _db.Prestations
                    .Where(p => prestationIds.Contains(p.IdPrestation))
                    .Select(p => p.IdPrestation)
                    .ToListAsync(ct);

                var prestationsManquantes = prestationIds.Except(prestationsExistantes).ToList();
                if (prestationsManquantes.Any())
                {
                    var errorResponse = _errorService.CreateBusinessError(
                        ErrorCodes.BUSINESS_PRESTATION_INEXISTANTE,
                        $"Les prestations suivantes n'existent pas : {string.Join(", ", prestationsManquantes)}",
                        prestationsManquantes.Select(id => new ErrorDetail
                        {
                            Field = "PrestationId",
                            Value = id,
                            Issue = "Prestation non trouvée",
                            Expected = "Prestation existante dans la base de données"
                        }).ToList());
                    
                    return BadRequest(errorResponse);
                }
            }

            // ✅ VALIDATION 2.1 : Vérifier l'existence des frais
            var fraisIds = collectesCreateDtos
                .Where(c => c.FraisId.HasValue)
                .Select(c => c.FraisId.Value)
                .Distinct()
                .ToList();

            if (fraisIds.Any())
            {
                var fraisExistant = await _db.Frais
                    .Where(f => fraisIds.Contains(f.IdFrais))
                    .Select(f => f.IdFrais)
                    .ToListAsync(ct);

                var fraisManquants = fraisIds.Except(fraisExistant).ToList();
                if (fraisManquants.Any())
                {
                    var errorResponse = _errorService.CreateBusinessError(
                        ErrorCodes.BUSINESS_FRAIS_INEXISTANT,
                        $"Les frais suivants n'existent pas : {string.Join(", ", fraisManquants)}",
                        fraisManquants.Select(id => new ErrorDetail
                        {
                            Field = "FraisId",
                            Value = id,
                            Issue = "Frais non trouvé",
                            Expected = "Frais existant dans la base de données"
                        }).ToList());
                    
                    return BadRequest(errorResponse);
                }
            }

            var cotisationPayeeDansLot = collectesCreateDtos.Any(c =>
                c.TypeCollecte == TypeCollecte.Cotisation && c.Statut);

            var prestationIdsProduits = collectesCreateDtos
                .Where(c => c.TypeCollecte == TypeCollecte.Souscription && c.SouscriptionPrestationId.HasValue)
                .Select(c => c.SouscriptionPrestationId!.Value)
                .Distinct()
                .ToList();

            foreach (var prestationId in prestationIdsProduits)
            {
                try
                {
                    await ProduitEligibiliteRules.ValidateAchatProduitAsync(
                        _db,
                        affilieId: 0,
                        prestationId,
                        ct,
                        dateNaissanceOverride: input.DateNaissance,
                        typeAdhesionIdOverride: input.TypeAdhesionId,
                        cotisationPayeeDansLot: cotisationPayeeDansLot,
                        nouvelleAdhesionNiveau1: true);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            foreach (var collecteDto in collectesCreateDtos)
            {
                // 🆕 Créer la souscription si elle existe
                if (collecteDto.SouscriptionPrestationId.HasValue) // ✅ CORRIGÉ : Utiliser la propriété mappée
                {
                    var souscription = new SouscriptionPrestation
                    {
                        PrestationId = collecteDto.SouscriptionPrestationId.Value, // ✅ CORRIGÉ : Utiliser la valeur mappée
                        DateSouscription = DateTime.Now,
                        Statut = collecteDto.Statut,
                        DateCreation = DateTime.Now
                    };
                    allSouscriptions.Add(souscription);
                }

                // 🆕 Créer la collecte
                var collecte = new Collecte
                {
                    TypeCollecte = collecteDto.TypeCollecte,
                    FraisId = collecteDto.FraisId,
                    CotisationAffilieId = collecteDto.CotisationAffilieId,
                    SouscriptionPrestationId = collecteDto.SouscriptionPrestationId,
                    Montant = collecteDto.Montant,
                    Mois = collecteDto.Mois,
                    Annee = collecteDto.Annee,
                    ReferencePaiement = collecteDto.ReferencePaiement,
                    ModePaiement = collecteDto.ModePaiement,
                    Operateur = collecteDto.Operateur,
                    StatutPaiement = collecteDto.StatutPaiement,
                    MontantRecu = collecteDto.MontantRecu,
                    MontantAttendu = collecteDto.MontantAttendu,
                    DeviseId = collecteDto.DeviseId,
                    Observation = collecteDto.Observation,
                    DateCollecte = CollecteAdhesionHelper.ResolveDateCollecte(collecteDto),
                    DateCreation = DateTime.Now,
                    Statut = collecteDto.Statut,
                    // ✅ AJOUT : AffilieId sera assigné après création de l'affilié
                    AgentId = collecteDto.AgentId // Utiliser l'agent du DTO
                };
                allCollectes.Add(collecte);
            }

            try
            {
                // 🆕 GESTION TRANSACTIONNELLE : Wrapper dans une transaction atomique
                using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct);
                
                try
                {
                    // ✅ METTRE À JOUR : Assigner l'AffilieId aux collectes avant la création
                    foreach (var collecte in allCollectes)
                    {
                        collecte.AffilieId = affilie.IdAffilie;
                    }

                    var created = await _repo.CreateWithAffilieAsync(
                        affilie, adhesion, allSouscriptions, allCollectes, nombreDependants, ct);

                    var createdAffilie = await _affilieRepo.GetByIdAsync(created.AffilieId, ct);
                    
                    // 🆕 Créer les dépendants APRÈS la création de l'affilie avec l'ID disponible
                    List<DependantReadDto> dependantsDto = new();
                    if (input.Dependants.Any())
                    {
                        // Préparer les dépendants avec l'ID de l'affilié créé
                        var dependantsToCreate = input.Dependants
                            .Select(d => MapDependantFromCreate(d, createdAffilie.IdAffilie))
                            .ToList();
                        
                        var createdDependants = await _repo.CreateDependantsAsync(createdAffilie.IdAffilie, dependantsToCreate, ct);
                        dependantsDto = createdDependants.Select(DependantDtoMapper.ToReadDto).ToList();
                        _logger.LogInformation("Dépendants créés: {Count}", createdDependants.Count);
                    }

                    //  NOUVEAU : Créer les antécédents après la création de l'affilie
                    List<AntecedantReadDto> antecedantsDto = new();
                    if (input.Antecedants.Any())
                    {
                        // Préparer les antécédents avec l'ID de l'affilié créé
                        var antecedantsToCreate = input.Antecedants.Select(a => new Antecedant
                        {
                            Description = a.Description.Trim(),
                            AffilieId = createdAffilie.IdAffilie,
                            DateCreation = DateTime.Now,
                            Statut = a.Statut
                        }).ToList();

                        _db.Antecedants.AddRange(antecedantsToCreate);
                        await _db.SaveChangesAsync(ct);

                        // Mapper les antécédents créés vers les DTOs
                        antecedantsDto = antecedantsToCreate.Select(a => new AntecedantReadDto
                        {
                            IdAntecedant = a.IdAntecedant,
                            Description = a.Description,
                            AffilieId = a.AffilieId,
                            DateCreation = a.DateCreation,
                            DateModification = a.DateModification,
                            Statut = a.Statut
                        }).ToList();
                        _logger.LogInformation("Antécédents créés: {Count}", antecedantsDto.Count);
                    }

                    PersonneContactReadDto? personneContactDto = null;
                    if (AdhesionNiveau2Regles.EstRenseigne(input.PersonneContact))
                    {
                        var contactDb = await _repo.CreateOrUpdatePersonneContactAsync(
                            createdAffilie.IdAffilie,
                            AdhesionNiveau2Regles.MapToEntity(input.PersonneContact!),
                            ct);
                        personneContactDto = MapPersonneContactReadDto(contactDb);
                        _logger.LogInformation("Personne de contact créée pour l'affilié {AffilieId}", createdAffilie.IdAffilie);
                    }

                    // 🆕 DÉBITER WALLET VIRTUEL si nécessaire (déjà fait dans CommissionService)
                    // La validation a déjà été faite en amont, donc le débit devrait réussir

                    // 🆕 Construire le DTO de réponse
                    var typeAdhesion = await _db.TypeAdhesions
                        .Where(t => t.IdTypeAdhesion == input.TypeAdhesionId)
                        .Select(t => t.Libelle)
                        .FirstOrDefaultAsync(ct);

                    var agent = await _db.Agents
                        .Where(a => a.IdAgent == input.AgentId)
                        .Select(a => new { a.IdAgent, a.NomComplet })
                        .FirstOrDefaultAsync(ct);
                
                    var createdCollectes = await _db.Collectes
                        .Where(c => c.AffilieId == created.AffilieId)
                        .Include(c => c.Devise)
                        .Include(c => c.Frais)
                        .Include(c => c.CotisationAffilie)
                            .ThenInclude(ca => ca!.TypeAdhesion)
                        .Include(c => c.SouscriptionPrestationRef)
                        .ToListAsync(ct);

                    var collectesDto = createdCollectes.Select(c => new CollecteReadDto
                    {
                        IdCollecte = c.IdCollecte,
                        TypeCollecte = c.TypeCollecte,
                        FraisId = c.FraisId,
                        FraisLibelle = c.Frais?.Libelle,
                        FraisMontant = c.Frais?.Montant,
                        CotisationAffilieId = c.CotisationAffilieId,
                        CotisationPeriodicite = c.CotisationAffilie?.Periodicite,
                        CotisationMontantReference = c.CotisationAffilie?.Montant,
                        CotisationTypeAdhesionId = c.CotisationAffilie?.TypeAdhesionId,
                        CotisationTypeAdhesionLibelle = c.CotisationAffilie?.TypeAdhesion?.Libelle,
                        Montant = c.Montant,
                        Mois = c.Mois,
                        Annee = c.Annee,
                        ReferencePaiement = c.ReferencePaiement,
                        ModePaiement = c.ModePaiement,
                        Operateur = c.Operateur,
                        StatutPaiement = c.StatutPaiement,
                        MontantRecu = c.MontantRecu,
                        MontantAttendu = c.MontantAttendu,
                        DeviseId = c.DeviseId,
                        DeviseNom = c.Devise?.Nom,
                        DeviseCode = c.Devise?.Code,
                        PrestationLibelle = c.SouscriptionPrestationRef?.Prestation?.NomPrestation ?? string.Empty,
                        DateCollecte = c.DateCreation, // Utiliser DateCreation comme DateCollecte
                        Observation = c.Observation,
                        DateCreation = c.DateCreation,
                        DateModification = c.DateModification,
                        Statut = c.Statut,
                        SouscriptionPrestationId = c.SouscriptionPrestationId,
                        AffilieId = c.AffilieId,
                        AgentId = c.AgentId
                    }).ToList();

                    var dto = new AdhesionWithAffilieReadDto
                    {
                        Id = created.IdAdhesion,
                        StatutDossier = created.StatutDossier,
                        DateCreation = created.DateCreation,
                        DateModification = created.DateModification,
                        Statut = created.Statut,
                        AffilieId = created.AffilieId,
                        TypeAdhesionId = created.TypeAdhesionId,
                        TypeAdhesionLibelle = typeAdhesion ?? string.Empty,
                        AgentId = created.AgentId,
                        AgentNom = agent?.NomComplet ?? string.Empty,
                        CodeAdhesion = createdAffilie?.CodeAdhesion ?? string.Empty,
                        Affilie = AffilieDtoMapper.ToReadDto(affilie),
                        Collectes = collectesDto,
                        Dependants = dependantsDto, // 🆕 Dépendants réellement créés
                        Antecedants = antecedantsDto, // ✅ NOUVEAU : Antécédents réellement créés
                        PersonneContact = personneContactDto
                    };

                    await transaction.CommitAsync(ct);
                    _logger.LogInformation("Transaction validée avec succès pour l'adhésion {AdhesionId}", created.IdAdhesion);

                    // 🆕 Envoyer la notification d'adhésion via le service unifié
                    try
                    {
                        await _notificationService.SendAdhesionConfirmationAsync(
                            affilie.IdAffilie,
                            affilie.NomComplet,
                            affilie.CodeAdhesion,
                            typeAdhesion ?? "Standard"
                        );
                        
                        _logger.LogInformation("Notification d'adhésion envoyée pour l'affilié {AffilieId}", affilie.IdAffilie);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur lors de l'envoi de la notification d'adhésion pour l'affilié {AffilieId}", affilie.IdAffilie);
                        // Ne pas échouer la requête si la notification échoue
                    }

                    return CreatedAtAction(nameof(GetById), new { id = created.IdAdhesion }, dto);
                }
                catch (Exception ex)
                {
                    // 🆕 ROLLBACK en cas d'erreur dans la transaction
                    await transaction.RollbackAsync(ct);
                    _logger.LogError(ex, "Erreur lors de la création de l'adhésion, transaction rollback");
                    
                    // Relancer l'exception pour la gestion par les blocs catch extérieurs
                    throw;
                }
            }
            catch (AdhesionAlreadyExistsException ex)
            {
                var errorResponse = _errorService.CreateConflictError(
                    ErrorCodes.BUSINESS_ADHESION_EXISTANTE,
                    ex.Message,
                    new { affilieId = ex.AffilieId });
                
                return Conflict(errorResponse);
            }
            catch (ArgumentException ex)
            {
                // Vérifier si l'erreur contient des détails de validation structurée
                if (ex.Message.Contains("VALIDATION_"))
                {
                    var validationErrors = new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Field = "General",
                            Message = ex.Message,
                            ErrorCode = "VALIDATION_ERROR"
                        }
                    };
                    
                    var errorResponse = _errorService.CreateValidationError(
                        "VALIDATION_ERROR",
                        ex.Message,
                        validationErrors);
                    
                    return BadRequest(errorResponse);
                }
                else
                {
                    var errorResponse = _errorService.CreateValidationError(
                        "VALIDATION_ERROR",
                        ex.Message);
                    
                    return BadRequest(errorResponse);
                }
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la création de l'adhésion",
                    ex,
                    ErrorCodes.TECHNICAL_DATABASE_ERROR);
            }
        }

        [HttpPost("with-affilie-paiement-electronique")]
        [AllowAnonymous]
        public async Task<ActionResult<InitiateFlexPayResponseDto>> CreateWithAffiliePaiementElectronique(
            [FromBody] AdhesionWithAffiliePaiementElectroniqueCreateDto request,
            CancellationToken ct)
        {
            if (request == null || request.Adhesion == null)
                return BadRequest("Le payload d'adhésion est obligatoire.");

            var modeNormalized = MethodePaiementHelper.NormalizeForStorage(request.ModePaiement);
            if (!MethodePaiementHelper.IsFlexPay(modeNormalized))
            {
                return BadRequest(
                    "ModePaiement invalide pour cet endpoint. Valeurs autorisées : MOBILE_MONEY, CARTE_BANCAIRE.");
            }

            if (request.Adhesion.Collectes == null || !request.Adhesion.Collectes.Any())
                return BadRequest("Au moins une collecte est requise.");

            if (modeNormalized == MethodePaiementHelper.MobileMoney &&
                string.IsNullOrWhiteSpace(request.TelephonePaiement))
            {
                return BadRequest("TelephonePaiement est obligatoire pour MOBILE_MONEY.");
            }

            var modesInvalides = request.Adhesion.Collectes
                .Where(c => !MethodePaiementHelper.IsFlexPay(c.ModePaiement))
                .ToList();
            if (modesInvalides.Count > 0)
            {
                return BadRequest(
                    "Toutes les collectes doivent utiliser MOBILE_MONEY ou CARTE_BANCAIRE pour ce endpoint.");
            }

            var devises = request.Adhesion.Collectes.Select(c => c.DeviseId).Distinct().ToList();
            if (devises.Count != 1 || devises[0] != request.DevisePaiementId)
            {
                return BadRequest(
                    "DevisePaiementId doit correspondre à l'unique devise utilisée dans les collectes.");
            }

            foreach (var collecte in request.Adhesion.Collectes)
            {
                collecte.ModePaiement = modeNormalized;
                collecte.DeviseId = request.DevisePaiementId;
            }

            try
            {
                var response = await _flexPayAdhesionService.InitiateAsync(
                    request.Adhesion,
                    TryGetCurrentUserId(),
                    request.TelephonePaiement,
                    modeNormalized,
                    request.DevisePaiementId,
                    ct);
                return Accepted(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var deny = AffilieMemberScopeHelper.DenyStaffOnlyForMembre(User, "la suppression d'adhésion");
            if (deny != null)
                return deny;

            if (!HasPermission("DELETE_ADHESION"))
                return ForbiddenPermission("DELETE_ADHESION");

            var ok = await _repo.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }

        /// <summary>Fiche papier à compléter par l'encodeur (AA) — dossier EN ATTENTE.</summary>
        [HttpGet("{id:int}/fiche-encodeur")]
        public async Task<ActionResult<AdhesionFicheEncodeurReadDto>> GetFicheEncodeur(int id, CancellationToken ct)
        {
            var deny = AffilieMemberScopeHelper.DenyStaffOnlyForMembre(User, "la fiche encodeur");
            if (deny != null)
                return deny;

            if (!HasPermission("READ_ADHESION"))
                return ForbiddenPermission("READ_ADHESION");

            var adhesion = await _db.Adhesions
                .AsNoTracking()
                .Include(a => a.Affilie)
                    .ThenInclude(af => af.Dependants)
                        .ThenInclude(d => d.Antecedants)
                            .ThenInclude(an => an.Affilie)
                .Include(a => a.Affilie)
                    .ThenInclude(af => af.PersonneContact)
                .FirstOrDefaultAsync(a => a.IdAdhesion == id, ct);

            if (adhesion == null)
                return NotFound();

            var aff = adhesion.Affilie;
            var hasPhoto = AffilieFichierHelper.ADesDonnees(aff.PhotoData);
            var hasCarte = AffilieFichierHelper.ADesDonnees(aff.CarteIdentiteData);
            var hasContact = aff.PersonneContact != null && aff.PersonneContact.Statut;
            var identiteComplete = !string.IsNullOrWhiteSpace(aff.Nom)
                && !string.IsNullOrWhiteSpace(aff.Prenom)
                && aff.DateNaissance != default;
            var adresseActiviteComplete = !string.IsNullOrWhiteSpace(aff.CommuneActivite)
                && !string.IsNullOrWhiteSpace(aff.QuartierActivite);
            var dossierComplet = identiteComplete
                && adresseActiviteComplete
                && hasPhoto
                && hasCarte
                && hasContact;

            return Ok(new AdhesionFicheEncodeurReadDto
            {
                IdAdhesion = adhesion.IdAdhesion,
                StatutDossier = adhesion.StatutDossier,
                AffilieId = adhesion.AffilieId,
                CodeAdhesion = aff.CodeAdhesion,
                NomCompletAffilie = aff.NomComplet,
                Nom = aff.Nom,
                Prenom = aff.Prenom,
                Postnom = aff.Postnom,
                Telephone = aff.Telephone,
                DateNaissance = aff.DateNaissance,
                CommuneActivite = aff.CommuneActivite,
                QuartierActivite = aff.QuartierActivite,
                AvenueActivite = aff.AvenueActivite,
                NumeroActivite = aff.NumeroActivite,
                HasPhoto = hasPhoto,
                HasCarteIdentite = hasCarte,
                HasPersonneContact = hasContact,
                IdentiteComplete = identiteComplete,
                AdresseActiviteComplete = adresseActiviteComplete,
                DossierComplet = dossierComplet,
                Dependants = aff.Dependants
                    .Where(d => d.Statut)
                    .Select(DependantDtoMapper.ToReadDto)
                    .ToList(),
                PersonneContact = aff.PersonneContact == null
                    ? null
                    : MapPersonneContactReadDto(aff.PersonneContact)
            });
        }

        /// <summary>Niveau 2 — Agent Administratif : personnes à charge, contact, validation.</summary>
        [HttpPut("{id:int}/niveau-2-encodeur")]
        public async Task<ActionResult<AdhesionNiveau2EncodeurReadDto>> CompleteNiveau2Encodeur(
            int id,
            [FromBody] AdhesionNiveau2EncodeurDto input,
            CancellationToken ct)
        {
            var deny = AffilieMemberScopeHelper.DenyStaffOnlyForMembre(User, "l'encodage niveau 2");
            if (deny != null)
                return deny;

            if (!HasPermission("UPDATE_ADHESION"))
                return ForbiddenPermission("UPDATE_ADHESION");

            if (input == null)
                return BadRequest("Le corps de la requête est obligatoire.");

            try
            {
                var adhesionPreview = await _db.Adhesions
                    .AsNoTracking()
                    .Include(a => a.Affilie)
                    .FirstOrDefaultAsync(a => a.IdAdhesion == id, ct);

                if (adhesionPreview == null)
                    return NotFound($"Adhésion {id} introuvable.");

                var contactExistantEnBase = await _db.PersonnesContact
                    .AsNoTracking()
                    .AnyAsync(p => p.AffilieId == adhesionPreview.AffilieId && p.Statut, ct);

                var affilieSource = adhesionPreview.Affilie;
                var affiliePourValidation = new Affilie
                {
                    Nom = affilieSource.Nom,
                    Prenom = affilieSource.Prenom,
                    Postnom = affilieSource.Postnom,
                    Telephone = affilieSource.Telephone,
                    DateNaissance = affilieSource.DateNaissance,
                    NomComplet = affilieSource.NomComplet,
                    CommuneActivite = affilieSource.CommuneActivite,
                    QuartierActivite = affilieSource.QuartierActivite,
                    AvenueActivite = affilieSource.AvenueActivite,
                    NumeroActivite = affilieSource.NumeroActivite,
                    PhotoData = affilieSource.PhotoData,
                    PhotoContentType = affilieSource.PhotoContentType,
                    CarteIdentiteData = affilieSource.CarteIdentiteData,
                    CarteIdentiteContentType = affilieSource.CarteIdentiteContentType
                };
                AdhesionNiveau2Regles.AppliquerIdentiteActivite(affiliePourValidation, input);

                var errors = AdhesionNiveau2Regles.Valider(
                    input, affiliePourValidation.DateNaissance, contactExistantEnBase);
                errors.AddRange(AdhesionNiveau2Regles.ValiderDossierCompletPourValidation(
                    affiliePourValidation, input, contactExistantEnBase));
                if (errors.Any())
                {
                    var errorResponse = _errorService.CreateValidationError(
                        ErrorCodes.VALIDATION_NIVEAU2_ENCODEUR,
                        string.Join(" ", errors),
                        errors.Select(msg => new ValidationError
                        {
                            Field = "Niveau2",
                            Message = msg,
                            ErrorCode = ErrorCodes.VALIDATION_NIVEAU2_ENCODEUR
                        }).ToList());
                    return BadRequest(errorResponse);
                }

                var dependants = input.Dependants.Select(MapDependantFromNiveau2).ToList();

                PersonneContact? personneContact = AdhesionNiveau2Regles.EstRenseigne(input.PersonneContact)
                    ? AdhesionNiveau2Regles.MapToEntity(input.PersonneContact)
                    : null;

                var updated = await _repo.CompleteNiveau2EncodeurAsync(
                    id, dependants, personneContact, input, ct);

                var affilieId = updated.AffilieId;
                var dependantsDb = await _repo.GetDependantsByAffilieIdAsync(affilieId, ct);
                var contactDb = await _db.PersonnesContact
                    .AsNoTracking()
                    .FirstAsync(p => p.AffilieId == affilieId, ct);

                return Ok(new AdhesionNiveau2EncodeurReadDto
                {
                    IdAdhesion = updated.IdAdhesion,
                    StatutDossier = updated.StatutDossier,
                    AffilieId = affilieId,
                    Dependants = dependantsDb.Select(DependantDtoMapper.ToReadDto).ToList(),
                    PersonneContact = MapPersonneContactReadDto(contactDb)
                });
            }
            catch (AdhesionNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (AdhesionNotInWaitingStateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors du traitement niveau 2 encodeur",
                    ex);
            }
        }

        private static PersonneContactReadDto MapPersonneContactReadDto(PersonneContact p) => new()
        {
            IdPersonneContact = p.IdPersonneContact,
            AffilieId = p.AffilieId,
            NomComplet = p.NomComplet,
            LienParente = p.LienParente,
            Adresse = p.Adresse,
            Statut = p.Statut
        };

        // 🆕 ENDPOINT UPDATE WITH AFFILIE
        [HttpPut("UpdateWithAffilieAsync/{id:int}")]
        public async Task<ActionResult<AdhesionReadDto>> UpdateWithAffilieAsync(
            [FromRoute] int id, 
            [FromBody] AdhesionUpdateWithAffilieDto updateDto, 
            CancellationToken ct)
        {
            try
            {
                var deny = AffilieMemberScopeHelper.DenyStaffOnlyForMembre(User, "la mise à jour d'adhésion");
                if (deny != null)
                    return deny;

                if (!HasPermission("UPDATE_ADHESION"))
                    return ForbiddenPermission("UPDATE_ADHESION");

                // Construire l'objet Affilie
                var affilie = new Affilie
                {
                    Nom = updateDto.Affilie.Nom,
                    Prenom = updateDto.Affilie.Prenom,
                    DateNaissance = updateDto.Affilie.DateNaissance,
                    Telephone = updateDto.Affilie.Telephone,
                    Postnom = updateDto.Affilie.Postnom,
                    ProvinceResidence = updateDto.Affilie.ProvinceResidence,
                    CommuneResidence = updateDto.Affilie.CommuneResidence,
                    QuartierResidence = updateDto.Affilie.QuartierResidence,
                    AvenueResidence = updateDto.Affilie.AvenueResidence,
                    NumeroResidence = updateDto.Affilie.NumeroResidence,
                    CommuneActivite = updateDto.Affilie.CommuneActivite,
                    QuartierActivite = updateDto.Affilie.QuartierActivite,
                    AvenueActivite = updateDto.Affilie.AvenueActivite,
                    NumeroActivite = updateDto.Affilie.NumeroActivite,
                    Statut = updateDto.Affilie.Statut
                };

                // Construire l'objet Adhesion
                var adhesion = new Adhesion
                {
                    StatutDossier = updateDto.Adhesion.StatutDossier
                };

                // Construire les souscriptions
                var souscriptions = updateDto.Souscriptions.Select(s => new SouscriptionPrestation
                {
                    PrestationId = s.PrestationId,
                    DateSouscription = s.DateDebut,
                    Statut = s.Statut
                });

                // Construire les dépendants
                var dependants = updateDto.Dependents.Select(d => new Dependant
                {
                    IdDependant = d.IdDependant ?? 0,
                    Nom = d.Nom,
                    LienParente = d.LienParente,
                    Statut = d.Statut
                });

                var updated = await _repo.UpdateWithAffilieAsync(id, affilie, adhesion, souscriptions, dependants, ct);

                var dto = ToReadDto(updated);
                return Ok(dto);
            }
            catch (AdhesionNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (AdhesionNotInWaitingStateException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (AffilieDuplicateException ex)
            {
                return Conflict($"Conflit d'affilié: {ex.Message}");
            }
            catch (AdresseAffilieIncompleteException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la mise à jour de l'adhésion",
                    ex);
            }
        }

        /// <summary>
        /// Récupère les adhésions paginées
        /// </summary>
        [HttpGet("paginated")]
        public async Task<ActionResult<PaginatedResponse<AdhesionReadDto>>> GetPaginated(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var deny = AffilieMemberScopeHelper.DenyListAccessForMembre(User, "des adhésions");
                if (deny != null)
                    return deny;

                if (!HasPermission("READ_ADHESION"))
                    return ForbiddenPermission("READ_ADHESION");

                var query = _db.Adhesions
                    .Include(a => a.Affilie)
                    .Include(a => a.TypeAdhesion)
                    .AsQueryable();

                query = query.ApplyAdhesionSearch(request.Search);
                request.Search = null;

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(ToReadDto).ToList();
                var paginatedDtos = new PaginatedResponse<AdhesionReadDto>
                {
                    Data = dtos,
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasNextPage = result.HasNextPage,
                    HasPreviousPage = result.HasPreviousPage
                };

                return Ok(paginatedDtos);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des adhésions paginées",
                    ex);
            }
        }

        /// <summary>
        /// Récupère les adhésions avec filtres avancés
        /// </summary>
        [HttpPost("advanced")]
        public async Task<ActionResult<ExtendedPaginatedResponse<AdhesionReadDto>>> GetAdhesionsAdvanced(
            [FromBody] AdvancedPaginationRequest request)
        {
            try
            {
                var deny = AffilieMemberScopeHelper.DenyListAccessForMembre(User, "des adhésions");
                if (deny != null)
                    return deny;

                if (!HasPermission("READ_ADHESION"))
                    return ForbiddenPermission("READ_ADHESION");

                // Construire la requête de base
                var query = _db.Adhesions
                    .Include(a => a.Affilie)
                    .Include(a => a.TypeAdhesion)
                    .AsQueryable();

                // Appliquer les filtres de base
                if (request.FilterList != null && request.FilterList.Any())
                {
                    foreach (var filter in request.FilterList)
                    {
                        switch (filter.Field.ToLower())
                        {
                            case "statutdossier":
                                if (filter.Operator == "eq")
                                    query = query.Where(a => a.StatutDossier == filter.Value);
                                break;
                            case "affilieid":
                                if (int.TryParse(filter.Value, out int affilieId))
                                {
                                    if (filter.Operator == "eq")
                                        query = query.Where(a => a.AffilieId == affilieId);
                                    else if (filter.Operator == "gt")
                                        query = query.Where(a => a.AffilieId > affilieId);
                                    else if (filter.Operator == "lt")
                                        query = query.Where(a => a.AffilieId < affilieId);
                                }
                                break;
                            case "typeadhesionid":
                                if (int.TryParse(filter.Value, out int typeId))
                                {
                                    if (filter.Operator == "eq")
                                        query = query.Where(a => a.TypeAdhesionId == typeId);
                                }
                                break;
                        }
                    }
                }

                // Appliquer la pagination
                var response = await _paginationService.CreateExtendedPaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs
                var adhesionDtos = response.Data.Select(ToReadDto).ToList();
                
                // Créer une nouvelle réponse avec les DTOs
                var dtoResponse = new ExtendedPaginatedResponse<AdhesionReadDto>
                {
                    Data = adhesionDtos,
                    CurrentPage = response.CurrentPage,
                    PageSize = response.PageSize,
                    TotalItems = response.TotalItems,
                    TotalPages = response.TotalPages,
                    HasNextPage = response.HasNextPage,
                    HasPreviousPage = response.HasPreviousPage,
                    AppliedFilters = request.FilterList?.Select(f => $"{f.Field} {f.Operator} {f.Value}").ToList() ?? new(),
                    AppliedSorting = $"{request.SortBy} {request.SortDirection}"
                };

                return Ok(dtoResponse);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des adhésions avancées",
                    ex);
            }
        }

        private static AdhesionReadDto ToReadDto(Adhesion entity)
        {
            return new AdhesionReadDto
            {
                Id = entity.IdAdhesion,
                StatutDossier = entity.StatutDossier,
                DateCreation = entity.DateCreation,
                DateModification = entity.DateModification,
                Statut = entity.Statut,
                AffilieId = entity.AffilieId,
                TypeAdhesionId = entity.TypeAdhesionId,
                AgentId = entity.AgentId
            };
    }

    // Méthode utilitaire pour mapper les collectes
    private List<CollecteReadDto> MapCollectesToDto(List<Collecte> createdCollectes)
    {
        return createdCollectes.Select(c => new CollecteReadDto
        {
            IdCollecte = c.IdCollecte,
            TypeCollecte = c.TypeCollecte,
            FraisId = c.FraisId,
            AffilieId = c.AffilieId,
            AgentId = c.AgentId,
            Montant = c.Montant,
            ReferencePaiement = c.ReferencePaiement,
            ModePaiement = c.ModePaiement,
            Operateur = c.Operateur,
            StatutPaiement = c.StatutPaiement,
            SouscriptionPrestationId = c.SouscriptionPrestationId,
            MontantRecu = c.MontantRecu,
            MontantAttendu = c.MontantAttendu,
            DeviseId = c.DeviseId,
            DeviseNom = c.Devise?.Nom,
            DeviseCode = c.Devise?.Code,
            FraisLibelle = c.Frais?.Libelle,
            CotisationAffilieId = c.CotisationAffilieId,
            CotisationPeriodicite = c.CotisationAffilie?.Periodicite,
            CotisationMontantReference = c.CotisationAffilie?.Montant,
            CotisationTypeAdhesionId = c.CotisationAffilie?.TypeAdhesionId,
            CotisationTypeAdhesionLibelle = c.CotisationAffilie?.TypeAdhesion?.Libelle,
            PrestationLibelle = c.SouscriptionPrestationRef?.Prestation?.NomPrestation ?? string.Empty,
            DateCollecte = c.DateCollecte,
            Observation = c.Observation,
            DateCreation = c.DateCreation,
            DateModification = c.DateModification,
            Statut = c.Statut,
            
            // 🆕 Ajouter les champs période
            Mois = c.Mois,
            Annee = c.Annee
        }).ToList();
    }
}
}
