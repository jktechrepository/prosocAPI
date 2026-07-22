using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;

namespace Prosoc.Utilities
{
    /// <summary>
    /// Vérifications de périmètre hôpital partenaire (bons, jetons, demandes).
    /// </summary>
    public static class HopitalScopeHelper
    {
        public static IQueryable<JetonMedical> QueryJetonsForHopital(ProsocDbContext db, int hopitalPartenaireId) =>
            db.JetonsMedicaux.AsNoTracking()
                .Where(j => j.Statut && j.HopitalPartenaireId == hopitalPartenaireId);

        public static IQueryable<int> QueryAffilieIdsForHopital(ProsocDbContext db, int hopitalPartenaireId) =>
            QueryJetonsForHopital(db, hopitalPartenaireId)
                .Select(j => j.AffilieId)
                .Distinct();

        public static IQueryable<BonEnvoi> QueryBonsForHopital(ProsocDbContext db, int hopitalPartenaireId) =>
            db.BonsEnvoi.AsNoTracking()
                .Where(b => b.Statut
                    && db.DemandesBonEnvoi.Any(d =>
                        d.BonEnvoiId == b.IdBonEnvoi
                        && d.JetonMedicalId != null
                        && db.JetonsMedicaux.Any(j =>
                            j.IdJeton == d.JetonMedicalId
                            && j.HopitalPartenaireId == hopitalPartenaireId)));

        public static async Task<bool> IsBonLinkedToHopitalAsync(
            ProsocDbContext db,
            int bonEnvoiId,
            int hopitalPartenaireId,
            CancellationToken ct = default)
        {
            if (hopitalPartenaireId <= 0)
                return false;

            return await (
                from d in db.DemandesBonEnvoi.AsNoTracking()
                join j in db.JetonsMedicaux.AsNoTracking() on d.JetonMedicalId equals j.IdJeton
                where d.BonEnvoiId == bonEnvoiId
                      && j.HopitalPartenaireId == hopitalPartenaireId
                select d.IdDemande
            ).AnyAsync(ct);
        }

        public static async Task<bool> IsJetonLinkedToHopitalAsync(
            ProsocDbContext db,
            int jetonId,
            int hopitalPartenaireId,
            CancellationToken ct = default)
        {
            if (hopitalPartenaireId <= 0)
                return false;

            return await db.JetonsMedicaux
                .AsNoTracking()
                .AnyAsync(j =>
                    j.IdJeton == jetonId
                    && j.HopitalPartenaireId == hopitalPartenaireId, ct);
        }

        public static async Task<bool> IsJetonCodeLinkedToHopitalAsync(
            ProsocDbContext db,
            string codeJeton,
            int hopitalPartenaireId,
            CancellationToken ct = default)
        {
            if (hopitalPartenaireId <= 0 || string.IsNullOrWhiteSpace(codeJeton))
                return false;

            return await db.JetonsMedicaux
                .AsNoTracking()
                .AnyAsync(j =>
                    j.CodeJeton == codeJeton
                    && j.HopitalPartenaireId == hopitalPartenaireId, ct);
        }
    }
}
