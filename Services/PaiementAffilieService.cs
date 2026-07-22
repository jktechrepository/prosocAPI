using ProsocAPI.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;

namespace ProsocAPI.Services
{
    public class PaiementAffilieService : IPaiementAffilieService
    {
        private readonly ProsocDbContext _db;
        private readonly ICommissionService _commissionService;
        private readonly ICollecteMultideviseService _multidevise;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PaiementAffilieService> _logger;

        public PaiementAffilieService(
            ProsocDbContext db,
            ICommissionService commissionService,
            ICollecteMultideviseService multidevise,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PaiementAffilieService> logger)
        {
            _db = db;
            _commissionService = commissionService;
            _multidevise = multidevise;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<SouscriptionPrestation>> GetSouscriptionsPayablesAsync(int affilieId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des souscriptions payables pour l'affilié {AffilieId}", affilieId);

                var souscriptions = await _db.SouscriptionsPrestations
                    .Include(sp => sp.Affilie)
                    .Include(sp => sp.Prestation)
                    .Where(sp => sp.AffilieId == affilieId && sp.Statut)
                    .OrderBy(sp => sp.DateCreation)
                    .ToListAsync(ct);

                _logger.LogInformation("Trouvé {Count} souscriptions pour l'affilié {AffilieId}", souscriptions.Count, affilieId);

                return souscriptions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des souscriptions payables pour l'affilié {AffilieId}", affilieId);
                throw;
            }
        }

        public async Task<Collecte> PayerSouscriptionAsync(int affilieId, PayerSouscriptionDto dto, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Début du paiement de souscription par l'affilié {AffilieId} - Souscription: {SouscriptionId}", 
                    affilieId, dto.SouscriptionPrestationId);

                MethodePaiementHelper.EnsureGuichetSyncOnly(dto.ModePaiement);
                WalletVirtuelPaiementAutorisation.EnsureSiVirtualAccount(
                    dto.ModePaiement,
                    _httpContextAccessor.HttpContext?.User);

                // 1. Valider que la souscription appartient à l'affilié
                var souscription = await _db.SouscriptionsPrestations
                    .Include(sp => sp.Affilie)
                    .FirstOrDefaultAsync(sp => sp.IdSouscriptionPrestation == dto.SouscriptionPrestationId, ct);

                if (souscription == null)
                    throw new ArgumentException("Souscription introuvable");

                if (souscription.AffilieId != affilieId)
                    throw new UnauthorizedAccessException("Cette souscription ne vous appartient pas");

                var moisPaiement = dto.Mois > 0 ? dto.Mois : DateTime.UtcNow.Month;
                var anneePaiement = dto.Annee > 0 ? dto.Annee : DateTime.UtcNow.Year;

                // 2. Vérifier que la période n'est pas déjà payée
                var dejaPayee = await _db.Collectes
                    .AnyAsync(c => c.SouscriptionPrestationId == dto.SouscriptionPrestationId
                                   && c.TypeCollecte == TypeCollecte.Souscription
                                   && c.Mois == moisPaiement
                                   && c.Annee == anneePaiement
                                   && c.Statut, ct);

                if (dejaPayee)
                    throw new InvalidOperationException(
                        $"Cette souscription est déjà payée pour la période {moisPaiement:D2}/{anneePaiement}");

                await ProduitEligibiliteRules.ValidateAchatProduitBySouscriptionAsync(
                    _db, affilieId, dto.SouscriptionPrestationId, ct);

                var nombreDependants = await _db.Dependants
                    .CountAsync(d => d.AffilieId == affilieId && d.Statut, ct);

                // 3. Créer la collecte
                var collecte = new Collecte
                {
                    TypeCollecte = TypeCollecte.Souscription,
                    SouscriptionPrestationId = dto.SouscriptionPrestationId,
                    AffilieId = affilieId,
                    AgentId = 0, // Agent référent - à implémenter via Adhesion si nécessaire
                    Montant = dto.Montant,
                    ReferencePaiement = dto.ReferencePaiement,
                    ModePaiement = dto.ModePaiement,
                    Operateur = "AUTO_PAIEMENT_AFFILIE",
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    MontantRecu = dto.Montant,
                    MontantAttendu = dto.Montant,
                    DeviseId = dto.DeviseId,
                    Mois = moisPaiement,
                    Annee = anneePaiement,
                    Observation = dto.Observation ?? $"Paiement automatique par affilié - Souscription {souscription.IdSouscriptionPrestation}",
                    DateCollecte = DateTime.UtcNow,
                    DateCreation = DateTime.UtcNow,
                    Statut = true
                };

                await _multidevise.ValidateAndApplySnapshotAsync(collecte, nombreDependants, ct);

                _db.Collectes.Add(collecte);
                await _db.SaveChangesAsync(ct);

                // 4. Calculer et traiter la commission
                await _commissionService.ProcessCommissionAsync(collecte, ct);

                _logger.LogInformation("Paiement de souscription terminé avec succès - Collecte: {CollecteId}", 
                    collecte.IdCollecte);

