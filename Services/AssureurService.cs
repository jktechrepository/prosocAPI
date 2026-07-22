using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class AssureurService : IAssureurRepository
    {
        private readonly ProsocDbContext _db;

        public AssureurService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<Assureur>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Assureurs
                .Include(a => a.Produits)
                .AsNoTracking()
                .OrderBy(x => x.Nom)
                .ToListAsync(ct);
        }

        public async Task<Assureur?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Assureurs
                .Include(a => a.Produits)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdAssureur == id, ct);
        }

        public async Task<List<Assureur>> GetActivesAsync(CancellationToken ct = default)
        {
            return await _db.Assureurs
                .Include(a => a.Produits)
                .AsNoTracking()
                .Where(x => x.Statut)
                .OrderBy(x => x.Nom)
                .ToListAsync(ct);
        }

        public async Task<Assureur> CreateAsync(Assureur entity, CancellationToken ct = default)
        {
            _db.Assureurs.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<Assureur?> UpdateAsync(int id, Assureur entity, CancellationToken ct = default)
        {
            var existing = await _db.Assureurs.FirstOrDefaultAsync(x => x.IdAssureur == id, ct);
            if (existing == null)
                return null;

            existing.Nom = entity.Nom;
            existing.Description = entity.Description;
            existing.Statut = entity.Statut;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.Assureurs.FirstOrDefaultAsync(x => x.IdAssureur == id, ct);
            if (existing == null)
                return false;

            _db.Assureurs.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
