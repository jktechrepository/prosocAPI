namespace ProsocAPI.Models.DTOs.DashboardAssureur
{
    public class AssureurKpisDto
    {
        public int NombreAffilies { get; set; }
        public int NombreDependants { get; set; }
        public int NombreAntecedents { get; set; }
        public int NombreProduitsActifs { get; set; }
        public int NombreSouscriptionsActives { get; set; }
        public int NouvellesSouscriptionsMois { get; set; }
        public decimal MontantCollectesMois { get; set; }
        public int BonsEmisMois { get; set; }
        public int BonsUtilisesMois { get; set; }
        public int DemandesBonEnAttente { get; set; }
    }

    public class AssureurAffilieDto
    {
        public int IdAffilie { get; set; }
        public string CodeAdhesion { get; set; } = string.Empty;
        public string NomComplet { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public DateTime DateNaissance { get; set; }
        public bool Statut { get; set; }
        public int NombreDependants { get; set; }
        public int NombreAntecedents { get; set; }
        public int NombreSouscriptionsActives { get; set; }
    }

    public class AssureurDependantDto
    {
        public int IdDependant { get; set; }
        public int AffilieId { get; set; }
        public string AffilieNomComplet { get; set; } = string.Empty;
        public string CodeAdhesion { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string LienParente { get; set; } = string.Empty;
        public DateTime? DateNaissance { get; set; }
        public string? Telephone { get; set; }
        public bool Statut { get; set; }
    }

    public class AssureurAntecedentDto
    {
        public int IdAntecedant { get; set; }
        public int AffilieId { get; set; }
        public string AffilieNomComplet { get; set; } = string.Empty;
        public string CodeAdhesion { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; }
        public bool Statut { get; set; }
    }

    public class AssureurRepartitionProduitDto
    {
        public int ProduitAssureurId { get; set; }
        public string NomProduit { get; set; } = string.Empty;
        public int NombreSouscriptions { get; set; }
        public decimal MontantCollecteMois { get; set; }
    }

    public class DashboardAssureurDto
    {
        public string NomAssureur { get; set; } = string.Empty;
        public AssureurKpisDto Kpis { get; set; } = new();
        public List<AssureurRepartitionProduitDto> RepartitionProduits { get; set; } = new();
        public List<AssureurAffilieDto> AffiliesRecents { get; set; } = new();
        public List<AssureurDependantDto> Dependants { get; set; } = new();
        public List<AssureurAntecedentDto> Antecedents { get; set; } = new();
        public DateTime DerniereMiseAJour { get; set; }
    }
}
