using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class PrestationService : IPrestationRepository
    {
        private readonly ProsocDbContext _db;

        public PrestationService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<Prestation>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Prestations
                .Include(p => p.ProduitMutuel)
                .Include(p => p.ProduitAssureur)
                .Include(p => p.Devise)
                .AsNoTracking()
                .OrderBy(x => x.NomPrestation)
                .ToListAsync(ct);
        }

        public async Task<Prestation?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Prestations
                .Include(p => p.ProduitMutuel)
                .Include(p => p.ProduitAssureur)
                .Include(p => p.Devise)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdPrestation == id, ct);
        }

        public async Task<List<Prestation>> GetByProduitMutuelAsync(int produitMutuelId, CancellationToken ct = default)
        {
            return await _db.Prestations
                .Include(p => p.ProduitMutuel)
                .Include(p => p.ProduitAssureur)
                .Include(p => p.Devise)
                .AsNoTracking()
                .Where(x => x.ProduitMutuelId == produitMutuelId)
                .OrderBy(x => x.NomPrestation)
                .ToListAsync(ct);
        }

        public async Task<List<Prestation>> GetByProduitAssureurAsync(int produitAssureurId, CancellationToken ct = default)
        {
            return await _db.Prestations
                .Include(p => p.ProduitMutuel)
                .Include(p => p.ProduitAssureur)
                .Include(p => p.Devise)
                .AsNoTracking()
                .Where(x => x.ProduitAssureurId == produitAssureurId)
                .OrderBy(x => x.NomPrestation)
                .ToListAsync(ct);
        }

        public async Task<Prestation> CreateAsync(Prestation entity, CancellationToken ct = default)
        {
            entity.Periodicite = PeriodicitePrestationRegles.Normaliser(entity.Periodicite, "Mensuel");
            _db.Prestations.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<Prestation?> UpdateAsync(int id, Prestation entity, CancellationToken ct = default)
        {
            var existing = await _db.Prestations.FirstOrDefaultAsync(x => x.IdPrestation == id, ct);
            if (existing == null)
                return null;

            existing.NomPrestation = entity.NomPrestation;
            existing.Description = entity.Description;
            if (!string.IsNullOrWhiteSpace(entity.Periodicite))
                existing.Periodicite = PeriodicitePrestationRegles.Normaliser(entity.Periodicite);
            existing.ProduitMutuelId = entity.ProduitMutuelId;
            existing.ProduitAssureurId = entity.ProduitAssureurId;
            existing.Montant = entity.Montant;
            existing.DeviseId = entity.DeviseId;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.Prestations.FirstOrDefaultAsync(x => x.IdPrestation == id, ct);
            if (existing == null)
                return false;

            _db.Prestations.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
