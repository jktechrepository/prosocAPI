using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class ProduitMutuelService : IProduitMutuelRepository
    {
        private readonly ProsocDbContext _db;

        public ProduitMutuelService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<ProduitMutuel>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.ProduitsMutuels
                .AsNoTracking()
                .Include(pm => pm.Devise)
                .OrderBy(x => x.Nom)
                .ToListAsync(ct);
        }

        public async Task<ProduitMutuel?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.ProduitsMutuels
                .AsNoTracking()
                .Include(pm => pm.Devise)
                .FirstOrDefaultAsync(x => x.IdProduit == id, ct);
        }

        public async Task<List<ProduitMutuel>> GetActivesAsync(CancellationToken ct = default)
        {
            return await _db.ProduitsMutuels
                .AsNoTracking()
                .Include(pm => pm.Devise)
                .Where(x => x.Statut)
                .OrderBy(x => x.Nom)
                .ToListAsync(ct);
        }

        public async Task<ProduitMutuel> CreateAsync(ProduitMutuel entity, CancellationToken ct = default)
        {
            await ProduitTarifRules.ValidateReferencesAsync(_db, entity.DeviseId, ct: ct);
            ProduitTarifRules.PrepareForSave(entity);
            _db.ProduitsMutuels.Add(entity);
            await _db.SaveChangesAsync(ct);

            await ProduitPrestationSync.EnsureAndSyncMutuelAsync(_db, entity, ct);

            return (await GetByIdAsync(entity.IdProduit, ct))!;
        }

        public async Task<ProduitMutuel?> UpdateAsync(int id, ProduitMutuel entity, CancellationToken ct = default)
        {
            var existing = await _db.ProduitsMutuels.FirstOrDefaultAsync(x => x.IdProduit == id, ct);
            if (existing == null)
                return null;

            await ProduitTarifRules.ValidateReferencesAsync(_db, entity.DeviseId, ct: ct);
            ProduitTarifRules.PrepareForSave(entity);
            ProduitTarifRules.CopyTarifFields(existing, entity);
            existing.DeviseId = entity.DeviseId;

            await _db.SaveChangesAsync(ct);
            await ProduitPrestationSync.EnsureAndSyncMutuelAsync(_db, existing, ct);

            return await GetByIdAsync(id, ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.ProduitsMutuels.FirstOrDefaultAsync(x => x.IdProduit == id, ct);
            if (existing == null)
                return false;

            await ProduitPrestationSync.ValidateDeleteMutuelAsync(_db, id, ct);
            await ProduitPrestationSync.RemoveLinkedPrestationsMutuelAsync(_db, id, ct);
            _db.ProduitsMutuels.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
