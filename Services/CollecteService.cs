using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class CollecteService : ICollecteRepository
    {
        private readonly ProsocDbContext _db;
        private readonly ICommissionService _commissionService;
        private readonly ICotisationAffilieMetierService _cotisationMetier;
        private readonly ICollecteMultideviseService _multidevise;
        private readonly ICaisseService _caisseService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CollecteService> _logger;

        public CollecteService(
            ProsocDbContext db,
            ICommissionService commissionService,
            ICotisationAffilieMetierService cotisationMetier,
            ICollecteMultideviseService multidevise,
            ICaisseService caisseService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CollecteService> logger)
        {
            _db = db;
            _commissionService = commissionService;
            _cotisationMetier = cotisationMetier;
            _multidevise = multidevise;
            _caisseService = caisseService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<Collecte>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Collectes
                .Include(c => c.Affilie)
                .Include(c => c.Agent)
                .Include(c => c.Devise)
                .Include(c => c.Frais) // NOUVEAU
                .Include(c => c.CotisationAffilie)
                .Include(c => c.SouscriptionPrestationRef) // NOUVEAU
                .AsNoTracking()
                .OrderByDescending(x => x.DateCollecte)
                .ToListAsync(ct);
        }

        public async Task<Collecte?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Collectes
                .Include(c => c.Affilie)
                .Include(c => c.Agent)
                .Include(c => c.Devise)
                .Include(c => c.Frais) // NOUVEAU
                .Include(c => c.CotisationAffilie)
                .Include(c => c.SouscriptionPrestationRef) // NOUVEAU
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdCollecte == id, ct);
        }

        public async Task<List<Collecte>> GetByAffilieAsync(int affilieId, CancellationToken ct = default)
        {
            return await _db.Collectes
                .Include(c => c.Affilie)
                .Include(c => c.Agent)
                .Include(c => c.Devise)
                .Include(c => c.Frais) // NOUVEAU
                .Include(c => c.CotisationAffilie)
                .Include(c => c.SouscriptionPrestationRef) // NOUVEAU
                .AsNoTracking()
                .Where(x => x.AffilieId == affilieId)
                .OrderByDescending(x => x.DateCollecte)
                .ToListAsync(ct);
        }

        public async Task<List<Collecte>> GetByAgentAsync(int agentId, CancellationToken ct = default)
        {
            return await _db.Collectes
                .Include(c => c.Affilie)
                .Include(c => c.Agent)
                .Include(c => c.Devise)
                .Include(c => c.Frais) // NOUVEAU
                .Include(c => c.CotisationAffilie)
                .Include(c => c.SouscriptionPrestationRef) // NOUVEAU
                .AsNoTracking()
                .Where(x => x.AgentId == agentId)
                .OrderByDescending(x => x.DateCollecte)
                .ToListAsync(ct);
        }

        public async Task<List<Collecte>> GetByDeviseAsync(int deviseId, CancellationToken ct = default)
        {
            return await _db.Collectes
                .Include(c => c.Affilie)
                .Include(c => c.Agent)
                .Include(c => c.Devise)
                .Include(c => c.Frais) // NOUVEAU
                .Include(c => c.CotisationAffilie)
                .Include(c => c.SouscriptionPrestationRef) // NOUVEAU
                .AsNoTracking()
                .Where(x => x.DeviseId == deviseId)
                .OrderByDescending(x => x.DateCollecte)
                .ToListAsync(ct);
        }

        public async Task<List<Collecte>> GetByDateRangeAsync(DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            return await _db.Collectes
                .Include(c => c.Affilie)
                .Include(c => c.Agent)
                .Include(c => c.Devise)
                .Include(c => c.Frais) // NOUVEAU
                .Include(c => c.CotisationAffilie)
                .Include(c => c.SouscriptionPrestationRef) // NOUVEAU
                .AsNoTracking()
                .Where(x => x.DateCollecte >= debut && x.DateCollecte <= fin)
                .OrderByDescending(x => x.DateCollecte)
                .ToListAsync(ct);
        }

        public async Task<Collecte> CreateAsync(Collecte entity, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("=== DÉBUT CRÉATION COLLECTE ===");
                _logger.LogInformation("Création de la collecte pour l'affilié {AffilieId} par l'agent {AgentId}", entity.AffilieId, entity.AgentId);
                _logger.LogInformation("Montant: {Montant}, ModePaiement: {ModePaiement}, TypeCollecte: {TypeCollecte}", entity.Montant, entity.ModePaiement, entity.TypeCollecte);

                WalletVirtuelPaiementAutorisation.EnsureSiVirtualAccount(
                    entity.ModePaiement,
                    _httpContextAccessor.HttpContext?.User);

                // NOUVEAU : Validation du type de collecte
                if (!entity.IsValid())
                {
                    // ✅ AMÉLIORATION : Gestion du cas TypeCollecte = 0
                    if (entity.TypeCollecte == 0)
                    {
                        throw new ArgumentException($"Type de collecte non spécifié. Valeurs valides: {string.Join(", ", Enum.GetValues<TypeCollecte>())}");
                    }
                    else if (entity.TypeCollecte == TypeCollecte.Frais)
                    {
                        if (entity.FraisId == null)
                        {
                            throw new ArgumentException($"Type de collecte invalide. Pour TypeCollecte={entity.TypeCollecte}, FraisId est requis");
                        }
                    }
                    else if (entity.TypeCollecte == TypeCollecte.Souscription)
                    {
                        if (entity.SouscriptionPrestationId == null)
                        {
                            throw new ArgumentException($"Type de collecte invalide. Pour TypeCollecte={entity.TypeCollecte}, SouscriptionPrestationId est requis");
                        }
                    }
                    else if (entity.TypeCollecte == TypeCollecte.Cotisation)
                    {
                        if (entity.CotisationAffilieId == null)
                        {
                            throw new ArgumentException($"Type de collecte invalide. Pour TypeCollecte={entity.TypeCollecte}, CotisationAffilieId est requis");
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"Type de collecte invalide. Pour TypeCollecte={entity.TypeCollecte}, TypeCollecte est requis");
                    }
                }

                if (entity.TypeCollecte == TypeCollecte.Souscription && entity.SouscriptionPrestationId.HasValue)
                {
                    await ProduitEligibiliteRules.ValidateAchatProduitBySouscriptionAsync(
                        _db, entity.AffilieId, entity.SouscriptionPrestationId.Value, ct);
                }

                var nombreDependants = await _db.Dependants
                    .CountAsync(d => d.AffilieId == entity.AffilieId && d.Statut, ct);

                if (entity.TypeCollecte == TypeCollecte.Cotisation && entity.CotisationAffilieId.HasValue)
                {
                    var adhesion = await _db.Adhesions
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => a.AffilieId == entity.AffilieId, ct);

                    if (adhesion == null)
                    {
                        throw new ArgumentException(
                            $"Aucune adhésion trouvée pour l'affilié {entity.AffilieId}. Impossible de valider la cotisation.");
                    }

                    await _cotisationMetier.ValidateCollecteCotisationStructureAsync(
                        entity.CotisationAffilieId.Value,
                        adhesion.TypeAdhesionId,
                        nombreDependants,
                        ct);
                }

                await _multidevise.ValidateAndApplySnapshotAsync(entity, nombreDependants, ct);

                // Validation du mode de paiement
                var modesValidés = Enum.GetValues<ModePaiement>().Select(m => m.ToString()).ToList();
                var modePaiementStr = entity.ModePaiement?.ToUpperInvariant();
                
                if (string.IsNullOrWhiteSpace(modePaiementStr) || !modesValidés.Contains(modePaiementStr))
                {
                    throw new ArgumentException($"Mode de paiement '{entity.ModePaiement}' non valide. Modes acceptés: {string.Join(", ", modesValidés)}");
                }

                if (MethodePaiementHelper.IsVirtualAccount(entity.ModePaiement)
                    && CollecteStatutPaiementRegles.EstValide(entity.StatutPaiement))
                {
                    entity.StatutPerception = CollecteStatutPerception.NonPerçu;
                }

                // Ajouter la collecte
                _db.Collectes.Add(entity);
                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Collecte {CollecteId} créée avec succès (Type: {TypeCollecte})", entity.IdCollecte, entity.TypeCollecte);

                // Traiter la commission et les wallets
                _logger.LogInformation("Appel du CommissionService...");
                await _commissionService.ProcessCommissionAsync(entity, ct);
                _logger.LogInformation("CommissionService terminé");

                await _caisseService.TryEnregistrerEntreeCollecteGuichetAsync(entity, ct);

                _logger.LogInformation("=== FIN CRÉATION COLLECTE ===");
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la collecte");
                throw;
            }
        }

        public async Task<Collecte?> UpdateAsync(int id, Collecte entity, CancellationToken ct = default)
        {
            var existing = await _db.Collectes.FirstOrDefaultAsync(x => x.IdCollecte == id, ct);
            if (existing == null)
                return null;

            // NOUVEAU : Validation du type de collecte
            if (!entity.IsValid())
            {
                var detail = entity.TypeCollecte switch
                {
                    TypeCollecte.Frais => "FraisId est requis",
                    TypeCollecte.Souscription => "SouscriptionPrestationId est requis",
                    TypeCollecte.Cotisation => "CotisationAffilieId est requis",
                    _ => "référence invalide"
                };
                throw new ArgumentException($"Type de collecte invalide. Pour TypeCollecte={entity.TypeCollecte}, {detail}");
            }

            existing.TypeCollecte = entity.TypeCollecte; // NOUVEAU
            existing.FraisId = entity.FraisId; // NOUVEAU
            existing.CotisationAffilieId = entity.CotisationAffilieId;
            existing.AffilieId = entity.AffilieId;
            existing.AgentId = entity.AgentId;
            existing.Montant = entity.Montant;
            existing.ReferencePaiement = entity.ReferencePaiement;
            existing.ModePaiement = entity.ModePaiement;
            existing.Operateur = entity.Operateur;
            existing.StatutPaiement = entity.StatutPaiement;
            existing.SouscriptionPrestationId = entity.SouscriptionPrestationId;
            existing.MontantRecu = entity.MontantRecu;
            existing.MontantAttendu = entity.MontantAttendu;
            existing.DeviseId = entity.DeviseId;
            existing.Observation = entity.Observation;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.Collectes.FirstOrDefaultAsync(x => x.IdCollecte == id, ct);
            if (existing == null)
                return false;

            _db.Collectes.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<CollecteStatsDto> GetStatsAsync(CancellationToken ct = default)
        {
            var collectes = await _db.Collectes
                .Include(c => c.Devise)
                .Include(c => c.Agent)
                .Include(c => c.Frais) // NOUVEAU
                .Include(c => c.SouscriptionPrestationRef) // NOUVEAU
                .AsNoTracking()
                .ToListAsync(ct);

            var stats = new CollecteStatsDto
            {
                NombreCollectes = collectes.Count,
                TotalMontant = collectes.Sum(x => x.Montant),
                TotalMontantDevisePrincipale = collectes
                    .Where(x => x.MontantDevisePrincipale.HasValue)
                    .Sum(x => x.MontantDevisePrincipale!.Value),
                MontantMoyen = collectes.Count > 0 ? collectes.Average(x => x.Montant) : 0
            };

            // Montants par devise
            stats.MontantsParDevise = collectes
                .GroupBy(x => x.Devise?.Code ?? "Inconnue")
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Montant));

            // Collectes par agent
            stats.CollectesParAgent = collectes
                .GroupBy(x => x.Agent?.NomComplet ?? "Agent Inconnu")
                .ToDictionary(g => g.Key, g => g.Count());

            // NOUVEAU : Statistiques par type de collecte
            stats.MontantsParType = collectes
                .GroupBy(x => x.TypeCollecte)
                .ToDictionary(g => g.Key, g => g.Sum(x => (decimal)x.Montant));

            stats.NombreParType = collectes
                .GroupBy(x => x.TypeCollecte)
                .ToDictionary(g => g.Key, g => g.Count());

            return stats;
        }
        
        // NOUVEAU : Méthodes pour filtrer par type de collecte
        public async Task<List<Collecte>> GetByTypeAsync(TypeCollecte typeCollecte, CancellationToken ct = default)
        {
            return await _db.Collectes
                .Include(c => c.Affilie)
                .Include(c => c.Agent)
                .Include(c => c.Devise)
                .Include(c => c.Frais)
                .Include(c => c.SouscriptionPrestationRef)
                .AsNoTracking()
                .Where(x => x.TypeCollecte == typeCollecte)
                .OrderByDescending(x => x.DateCollecte)
                .ToListAsync(ct);
        }
        
        // NOUVEAU : Méthodes pour les collectes de frais
        public async Task<List<Collecte>> GetByFraisAsync(int fraisId, CancellationToken ct = default)
        {
            return await _db.Collectes
                .Include(c => c.Affilie)
                .Include(c => c.Agent)
                .Include(c => c.Devise)
                .Include(c => c.Frais)
                .AsNoTracking()
                .Where(x => x.FraisId == fraisId && x.TypeCollecte == TypeCollecte.Frais)
                .OrderByDescending(x => x.DateCollecte)
                .ToListAsync(ct);
        }
        
        // NOUVEAU : Méthodes pour les collectes de souscription
        public async Task<List<Collecte>> GetBySouscriptionAsync(int souscriptionPrestationId, CancellationToken ct = default)
        {
            return await _db.Collectes
                .Include(c => c.Affilie)
                .Include(c => c.Agent)
                .Include(c => c.Devise)
                .Include(c => c.SouscriptionPrestationRef)
                .AsNoTracking()
                .Where(x => x.SouscriptionPrestationId == souscriptionPrestationId && x.TypeCollecte == TypeCollecte.Souscription)
                .OrderByDescending(x => x.DateCollecte)
                .ToListAsync(ct);
        }
    }
}
