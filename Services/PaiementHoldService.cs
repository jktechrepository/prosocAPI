using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public interface IPaiementHoldService
    {
        Task EnsureNoActiveHoldAsync(
            int? affilieId,
            TypeCollecte typeCollecte,
            int mois,
            int annee,
            int? fraisId,
            int? souscriptionPrestationId,
            int? cotisationAffilieId,
            string? telephoneAffilie,
            CancellationToken ct = default);

        Task<PaiementHold> CreateHoldAsync(
            Guid idCollecteEnAttente,
            int? affilieId,
            TypeCollecte typeCollecte,
            int mois,
            int annee,
            int? fraisId,
            int? souscriptionPrestationId,
            int? cotisationAffilieId,
            string? telephoneAffilie,
            DateTime expireAt,
            CancellationToken ct = default);

        Task ReleaseHoldAsync(Guid idCollecteEnAttente, CancellationToken ct = default);
    }

    public class PaiementHoldService : IPaiementHoldService
    {
        private readonly ProsocDbContext _db;

        public PaiementHoldService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task EnsureNoActiveHoldAsync(
            int? affilieId,
            TypeCollecte typeCollecte,
            int mois,
            int annee,
            int? fraisId,
            int? souscriptionPrestationId,
            int? cotisationAffilieId,
            string? telephoneAffilie,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var query = _db.PaiementHolds.AsNoTracking().Where(h => h.ExpireAt > now);

            if (affilieId.HasValue)
            {
                query = query.Where(h =>
                    h.AffilieId == affilieId
                    && h.TypeCollecte == typeCollecte
                    && h.Mois == mois
                    && h.Annee == annee
                    && h.FraisId == fraisId
                    && h.SouscriptionPrestationId == souscriptionPrestationId
                    && h.CotisationAffilieId == cotisationAffilieId);
            }
            else if (!string.IsNullOrWhiteSpace(telephoneAffilie))
            {
                query = query.Where(h => h.TelephoneAffilie == telephoneAffilie);
            }

            if (await query.AnyAsync(ct))
            {
                throw new InvalidOperationException(
                    "Un paiement électronique est déjà en cours pour cette période ou cet affilié. " +
                    "Veuillez attendre la confirmation ou l'expiration du délai.");
            }
        }

        public async Task<PaiementHold> CreateHoldAsync(
            Guid idCollecteEnAttente,
            int? affilieId,
            TypeCollecte typeCollecte,
            int mois,
            int annee,
            int? fraisId,
            int? souscriptionPrestationId,
            int? cotisationAffilieId,
            string? telephoneAffilie,
            DateTime expireAt,
            CancellationToken ct = default)
        {
            var hold = new PaiementHold
            {
                IdCollecteEnAttente = idCollecteEnAttente,
                AffilieId = affilieId,
                TypeCollecte = typeCollecte,
                Mois = mois,
                Annee = annee,
                FraisId = fraisId,
                SouscriptionPrestationId = souscriptionPrestationId,
                CotisationAffilieId = cotisationAffilieId,
                TelephoneAffilie = telephoneAffilie,
                ExpireAt = expireAt
            };
            _db.PaiementHolds.Add(hold);
            await _db.SaveChangesAsync(ct);
            return hold;
        }

        public async Task ReleaseHoldAsync(Guid idCollecteEnAttente, CancellationToken ct = default)
        {
            var holds = await _db.PaiementHolds
                .Where(h => h.IdCollecteEnAttente == idCollecteEnAttente)
                .ToListAsync(ct);
            if (holds.Count == 0)
                return;
            _db.PaiementHolds.RemoveRange(holds);
            await _db.SaveChangesAsync(ct);
        }
    }
}
