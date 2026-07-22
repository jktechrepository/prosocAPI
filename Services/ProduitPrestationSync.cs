using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public static class ProduitPrestationSync
    {
        public static async Task EnsureAndSyncMutuelAsync(ProsocDbContext db, ProduitMutuel produit, CancellationToken ct = default)
        {
            var prestations = await db.Prestations
                .Where(p => p.ProduitMutuelId == produit.IdProduit)
                .ToListAsync(ct);

            if (prestations.Count == 0)
            {
                db.Prestations.Add(BuildPrestation(produit, "mutuel", produitMutuelId: produit.IdProduit));
                await db.SaveChangesAsync(ct);
                return;
            }

            foreach (var prestation in prestations)
                ApplyProduitFields(prestation, produit, "mutuel");

            await db.SaveChangesAsync(ct);
        }

        public static async Task EnsureAndSyncAssureurAsync(ProsocDbContext db, ProduitAssureur produit, CancellationToken ct = default)
        {
            var prestations = await db.Prestations
                .Where(p => p.ProduitAssureurId == produit.IdProduit)
                .ToListAsync(ct);

            if (prestations.Count == 0)
            {
                db.Prestations.Add(BuildPrestation(produit, "assureur", produitAssureurId: produit.IdProduit));
                await db.SaveChangesAsync(ct);
                return;
            }

            foreach (var prestation in prestations)
                ApplyProduitFields(prestation, produit, "assureur");

            await db.SaveChangesAsync(ct);
        }

        public static async Task ValidateDeleteMutuelAsync(ProsocDbContext db, int produitId, CancellationToken ct = default)
        {
            if (await HasLinkedSouscriptionsAsync(db, produitMutuelId: produitId, ct: ct))
                throw new ArgumentException(
                    "Impossible de supprimer ce produit mutuel : des souscriptions existent sur les prestations liées.");
        }

        public static async Task ValidateDeleteAssureurAsync(ProsocDbContext db, int produitId, CancellationToken ct = default)
        {
            if (await HasLinkedSouscriptionsAsync(db, produitAssureurId: produitId, ct: ct))
                throw new ArgumentException(
                    "Impossible de supprimer ce produit assureur : des souscriptions existent sur les prestations liées.");
        }

        public static async Task RemoveLinkedPrestationsMutuelAsync(ProsocDbContext db, int produitId, CancellationToken ct = default)
        {
            var prestations = await db.Prestations
                .Where(p => p.ProduitMutuelId == produitId)
                .ToListAsync(ct);
            db.Prestations.RemoveRange(prestations);
        }

        public static async Task RemoveLinkedPrestationsAssureurAsync(ProsocDbContext db, int produitId, CancellationToken ct = default)
        {
            var prestations = await db.Prestations
                .Where(p => p.ProduitAssureurId == produitId)
                .ToListAsync(ct);
            db.Prestations.RemoveRange(prestations);
        }

        private static async Task<bool> HasLinkedSouscriptionsAsync(
            ProsocDbContext db,
            int? produitMutuelId = null,
            int? produitAssureurId = null,
            CancellationToken ct = default)
        {
            var query = db.Prestations.AsQueryable();

            if (produitMutuelId.HasValue)
                query = query.Where(p => p.ProduitMutuelId == produitMutuelId.Value);
            else if (produitAssureurId.HasValue)
                query = query.Where(p => p.ProduitAssureurId == produitAssureurId.Value);
            else
                return false;

            var prestationIds = await query.Select(p => p.IdPrestation).ToListAsync(ct);
            if (prestationIds.Count == 0)
                return false;

            return await db.SouscriptionsPrestations
                .AnyAsync(s => prestationIds.Contains(s.PrestationId), ct);
        }

        private static Prestation BuildPrestation(
            ProduitBase produit,
            string typeLibelle,
            int? produitMutuelId = null,
            int? produitAssureurId = null)
        {
            var prestation = new Prestation
            {
                NomPrestation = produit.Nom,
                Montant = produit.Montant,
                Periodicite = PeriodicitePrestationRegles.Normaliser(produit.Periodicite, "Mensuel"),
                Description = ProduitTarifRules.BuildPrestationDescription(produit, typeLibelle),
                DateCreation = DateTime.Now,
                Statut = produit.Statut,
                ProduitMutuelId = produitMutuelId,
                ProduitAssureurId = produitAssureurId
            };

            if (produit is ProduitMutuel mutuel)
                prestation.DeviseId = mutuel.DeviseId;
            else if (produit is ProduitAssureur assureur)
                prestation.DeviseId = assureur.DeviseId;

            return prestation;
        }

        private static void ApplyProduitFields(Prestation prestation, ProduitBase produit, string typeLibelle)
        {
            prestation.NomPrestation = produit.Nom;
            prestation.Montant = produit.Montant;
            prestation.Periodicite = PeriodicitePrestationRegles.Normaliser(produit.Periodicite, "Mensuel");
            prestation.Description = ProduitTarifRules.BuildPrestationDescription(produit, typeLibelle);
            prestation.Statut = produit.Statut;
            prestation.DateModification = DateTime.Now;

            if (produit is ProduitMutuel mutuel)
                prestation.DeviseId = mutuel.DeviseId;
            else if (produit is ProduitAssureur assureur)
                prestation.DeviseId = assureur.DeviseId;
        }
    }
}
