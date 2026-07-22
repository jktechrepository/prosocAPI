using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    // KPIs principaux pour l'affilié
    public class AffilieKpisDto
    {
        public int IdAffilie { get; set; }
        public string CodeAdhesion { get; set; } = string.Empty;
        public string NomComplet { get; set; } = string.Empty;
        public decimal SoldeTotal { get; set; }
        public decimal SoldeDisponible { get; set; }
        public decimal TotalCotisations { get; set; }
        public decimal TotalPrestations { get; set; }
        /// <summary>Code ISO de la devise principale (ex. USD) pour les montants consolidés.</summary>
        public string? DevisePrincipaleCode { get; set; }
        public int NombrePrestations { get; set; }
        public decimal MontantDerniereCotisation { get; set; }
        public DateTime? DateDerniereCotisation { get; set; }
        public decimal MontantDernierePrestation { get; set; }
        public DateTime? DateDernierePrestation { get; set; }
        public string StatutAdhesion { get; set; } = string.Empty;
        public DateTime DateAdhesion { get; set; }
        public int AncienneteMois { get; set; }
        public decimal TauxUtilisation { get; set; }
        public decimal TauxCouverture { get; set; }
        public bool EstActif { get; set; }
        public int NombreBeneficiaires { get; set; }
        public decimal MontantPlafond { get; set; }
        public decimal RestePlafond { get; set; }
        public string StatutGlobal { get; set; } = AffilieConformiteStatuts.EnOrdre;
        public string StatutCotisation { get; set; } = AffilieConformiteStatuts.EnOrdre;
        public string StatutPrestation { get; set; } = AffilieConformiteStatuts.EnOrdre;
        public int NombreArrieresOuverts { get; set; }
        public decimal MontantRestantDu { get; set; }
    }

    // Informations de base de l'affilié
    public class AffilieInfoDto
    {
        public int IdAffilie { get; set; }
        public string CodeAdhesion { get; set; } = string.Empty;
        public string NomComplet { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateNaissance { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTime DateAdhesion { get; set; }
        public string StatutAdhesion { get; set; } = string.Empty;
        public bool EstActif { get; set; }
        public string? ProvinceResidence { get; set; }
        public string? CommuneResidence { get; set; }
        public string? QuartierResidence { get; set; }
        public string? AvenueResidence { get; set; }
        public string? NumeroResidence { get; set; }
        public string? CommuneActivite { get; set; }
        public string? QuartierActivite { get; set; }
        public string? AvenueActivite { get; set; }
        public string? NumeroActivite { get; set; }
        public int NombreBeneficiaires { get; set; }
        public string TypeAdhesion { get; set; } = string.Empty;
        public string CategorieAdhesion { get; set; } = string.Empty;
    }

    // Historique des cotisations
    public class AffilieCotisationDto
    {
        public int IdCotisation { get; set; }
        public decimal Montant { get; set; }
        public DateTime DateCotisation { get; set; }
        public string TypeCotisation { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public string Statut { get; set; } = string.Empty;
        public string? AgentCollecteur { get; set; }
        public string? ModePaiement { get; set; }
        public string? Periodicite { get; set; }
        public decimal CumulMois { get; set; }
        public decimal CumulAnnee { get; set; }
        public bool EstEnRetard { get; set; }
        public int JoursRetard { get; set; }
    }

    // Historique des prestations
    public class AffiliePrestationDto
    {
        public int IdPrestation { get; set; }
        public decimal MontantTotal { get; set; }
        public decimal MontantRembourse { get; set; }
        public decimal MontantPriseEnCharge { get; set; }
        public decimal TauxRemboursement { get; set; }
        public DateTime DatePrestation { get; set; }
        public DateTime DateDemande { get; set; }
        public DateTime? DateRemboursement { get; set; }
        public string TypePrestation { get; set; } = string.Empty;
        public string? PrestationNom { get; set; }
        public string Statut { get; set; } = string.Empty;
        public string? Beneficiaire { get; set; }
        public string? StructureSante { get; set; }
        public string? ReferenceFacture { get; set; }
        public string? MedecinTraitant { get; set; }
        public int DelaiTraitementJours { get; set; }
        public bool EstUrgent { get; set; }
        public decimal FranchiseAppliquee { get; set; }
        public decimal PlafondDepasse { get; set; }
        public string? MotifRejet { get; set; }
    }

    // Bénéficiaires de l'affilié
    public class AffilieBeneficiaireDto
    {
        public int IdBeneficiaire { get; set; }
        public int IdAffilie { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string LienParente { get; set; } = string.Empty;
        public DateTime DateNaissance { get; set; }
        public string TypeBeneficiaire { get; set; } = string.Empty;
        public bool EstActif { get; set; }
        public DateTime DateAjout { get; set; }
        public string? NumeroCNI { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public decimal PlafondIndividuel { get; set; }
        public decimal UtiliseAnnee { get; set; }
        public decimal ResteDisponible { get; set; }
        public int Age { get; set; }
        public bool EstPrincipal { get; set; }
    }

    // Graphiques et statistiques
    public class AffilieGraphsDto
    {
        public List<AffilieCotisationMensuelleDto> CotisationsMensuelles { get; set; } = new();
        public List<AffiliePrestationMensuelleDto> PrestationsMensuelles { get; set; } = new();
        public List<AffilieEvolutionSoldeDto> EvolutionSolde { get; set; } = new();
        public List<AffilieRepartitionPrestationsDto> RepartitionPrestations { get; set; } = new();
        public List<AffilieTauxUtilisationDto> TauxUtilisationMensuel { get; set; } = new();
        public AffilieResumeAnnuelDto ResumeAnnuel { get; set; } = new();
    }

    // Cotisations mensuelles pour graphique
    public class AffilieCotisationMensuelleDto
    {
        public int Mois { get; set; }
        public int Annee { get; set; }
        public string MoisAnnee { get; set; } = string.Empty;
        public decimal MontantCotise { get; set; }
        public decimal ObjectifCotisation { get; set; }
        public decimal TauxRealisation { get; set; }
        public int NombreCotisations { get; set; }
        public decimal MoyenneCotisation { get; set; }
        public decimal CumulAnnee { get; set; }
    }

    // Prestations mensuelles pour graphique
    public class AffiliePrestationMensuelleDto
    {
        public int Mois { get; set; }
        public int Annee { get; set; }
        public string MoisAnnee { get; set; } = string.Empty;
        public decimal MontantTotalPrestations { get; set; }
        public decimal MontantRembourse { get; set; }
        public int NombrePrestations { get; set; }
        public decimal TauxRemboursementMoyen { get; set; }
        public decimal MoyennePrestation { get; set; }
        public int PrestationsUrgentes { get; set; }
        public decimal CumulAnnee { get; set; }
    }

    // Évolution du solde
    public class AffilieEvolutionSoldeDto
    {
        public DateTime Date { get; set; }
        public decimal SoldeApresOperation { get; set; }
        public decimal Variation { get; set; }
        public decimal VariationPourcentage { get; set; }
        public string TypeOperation { get; set; } = string.Empty;
        public string? DescriptionOperation { get; set; }
        public decimal CumulCotisations { get; set; }
        public decimal CumulPrestations { get; set; }
    }

    // Répartition des prestations par type
    public class AffilieRepartitionPrestationsDto
    {
        public string TypePrestation { get; set; } = string.Empty;
        public decimal MontantTotal { get; set; }
        public int NombrePrestations { get; set; }
        public decimal PourcentageTotal { get; set; }
        public decimal MontantMoyen { get; set; }
        public decimal TauxRemboursementMoyen { get; set; }
        public string? CouleurGraphique { get; set; }
    }

    // Taux d'utilisation mensuel
    public class AffilieTauxUtilisationDto
    {
        public int Mois { get; set; }
        public int Annee { get; set; }
        public string MoisAnnee { get; set; } = string.Empty;
        public decimal TauxUtilisation { get; set; }
        public decimal MontantUtilise { get; set; }
        public decimal PlafondDisponible { get; set; }
        public decimal TauxCouverture { get; set; }
        public int NombreBeneficiairesActifs { get; set; }
    }

    // Résumé annuel
    public class AffilieResumeAnnuelDto
    {
        public int Annee { get; set; }
        public decimal TotalCotisations { get; set; }
        public decimal TotalPrestations { get; set; }
        public decimal SoldeFinAnnee { get; set; }
        public decimal SoldeDebutAnnee { get; set; }
        public decimal VariationAnnuelle { get; set; }
        public decimal VariationPourcentage { get; set; }
        public int TotalCotisationsEffectuees { get; set; }
        public int TotalPrestationsRecues { get; set; }
        public decimal TauxUtilisationMoyen { get; set; }
        public decimal TauxRemboursementMoyen { get; set; }
        public decimal MeilleurMoisCotisation { get; set; }
        public decimal MeilleurMoisPrestation { get; set; }
        public int JoursCouvertureMoyenne { get; set; }
        public decimal SatisfactionGlobale { get; set; }
    }

    // Notifications et alertes
    public class AffilieNotificationDto
    {
        public int IdNotification { get; set; }
        public string TypeNotification { get; set; } = string.Empty;
        public string Titre { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime DateNotification { get; set; }
        public bool EstLue { get; set; }
        public DateTime? DateLecture { get; set; }
        public string? Priorite { get; set; }
        public string? Categorie { get; set; }
        public bool EstActionRequise { get; set; }
        public string? UrlAction { get; set; }
        public int IdAffilie { get; set; }
        public string CodeAdhesion { get; set; } = string.Empty;
        public string NomAffilie { get; set; } = string.Empty;
    }

    // Documents de l'affilié
    public class AffilieDocumentDto
    {
        public int IdDocument { get; set; }
        public string TypeDocument { get; set; } = string.Empty;
        public string NomDocument { get; set; } = string.Empty;
        public string UrlDocument { get; set; } = string.Empty;
        public DateTime DateUpload { get; set; }
        public string? Extension { get; set; }
        public long TailleOctets { get; set; }
        public string TailleAffichee { get; set; } = string.Empty;
        public bool EstValide { get; set; }
        public DateTime? DateValidation { get; set; }
        public string? Validateur { get; set; }
        public string? MotifRejet { get; set; }
        public int IdAffilie { get; set; }
        public string CodeAdhesion { get; set; } = string.Empty;
        public bool EstObligatoire { get; set; }
        public DateTime? DateExpiration { get; set; }
        public int JoursAvantExpiration { get; set; }
    }

    // Paramètres et préférences
    public class AffiliePreferencesDto
    {
        public int IdAffilie { get; set; }
        public string CodeAdhesion { get; set; } = string.Empty;
        public bool NotificationsEmail { get; set; }
        public bool NotificationsSMS { get; set; }
        public string LanguePreferee { get; set; } = "fr";
        public string FuseauHoraire { get; set; } = "UTC";
        public bool RecevoirRappelsCotisation { get; set; }
        public bool RecevoirAlertesPrestation { get; set; }
        public bool RecevoirNewsletter { get; set; }
        public int FrequenceRappelsJours { get; set; }
        public string? EmailSecondaire { get; set; }
        public string? TelephoneSecondaire { get; set; }
        public bool ModeSombre { get; set; }
        public string FormatRapports { get; set; } = "PDF";
        public bool PartagerDonneesStatistiques { get; set; }
        public DateTime DerniereMiseAJour { get; set; }
    }

    // Résumé pour le dashboard principal
    public class AffilieDashboardResumeDto
    {
        public AffilieKpisDto Kpis { get; set; } = new();
        public AffilieInfoDto Informations { get; set; } = new();
        public List<AffilieNotificationDto> NotificationsRecentes { get; set; } = new();
        public List<AffilieCotisationDto> CotisationsRecentes { get; set; } = new();
        public List<AffiliePrestationDto> PrestationsRecentes { get; set; } = new();
        public List<AffilieBeneficiaireDto> Beneficiaires { get; set; } = new();
        public AffilieGraphsDto Graphiques { get; set; } = new();
        public List<AffilieDocumentDto> DocumentsEnAttente { get; set; } = new();
        public AffiliePreferencesDto Preferences { get; set; } = new();
    }
}
