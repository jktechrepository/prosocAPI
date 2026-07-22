using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class ZoneSocialeService : IZoneSocialeRepository
    {
        private readonly ProsocDbContext _db;

        public ZoneSocialeService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<ZoneSociale>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.ZonesSociales
                .Include(z => z.Commune)
                .Include(z => z.ChefEquipe)
                .AsNoTracking()
                .OrderBy(x => x.Nom)
                .ToListAsync(ct);
        }

        public async Task<ZoneSociale?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.ZonesSociales
                .Include(z => z.Commune)
                .Include(z => z.ChefEquipe)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdZoneSociale == id, ct);
        }

        public async Task<List<ZoneSociale>> GetByCommuneAsync(int communeId, CancellationToken ct = default)
        {
            return await _db.ZonesSociales
                .Include(z => z.Commune)
                .Include(z => z.ChefEquipe)
                .AsNoTracking()
                .Where(x => x.CommuneId == communeId)
                .OrderBy(x => x.Nom)
                .ToListAsync(ct);
        }

        public async Task<ZoneSociale> CreateAsync(ZoneSociale entity, CancellationToken ct = default)
        {
            _db.ZonesSociales.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<ZoneSociale?> UpdateAsync(int id, ZoneSociale entity, CancellationToken ct = default)
        {
            var existing = await _db.ZonesSociales.FirstOrDefaultAsync(x => x.IdZoneSociale == id, ct);
            if (existing == null)
                return null;

            existing.Nom = entity.Nom;
            existing.CommuneId = entity.CommuneId;
            existing.Statut = entity.Statut;
            existing.DateModification = DateTime.Now;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.ZonesSociales.FirstOrDefaultAsync(x => x.IdZoneSociale == id, ct);
            if (existing == null)
                return false;

            _db.ZonesSociales.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
