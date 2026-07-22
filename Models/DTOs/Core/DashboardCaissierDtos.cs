namespace ProsocAPI.Models.DTOs.Core
{
    public class CaissierKpisDto
    {
        /// <summary>Montant collecté aujourd'hui, consolidé en devise principale.</summary>
        public decimal MontantDuJour { get; set; }
        /// <summary>Montant collecté sur 7 jours, consolidé en devise principale.</summary>
        public decimal MontantSemaine { get; set; }
        /// <summary>Montant collecté sur 30 jours, consolidé en devise principale.</summary>
        public decimal MontantMois { get; set; }
        public int NombreCollectesDuJour { get; set; }
        public int NombreCollectesMois { get; set; }
        /// <summary>Moyenne par collecte (30 jours), en devise principale.</summary>
        public decimal MontantMoyen { get; set; }
        public decimal TauxSucces { get; set; }
        public int NombreAdhesionsDuJour { get; set; }
        public int NombreSortiesDuJour { get; set; }
        public decimal MontantSortiesDuJour { get; set; }
        /// <summary>Code ISO de la devise principale (ex. USD).</summary>
        public string? DevisePrincipaleCode { get; set; }
    }

    public class CaissierCollecteDto
    {
        public int IdCollecte { get; set; }
        public DateTime DateCollecte { get; set; }
        public decimal Montant { get; set; }
        public string TypeCollecte { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string NomAffilie { get; set; } = string.Empty;
        public string ModePaiement { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    /// <summary>Filtres pour l'historique paginé des collectes guichet (opérateur connecté).</summary>
    public class GuichetCollecteHistoriqueFiltreDto
    {
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public string? ModePaiement { get; set; }
    }

    public class CaissierRepartitionDto
    {
        public string Libelle { get; set; } = string.Empty;
        public decimal Montant { get; set; }
        public int Nombre { get; set; }
        public decimal Pourcentage { get; set; }
    }

    public class CaissierAdhesionDuJourDto
    {
        public int IdAdhesion { get; set; }
        public int AffilieId { get; set; }
        public string NomAffilie { get; set; } = string.Empty;
        public string StatutDossier { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; }
    }

    public class DashboardCaissierDto
    {
        public CaissierKpisDto Kpis { get; set; } = new();
        public List<CaissierCollecteDto> CollectesRecentes { get; set; } = new();
        public List<CaissierRepartitionDto> RepartitionParType { get; set; } = new();
        public List<CaissierRepartitionDto> RepartitionParMode { get; set; } = new();
        public List<CaissierAdhesionDuJourDto> AdhesionsDuJour { get; set; } = new();
        public DateTime DerniereMiseAJour { get; set; }
    }
}
