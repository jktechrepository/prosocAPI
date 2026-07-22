using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public static class ProduitTarifRules
    {
        public static async Task ValidateReferencesAsync(
            ProsocDbContext db,
            int deviseId,
            int? assureurId = null,
            CancellationToken ct = default)
        {
            var deviseExists = await db.Devises
                .AnyAsync(d => d.IdDevise == deviseId && d.Statut, ct);

            if (!deviseExists)
                throw new ArgumentException("La devise spécifiée est introuvable ou inactive.");

            if (!assureurId.HasValue)
                return;

            var assureurExists = await db.Assureurs
                .AnyAsync(a => a.IdAssureur == assureurId.Value && a.Statut, ct);

            if (!assureurExists)
                throw new ArgumentException("L'assureur spécifié est introuvable ou inactif.");
        }

        public static void PrepareForSave(ProduitBase produit)
        {
            produit.Periodicite = NormalizePeriodicite(produit.Periodicite);
            ValidateTrancheAge(produit.AgeMin, produit.AgeMax);
            ValidateGratuitPayant(produit);
            ValidateTauxCommission(produit.TauxCommissionAT, "TauxCommissionAT");
            ValidateTauxCommission(produit.TauxCommissionAA, "TauxCommissionAA");
            ValidateTauxCommission(produit.TauxCommissionAAMash, "TauxCommissionAAMash");
            ValidateTauxCommission(produit.TauxCommissionAAStructure, "TauxCommissionAAStructure");
        }

        public static void ValidateGratuitPayant(ProduitBase produit)
        {
            if (produit.EstGratuit)
            {
                if (produit.Montant != 0)
                    throw new ArgumentException(
                        "Un produit gratuit (inclus dans la cotisation) doit avoir un montant de 0.");

                produit.TauxCommissionAT = 0;
                produit.TauxCommissionAA = 0;
                produit.TauxCommissionAAMash = 0;
                produit.TauxCommissionAAStructure = 0;
                return;
            }

            ValidateMontant(produit.Montant);
        }

        public static void CopyTarifFields(ProduitBase target, ProduitBase source)
        {
            target.Nom = source.Nom;
            target.Montant = source.Montant;
            target.Periodicite = source.Periodicite;
            target.AgeMin = source.AgeMin;
            target.AgeMax = source.AgeMax;
            target.EstGratuit = source.EstGratuit;
            target.Statut = source.Statut;
            CopyCommissionFields(target, source);
        }

        public static void CopyCommissionFields(ProduitBase target, ProduitBase source)
        {
            target.TauxCommissionAT = source.TauxCommissionAT;
            target.TauxCommissionAA = source.TauxCommissionAA;
            target.TauxCommissionAAMash = source.TauxCommissionAAMash;
            target.TauxCommissionAAStructure = source.TauxCommissionAAStructure;
        }

        public static void ValidateTauxCommission(decimal taux, string nomChamp)
        {
            if (taux < 0 || taux > 100)
                throw new ArgumentException($"Le taux {nomChamp} doit être compris entre 0 et 100 %.");
        }

        public static string BuildPrestationDescription(ProduitBase produit, string typeLibelle)
        {
            var mode = produit.EstGratuit ? "Gratuit (cotisation)" : "Payant";
            return $"Prestation dérivée du produit {typeLibelle}: {produit.Nom} - " +
                   $"[{mode}] Montant: {produit.Montant} ({produit.Periodicite}) - Éligibilité: {produit.AgeMin}-{produit.AgeMax} ans";
        }

        public static string NormalizePeriodicite(string periodicite)
        {
            return CotisationAffilieService.NormalizePeriodicite(periodicite);
        }

        public static void ValidateTrancheAge(int ageMin, int ageMax)
        {
            if (ageMin < 0)
                throw new ArgumentException("L'âge minimum ne peut pas être négatif.");

            if (ageMax < ageMin)
                throw new ArgumentException($"L'âge maximum ({ageMax}) doit être supérieur ou égal à l'âge minimum ({ageMin}).");

            if (ageMax > 120)
                throw new ArgumentException("L'âge maximum ne peut pas dépasser 120 ans.");
        }

        public static void ValidateMontant(decimal montant)
        {
            if (montant <= 0)
                throw new ArgumentException("Le montant du produit doit être supérieur à zéro.");
        }
    }
}
