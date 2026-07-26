using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    /// <summary>
    /// Garde-fou : une période (Mois/Annee) de souscription déjà soldée
    /// (somme des collectes VALIDE ≥ montant attendu) ne peut plus recevoir de paiement.
    /// </summary>
    public static class SouscriptionPeriodePaiementRules
    {
        public const string CodeErreurDejaPayeePeriode = "DEJA_PAYEE_PERIODE";

        public static string FormatMessagePeriodeSoldee(int mois, int annee) =>
            $"[{CodeErreurDejaPayeePeriode}] Cette souscription est déjà soldée pour la période {mois:D2}/{annee}";

        /// <summary>
        /// Refuse si la période est entièrement couverte. No-op si hors souscription / ids invalides.
        /// </summary>
        public static async Task EnsurePeriodeNonSoldeeAsync(
            ProsocDbContext db,
            int? souscriptionPrestationId,
            int mois,
            int annee,
            CancellationToken ct = default,
            decimal? montantAttenduPeriode = null,
            decimal tolerance = 0.01m)
        {
            if (!souscriptionPrestationId.HasValue || souscriptionPrestationId.Value <= 0)
                return;

            if (mois < 1 || mois > 12 || annee < 2020)
                throw new ArgumentException("Période de collecte invalide (Mois/Annee).");

            var attendu = montantAttenduPeriode
                ?? await ResolveMontantAttenduSouscriptionAsync(db, souscriptionPrestationId.Value, ct);

            if (await EstPeriodeSoldeeAsync(
                    db, souscriptionPrestationId.Value, mois, annee, attendu, ct, tolerance))
            {
                throw new InvalidOperationException(FormatMessagePeriodeSoldee(mois, annee));
            }
        }

        public static async Task<bool> EstPeriodeSoldeeAsync(
            ProsocDbContext db,
            int souscriptionPrestationId,
            int mois,
            int annee,
            decimal montantAttenduPeriode,
            CancellationToken ct = default,
            decimal tolerance = 0.01m)
        {
            var sommePayee = await CalculerMontantPayePeriodeAsync(
                db, souscriptionPrestationId, mois, annee, ct);

            if (montantAttenduPeriode <= 0)
                return sommePayee > 0 || await ExisteCollecteValidePeriodeAsync(
                    db, souscriptionPrestationId, mois, annee, ct);

            return sommePayee + tolerance >= montantAttenduPeriode;
        }

        public static async Task<decimal> CalculerMontantPayePeriodeAsync(
            ProsocDbContext db,
            int souscriptionPrestationId,
            int mois,
            int annee,
            CancellationToken ct = default)
        {
            var collectes = await db.Collectes
                .AsNoTracking()
                .Where(c => c.SouscriptionPrestationId == souscriptionPrestationId
                            && c.TypeCollecte == TypeCollecte.Souscription
                            && c.Mois == mois
                            && c.Annee == annee
                            && c.Statut)
                .Select(c => new
                {
                    c.StatutPaiement,
                    c.MontantTarifAttendu,
                    c.MontantAttendu,
                    c.Montant
                })
                .ToListAsync(ct);

            return collectes
                .Where(c => CollecteStatutPaiementRegles.EstValide(c.StatutPaiement))
                .Sum(c => c.MontantTarifAttendu
                          ?? c.MontantAttendu
                          ?? c.Montant);
        }

        public static async Task<decimal> ResolveMontantAttenduSouscriptionAsync(
            ProsocDbContext db,
            int souscriptionPrestationId,
            CancellationToken ct = default)
        {
            var produit = await ProduitSouscriptionRules.ResolveProduitFromSouscriptionAsync(
                db, souscriptionPrestationId, ct);

            if (produit == null)
                throw new ArgumentException("Produit introuvable pour cette souscription.");

            return produit.EstGratuit ? 0m : produit.Montant;
        }

        private static async Task<bool> ExisteCollecteValidePeriodeAsync(
            ProsocDbContext db,
            int souscriptionPrestationId,
            int mois,
            int annee,
            CancellationToken ct)
        {
            var collectes = await db.Collectes
                .AsNoTracking()
                .Where(c => c.SouscriptionPrestationId == souscriptionPrestationId
                            && c.TypeCollecte == TypeCollecte.Souscription
                            && c.Mois == mois
                            && c.Annee == annee
                            && c.Statut)
                .Select(c => c.StatutPaiement)
                .ToListAsync(ct);

            return collectes.Any(CollecteStatutPaiementRegles.EstValide);
        }
    }
}
