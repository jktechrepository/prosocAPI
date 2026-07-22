using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class TargetAgentService : ITargetAgentRepository
    {
        private readonly ProsocDbContext _db;

        public TargetAgentService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<TargetAgent>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.TargetsAgents
                .Include(t => t.Role)
                .AsNoTracking()
                .OrderByDescending(x => x.DateCreation)
                .ToListAsync(ct);
        }

        public async Task<TargetAgent?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.TargetsAgents
                .Include(t => t.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdTargetAgent == id, ct);
        }

        public async Task<List<TargetAgent>> GetByRoleAsync(int roleId, CancellationToken ct = default)
        {
            return await _db.TargetsAgents
                .Include(t => t.Role)
                .AsNoTracking()
                .Where(x => x.RoleId == roleId)
                .OrderByDescending(x => x.DateCreation)
                .ToListAsync(ct);
        }

        public async Task<List<TargetAgent>> GetActifsAsync(CancellationToken ct = default)
        {
            return await _db.TargetsAgents
                .Include(t => t.Role)
                .AsNoTracking()
                .Where(x => x.Statut)
                .OrderByDescending(x => x.DateCreation)
                .ToListAsync(ct);
        }

        public async Task<bool> HasActiveConflictAsync(
            int roleId,
            PeriodiciteTarget periodicite,
            int? excludeId = null,
            CancellationToken ct = default)
        {
            var query = _db.TargetsAgents
                .AsNoTracking()
                .Where(t => t.RoleId == roleId
                    && t.Periodicite == periodicite
                    && t.Statut);

            if (excludeId.HasValue)
                query = query.Where(t => t.IdTargetAgent != excludeId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task<TargetAgent> CreateAsync(TargetAgent entity, CancellationToken ct = default)
        {
            _db.TargetsAgents.Add(entity);
            await _db.SaveChangesAsync(ct);

            await _db.Entry(entity).Reference(t => t.Role).LoadAsync(ct);
            return entity;
        }

        public async Task<TargetAgent?> UpdateAsync(int id, TargetAgent entity, CancellationToken ct = default)
        {
            var existing = await _db.TargetsAgents.FirstOrDefaultAsync(x => x.IdTargetAgent == id, ct);
            if (existing == null)
                return null;

            existing.RoleId = entity.RoleId;
            existing.LibelleTarget = entity.LibelleTarget;
            existing.Periodicite = entity.Periodicite;
            existing.Nombre = entity.Nombre;
            existing.Statut = entity.Statut;
            existing.DateModification = DateTime.Now;

            await _db.SaveChangesAsync(ct);
            await _db.Entry(existing).Reference(t => t.Role).LoadAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.TargetsAgents.FirstOrDefaultAsync(x => x.IdTargetAgent == id, ct);
            if (existing == null)
                return false;

            _db.TargetsAgents.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
