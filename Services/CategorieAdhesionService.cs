using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class CategorieAdhesionService : ICategorieAdhesionRepository
    {
        private readonly ProsocDbContext _db;

        public CategorieAdhesionService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<CategorieAdhesion>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.CategoriesAdhesions
                .AsNoTracking()
                .OrderBy(x => x.Libelle)
                .ToListAsync(ct);
        }

        public async Task<CategorieAdhesion?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.CategoriesAdhesions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdCategorieAdhesion == id, ct);
        }

        public async Task<List<CategorieAdhesion>> GetActivesAsync(CancellationToken ct = default)
        {
            return await _db.CategoriesAdhesions
                .AsNoTracking()
                .Where(x => x.Statut)
                .OrderBy(x => x.Libelle)
                .ToListAsync(ct);
        }

        public async Task<CategorieAdhesion> CreateAsync(CategorieAdhesion entity, CancellationToken ct = default)
        {
            _db.CategoriesAdhesions.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<CategorieAdhesion?> UpdateAsync(int id, CategorieAdhesion entity, CancellationToken ct = default)
        {
            var existing = await _db.CategoriesAdhesions.FirstOrDefaultAsync(x => x.IdCategorieAdhesion == id, ct);
            if (existing == null)
                return null;

            existing.Libelle = entity.Libelle;
            existing.Description = entity.Description;
            existing.Statut = entity.Statut;
            existing.DateModification = entity.DateModification;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.CategoriesAdhesions.FirstOrDefaultAsync(x => x.IdCategorieAdhesion == id, ct);
            if (existing == null)
                return false;

            _db.CategoriesAdhesions.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
