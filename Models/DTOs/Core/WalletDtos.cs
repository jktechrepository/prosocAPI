using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class WalletAgentReadDto
    {
        public int IdWalletAgent { get; set; }
        public int AgentId { get; set; }
        public int DeviseId { get; set; }
        public string? DeviseCode { get; set; }
        public string? DeviseNom { get; set; }
        public string? DeviseSymbole { get; set; }
        public decimal SoldeCourant { get; set; }
        public decimal SoldeDisponible { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }
        
        // Informations sur l'agent associé
        public string? AgentNom { get; set; }
        public string? AgentMatricule { get; set; }
    }

    public class WalletAgentUpdateDto
    {
        [Range(1, int.MaxValue)]
        public int? DeviseId { get; set; }

        public decimal SoldeCourant { get; set; }
        public bool Statut { get; set; }
    }

    public class WalletAgentCreateDto
    {
        [Required]
        public int AgentId { get; set; }

        [Range(1, int.MaxValue)]
        public int DeviseId { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal SoldeInitial { get; set; }
        
        public bool Statut { get; set; } = true;
    }

    public class WalletVirtuelAgentReadDto
    {
        public int IdWalletVirtuelAgent { get; set; }
        public int AgentId { get; set; }
        public int DeviseId { get; set; }
        public string? DeviseCode { get; set; }
        public string? DeviseNom { get; set; }
        public string? DeviseSymbole { get; set; }
        public decimal SoldeVirtuel { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }
        
        // Informations sur l'agent associé
        public string? AgentNom { get; set; }
        public string? AgentMatricule { get; set; }
    }

    public class WalletVirtuelAgentUpdateDto
    {
        public decimal SoldeVirtuel { get; set; }
        public bool Statut { get; set; }
    }

    public class WalletVirtuelAgentCreateDto
    {
        [Required]
        public int AgentId { get; set; }

        [Range(1, int.MaxValue)]
        public int? DeviseId { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal SoldeInitial { get; set; }
        
        public bool Statut { get; set; } = true;
    }

    public class WalletVirtuelAgentAjouterSoldeDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0")]
        public decimal Montant { get; set; }

        [StringLength(500)]
        public string? Observation { get; set; }
    }

    public class WalletVirtuelAgentAjouterSoldeResultDto
    {
        public WalletVirtuelAgentReadDto Wallet { get; set; } = new();
        public decimal AncienSolde { get; set; }
        public decimal MontantAjoute { get; set; }
        public decimal NouveauSolde { get; set; }
    }

    public class WalletVirtuelAgentModifierItemDto
    {
        [Range(1, int.MaxValue)]
        public int AgentId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SoldeVirtuel { get; set; }
    }

    public class WalletVirtuelAgentModifierResultDto
    {
        public int TotalDemandes { get; set; }
        public int TotalReussites { get; set; }
        public int TotalEchecs { get; set; }
        public List<WalletVirtuelAgentModifierItemResultDto> Resultats { get; set; } = new();
    }

    public class WalletVirtuelAgentModifierItemResultDto
    {
        public int AgentId { get; set; }
        public bool Succes { get; set; }
        public string? Message { get; set; }
        public int? IdWalletVirtuelAgent { get; set; }
        public decimal? AncienSolde { get; set; }
        public decimal? NouveauSolde { get; set; }
    }

    public class WalletVirtuelMouvementReadDto
    {
        public int IdWalletVirtuelMouvement { get; set; }
        public int WalletVirtuelId { get; set; }
        public decimal Montant { get; set; }
        public string TypeOperation { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string SourceLibelle { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ReferenceExterne { get; set; }
        public DateTime DateOperation { get; set; }
        public int? AgentId { get; set; }
        public string? AgentNom { get; set; }
        public string? AgentMatricule { get; set; }
        public int? DeviseId { get; set; }
        public string? DeviseCode { get; set; }
        public string? DeviseNom { get; set; }
        public string? DeviseSymbole { get; set; }
        public decimal? SoldeAvant { get; set; }
        public decimal? SoldeApres { get; set; }
        public int? OperateurUtilisateurId { get; set; }
        public string? OperateurNom { get; set; }
        /// <summary>Agent à l'origine de l'opération (ex. qui a rechargé) — dérivé de OperateurUtilisateur.AgentId.</summary>
        public int? IdAgentFrom { get; set; }
        /// <summary>Nom complet de l'agent à l'origine de l'opération.</summary>
        public string? NomAgentFrom { get; set; }
        public int? CollecteId { get; set; }
        public string? AffilieNom { get; set; }
        public string? AffilieCode { get; set; }
    }

    public class WalletVirtuelMouvementFiltreDto
    {
        public string? TypeOperation { get; set; }
        public string? Source { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }

    public class WalletMouvementReadDto
    {
        public int IdWalletMouvement { get; set; }
        public int WalletId { get; set; }
        public decimal Montant { get; set; }
        public string TypeOperation { get; set; } = string.Empty; // CREDIT ou DEBIT
        public string Source { get; set; } = string.Empty; // COLLECTE, BONUS, COMMISSION, RETRAIT
        public string? Description { get; set; }
        public DateTime DateOperation { get; set; }
        
        // Informations sur le wallet et l'agent
        public int? WalletAgentId { get; set; }
        public string? AgentNom { get; set; }
        public string? AgentMatricule { get; set; }

        public int DeviseId { get; set; }
        public string? DeviseCode { get; set; }
        public string? DeviseNom { get; set; }
        public string? DeviseSymbole { get; set; }
    }

    public class WalletMouvementCreateDto
    {
        [Required]
        public int WalletId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0")]
        public decimal Montant { get; set; }
        
        [Required]
        [StringLength(10)]
        public string TypeOperation { get; set; } = string.Empty; // CREDIT ou DEBIT
        
        [Required]
        [StringLength(20)]
        public string Source { get; set; } = string.Empty; // COLLECTE, BONUS, COMMISSION, RETRAIT
        
        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class WalletMouvementUpdateDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le montant doit être supérieur à 0")]
        public decimal Montant { get; set; }
        
        [Required]
        [StringLength(10)]
        public string TypeOperation { get; set; } = string.Empty; // CREDIT ou DEBIT
        
        [Required]
        [StringLength(20)]
        public string Source { get; set; } = string.Empty; // COLLECTE, BONUS, COMMISSION, RETRAIT
        
        [StringLength(500)]
        public string? Description { get; set; }
    }
}
