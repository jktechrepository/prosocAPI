namespace ProsocAPI.Models.DTOs.Core
{
    public class ChefEquipeKpisDto
    {
        public int ZoneSocialeId { get; set; }
        public string? ZoneSocialeNom { get; set; }
        public int NombreAgentsAt { get; set; }
        public int CollectesMoisZone { get; set; }
        public decimal TotalCollectesMoisZone { get; set; }
        public string? DevisePrincipaleCode { get; set; }
        public int CollectesEnAttenteZone { get; set; }
        public int TransactionsValidesMoisZone { get; set; }
    }

    public class ChefEquipeAgentResumeDto
    {
        public int AgentId { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Matricule { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public bool Statut { get; set; }
        public int CollectesMois { get; set; }
        public decimal TotalCollectesMois { get; set; }
        public int CollectesEnAttente { get; set; }
    }

    public class ChefEquipeCollecteResumeDto
    {
        public int IdCollecte { get; set; }
        public int AgentId { get; set; }
        public string? AgentNom { get; set; }
        public string? AffilieNom { get; set; }
        public decimal Montant { get; set; }
        public string? StatutPaiement { get; set; }
        public string? ModePaiement { get; set; }
        public DateTime DateCollecte { get; set; }
    }
}
