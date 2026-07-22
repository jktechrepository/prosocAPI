using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.DTOs.FlexPay;
using ProsocAPI.Services.Repositories;
using ProsocAPI.Services;
using ProsocAPI.Utilities;
using Prosoc.Utilities;

namespace ProsocAPI.Services
{
    public interface IAdhesionWithAffilieExecutorService
    {
        List<CollecteCreateDto> MapCollectesCreateDtos(AdhesionWithAffilieCreateDto input, bool isOnlineFlexPay = false);

        Task<AdhesionWithAffilieReadDto> ExecuteAsync(
            AdhesionWithAffilieCreateDto input,
            int? utilisateurId,
            FlexPayCallbackDto? flexPayCallback,
            string? flexPayModeNormalized,
            CancellationToken ct = default);
    }

    public class AdhesionWithAffilieExecutorService : IAdhesionWithAffilieExecutorService
    {
        private readonly ProsocDbContext _db;
        private readonly IAdhesionRepository _adhesionRepo;
        private readonly IAffilieRepository _affilieRepo;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AdhesionWithAffilieExecutorService> _logger;

        public AdhesionWithAffilieExecutorService(
            ProsocDbContext db,
            IAdhesionRepository adhesionRepo,
            IAffilieRepository affilieRepo,
            INotificationService notificationService,
            ILogger<AdhesionWithAffilieExecutorService> logger)
        {
            _db = db;
            _adhesionRepo = adhesionRepo;
            _affilieRepo = affilieRepo;
            _notificationService = notificationService;
            _logger = logger;
        }

        public List<CollecteCreateDto> MapCollectesCreateDtos(AdhesionWithAffilieCreateDto input, bool isOnlineFlexPay = false)
        {
            var adhesionAgentId = AdhesionAgentIdHelper.ResolveAdhesionAgentId(input.AgentId, isOnlineFlexPay);
            var collecteAgentId = AdhesionAgentIdHelper.ResolveCollecteAgentId(adhesionAgentId);

            return input.Collectes.Select(c =>
            {
                if (c == null)
                    throw new ArgumentException("Une collecte ne peut pas être null");

                return new CollecteCreateDto
                {
                    TypeCollecte = c.TypeCollecte,
                    FraisId = c.FraisId,
                    CotisationAffilieId = c.CotisationAffilieId,
                    Montant = c.Montant,
                    Mois = c.Mois,
                    Annee = c.Annee,
                    ReferencePaiement = c.ReferencePaiement,
                    ModePaiement = c.ModePaiement ?? string.Empty,
                    Operateur = c.Operateur,
                    StatutPaiement = c.StatutPaiement,
                    MontantRecu = c.MontantRecu,
                    MontantAttendu = c.MontantAttendu,
                    DeviseId = c.DeviseId,
                    Observation = c.Observation,
                    Statut = c.Statut,
                    SouscriptionPrestationId = c.Souscription?.PrestationId,
                    AffilieId = 0,
                    AgentId = collecteAgentId
                };
            }).ToList();
        }

        public async Task<AdhesionWithAffilieReadDto> ExecuteAsync(
            AdhesionWithAffilieCreateDto input,
            int? utilisateurId,
            FlexPayCallbackDto? flexPayCallback,
            string? flexPayModeNormalized,
            CancellationToken ct = default)
        {
            var isOnlineFlexPay = flexPayCallback != null;
            var collectesCreateDtos = MapCollectesCreateDtos(input, isOnlineFlexPay);
            var adhesionAgentId = AdhesionAgentIdHelper.ResolveAdhesionAgentId(input.AgentId, isOnlineFlexPay);

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
                DateCreation = DateTime.UtcNow
            };

            AffilieFichierApplicator.AppliquerCreation(affilie, input);

            var adhesion = new Adhesion
            {
                StatutDossier = input.StatutDossier,
                TypeAdhesionId = input.TypeAdhesionId,
                AgentId = adhesionAgentId,
                UtilisateurId = utilisateurId,
                Statut = input.AdhesionStatut,
                DateCreation = DateTime.UtcNow
            };

