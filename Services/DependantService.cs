using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class DependantService : IDependantRepository
    {
        private readonly ProsocDbContext _db;

        public DependantService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<Dependant>> GetAllAsync(CancellationToken ct = default)
        {
            return await WithAntecedants(_db.Dependants.AsNoTracking())
                .OrderBy(x => x.IdDependant)
                .ToListAsync(ct);
        }

        public async Task<Dependant?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await WithAntecedants(_db.Dependants.AsNoTracking())
                .FirstOrDefaultAsync(x => x.IdDependant == id, ct);
        }

        public async Task<Dependant> CreateAsync(Dependant entity, CancellationToken ct = default)
        {
            _db.Dependants.Add(entity);
            await _db.SaveChangesAsync(ct);
            return await GetByIdAsync(entity.IdDependant, ct) ?? entity;
        }

        public async Task<Dependant?> UpdateAsync(int id, Dependant entity, CancellationToken ct = default)
        {
            var existing = await _db.Dependants.FirstOrDefaultAsync(x => x.IdDependant == id, ct);
            if (existing == null)
                return null;

            existing.Nom = entity.Nom;
            existing.LienParente = entity.LienParente;
            existing.AffilieId = entity.AffilieId;

            await _db.SaveChangesAsync(ct);
            return await GetByIdAsync(id, ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.Dependants.FirstOrDefaultAsync(x => x.IdDependant == id, ct);
            if (existing == null)
                return false;

            _db.Dependants.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        private static IQueryable<Dependant> WithAntecedants(IQueryable<Dependant> query) =>
            query.Include(d => d.Antecedants).ThenInclude(a => a.Affilie);
    }
}
