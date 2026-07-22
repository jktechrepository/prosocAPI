using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class AgentBeneficiaireMaashDto
    {
        public int? IdAgentBeneficiaireMaash { get; set; }

        [Required]
        [StringLength(200)]
        public string NomComplet { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LienParente { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Adresse { get; set; } = string.Empty;
    }

    public class AgentBeneficiaireMaashReadDto
    {
        public int IdAgentBeneficiaireMaash { get; set; }
        public int AgentId { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string LienParente { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public bool Statut { get; set; }
    }

    public class AgentMaashCouvertureReadDto
    {
        public int AgentId { get; set; }
        public string NomCompletAgent { get; set; } = string.Empty;
        public bool EstEligible { get; set; }
        public bool CotisationMaashPayeePourPeriodeCourante { get; set; }
        public decimal MontantRetenueMensuelle { get; set; }
        public int DeviseId { get; set; }
        public string PeriodeCourante { get; set; } = string.Empty;
        public DateTime? DateDerniereRetenue { get; set; }
        public int? ProduitMaashId { get; set; }
        public string? ProduitMaashNom { get; set; }
        public List<AgentBeneficiaireMaashReadDto> BeneficiairesFamille { get; set; } = new();
    }

    public class AgentMaashRetenueRequestDto
    {
        public List<AgentBeneficiaireMaashDto>? BeneficiairesFamille { get; set; }

        /// <summary>Année/mois ciblés (défaut : période courante).</summary>
        public int? Annee { get; set; }

        [Range(1, 12)]
        public int? Mois { get; set; }
    }

    public class AgentMaashBatchEchecDto
    {
        public int AgentId { get; set; }
        public string? NomComplet { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class AgentMaashBatchResultDto
    {
        public int Annee { get; set; }
        public int Mois { get; set; }
        public int NbSucces { get; set; }
        public int NbDejaPaye { get; set; }
        public int NbEchec { get; set; }
        public int NbAgentsEligibles { get; set; }
        public DateTime DateExecution { get; set; }
        public List<AgentMaashBatchEchecDto> Echecs { get; set; } = new();
    }

    public class AgentMaashRetenueReadDto
    {
        public int IdRetenueMaashAgent { get; set; }
        public int AgentId { get; set; }
        public int Annee { get; set; }
        public int Mois { get; set; }
        public decimal Montant { get; set; }
        public int DeviseId { get; set; }
        public int? WalletMouvementId { get; set; }
        public decimal NouveauSoldeWallet { get; set; }
        public DateTime DatePaiement { get; set; }
        public List<AgentBeneficiaireMaashReadDto> BeneficiairesFamille { get; set; } = new();
    }
}
