namespace ProsocAPI.Models.DTOs.DashboardAgentHopital
{
    public class HopitalKpisDto
    {
        public int JetonsEmisTotal { get; set; }
        public int JetonsEmisMois { get; set; }
        public int JetonsUtilisesMois { get; set; }
        public int JetonsValidesEnAttente { get; set; }
        public int JetonsExpires { get; set; }
        public int BonsLiesTotal { get; set; }
        public int BonsUtilisesMois { get; set; }
        public int PatientsUniques { get; set; }
        public int TotalDependants { get; set; }
        public int TotalAntecedents { get; set; }
        /// <summary>Valeur catalogue des prestations liées aux jetons émis (toutes périodes), consolidée en devise principale.</summary>
        public decimal ValeurPrestationsJetonsTotal { get; set; }
        /// <summary>Valeur des prestations liées aux jetons émis ce mois.</summary>
        public decimal ValeurPrestationsJetonsMois { get; set; }
        /// <summary>Valeur des prestations liées aux jetons utilisés ce mois.</summary>
        public decimal ValeurPrestationsJetonsUtilisesMois { get; set; }
        /// <summary>Valeur catalogue des prestations des bons liés à l'hôpital (toutes périodes).</summary>
        public decimal ValeurPrestationsBonsTotal { get; set; }
        /// <summary>Valeur des prestations des bons émis ce mois.</summary>
        public decimal ValeurPrestationsBonsMois { get; set; }
        /// <summary>Valeur des prestations des bons utilisés ce mois.</summary>
        public decimal ValeurPrestationsBonsUtilisesMois { get; set; }
        /// <summary>Code ISO de la devise principale (ex. USD).</summary>
        public string? DevisePrincipaleCode { get; set; }
    }

    public class HopitalJetonEnAttenteDto
    {
        public int IdJeton { get; set; }
        public string CodeJeton { get; set; } = string.Empty;
        public int AffilieId { get; set; }
        public string AffilieNomComplet { get; set; } = string.Empty;
        public string CodeAdhesion { get; set; } = string.Empty;
        public string? NomPrestation { get; set; }
        /// <summary>Montant catalogue de la prestation liée, consolidé en devise principale.</summary>
        public decimal? MontantPrestation { get; set; }
        public DateTime DateEmission { get; set; }
        public DateTime? DateExpiration { get; set; }
    }

    public class HopitalBonRecentDto
    {
        public int IdBonEnvoi { get; set; }
        public string NumeroBon { get; set; } = string.Empty;
        public int AffilieId { get; set; }
        public string AffilieNomComplet { get; set; } = string.Empty;
        public string CodeAdhesion { get; set; } = string.Empty;
        public string NomPrestation { get; set; } = string.Empty;
        /// <summary>Montant catalogue de la prestation, consolidé en devise principale.</summary>
        public decimal MontantPrestation { get; set; }
        public DateTime DateEmission { get; set; }
        public bool EstUtilise { get; set; }
        public DateTime? DateUtilisation { get; set; }
    }

    public class HopitalPatientDto
    {
        public int IdAffilie { get; set; }
        public string CodeAdhesion { get; set; } = string.Empty;
        public string NomComplet { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public DateTime DateNaissance { get; set; }
        public int NombreDependants { get; set; }
        public int NombreAntecedents { get; set; }
        public int NombreJetons { get; set; }
        public DateTime? DernierJetonEmission { get; set; }
    }

    public class HopitalDependantDto
    {
        public int IdDependant { get; set; }
        public int AffilieId { get; set; }
        public string AffilieNomComplet { get; set; } = string.Empty;
        public string CodeAdhesion { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string LienParente { get; set; } = string.Empty;
        public DateTime? DateNaissance { get; set; }
        public string? Telephone { get; set; }
    }

    public class HopitalAntecedentDto
    {
        public int IdAntecedant { get; set; }
        public int AffilieId { get; set; }
        public string AffilieNomComplet { get; set; } = string.Empty;
        public string CodeAdhesion { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; }
    }

    public class HopitalRepartitionPrestationDto
    {
        public int PrestationId { get; set; }
        public string NomPrestation { get; set; } = string.Empty;
        public int NombreJetons { get; set; }
        public int NombreBons { get; set; }
        /// <summary>Somme consolidée des montants prestation pour les jetons de cette prestation.</summary>
        public decimal MontantTotalJetons { get; set; }
        /// <summary>Somme consolidée des montants prestation pour les bons de cette prestation.</summary>
        public decimal MontantTotalBons { get; set; }
    }

    public class DashboardAgentHopitalDto
    {
        public string NomHopital { get; set; } = string.Empty;
        public HopitalKpisDto Kpis { get; set; } = new();
        public List<HopitalRepartitionPrestationDto> RepartitionPrestations { get; set; } = new();
        public List<HopitalJetonEnAttenteDto> JetonsEnAttente { get; set; } = new();
        public List<HopitalBonRecentDto> BonsRecents { get; set; } = new();
        public List<HopitalPatientDto> Patients { get; set; } = new();
        public List<HopitalDependantDto> Dependants { get; set; } = new();
        public List<HopitalAntecedentDto> Antecedents { get; set; } = new();
        public DateTime DerniereMiseAJour { get; set; }
        /// <summary>Code ISO de la devise principale (ex. USD) pour les montants consolidés.</summary>
        public string? DevisePrincipaleCode { get; set; }
    }
}
