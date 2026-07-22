using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public class FraisService : IFraisService
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<FraisService> _logger;

        public FraisService(ProsocDbContext db, ILogger<FraisService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Frais> CreateAsync(Frais frais)
        {
            try
            {
                frais.Periodicite = ArrieresAffilieRules.NormalizePeriodiciteFrais(frais.Periodicite);
                if (!string.IsNullOrWhiteSpace(frais.Code))
                {
                    frais.Code = frais.Code.Trim().ToUpperInvariant();
                    if (await _db.Frais.AnyAsync(f => f.Code == frais.Code && !f.EstSupprime))
                        throw new InvalidOperationException($"Un frais avec le code « {frais.Code} » existe déjà.");
                }

                _db.Frais.Add(frais);
                await _db.SaveChangesAsync();
                
                _logger.LogInformation("Frais créé avec succès - ID: {Id}, Libelle: {Libelle}, Montant: {Montant}", 
                    frais.IdFrais, frais.Libelle, frais.Montant);
                
                return frais;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création des frais");
                throw;
            }
        }

        public async Task<Frais?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            var normalized = code.Trim().ToUpperInvariant();
            return await _db.Frais
                .Include(f => f.Devise)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Code == normalized && f.Statut && !f.EstSupprime, ct);
        }

        public async Task<Frais?> GetByIdAsync(int id)
        {
            try
            {
                var frais = await _db.Frais
                    .Include(f => f.Devise)
                    .FirstOrDefaultAsync(f => f.IdFrais == id && f.Statut && !f.EstSupprime);
                
                return frais;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des frais ID: {Id}", id);
                throw;
            }
        }

        public async Task<List<Frais>> GetAllAsync()
        {
            try
            {
                var fraisList = await _db.Frais
                  //  .Include(f => f.Devise)
                    .Where(f => f.Statut && !f.EstSupprime)
                    .OrderBy(f => f.DateCreation)
                    .ToListAsync();
                
                return fraisList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la liste des frais");
                throw;
            }
        }

        public async Task<List<Frais>> GetByDeviseAsync(int deviseId)
        {
            try
            {
                var fraisList = await _db.Frais
                    .Include(f => f.Devise)
                    .Where(f => f.DeviseId == deviseId && f.Statut && !f.EstSupprime)
                    .OrderBy(f => f.DateCreation)
                    .ToListAsync();
                
                return fraisList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des frais par devise ID: {DeviseId}", deviseId);
                throw;
            }
        }

        public async Task<Frais> UpdateAsync(Frais frais)
        {
            try
            {
                var existingFrais = await _db.Frais.FindAsync(frais.IdFrais);
                if (existingFrais == null)
                {
                    throw new InvalidOperationException($"Frais avec ID {frais.IdFrais} non trouvé");
                }

                existingFrais.Libelle = frais.Libelle;
                existingFrais.Montant = frais.Montant;
                existingFrais.DeviseId = frais.DeviseId;
                existingFrais.Periodicite = ArrieresAffilieRules.NormalizePeriodiciteFrais(frais.Periodicite);
                if (!string.IsNullOrWhiteSpace(frais.Code))
                {
                    var code = frais.Code.Trim().ToUpperInvariant();
                    if (await _db.Frais.AnyAsync(f =>
                            f.Code == code && f.IdFrais != frais.IdFrais && !f.EstSupprime))
                        throw new InvalidOperationException($"Un frais avec le code « {code} » existe déjà.");
                    existingFrais.Code = code;
                }
                existingFrais.DateModification = DateTime.Now;
                existingFrais.ModifieParId = frais.ModifieParId;

                await _db.SaveChangesAsync();
                
                _logger.LogInformation("Frais mis à jour avec succès - ID: {Id}", frais.IdFrais);
                return existingFrais;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour des frais ID: {Id}", frais.IdFrais);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var frais = await _db.Frais.FindAsync(id);
                if (frais == null)
                {
                    return false;
                }

                frais.EstSupprime = true;
                frais.DateSuppression = DateTime.Now;
                await _db.SaveChangesAsync();
                
                _logger.LogInformation("Frais supprimé avec succès - ID: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression des frais ID: {Id}", id);
                return false;
            }
        }

        public async Task<double> GetTotalByDeviseAsync(int deviseId)
        {
            try
            {
                var total = await _db.Frais
                    .Where(f => f.DeviseId == deviseId && f.Statut && !f.EstSupprime)
                    .SumAsync(f => f.Montant);
                
                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du calcul du total des frais par devise ID: {DeviseId}", deviseId);
                return 0;
            }
        }

        public async Task<List<Frais>> GetActiveByDeviseAsync(int deviseId)
        {
            try
            {
                var fraisList = await _db.Frais
                    .Include(f => f.Devise)
                    .Where(f => f.DeviseId == deviseId && f.Statut && !f.EstSupprime)
                    .OrderBy(f => f.Libelle)
                    .ToListAsync();
                
                return fraisList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des frais actifs par devise ID: {DeviseId}", deviseId);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _db.Frais
                    .AnyAsync(f => f.IdFrais == id && f.Statut && !f.EstSupprime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification d'existence des frais ID: {Id}", id);
                return false;
            }
        }
        
        // NOUVEAU : Méthodes pour les collectes associées aux frais
        
        public async Task<List<Collecte>> GetCollectesByFraisAsync(int fraisId)
        {
            try
            {
                var collectes = await _db.Collectes
                    .Include(c => c.Affilie)
                    .Include(c => c.Agent)
                    .Include(c => c.Devise)
                    .Where(c => c.FraisId == fraisId && c.TypeCollecte == TypeCollecte.Frais && c.Statut)
                    .OrderByDescending(c => c.DateCreation)
                    .ToListAsync();
                
                _logger.LogInformation("Récupération de {Count} collectes pour les frais ID: {FraisId}", collectes.Count, fraisId);
                
                return collectes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des collectes pour les frais ID: {FraisId}", fraisId);
                throw;
            }
        }
        
        public async Task<double> GetTotalCollectesByFraisAsync(int fraisId)
        {
            try
            {
                var total = await _db.Collectes
                    .Where(c => c.FraisId == fraisId && c.TypeCollecte == TypeCollecte.Frais && c.Statut)
                    .SumAsync(c => (double)c.Montant);
                
                _logger.LogInformation("Total des collectes pour les frais ID: {FraisId} = {Total}", fraisId, total);
                
                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du calcul du total des collectes pour les frais ID: {FraisId}", fraisId);
                return 0;
            }
        }
        
        public async Task<int> GetCountCollectesByFraisAsync(int fraisId)
        {
            try
            {
                var count = await _db.Collectes
                    .CountAsync(c => c.FraisId == fraisId && c.TypeCollecte == TypeCollecte.Frais && c.Statut);
                
                _logger.LogInformation("Nombre de collectes pour les frais ID: {FraisId} = {Count}", fraisId, count);
                
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du comptage des collectes pour les frais ID: {FraisId}", fraisId);
                return 0;
            }
        }
        
        public async Task<Dictionary<int, int>> GetCollectesStatsByFraisAsync()
        {
            try
            {
                var stats = await _db.Collectes
                    .Where(c => c.TypeCollecte == TypeCollecte.Frais && c.Statut)
                    .GroupBy(c => c.FraisId)
                    .Select(g => new { FraisId = g.Key ?? 0, Count = g.Count() })
                    .ToListAsync();
                
                var result = stats.ToDictionary(x => x.FraisId, x => x.Count);
                
                _logger.LogInformation("Statistiques des collectes par frais calculées pour {Count} types de frais", result.Count);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du calcul des statistiques des collectes par frais");
                throw;
            }
        }
        
        public async Task<Frais?> GetByIdWithCollectesAsync(int id)
        {
            try
            {
                var frais = await _db.Frais
                    .Include(f => f.Devise)
                    .Include(f => f.Collectes.Where(c => c.Statut))
                        .ThenInclude(c => c.Affilie)
                    .Include(f => f.Collectes.Where(c => c.Statut))
                        .ThenInclude(c => c.Agent)
                    .Include(f => f.Collectes.Where(c => c.Statut))
                        .ThenInclude(c => c.Devise)
                    .FirstOrDefaultAsync(f => f.IdFrais == id && f.Statut && !f.EstSupprime);
                
                if (frais != null)
                {
                    _logger.LogInformation("Frais récupéré avec {Count} collectes - ID: {Id}, Libelle: {Libelle}", 
                        frais.Collectes.Count, frais.IdFrais, frais.Libelle);
                }
                
                return frais;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des frais avec collectes ID: {Id}", id);
                throw;
            }
        }
    }
}
