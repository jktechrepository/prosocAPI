using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace ProsocAPI.Services
{
    public class JetonMedicalService : IJetonMedicalRepository
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<JetonMedicalService> _logger;
        private const int JETON_EXPIRY_DAYS = 30; // 30 jours de validité

        public JetonMedicalService(ProsocDbContext db, ILogger<JetonMedicalService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<JetonMedical>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.JetonsMedicaux
                .Include(j => j.Affilie)
                .Include(j => j.HopitalPartenaire)
                .AsNoTracking()
                .OrderByDescending(j => j.DateCreation)
                .ToListAsync(ct);
        }

        public async Task<JetonMedical?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.JetonsMedicaux
                .Include(j => j.Affilie)
                .Include(j => j.HopitalPartenaire)
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.IdJeton == id, ct);
        }

        public async Task<JetonMedical?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            return await _db.JetonsMedicaux
                .Include(j => j.Affilie)
                .Include(j => j.HopitalPartenaire)
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.CodeJeton == code, ct);
        }

        public async Task<List<JetonMedical>> GetByAffilieAsync(int affilieId, CancellationToken ct = default)
        {
            return await _db.JetonsMedicaux
                .Include(j => j.Affilie)
                .Include(j => j.HopitalPartenaire)
                .Where(j => j.AffilieId == affilieId)
                .AsNoTracking()
                .OrderByDescending(j => j.DateCreation)
                .ToListAsync(ct);
        }

        public async Task<List<JetonMedical>> GetByHopitalAsync(int hopitalId, CancellationToken ct = default)
        {
            return await _db.JetonsMedicaux
                .Include(j => j.Affilie)
                .Include(j => j.HopitalPartenaire)
                .Where(j => j.HopitalPartenaireId == hopitalId)
                .AsNoTracking()
                .OrderByDescending(j => j.DateCreation)
                .ToListAsync(ct);
        }

        public async Task<List<JetonMedical>> GetValidesAsync(CancellationToken ct = default)
        {
            var maintenant = DateTime.Now;
            return await _db.JetonsMedicaux
                .Include(j => j.Affilie)
                .Include(j => j.HopitalPartenaire)
                .Where(j => j.EstValide && !j.EstUtilise && 
                           (!j.DateExpiration.HasValue || j.DateExpiration.Value > maintenant))
                .AsNoTracking()
                .OrderBy(j => j.DateExpiration)
                .ToListAsync(ct);
        }

        public async Task<List<JetonMedical>> GetExpiresAsync(CancellationToken ct = default)
        {
            var maintenant = DateTime.Now;
            return await _db.JetonsMedicaux
                .Include(j => j.Affilie)
                .Where(j => j.EstValide && !j.EstUtilise && 
                           j.DateExpiration.HasValue && j.DateExpiration.Value <= maintenant)
                .AsNoTracking()
                .OrderBy(j => j.DateExpiration)
                .ToListAsync(ct);
        }

        public async Task<List<JetonMedical>> GetUtilisesAsync(CancellationToken ct = default)
        {
            return await _db.JetonsMedicaux
                .Include(j => j.Affilie)
                .Include(j => j.HopitalPartenaire)
                .Where(j => j.EstUtilise)
                .AsNoTracking()
                .OrderByDescending(j => j.DateUtilisation)
                .ToListAsync(ct);
        }

        public async Task<JetonMedical> CreateAsync(JetonMedical entity, CancellationToken ct = default)
        {
            // Générer un code unique
            entity.CodeJeton = await GenerateUniqueCodeAsync(ct);
            
            // Définir la date d'expiration
            if (!entity.DateExpiration.HasValue)
                entity.DateExpiration = DateTime.Now.AddDays(JETON_EXPIRY_DAYS);

            _db.JetonsMedicaux.Add(entity);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Jeton médical créé: {CodeJeton} pour l'affilié {AffilieId}", 
                entity.CodeJeton, entity.AffilieId);

            return entity;
        }

        public async Task<JetonMedical?> UpdateAsync(int id, JetonMedical entity, CancellationToken ct = default)
        {
            var existing = await _db.JetonsMedicaux.FirstOrDefaultAsync(j => j.IdJeton == id, ct);
            if (existing == null)
                return null;

            existing.DateUtilisation = entity.DateUtilisation;
            existing.EstUtilise = entity.EstUtilise;
            existing.EstValide = entity.EstValide;
            existing.Observation = entity.Observation;
            existing.DateModification = DateTime.Now;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.JetonsMedicaux.FirstOrDefaultAsync(j => j.IdJeton == id, ct);
            if (existing == null)
                return false;

            _db.JetonsMedicaux.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> ValiderJetonAsync(string code, int hopitalId, CancellationToken ct = default)
        {
            var jeton = await GetByCodeAsync(code, ct);
            if (jeton == null)
            {
                _logger.LogWarning("Tentative de validation avec un jeton inexistant: {Code}", code);
                return false;
            }

            // Vérifier si le jeton est valide
            var maintenant = DateTime.Now;
            if (!jeton.EstValide || jeton.EstUtilise || 
                (jeton.DateExpiration.HasValue && jeton.DateExpiration.Value <= maintenant))
            {
                _logger.LogWarning("Tentative de validation avec un jeton invalide/expiré: {Code}", code);
                return false;
            }

            // Vérifier si l'hôpital est autorisé
            if (jeton.HopitalPartenaireId.HasValue && jeton.HopitalPartenaireId != hopitalId)
            {
                _logger.LogWarning("Tentative de validation dans un hôpital non autorisé: {Code}, Hopital: {HopitalId}", 
                    code, hopitalId);
                return false;
            }

            _logger.LogInformation("Jeton validé avec succès: {Code} par l'hôpital {HopitalId}", code, hopitalId);
            return true;
        }

        public async Task<bool> UtiliserJetonAsync(int id, string observation, CancellationToken ct = default)
        {
            var jeton = await GetByIdAsync(id, ct);
            if (jeton == null)
                return false;

            // Marquer comme utilisé
            jeton.EstUtilise = true;
            jeton.DateUtilisation = DateTime.Now;
            jeton.Observation = observation;
            jeton.DateModification = DateTime.Now;

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Jeton utilisé: {CodeJeton} à {DateUtilisation}", 
                jeton.CodeJeton, jeton.DateUtilisation);

            return true;
        }

        public async Task<JetonMedicalStatsDto> GetStatsAsync(DateTime date, CancellationToken ct = default)
        {
            var debutMois = new DateTime(date.Year, date.Month, 1);
            var finMois = debutMois.AddMonths(1).AddDays(-1);

            var totalEmis = await _db.JetonsMedicaux
                .Where(j => j.DateEmission >= debutMois && j.DateEmission <= finMois)
                .CountAsync(ct);

            var totalUtilises = await _db.JetonsMedicaux
                .Where(j => j.DateUtilisation.HasValue && 
                           j.DateUtilisation.Value >= debutMois && 
                           j.DateUtilisation.Value <= finMois)
                .CountAsync(ct);

            var totalValides = await _db.JetonsMedicaux
                .Where(j => j.EstValide && !j.EstUtilise && 
                           (!j.DateExpiration.HasValue || j.DateExpiration.Value > DateTime.Now))
                .CountAsync(ct);

            var totalExpires = await GetExpiresAsync(ct);
            var tauxUtilisation = totalEmis > 0 ? (decimal)totalUtilises / totalEmis * 100 : 0;

            return new JetonMedicalStatsDto
            {
                TotalEmis = totalEmis,
                TotalUtilises = totalUtilises,
                TotalValides = totalValides,
                TotalExpires = totalExpires.Count,
                TauxUtilisation = Math.Round(tauxUtilisation, 2),
                DateStats = date
            };
        }

        public async Task<List<JetonMedical>> GetJetonsAArchiverAsync(CancellationToken ct = default)
        {
            var ilYADeuxMois = DateTime.Now.AddMonths(-2);
            return await _db.JetonsMedicaux
                .Where(j => j.EstUtilise && j.DateUtilisation.HasValue && 
                           j.DateUtilisation.Value <= ilYADeuxMois)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<bool> ArchiverJetonsExpiresAsync(CancellationToken ct = default)
        {
            var jetonsExpires = await GetExpiresAsync(ct);
            if (!jetonsExpires.Any())
                return true;

            foreach (var jeton in jetonsExpires)
            {
                jeton.EstValide = false;
                jeton.DateModification = DateTime.Now;
            }

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("{Count} jetons expirés archivés", jetonsExpires.Count);
            return true;
        }

        private async Task<string> GenerateUniqueCodeAsync(CancellationToken ct = default)
        {
            const int maxAttempts = 10;
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var code = GenerateCode();
                var exists = await _db.JetonsMedicaux
                    .AnyAsync(j => j.CodeJeton == code, ct);
                
                if (!exists)
                    return code;
            }

            throw new InvalidOperationException("Impossible de générer un code unique après plusieurs tentatives");
        }

        private static string GenerateCode()
        {
            // Format: JET + 8 caractères alphanumériques
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new StringBuilder();
            
            result.Append("JET");
            for (int i = 0; i < 8; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }
            
            return result.ToString();
        }
    }
}
