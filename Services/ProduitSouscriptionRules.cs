using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public static class ProduitSouscriptionRules
    {
        public static async Task<ProduitBase?> ResolveProduitFromSouscriptionAsync(
            ProsocDbContext db,
            int souscriptionPrestationId,
            CancellationToken ct = default)
        {
            var souscription = await db.SouscriptionsPrestations
                .AsNoTracking()
                .Include(sp => sp.Prestation)
                    .ThenInclude(p => p!.ProduitMutuel)
                .Include(sp => sp.Prestation)
                    .ThenInclude(p => p!.ProduitAssureur)
                .FirstOrDefaultAsync(sp => sp.IdSouscriptionPrestation == souscriptionPrestationId, ct);

            if (souscription?.Prestation?.ProduitMutuel != null)
                return souscription.Prestation.ProduitMutuel;

            if (souscription?.Prestation?.ProduitAssureur != null)
                return souscription.Prestation.ProduitAssureur;

            return null;
        }

        public static async Task<ProduitBase?> ResolveProduitFromPrestationAsync(
            ProsocDbContext db,
            int prestationId,
            CancellationToken ct = default)
        {
            var prestation = await db.Prestations
                .AsNoTracking()
                .Include(p => p.ProduitMutuel)
                .Include(p => p.ProduitAssureur)
                .FirstOrDefaultAsync(p => p.IdPrestation == prestationId, ct);

            if (prestation?.ProduitMutuel != null)
                return prestation.ProduitMutuel;

            if (prestation?.ProduitAssureur != null)
                return prestation.ProduitAssureur;

            return null;
        }

        /// <summary>
        /// Résout le produit d'une collecte souscription : par IdSouscriptionPrestation,
        /// ou par IdPrestation lors d'une adhésion (souscription pas encore créée).
        /// </summary>
        public static async Task<ProduitBase?> ResolveProduitForCollecteSouscriptionAsync(
            ProsocDbContext db,
            int souscriptionOrPrestationId,
            CancellationToken ct = default) =>
            await ResolveProduitFromSouscriptionAsync(db, souscriptionOrPrestationId, ct)
            ?? await ResolveProduitFromPrestationAsync(db, souscriptionOrPrestationId, ct);

        public static void ValidateMontantCollecteSouscription(decimal montantCollecte, ProduitBase produit)
        {
            if (produit.EstGratuit)
            {
                if (montantCollecte != 0)
                    throw new ArgumentException(
                        "Ce produit est gratuit (couvert par la cotisation) : le montant de la collecte souscription doit être 0.");
                return;
            }

            if (montantCollecte <= 0)
                throw new ArgumentException("Le montant de la collecte doit être supérieur à zéro pour un produit payant.");

            if (montantCollecte != produit.Montant)
                throw new ArgumentException(
                    $"Montant incorrect pour ce produit payant. Montant attendu : {produit.Montant}.");
        }

        public static async Task ValidateCollecteSouscriptionAsync(
            ProsocDbContext db,
            int affilieId,
            int souscriptionPrestationId,
            decimal montantCollecte,
            CancellationToken ct = default)
        {
            await ProduitEligibiliteRules.ValidateAchatProduitBySouscriptionAsync(
                db, affilieId, souscriptionPrestationId, ct);

            var produit = await ResolveProduitFromSouscriptionAsync(db, souscriptionPrestationId, ct);
            if (produit == null)
                throw new ArgumentException(
                    "Impossible de valider la collecte : aucun produit mutuel ou assureur lié à cette souscription.");

            ValidateMontantCollecteSouscription(montantCollecte, produit);
        }
    }
}
