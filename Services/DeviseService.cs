using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class DeviseService : IDeviseRepository
    {
        private readonly ProsocDbContext _db;

        public DeviseService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<Devise>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Devises.AsNoTracking().OrderBy(x => x.Code).ToListAsync(ct);
        }

        public async Task<Devise?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Devises.AsNoTracking().FirstOrDefaultAsync(x => x.IdDevise == id, ct);
        }

        public async Task<Devise?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            return await _db.Devises.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code, ct);
        }

        public async Task<List<Devise>> GetActivesAsync(CancellationToken ct = default)
        {
            return await _db.Devises.AsNoTracking().Where(x => x.Statut).OrderBy(x => x.Code).ToListAsync(ct);
        }

        public async Task<Devise> CreateAsync(Devise entity, CancellationToken ct = default)
        {
            entity.Code = entity.Code.ToUpperInvariant();
            await EnsureSingleDevisePrincipaleAsync(entity, ct);
            _db.Devises.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<Devise?> UpdateAsync(int id, Devise entity, CancellationToken ct = default)
        {
            var existing = await _db.Devises.FirstOrDefaultAsync(x => x.IdDevise == id, ct);
            if (existing == null)
                return null;

            if (existing.EstDevisePrincipale && !entity.EstDevisePrincipale && entity.Statut)
                throw new InvalidOperationException(
                    "Impossible de retirer le statut de devise principale sans en désigner une autre.");

            existing.Code = entity.Code.ToUpperInvariant();
            existing.Nom = entity.Nom;
            existing.Symbole = entity.Symbole;
            existing.Statut = entity.Statut;
            existing.EstDevisePrincipale = entity.EstDevisePrincipale;

            await EnsureSingleDevisePrincipaleAsync(existing, ct);
            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.Devises.FirstOrDefaultAsync(x => x.IdDevise == id, ct);
            if (existing == null)
                return false;

            if (existing.EstDevisePrincipale)
                throw new InvalidOperationException("Impossible de supprimer la devise principale.");

            _db.Devises.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        private async Task EnsureSingleDevisePrincipaleAsync(Devise entity, CancellationToken ct)
        {
            if (!entity.EstDevisePrincipale)
                return;

            if (!entity.Statut)
                throw new InvalidOperationException("La devise principale doit être active.");

            var autres = await _db.Devises
                .Where(d => d.EstDevisePrincipale && d.IdDevise != entity.IdDevise)
                .ToListAsync(ct);

            foreach (var autre in autres)
                autre.EstDevisePrincipale = false;
        }
    }
}
