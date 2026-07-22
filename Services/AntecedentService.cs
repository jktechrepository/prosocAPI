using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class AntecedentService : IAntecedentRepository
    {
        private readonly ProsocDbContext _db;

        public AntecedentService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<Antecedant>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Antecedants
                .Include(a => a.Affilie)
                .AsNoTracking()
                .OrderByDescending(x => x.DateCreation)
                .ToListAsync(ct);
        }

        public async Task<Antecedant?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Antecedants
                .Include(a => a.Affilie)
                .Include(a => a.Dependant)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdAntecedant == id, ct);
        }

        public async Task<List<Antecedant>> GetByAffilieAsync(int affilieId, CancellationToken ct = default)
        {
            return await _db.Antecedants
                .Include(a => a.Affilie)
                .AsNoTracking()
                .Where(x => x.AffilieId == affilieId)
                .OrderByDescending(x => x.DateCreation)
                .ToListAsync(ct);
        }

        public async Task<List<Antecedant>> GetActifsAsync(CancellationToken ct = default)
        {
            return await _db.Antecedants
                .Include(a => a.Affilie)
                .AsNoTracking()
                .Where(x => x.Statut)
                .OrderByDescending(x => x.DateCreation)
                .ToListAsync(ct);
        }

        public async Task<Antecedant> CreateAsync(Antecedant entity, CancellationToken ct = default)
        {
            _db.Antecedants.Add(entity);
            await _db.SaveChangesAsync(ct);
            return await GetByIdAsync(entity.IdAntecedant, ct) ?? entity;
        }

        public async Task<Antecedant?> UpdateAsync(int id, Antecedant entity, CancellationToken ct = default)
        {
            var existing = await _db.Antecedants.FirstOrDefaultAsync(x => x.IdAntecedant == id, ct);
            if (existing == null)
                return null;

            existing.Description = entity.Description;
            existing.AffilieId = entity.AffilieId;
            existing.DependantId = entity.DependantId;
            existing.Statut = entity.Statut;
            existing.DateModification = entity.DateModification ?? DateTime.Now;

            await _db.SaveChangesAsync(ct);
            return await GetByIdAsync(id, ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.Antecedants.FirstOrDefaultAsync(x => x.IdAntecedant == id, ct);
            if (existing == null)
                return false;

            _db.Antecedants.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
