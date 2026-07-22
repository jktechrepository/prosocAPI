using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class ProduitAssureurService : IProduitAssureurRepository
    {
        private readonly ProsocDbContext _db;

        public ProduitAssureurService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<ProduitAssureur>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.ProduitsAssureurs
                .Include(p => p.Partenaire)
                .Include(p => p.Devise)
                .AsNoTracking()
                .OrderBy(x => x.Nom)
                .ToListAsync(ct);
        }

        public async Task<ProduitAssureur?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.ProduitsAssureurs
                .Include(p => p.Partenaire)
                .Include(p => p.Devise)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdProduit == id, ct);
        }

        public async Task<List<ProduitAssureur>> GetByAssureurAsync(int assureurId, CancellationToken ct = default)
        {
            return await _db.ProduitsAssureurs
                .Include(p => p.Partenaire)
                .Include(p => p.Devise)
                .AsNoTracking()
                .Where(x => x.AssureurId == assureurId)
                .OrderBy(x => x.Nom)
                .ToListAsync(ct);
        }

        public async Task<List<ProduitAssureur>> GetActivesAsync(CancellationToken ct = default)
        {
            return await _db.ProduitsAssureurs
                .Include(p => p.Partenaire)
                .Include(p => p.Devise)
                .AsNoTracking()
                .Where(x => x.Statut)
                .OrderBy(x => x.Nom)
                .ToListAsync(ct);
        }

        public async Task<ProduitAssureur> CreateAsync(ProduitAssureur entity, CancellationToken ct = default)
        {
            await ProduitTarifRules.ValidateReferencesAsync(_db, entity.DeviseId, entity.AssureurId, ct);
            ProduitTarifRules.PrepareForSave(entity);
            _db.ProduitsAssureurs.Add(entity);
            await _db.SaveChangesAsync(ct);

            await ProduitPrestationSync.EnsureAndSyncAssureurAsync(_db, entity, ct);

            return (await GetByIdAsync(entity.IdProduit, ct))!;
        }

        public async Task<ProduitAssureur?> UpdateAsync(int id, ProduitAssureur entity, CancellationToken ct = default)
        {
            var existing = await _db.ProduitsAssureurs.FirstOrDefaultAsync(x => x.IdProduit == id, ct);
            if (existing == null)
                return null;

            await ProduitTarifRules.ValidateReferencesAsync(_db, entity.DeviseId, entity.AssureurId, ct);
            ProduitTarifRules.PrepareForSave(entity);
            ProduitTarifRules.CopyTarifFields(existing, entity);
            existing.AssureurId = entity.AssureurId;
            existing.DeviseId = entity.DeviseId;

            await _db.SaveChangesAsync(ct);
            await ProduitPrestationSync.EnsureAndSyncAssureurAsync(_db, existing, ct);

            return await GetByIdAsync(id, ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.ProduitsAssureurs.FirstOrDefaultAsync(x => x.IdProduit == id, ct);
            if (existing == null)
                return false;

            await ProduitPrestationSync.ValidateDeleteAssureurAsync(_db, id, ct);
            await ProduitPrestationSync.RemoveLinkedPrestationsAssureurAsync(_db, id, ct);
            _db.ProduitsAssureurs.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
