using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class AgentReadDto
    {
        public int Id { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string Matricule { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? EmailAgent { get; set; }
        public string? Fonction { get; set; }
        public string? RoleAgent { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }
        public int? ZoneSocialeId { get; set; }
        public string? ZoneSocialeNom { get; set; }
        public int? CategorieAgentId { get; set; }
        public string? CategorieAgentCode { get; set; }
        public string? CategorieAgentDescription { get; set; }
        
        // 🆕 Informations des wallets
        public int? WalletId { get; set; }
        public decimal WalletSolde { get; set; }
        public bool WalletCree { get; set; }
        
        public int? WalletVirtuelId { get; set; }
        public decimal WalletVirtuelSolde { get; set; }
        public bool WalletVirtuelCree { get; set; }
        
        // 🆕 Informations sur le compte utilisateur créé
        public int? UtilisateurId { get; set; }
        public string? NomUtilisateur { get; set; }
        public bool UtilisateurCree { get; set; }
    }

    public class AgentCreateDto
    {
        [Required]
        [StringLength(200)]
        public string NomComplet { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Matricule { get; set; }

        [Required]
        [StringLength(20)]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [StringLength(200)]
        public string? EmailAgent { get; set; }

        [StringLength(100)]
        public string? Fonction { get; set; }

        [StringLength(100)]
        public string? RoleAgent { get; set; }

        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        public int? ZoneSocialeId { get; set; }

        public int? CategorieAgentId { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class AgentAffecterZoneSocialeDto
    {
        public int? ZoneSocialeId { get; set; }
    }

    public class AgentAffecterAffiliesDto
    {
        /// <summary>Agent dont on transfère le portefeuille (mode massif si AffilieIds est vide).</summary>
        public int? SourceAgentId { get; set; }

        public List<int> AffilieIds { get; set; } = new();
    }

    public class AgentAffecterAffiliesResultDto
    {
        public int AgentId { get; set; }
        public int TotalDemandes { get; set; }
        public int TotalReussites { get; set; }
        public int TotalEchecs { get; set; }
        public List<AgentAffilieAffectationItemDto> Resultats { get; set; } = new();
    }

    public class AgentAffilieAffectationItemDto
    {
        public int AffilieId { get; set; }
        public bool Succes { get; set; }
        public string? Message { get; set; }
        public int? AdhesionId { get; set; }
        public int? AncienAgentId { get; set; }
    }

    public class AgentUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string NomComplet { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Matricule { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? EmailAgent { get; set; }

        [StringLength(100)]
        public string? Fonction { get; set; }

        [StringLength(100)]
        public string? RoleAgent { get; set; }

        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        public int? ZoneSocialeId { get; set; }

        public int? CategorieAgentId { get; set; }

        public bool Statut { get; set; }
    }

    public class AgentAffilieReadDto
    {
        public int IdAffilie { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Matricule { get; set; } = string.Empty;
        public DateTime DateAdhesion { get; set; }
        public DateTime DateCreationAffilie { get; set; }
        public bool StatutAffilie { get; set; }
        public bool StatutAdhesion { get; set; }
        public string StatutDossier { get; set; } = string.Empty;
        public int NombreCollectes { get; set; }
        public decimal TotalCollectes { get; set; }
        public decimal TotalCommissions { get; set; }
        public DateTime? DerniereCollecte { get; set; }
        public string TypeAdhesion { get; set; } = string.Empty;
    }
}
