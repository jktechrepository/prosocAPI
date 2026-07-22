using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class CommuneService : ICommuneRepository
    {
        private readonly ProsocDbContext _db;

        public CommuneService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<Commune>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Communes
                .Include(c => c.Province)
                .Include(c => c.Superviseur)
                .Include(c => c.Zones)
                .AsNoTracking()
                .OrderBy(x => x.Nom)
                .ToListAsync(ct);
        }

        public async Task<Commune?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Communes
                .Include(c => c.Province)
                .Include(c => c.Superviseur)
                .Include(c => c.Zones)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdCommune == id, ct);
        }

        public async Task<List<Commune>> GetByProvinceAsync(int provinceId, CancellationToken ct = default)
        {
            return await _db.Communes
                .Include(c => c.Province)
                .Include(c => c.Superviseur)
                .Include(c => c.Zones)
                .AsNoTracking()
                .Where(x => x.ProvinceId == provinceId)
                .OrderBy(x => x.Nom)
                .ToListAsync(ct);
        }

        public async Task<Commune> CreateAsync(Commune entity, CancellationToken ct = default)
        {
            _db.Communes.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<Commune?> UpdateAsync(int id, Commune entity, CancellationToken ct = default)
        {
            var existing = await _db.Communes.FirstOrDefaultAsync(x => x.IdCommune == id, ct);
            if (existing == null)
                return null;

            existing.Nom = entity.Nom;
            existing.ProvinceId = entity.ProvinceId;
            existing.Statut = entity.Statut;
            existing.DateModification = DateTime.Now;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.Communes.FirstOrDefaultAsync(x => x.IdCommune == id, ct);
            if (existing == null)
                return false;

            _db.Communes.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
