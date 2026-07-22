using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class RoleService : IRoleRepository
    {
        private readonly ProsocDbContext _db;

        public RoleService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<Role>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Roles.AsNoTracking().OrderBy(x => x.IdRole).ToListAsync(ct);
        }

        public async Task<Role?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.IdRole == id, ct);
        }

        public async Task<Role> CreateAsync(Role entity, CancellationToken ct = default)
        {
            _db.Roles.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<Role?> UpdateAsync(int id, Role entity, CancellationToken ct = default)
        {
            var existing = await _db.Roles.FirstOrDefaultAsync(x => x.IdRole == id, ct);
            if (existing == null)
                return null;

            existing.Nom = entity.Nom;
            existing.Description = entity.Description;
            existing.Niveau = entity.Niveau;
            existing.Statut = entity.Statut;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.Roles.FirstOrDefaultAsync(x => x.IdRole == id, ct);
            if (existing == null)
                return false;

            _db.Roles.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
