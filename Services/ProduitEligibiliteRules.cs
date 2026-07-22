using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public static class ProduitEligibiliteRules
    {
        public static int CalculerAgeAns(DateTime dateNaissance, DateTime? dateReference = null)
        {
            var reference = (dateReference ?? DateTime.Today).Date;
            var naissance = dateNaissance.Date;
            var age = reference.Year - naissance.Year;
            if (naissance > reference.AddYears(-age))
                age--;
            return age;
        }

        public static void ValidateAgeEligibilite(DateTime dateNaissance, ProduitBase produit, DateTime? dateReference = null)
        {
            var age = CalculerAgeAns(dateNaissance, dateReference);
            if (age < produit.AgeMin || age > produit.AgeMax)
            {
                throw new ArgumentException(
                    $"L'affilié ({age} ans) n'est pas éligible au produit « {produit.Nom} » " +
                    $"(tranche autorisée : {produit.AgeMin}-{produit.AgeMax} ans).");
            }
        }

        public static (DateTime Debut, DateTime Fin) GetPeriodeCourante(string periodicite, DateTime? dateReference = null)
        {
            var refDate = (dateReference ?? DateTime.Today).Date;
            var normalisee = CotisationAffilieService.NormalizePeriodicite(periodicite);

            if (normalisee.Equals("Annuel", StringComparison.OrdinalIgnoreCase))
            {
                var debut = new DateTime(refDate.Year, 1, 1);
                return (debut, debut.AddYears(1));
            }

            var debutMois = new DateTime(refDate.Year, refDate.Month, 1);
            return (debutMois, debutMois.AddMonths(1));
        }

        public static async Task ValidateCotisationAJourAsync(
            ProsocDbContext db,
            int affilieId,
            CancellationToken ct = default,
            int? typeAdhesionIdOverride = null,
            bool cotisationPayeeDansLot = false)
        {
            if (cotisationPayeeDansLot)
                return;

            int typeAdhesionId;
            if (typeAdhesionIdOverride.HasValue)
            {
                typeAdhesionId = typeAdhesionIdOverride.Value;
            }
            else
            {
                var adhesion = await db.Adhesions
                    .AsNoTracking()
                    .Where(a => a.AffilieId == affilieId && a.Statut)
                    .OrderByDescending(a => a.DateCreation)
                    .FirstOrDefaultAsync(ct);

                if (adhesion == null)
                    throw new ArgumentException(
                        "Aucune adhésion active trouvée. Impossible de souscrire à un produit.");

                typeAdhesionId = adhesion.TypeAdhesionId;
            }

            var cotisations = await db.CotisationsAffilie
                .AsNoTracking()
                .Where(c => c.TypeAdhesionId == typeAdhesionId && c.Statut)
                .ToListAsync(ct);

            var cotisation = cotisations.FirstOrDefault(c => c.Periodicite == "Mensuel")
                ?? cotisations.OrderByDescending(c => c.DateCreation).FirstOrDefault();

            if (cotisation == null)
            {
                throw new ArgumentException(
                    "Aucune grille de cotisation active pour ce type d'adhésion. Paiement de cotisation requis.");
            }

            var (debut, fin) = GetPeriodeCourante(cotisation.Periodicite);

            var cotisationPayee = await db.Collectes.AnyAsync(c =>
                c.AffilieId == affilieId &&
                c.TypeCollecte == TypeCollecte.Cotisation &&
                c.CotisationAffilieId == cotisation.IdCotisationAffilie &&
                c.Statut &&
                c.DateCollecte >= debut &&
                c.DateCollecte < fin &&
                c.StatutPaiement != null &&
                CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement), ct);

            if (!cotisationPayee)
            {
                throw new ArgumentException(
                    $"Cotisation {cotisation.Periodicite} non à jour pour la période en cours " +
                    $"({debut:yyyy-MM-dd} → {fin.AddDays(-1):yyyy-MM-dd}). " +
                    "Réglez la cotisation avant d'acheter un produit mutuel ou assureur.");
            }
        }

        public static async Task<ProduitBase> ResolveProduitFromPrestationAsync(
            ProsocDbContext db,
            int prestationId,
            CancellationToken ct = default)
        {
            var prestation = await db.Prestations
                .AsNoTracking()
                .Include(p => p.ProduitMutuel)
                .Include(p => p.ProduitAssureur)
                .FirstOrDefaultAsync(p => p.IdPrestation == prestationId, ct);

            if (prestation == null)
                throw new ArgumentException($"Prestation {prestationId} introuvable.");

            if (prestation.ProduitMutuel != null)
                return prestation.ProduitMutuel;

            if (prestation.ProduitAssureur != null)
                return prestation.ProduitAssureur;

            throw new ArgumentException(
                $"La prestation {prestationId} n'est liée à aucun produit mutuel ou assureur.");
        }

        public static async Task ValidateAchatProduitAsync(
            ProsocDbContext db,
            int affilieId,
            int prestationId,
            CancellationToken ct = default,
            DateTime? dateNaissanceOverride = null,
            int? typeAdhesionIdOverride = null,
            bool cotisationPayeeDansLot = false,
            bool nouvelleAdhesionNiveau1 = false)
        {
            var produit = await ResolveProduitFromPrestationAsync(db, prestationId, ct);

            if (!produit.Statut)
                throw new ArgumentException($"Le produit « {produit.Nom} » est inactif.");

            DateTime dateNaissance;
            if (dateNaissanceOverride.HasValue)
            {
                dateNaissance = dateNaissanceOverride.Value;
            }
            else
            {
                var affilie = await db.Affilies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.IdAffilie == affilieId, ct);

                if (affilie == null || !affilie.Statut)
                    throw new ArgumentException("Affilié introuvable ou inactif.");

                dateNaissance = affilie.DateNaissance;
            }

            ValidateAgeEligibilite(dateNaissance, produit);

            if (affilieId > 0)
            {
                await ValidateCotisationAJourAsync(
                    db, affilieId, ct, typeAdhesionIdOverride, cotisationPayeeDansLot);
            }
            else if (!cotisationPayeeDansLot && !nouvelleAdhesionNiveau1)
            {
                throw new ArgumentException(
                    "La cotisation de la période en cours doit être incluse dans la demande avant les produits.");
            }
        }

        public static async Task ValidateAchatProduitBySouscriptionAsync(
            ProsocDbContext db,
            int affilieId,
            int souscriptionPrestationId,
            CancellationToken ct = default,
            bool cotisationPayeeDansLot = false,
            bool nouvelleAdhesionNiveau1 = false)
        {
            var souscription = await db.SouscriptionsPrestations
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IdSouscriptionPrestation == souscriptionPrestationId, ct);

            if (souscription == null)
                throw new ArgumentException($"Souscription {souscriptionPrestationId} introuvable.");

            if (souscription.AffilieId != affilieId)
                throw new ArgumentException("Cette souscription n'appartient pas à l'affilié indiqué.");

            await ValidateAchatProduitAsync(
                db,
                affilieId,
                souscription.PrestationId,
                ct,
                cotisationPayeeDansLot: cotisationPayeeDansLot,
                nouvelleAdhesionNiveau1: nouvelleAdhesionNiveau1);
        }

        private static bool EstCollectePayee(string? statutPaiement) =>
            string.IsNullOrWhiteSpace(statutPaiement)
            || CollecteStatutPaiementRegles.EstValide(statutPaiement);
    }
}
