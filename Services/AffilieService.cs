using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class AffilieService : IAffilieRepository
    {
        private readonly ProsocDbContext _db;

        public AffilieService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<List<Affilie>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Affilies.AsNoTracking().OrderBy(x => x.IdAffilie).ToListAsync(ct);
        }

        public async Task<Affilie?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await AffilieQueryHelper.WithAssociations(_db.Affilies.AsNoTracking())
                .FirstOrDefaultAsync(x => x.IdAffilie == id, ct);
        }

        public async Task<Affilie> CreateAsync(Affilie entity, CancellationToken ct = default)
        {
            entity.NomComplet = entity.Nom + " " + entity.Postnom + " " + entity.Prenom;
            _db.Affilies.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<Affilie?> UpdateAsync(int id, Affilie entity, CancellationToken ct = default)
        {
            var existing = await _db.Affilies.FirstOrDefaultAsync(x => x.IdAffilie == id, ct);
            if (existing == null)
                return null;

            existing.CodeAdhesion = entity.CodeAdhesion;
            existing.Nom = entity.Nom;
            existing.Prenom = entity.Prenom;
            existing.NomComplet = entity.NomComplet = entity.Nom + " " + entity.Postnom + " " + entity.Prenom;
            existing.DateNaissance = entity.DateNaissance;
            existing.Telephone = entity.Telephone;
            existing.Postnom = entity.Postnom;
            existing.ProvinceResidence = entity.ProvinceResidence;
            existing.CommuneResidence = entity.CommuneResidence;
            existing.QuartierResidence = entity.QuartierResidence;
            existing.AvenueResidence = entity.AvenueResidence;
            existing.NumeroResidence = entity.NumeroResidence;
            existing.CommuneActivite = entity.CommuneActivite;
            existing.QuartierActivite = entity.QuartierActivite;
            existing.AvenueActivite = entity.AvenueActivite;
            existing.NumeroActivite = entity.NumeroActivite;
            existing.PhotoData = entity.PhotoData;
            existing.PhotoContentType = entity.PhotoContentType;
            existing.CarteIdentiteData = entity.CarteIdentiteData;
            existing.CarteIdentiteContentType = entity.CarteIdentiteContentType;
            existing.Statut = entity.Statut;
            existing.DateModification = DateTime.Now;

            await _db.SaveChangesAsync(ct);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var existing = await _db.Affilies.FirstOrDefaultAsync(x => x.IdAffilie == id, ct);
            if (existing == null)
                return false;

            _db.Affilies.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
