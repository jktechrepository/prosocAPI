using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class ProvinceService : IProvinceRepository
    {
        private readonly ProsocDbContext _db;

        public ProvinceService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<Province>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Provinces
                .Include(p => p.Communes)
                .AsNoTracking()
                .OrderBy(x => x.Nom)
                .ToListAsync(ct);
        }

        public async Task<Province?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Provinces
                .Include(p => p.Communes)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdProvince == id, ct);
        }

        public async Task<List<Province>> GetActivesAsync(CancellationToken ct = default)
        {
            return await _db.Provinces
                .Include(p => p.Communes)
                .AsNoTracking()
                .Where(x => x.Statut)
                .OrderBy(x => x.Nom)
                .ToListAsync(ct);
        }

        public async Task<Province> CreateAsync(Province entity, CancellationToken ct = default)
        {
            _db.Provinces.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<Province?> UpdateAsync(int id, Province entity, CancellationToken ct = default)
        {
            var existing = await _db.Provinces.FirstOrDefaultAsync(x => x.IdProvince == id, ct);
            if (existing == null)
                return null;

            existing.Nom = entity.Nom;
            existing.Statut = entity.Statut;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.Provinces.FirstOrDefaultAsync(x => x.IdProvince == id, ct);
            if (existing == null)
                return false;

            _db.Provinces.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
