using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.FlexPay;

namespace ProsocAPI.Services
{
    public interface IInfoPaiementMarchandService
    {
        Task<InfoPaiementMarchand?> GetActifAsync(CancellationToken ct = default);
        Task<InfoPaiementMarchand?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<InfoPaiementMarchand>> GetAllAsync(CancellationToken ct = default);
        Task<InfoPaiementMarchand> CreateAsync(InfoPaiementMarchand entity, CancellationToken ct = default);
        Task<InfoPaiementMarchand?> UpdateAsync(int id, InfoPaiementMarchand entity, CancellationToken ct = default);
    }

    public class InfoPaiementMarchandService : IInfoPaiementMarchandService
    {
        private readonly ProsocDbContext _db;

        public InfoPaiementMarchandService(ProsocDbContext db)
        {
            _db = db;
        }

        public Task<InfoPaiementMarchand?> GetActifAsync(CancellationToken ct = default) =>
            _db.InfoPaiementsMarchand.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Statut, ct);

        public Task<InfoPaiementMarchand?> GetByIdAsync(int id, CancellationToken ct = default) =>
            _db.InfoPaiementsMarchand.AsNoTracking().FirstOrDefaultAsync(x => x.IdInfoPaiementMarchand == id, ct);

        public Task<List<InfoPaiementMarchand>> GetAllAsync(CancellationToken ct = default) =>
            _db.InfoPaiementsMarchand.AsNoTracking().OrderByDescending(x => x.DateCreation).ToListAsync(ct);

        public async Task<InfoPaiementMarchand> CreateAsync(InfoPaiementMarchand entity, CancellationToken ct = default)
        {
            var actifs = await _db.InfoPaiementsMarchand.Where(x => x.Statut).ToListAsync(ct);
            foreach (var a in actifs)
                a.Statut = false;

            entity.Statut = true;
            entity.DateCreation = DateTime.UtcNow;
            _db.InfoPaiementsMarchand.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<InfoPaiementMarchand?> UpdateAsync(int id, InfoPaiementMarchand entity, CancellationToken ct = default)
        {
            var existing = await _db.InfoPaiementsMarchand.FirstOrDefaultAsync(x => x.IdInfoPaiementMarchand == id, ct);
            if (existing == null)
                return null;

            if (entity.Statut && !existing.Statut)
            {
                var autres = await _db.InfoPaiementsMarchand.Where(x => x.Statut && x.IdInfoPaiementMarchand != id).ToListAsync(ct);
                foreach (var a in autres)
                    a.Statut = false;
            }

            existing.CodeMarchand = entity.CodeMarchand;
            if (!string.IsNullOrWhiteSpace(entity.ApiToken))
                existing.ApiToken = entity.ApiToken;
            existing.ActifMobileMoney = entity.ActifMobileMoney;
            existing.ActifCarteBancaire = entity.ActifCarteBancaire;
            existing.Statut = entity.Statut;
            existing.DateModification = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return existing;
        }
    }
}