                return collecte;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du paiement de souscription par l'affilié {AffilieId}", affilieId);
                throw;
            }
        }

        public IQueryable<Collecte> GetHistoriquePaiementsQuery(int affilieId) =>
            _db.Collectes
                .AsNoTracking()
                .Where(c => c.AffilieId == affilieId
                    && c.TypeCollecte == TypeCollecte.Souscription
                    && c.Statut)
                .OrderByDescending(c => c.DateCreation);

        public async Task<List<Collecte>> GetHistoriquePaiementsAsync(int affilieId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération de l'historique des paiements pour l'affilié {AffilieId}", affilieId);

                var paiements = await GetHistoriquePaiementsQuery(affilieId)
                    .Include(c => c.Affilie)
                    .Include(c => c.Agent)
                    .Include(c => c.Devise)
                    .Include(c => c.Frais)
                    .Include(c => c.SouscriptionPrestationRef)
                    .ToListAsync(ct);

                _logger.LogInformation("Trouvé {Count} paiements pour l'affilié {AffilieId}", paiements.Count, affilieId);

                return paiements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'historique des paiements pour l'affilié {AffilieId}", affilieId);
                throw;
            }
        }

        public async Task<bool> ValiderPaiementAutomatiqueAsync(Collecte collecte, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Validation automatique du paiement {CollecteId}", collecte.IdCollecte);

                var isValid = collecte.TypeCollecte == TypeCollecte.Souscription
                            && collecte.Statut
                            && collecte.MontantRecu >= collecte.MontantAttendu
                            && collecte.SouscriptionPrestationId.HasValue;

                if (isValid)
                {
                    collecte.StatutPaiement = CollecteStatutPaiement.Valide;
                    collecte.DateModification = DateTime.UtcNow;
                    await _db.SaveChangesAsync(ct);
                    
                    _logger.LogInformation("Paiement {CollecteId} validé automatiquement", collecte.IdCollecte);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation automatique du paiement {CollecteId}", collecte.IdCollecte);
                return false;
            }
        }

        public async Task<Commission> CalculerCommissionAsync(Collecte collecte, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Calcul de la commission pour la collecte {CollecteId}", collecte.IdCollecte);

                var taux = await ResolveCommissionRateAsync(collecte, ct);

                // Retourner un objet Commission simple pour compatibilité
                var commission = new Commission
                {
                    IdCommission = 0,
                    CollecteId = collecte.IdCollecte,
                    AgentId = collecte.AgentId ?? 0,
                    Montant = (collecte.MontantDevisePrincipale ?? collecte.Montant) * (taux / 100m),
                    Taux = taux / 100m,
                    DateCreation = DateTime.UtcNow,
                    Statut = true,
                    Source = "COMMISSION_COLLECTE_AFFILIE",
                    Description = $"Commission collecte #{collecte.IdCollecte} - Affilié {collecte.AffilieId}"
                };
                
                _logger.LogInformation("Commission calculée : {Montant}% pour l'agent {AgentId}", 
                    commission.Taux, collecte.AgentId);

                return commission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du calcul de la commission pour la collecte {CollecteId}", collecte.IdCollecte);
                return null;
            }
        }

        public async Task<bool> VerifierProprieteSouscriptionAsync(int souscriptionId, int affilieId, CancellationToken ct = default)
        {
            try
            {
                var souscription = await _db.SouscriptionsPrestations
                    .FirstOrDefaultAsync(sp => sp.IdSouscriptionPrestation == souscriptionId, ct);

                return souscription?.AffilieId == affilieId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de propriété de la souscription {SouscriptionId}", souscriptionId);
                return false;
            }
        }

        public async Task<bool> EstSouscriptionDejaPayeeAsync(int souscriptionId, CancellationToken ct = default)
        {
            try
            {
                var dejaPayee = await _db.Collectes
                    .AnyAsync(c => c.SouscriptionPrestationId == souscriptionId 
                                   && c.TypeCollecte == TypeCollecte.Souscription 
                                   && c.Statut, ct);

                return dejaPayee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification si la souscription {SouscriptionId} est déjà payée", souscriptionId);
                return false;
            }
        }

        private async Task<decimal> ResolveCommissionRateAsync(Collecte collecte, CancellationToken ct)
        {
            if (collecte.TypeCollecte == TypeCollecte.Frais && collecte.FraisId.HasValue)
            {
                var frais = await _db.Frais.AsNoTracking()
                    .FirstOrDefaultAsync(f => f.IdFrais == collecte.FraisId.Value, ct);
                if (frais != null) return frais.TauxCommission;
            }

            if (collecte.TypeCollecte == TypeCollecte.Souscription && collecte.SouscriptionPrestationId.HasValue)
            {
                var souscription = await _db.SouscriptionsPrestations
                    .AsNoTracking()
                    .Include(sp => sp.Prestation)
                        .ThenInclude(p => p.ProduitMutuel)
                    .Include(sp => sp.Prestation)
                        .ThenInclude(p => p.ProduitAssureur)
                    .FirstOrDefaultAsync(sp => sp.IdSouscriptionPrestation == collecte.SouscriptionPrestationId.Value, ct);

                if (souscription?.Prestation?.ProduitMutuel != null)
                    return souscription.Prestation.ProduitMutuel.EstGratuit
                        ? 0m
                        : souscription.Prestation.ProduitMutuel.TauxCommissionAT;
                if (souscription?.Prestation?.ProduitAssureur != null)
                    return souscription.Prestation.ProduitAssureur.EstGratuit
                        ? 0m
                        : souscription.Prestation.ProduitAssureur.TauxCommissionAT;
            }

            return 25m;
        }
    }
}
