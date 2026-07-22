using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services
{
    public interface ISouscriptionPrestationRepository
    {
        Task<List<SouscriptionPrestation>> GetAllAsync(CancellationToken ct = default);
        Task<SouscriptionPrestation?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<SouscriptionPrestation>> GetByAffilieAsync(int affilieId, CancellationToken ct = default);
        Task<List<SouscriptionPrestation>> GetByAffilieActivesAsync(int affilieId, CancellationToken ct = default);
        Task<List<SouscriptionPrestation>> GetByPrestationAsync(int prestationId, CancellationToken ct = default);
        Task<SouscriptionPrestation> CreateAsync(SouscriptionPrestation entity, CancellationToken ct = default);
        Task<SouscriptionPrestation?> UpdateAsync(int id, SouscriptionPrestation entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<SouscriptionPrestationStatsDto> GetStatsAsync(CancellationToken ct = default);
    }

    public class SouscriptionPrestationService : ISouscriptionPrestationRepository
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<SouscriptionPrestationService> _logger;

        public SouscriptionPrestationService(ProsocDbContext db, ILogger<SouscriptionPrestationService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<SouscriptionPrestation>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.SouscriptionsPrestations
                .Include(s => s.Affilie)
                .Include(s => s.Prestation)
                .AsNoTracking()
                .OrderByDescending(s => s.DateCreation)
                .ToListAsync(ct);
        }

        public async Task<SouscriptionPrestation?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.SouscriptionsPrestations
                .Include(s => s.Affilie)
                .Include(s => s.Prestation)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IdSouscriptionPrestation == id, ct);
        }

        public async Task<List<SouscriptionPrestation>> GetByAffilieAsync(int affilieId, CancellationToken ct = default)
        {
            return await _db.SouscriptionsPrestations
                .Include(s => s.Affilie)
                .Include(s => s.Prestation)
                .AsNoTracking()
                .Where(s => s.AffilieId == affilieId)
                .OrderByDescending(s => s.DateCreation)
                .ToListAsync(ct);
        }

        public async Task<List<SouscriptionPrestation>> GetByAffilieActivesAsync(int affilieId, CancellationToken ct = default)
        {
            return await _db.SouscriptionsPrestations
                .Include(s => s.Affilie)
                .Include(s => s.Prestation)
                .AsNoTracking()
                .Where(s => s.AffilieId == affilieId && s.Statut == true)
                .OrderByDescending(s => s.DateCreation)
                .ToListAsync(ct);
        }

        public async Task<List<SouscriptionPrestation>> GetByPrestationAsync(int prestationId, CancellationToken ct = default)
        {
            return await _db.SouscriptionsPrestations
                .Include(s => s.Affilie)
                .Include(s => s.Prestation)
                .AsNoTracking()
                .Where(s => s.PrestationId == prestationId)
                .OrderByDescending(s => s.DateCreation)
                .ToListAsync(ct);
        }

        public async Task<SouscriptionPrestation> CreateAsync(SouscriptionPrestation entity, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Création de la souscription pour l'affilié {AffilieId} - Prestation {PrestationId}", 
                    entity.AffilieId, entity.PrestationId);

                // Vérifier si la souscription existe déjà (active)
                var existing = await _db.SouscriptionsPrestations
                    .FirstOrDefaultAsync(s => s.AffilieId == entity.AffilieId && 
                                           s.PrestationId == entity.PrestationId && 
                                           s.Statut == true, ct);

                if (existing != null)
                {
                    throw new InvalidOperationException($"L'affilié {entity.AffilieId} a déjà une souscription active pour la prestation {entity.PrestationId}");
                }

                await ProduitEligibiliteRules.ValidateAchatProduitAsync(
                    _db, entity.AffilieId, entity.PrestationId, ct);

                entity.DateCreation = DateTime.Now;
                entity.DateModification = DateTime.Now;

                _db.SouscriptionsPrestations.Add(entity);
                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Souscription créée avec succès: Id {Id}", entity.IdSouscriptionPrestation);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la souscription");
                throw;
            }
        }

        public async Task<SouscriptionPrestation?> UpdateAsync(int id, SouscriptionPrestation entity, CancellationToken ct = default)
        {
            try
            {
                var existing = await _db.SouscriptionsPrestations.FirstOrDefaultAsync(s => s.IdSouscriptionPrestation == id, ct);
                if (existing == null)
                    return null;

                existing.AffilieId = entity.AffilieId;
                existing.PrestationId = entity.PrestationId;
                existing.Statut = entity.Statut;
                existing.DateModification = DateTime.Now;

                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Souscription {Id} mise à jour avec succès", id);
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la souscription {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var existing = await _db.SouscriptionsPrestations.FirstOrDefaultAsync(s => s.IdSouscriptionPrestation == id, ct);
                if (existing == null)
                    return false;

                // Soft delete
                existing.Statut = false;
                existing.DateModification = DateTime.Now;

                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Souscription {Id} désactivée (soft delete)", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la souscription {Id}", id);
                throw;
            }
        }

        public async Task<SouscriptionPrestationStatsDto> GetStatsAsync(CancellationToken ct = default)
        {
            var souscriptions = await _db.SouscriptionsPrestations
                .Include(s => s.Prestation)
                .Include(s => s.Affilie)
                .AsNoTracking()
                .ToListAsync(ct);

            var stats = new SouscriptionPrestationStatsDto
            {
                NombreTotalSouscriptions = souscriptions.Count,
                NombreSouscriptionsActives = souscriptions.Count(s => s.Statut)
            };

            // Souscriptions par prestation
            stats.SouscriptionsParPrestation = souscriptions
                .GroupBy(s => s.Prestation?.NomPrestation ?? "Inconnue")
                .ToDictionary(g => g.Key, g => g.Count());

            // Souscriptions par affilié
            stats.SouscriptionsParAffilie = souscriptions
                .GroupBy(s => $"{s.Affilie?.Nom} {s.Affilie?.Prenom}".Trim())
                .ToDictionary(g => g.Key, g => g.Count());

            return stats;
        }
    }
}
