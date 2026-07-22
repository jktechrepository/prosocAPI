using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class PermissionService : IPermissionRepository
    {
        private readonly ProsocDbContext _db;

        public PermissionService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<Permission>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Permissions.AsNoTracking().OrderBy(x => x.IdPermission).ToListAsync(ct);
        }

        public async Task<Permission?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Permissions.AsNoTracking().FirstOrDefaultAsync(x => x.IdPermission == id, ct);
        }

        public async Task<Permission> CreateAsync(Permission entity, CancellationToken ct = default)
        {
            _db.Permissions.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<Permission?> UpdateAsync(int id, Permission entity, CancellationToken ct = default)
        {
            var existing = await _db.Permissions.FirstOrDefaultAsync(x => x.IdPermission == id, ct);
            if (existing == null)
                return null;

            existing.Nom = entity.Nom;
            existing.Description = entity.Description;
            existing.Categorie = entity.Categorie;
            existing.Action = entity.Action;
            existing.Statut = entity.Statut;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.Permissions.FirstOrDefaultAsync(x => x.IdPermission == id, ct);
            if (existing == null)
                return false;

            _db.Permissions.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
