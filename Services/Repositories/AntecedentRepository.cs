using Microsoft.EntityFrameworkCore;
using ProsocAPI.Models.Core;
using Prosoc.Data;

namespace ProsocAPI.Services.Repositories
{
    public class AntecedentRepository : IAntecedentRepository
    {
        private readonly ProsocDbContext _context;

        public AntecedentRepository(ProsocDbContext context)
        {
            _context = context;
        }

        public async Task<List<Antecedant>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Antecedants
                .Include(a => a.Affilie)
                .ToListAsync(ct);
        }

        public async Task<Antecedant?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Antecedants
                .Include(a => a.Affilie)
                .FirstOrDefaultAsync(a => a.IdAntecedant == id, ct);
        }

        public async Task<List<Antecedant>> GetByAffilieAsync(int affilieId, CancellationToken ct = default)
        {
            return await _context.Antecedants
                .Include(a => a.Affilie)
                .Where(a => a.AffilieId == affilieId)
                .ToListAsync(ct);
        }

        public async Task<List<Antecedant>> GetActifsAsync(CancellationToken ct = default)
        {
            return await _context.Antecedants
                .Include(a => a.Affilie)
                .Where(a => a.Statut)
                .ToListAsync(ct);
        }

        public async Task<Antecedant> CreateAsync(Antecedant entity, CancellationToken ct = default)
        {
            _context.Antecedants.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<Antecedant?> UpdateAsync(int id, Antecedant entity, CancellationToken ct = default)
        {
            var existing = await _context.Antecedants.FindAsync(new object[] { id }, ct);
            if (existing == null)
                return null;

            existing.Description = entity.Description;
            existing.AffilieId = entity.AffilieId;
            existing.Statut = entity.Statut;
            existing.DateModification = DateTime.Now;

            await _context.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _context.Antecedants.FindAsync(new object[] { id }, ct);
            if (entity == null)
                return false;

            _context.Antecedants.Remove(entity);
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
