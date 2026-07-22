namespace ProsocAPI.Models.DTOs.DashboardAgentAa
{
    public class AgentAaKpisDto
    {
        public int TotalDossiers { get; set; }
        public int DossiersEnAttente { get; set; }
        public int DossiersValides { get; set; }
        public int DossiersValidesMois { get; set; }
        public decimal TauxCompletion { get; set; }
        public int TotalDependants { get; set; }
        public int DependantsAjoutesMois { get; set; }
        public int TotalAntecedents { get; set; }
        public int AntecedentsAjoutesMois { get; set; }
        public int DemandesBonEnAttente { get; set; }
        /// <summary>Collectes du mois sur les affiliés affectés, consolidées en devise principale.</summary>
        public decimal TotalCollectesMois { get; set; }
        /// <summary>Commissions wallet du mois (COMM_COLLECTE), consolidées en devise principale.</summary>
        public decimal TotalCommissionsMois { get; set; }
        /// <summary>Code ISO de la devise principale (ex. USD).</summary>
        public string? DevisePrincipaleCode { get; set; }
    }

    public class AgentAaDossierDto
    {
        public int IdAdhesion { get; set; }
        public int IdAffilie { get; set; }
        public string CodeAdhesion { get; set; } = string.Empty;
        public string NomComplet { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string StatutDossier { get; set; } = string.Empty;
        public string TypeAdhesion { get; set; } = string.Empty;
        public DateTime DateAdhesion { get; set; }
        public DateTime? DateModification { get; set; }
        public int NombreDependants { get; set; }
        public int NombreAntecedents { get; set; }
        public bool EstValide { get; set; }
    }

    public class AgentAaDependantRecentDto
    {
        public int IdDependant { get; set; }
        public int AffilieId { get; set; }
        public string AffilieNomComplet { get; set; } = string.Empty;
        public string CodeAdhesion { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string LienParente { get; set; } = string.Empty;
        public DateTime? DateNaissance { get; set; }
        public DateTime DateCreation { get; set; }
    }

    public class AgentAaAntecedentRecentDto
    {
        public int IdAntecedant { get; set; }
        public int AffilieId { get; set; }
        public string AffilieNomComplet { get; set; } = string.Empty;
        public string CodeAdhesion { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; }
    }

    public class AgentAaRepartitionStatutDto
    {
        public string StatutDossier { get; set; } = string.Empty;
        public int Nombre { get; set; }
    }

    public class DashboardAgentAaDto
    {
        public int AgentId { get; set; }
        public string NomAgent { get; set; } = string.Empty;
        public AgentAaKpisDto Kpis { get; set; } = new();
        public List<AgentAaRepartitionStatutDto> RepartitionStatuts { get; set; } = new();
        public List<AgentAaDossierDto> DossiersATraiter { get; set; } = new();
        public List<AgentAaDependantRecentDto> DependantsRecents { get; set; } = new();
        public List<AgentAaAntecedentRecentDto> AntecedentsRecents { get; set; } = new();
        public DateTime DerniereMiseAJour { get; set; }
        /// <summary>Code ISO de la devise principale (ex. USD) pour les montants consolidés.</summary>
        public string? DevisePrincipaleCode { get; set; }
    }
}
