using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public static class ArrieresAffilieRules
    {
        public static string NormalizePeriodicite(string periodicite)
        {
            if (string.IsNullOrWhiteSpace(periodicite))
                return "Mensuel";

            var value = periodicite.Trim();
            if (value.Equals("Ponctuel", StringComparison.OrdinalIgnoreCase))
                return "Ponctuel";
            if (value.Equals("Annuel", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Annuelle", StringComparison.OrdinalIgnoreCase))
                return "Annuel";

            return "Mensuel";
        }

        public static bool EstPeriodiciteRecurrente(string periodicite)
        {
            var normalisee = NormalizePeriodicite(periodicite);
            return normalisee is "Mensuel" or "Annuel";
        }

        public static bool DoitGenererPourDate(string periodicite, DateTime dateReference, int jourEcheanceMensuelle = 1)
        {
            var normalisee = NormalizePeriodicite(periodicite);
            if (normalisee == "Ponctuel")
                return false;

            if (normalisee == "Annuel")
                return dateReference.Month == 1 && dateReference.Day == Math.Clamp(jourEcheanceMensuelle, 1, 28);

            return dateReference.Day == Math.Clamp(jourEcheanceMensuelle, 1, 28);
        }

        public static (int Mois, int Annee) GetPeriodeComptable(string periodicite, DateTime dateReference)
        {
            var normalisee = NormalizePeriodicite(periodicite);
            if (normalisee == "Annuel")
                return (1, dateReference.Year);

            return (dateReference.Month, dateReference.Year);
        }

        public static DateTime CalculerDateEcheance(string periodicite, int mois, int annee, int jourEcheanceMensuelle = 1)
        {
            var normalisee = NormalizePeriodicite(periodicite);
            var jour = Math.Clamp(jourEcheanceMensuelle, 1, 28);

            if (normalisee == "Annuel")
                return new DateTime(annee, 1, jour);

            var dernierJour = DateTime.DaysInMonth(annee, mois);
            return new DateTime(annee, mois, Math.Min(jour, dernierJour));
        }

        public static string CalculerStatutPaiement(decimal montantAttendu, decimal montantPaye, string statutCourant)
        {
            if (montantAttendu <= 0 || montantPaye >= montantAttendu)
                return ArrieresAffilieStatuts.Paye;

            if (montantPaye > 0)
                return ArrieresAffilieStatuts.PartiellementPaye;

            if (statutCourant == ArrieresAffilieStatuts.EnRetard)
                return ArrieresAffilieStatuts.EnRetard;

            return ArrieresAffilieStatuts.EnAttente;
        }

        public static void AppliquerPaiement(ArrieresAffilie arriere, decimal montant)
        {
            arriere.MontantPaye += montant;
            arriere.RestAPayer = Math.Max(0, arriere.MontantAttendu - arriere.MontantPaye);
            arriere.DateModification = DateTime.Now;
            arriere.DateDernierPaiement = DateTime.Now;
            arriere.StatutPaiement = CalculerStatutPaiement(
                arriere.MontantAttendu,
                arriere.MontantPaye,
                arriere.StatutPaiement);
        }

        public static bool AdhesionEstValidee(Adhesion adhesion)
        {
            return adhesion.Statut &&
                   string.Equals(
                       adhesion.StatutDossier?.Trim(),
                       AdhesionNiveau2Regles.StatutValide,
                       StringComparison.OrdinalIgnoreCase);
        }

        public static string NormalizePeriodiciteFrais(string periodicite)
        {
            if (string.IsNullOrWhiteSpace(periodicite))
                return "Ponctuel";

            var value = periodicite.Trim();
            if (value.Equals("Mensuel", StringComparison.OrdinalIgnoreCase))
                return "Mensuel";
            if (value.Equals("Annuel", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Annuelle", StringComparison.OrdinalIgnoreCase))
                return "Annuel";

            return "Ponctuel";
        }
    }
}
