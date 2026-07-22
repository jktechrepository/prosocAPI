using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class TypeAdhesionService : ITypeAdhesionRepository
    {
        private readonly ProsocDbContext _db;

        public TypeAdhesionService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<TypeAdhesion>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.TypeAdhesions.AsNoTracking().OrderBy(x => x.IdTypeAdhesion).ToListAsync(ct);
        }

        public async Task<TypeAdhesion?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.TypeAdhesions.AsNoTracking().FirstOrDefaultAsync(x => x.IdTypeAdhesion == id, ct);
        }

        public async Task<TypeAdhesion> CreateAsync(TypeAdhesion entity, CancellationToken ct = default)
        {
            _db.TypeAdhesions.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<TypeAdhesion?> UpdateAsync(int id, TypeAdhesion entity, CancellationToken ct = default)
        {
            var existing = await _db.TypeAdhesions.FirstOrDefaultAsync(x => x.IdTypeAdhesion == id, ct);
            if (existing == null)
                return null;

            existing.Libelle = entity.Libelle;
            existing.CategorieAdhesionId = entity.CategorieAdhesionId;
            existing.MaxDependants = entity.MaxDependants;
            existing.Description = entity.Description;
            existing.Montant = entity.Montant;
            existing.DeviseId = entity.DeviseId;
            existing.Statut = entity.Statut;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.TypeAdhesions.FirstOrDefaultAsync(x => x.IdTypeAdhesion == id, ct);
            if (existing == null)
                return false;

            _db.TypeAdhesions.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