            var allSouscriptions = new List<SouscriptionPrestation>();
            var allCollectes = new List<Collecte>();

            for (var i = 0; i < collectesCreateDtos.Count; i++)
            {
                var collecteDto = collectesCreateDtos[i];
                if (collecteDto.SouscriptionPrestationId.HasValue)
                {
                    allSouscriptions.Add(new SouscriptionPrestation
                    {
                        PrestationId = collecteDto.SouscriptionPrestationId.Value,
                        DateSouscription = DateTime.UtcNow,
                        Statut = collecteDto.Statut,
                        DateCreation = DateTime.UtcNow
                    });
                }

                var referencePaiement = flexPayCallback?.OrderNumber != null
                    ? $"{flexPayCallback.OrderNumber}-{collecteDto.TypeCollecte}-{i + 1}"
                    : collecteDto.ReferencePaiement;

                var mode = flexPayModeNormalized ?? MethodePaiementHelper.NormalizeForStorage(collecteDto.ModePaiement);
                allCollectes.Add(new Collecte
                {
                    TypeCollecte = collecteDto.TypeCollecte,
                    FraisId = collecteDto.FraisId,
                    CotisationAffilieId = collecteDto.CotisationAffilieId,
                    SouscriptionPrestationId = collecteDto.SouscriptionPrestationId,
                    OperateurUtilisateurId = utilisateurId,
                    Montant = collecteDto.Montant,
                    Mois = collecteDto.Mois,
                    Annee = collecteDto.Annee,
                    ReferencePaiement = referencePaiement,
                    OrderNumberFlexPay = flexPayCallback?.OrderNumber,
                    ProviderReferenceFlexPay = flexPayCallback?.ProviderReference,
                    ModePaiement = mode,
                    Operateur = flexPayCallback?.Channel ?? collecteDto.Operateur,
                    StatutPaiement = flexPayCallback != null
                        ? CollecteStatutPaiement.Valide
                        : CollecteStatutPaiementRegles.NormaliserPourEcriture(collecteDto.StatutPaiement),
                    MontantRecu = collecteDto.MontantRecu ?? collecteDto.Montant,
                    MontantAttendu = collecteDto.MontantAttendu ?? collecteDto.Montant,
                    DeviseId = collecteDto.DeviseId,
                    Observation = collecteDto.Observation,
                    DateCollecte = CollecteAdhesionHelper.ResolveDateCollecte(collecteDto),
                    DateCreation = DateTime.UtcNow,
                    Statut = collecteDto.Statut,
                    AgentId = collecteDto.AgentId
                });
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var nombreDependants = input.Dependants?.Count ?? 0;
                var created = await _adhesionRepo.CreateWithAffilieAsync(
                    affilie, adhesion, allSouscriptions, allCollectes, nombreDependants, ct);

                var createdAffilie = await _affilieRepo.GetByIdAsync(created.AffilieId, ct)
                    ?? throw new InvalidOperationException("Affilié créé introuvable.");

                List<DependantReadDto> dependantsDto = new();
                if (input.Dependants?.Any() == true)
                {
                    var dependantsToCreate = input.Dependants
                        .Select(d => MapDependantFromCreate(d, createdAffilie.IdAffilie))
                        .ToList();
                    var createdDependants = await _adhesionRepo.CreateDependantsAsync(
                        createdAffilie.IdAffilie, dependantsToCreate, ct);
                    dependantsDto = createdDependants.Select(DependantDtoMapper.ToReadDto).ToList();
                }

                List<AntecedantReadDto> antecedantsDto = new();
                if (input.Antecedants?.Any() == true)
                {
                    var antecedantsToCreate = input.Antecedants.Select(a => new Antecedant
                    {
                        Description = a.Description.Trim(),
                        AffilieId = createdAffilie.IdAffilie,
                        DateCreation = DateTime.UtcNow,
                        Statut = a.Statut
                    }).ToList();
                    _db.Antecedants.AddRange(antecedantsToCreate);
                    await _db.SaveChangesAsync(ct);
                    antecedantsDto = antecedantsToCreate.Select(a => new AntecedantReadDto
                    {
                        IdAntecedant = a.IdAntecedant,
                        Description = a.Description,
                        AffilieId = a.AffilieId,
                        DateCreation = a.DateCreation,
                        DateModification = a.DateModification,
                        Statut = a.Statut
                    }).ToList();
                }

                PersonneContactReadDto? personneContactDto = null;
                if (AdhesionNiveau2Regles.EstRenseigne(input.PersonneContact))
                {
                    var contactDb = await _adhesionRepo.CreateOrUpdatePersonneContactAsync(
                        createdAffilie.IdAffilie,
                        AdhesionNiveau2Regles.MapToEntity(input.PersonneContact!),
                        ct);
                    personneContactDto = MapPersonneContactReadDto(contactDb);
                }

                var typeAdhesion = await _db.TypeAdhesions.AsNoTracking()
                    .Where(t => t.IdTypeAdhesion == input.TypeAdhesionId)
                    .Select(t => t.Libelle)
                    .FirstOrDefaultAsync(ct);

                var agent = adhesionAgentId.HasValue
                    ? await _db.Agents.AsNoTracking()
                        .Where(a => a.IdAgent == adhesionAgentId.Value)
                        .Select(a => new { a.IdAgent, a.NomComplet })
                        .FirstOrDefaultAsync(ct)
                    : null;

                var createdCollectes = await _db.Collectes
                    .Where(c => c.AffilieId == created.AffilieId)
                    .Include(c => c.Devise)
                    .Include(c => c.Frais)
                    .Include(c => c.CotisationAffilie)
                        .ThenInclude(ca => ca!.TypeAdhesion)
                    .Include(c => c.SouscriptionPrestationRef)
                        .ThenInclude(sp => sp!.Prestation)
                    .ToListAsync(ct);

                await transaction.CommitAsync(ct);

                try
                {
                    await _notificationService.SendAdhesionConfirmationAsync(
                        createdAffilie.IdAffilie,
                        createdAffilie.NomComplet,
                        createdAffilie.CodeAdhesion,
                        typeAdhesion ?? "Standard");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Notification adhésion FlexPay échouée pour affilié {AffilieId}", createdAffilie.IdAffilie);
                }

                return new AdhesionWithAffilieReadDto
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
                    CodeAdhesion = createdAffilie.CodeAdhesion ?? string.Empty,
                    Affilie = AffilieDtoMapper.ToReadDto(createdAffilie),
                    Collectes = createdCollectes.Select(MapCollecteRead).ToList(),
                    Dependants = dependantsDto,
                    Antecedants = antecedantsDto,
                    PersonneContact = personneContactDto
                };
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        private static CollecteReadDto MapCollecteRead(Collecte c) => new()
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
            DateCollecte = c.DateCollecte,
            Observation = c.Observation,
            DateCreation = c.DateCreation,
            DateModification = c.DateModification,
            Statut = c.Statut,
            SouscriptionPrestationId = c.SouscriptionPrestationId,
            AffilieId = c.AffilieId,
            AgentId = c.AgentId
        };

        private static Dependant MapDependantFromCreate(DependantCreateDto d, int affilieId)
        {
            var dependant = new Dependant
            {
                Nom = d.Nom.Trim(),
                Adresse = d.Adresse,
                LienParente = LienParenteRegles.Normaliser(d.LienParente),
                DateNaissance = d.DateNaissance,
                AffilieId = affilieId,
                DateCreation = DateTime.UtcNow,
                Statut = true
            };

            DependantCertificatApplicator.Appliquer(
                dependant, d.CertificatScolariteBase64, d.CertificatScolariteContentType);

            return dependant;
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
    }
}
