using System.ComponentModel.DataAnnotations;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Models.DTOs.Core
{
    public class CollecteReadDto
    {
        public int IdCollecte { get; set; }
        
        // NOUVEAU : Type de collecte
        public TypeCollecte TypeCollecte { get; set; }
        
        // NOUVEAU : Relation avec Frais
        public int? FraisId { get; set; }
        public string? FraisLibelle { get; set; }
        public double? FraisMontant { get; set; }

        public int? CotisationAffilieId { get; set; }
        public string? CotisationAffilieLibelle { get; set; }
        public string? CotisationPeriodicite { get; set; }
        public decimal? CotisationMontantReference { get; set; }
        public int? CotisationTypeAdhesionId { get; set; }
        public string? CotisationTypeAdhesionLibelle { get; set; }
        
        public int AffilieId { get; set; }
        public string? AffilieNom { get; set; }
        public int? AgentId { get; set; }
        public string? AgentNom { get; set; }
        public decimal Montant { get; set; }
        public string? ReferencePaiement { get; set; }
        public string? ModePaiement { get; set; }
        public string? Operateur { get; set; }
        public string? StatutPaiement { get; set; }
        public int? SouscriptionPrestationId { get; set; }
        public string? PrestationLibelle { get; set; }
        public decimal? MontantRecu { get; set; }
        public decimal? MontantAttendu { get; set; }
        public int DeviseId { get; set; }
        public string? DeviseNom { get; set; }
        public string? DeviseCode { get; set; }
        public DateTime DateCollecte { get; set; }
        
        // 🆕 Période de la collecte
        public int Mois { get; set; }
        public int Annee { get; set; }
        
        public string? Observation { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }
    }

    public class CollecteCreateDto
    {
        // NOUVEAU : Type de collecte (obligatoire)
        [Required]
        public TypeCollecte TypeCollecte { get; set; }
        
        // NOUVEAU : FraisId (requis si TypeCollecte = Frais)
        public int? FraisId { get; set; }

        public int? CotisationAffilieId { get; set; }
        
        [Required]
        public int AffilieId { get; set; }

        [Required]
        public int? AgentId { get; set; }

        [Range(0.01, 999999999.99)]
        public decimal Montant { get; set; }

        // 🆕 Période de la collecte
        [Range(1, 12)]
        public int Mois { get; set; } = DateTime.Now.Month;
        
        [Range(2020, 2100)]
        public int Annee { get; set; } = DateTime.Now.Year;

        [StringLength(100)]
        public string? ReferencePaiement { get; set; }

        /// <summary>
        /// Mode de paiement obligatoire. Valeurs acceptées : ESPECE, MOBILE_MONEY, CARTE_BANCAIRE (FlexPay), VIREMENT_BANCAIRE, CHEQUE, VIRTUAL_ACCOUNT
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ModePaiement { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Operateur { get; set; }

        [StringLength(20)]
        public string? StatutPaiement { get; set; }

        public int? SouscriptionPrestationId { get; set; }

        public decimal? MontantRecu { get; set; }

        public decimal? MontantAttendu { get; set; }

        [Required]
        public int DeviseId { get; set; }

        [StringLength(500)]
        public string? Observation { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        public bool Statut { get; set; } = true;
        
        // NOUVEAU : Validation personnalisée
        public bool IsValid()
        {
            return TypeCollecte switch
            {
                TypeCollecte.Frais => FraisId.HasValue && !CotisationAffilieId.HasValue && !SouscriptionPrestationId.HasValue,
                TypeCollecte.Souscription => SouscriptionPrestationId.HasValue && !FraisId.HasValue && !CotisationAffilieId.HasValue,
                TypeCollecte.Cotisation => CotisationAffilieId.HasValue && !FraisId.HasValue && !SouscriptionPrestationId.HasValue,
                _ => false
            };
        }
    }

    /// <summary>
    /// Création d'une collecte avec paiement électronique (FlexPay) via endpoint dédié public.
    /// </summary>
    public class CollecteWithPaiementElectroniqueCreateDto
    {
        [Required]
        public CollecteCreateDto Collecte { get; set; } = new();

        /// <summary>Mode FlexPay obligatoire : MOBILE_MONEY ou CARTE_BANCAIRE.</summary>
        [Required]
        [StringLength(20)]
        public string ModePaiement { get; set; } = string.Empty;

        /// <summary>Téléphone Mobile Money (obligatoire si MOBILE_MONEY).</summary>
        [StringLength(20)]
        public string? TelephonePaiement { get; set; }

        /// <summary>Devise de paiement FlexPay (doit correspondre à Collecte.DeviseId).</summary>
        [Range(1, int.MaxValue)]
        public int DevisePaiementId { get; set; }
    }

    public class CollecteUpdateDto
    {
        // NOUVEAU : Type de collecte (obligatoire)
        [Required]
        public TypeCollecte TypeCollecte { get; set; }
        
        // NOUVEAU : FraisId (requis si TypeCollecte = Frais)
        public int? FraisId { get; set; }

        public int? CotisationAffilieId { get; set; }
        
        [Required]
        public int AffilieId { get; set; }

        [Required]
        public int? AgentId { get; set; }

        [Range(0.01, 999999999.99)]
        public decimal Montant { get; set; }

        // 🆕 Période de la collecte
        [Range(1, 12)]
        public int Mois { get; set; } = DateTime.Now.Month;
        
        [Range(2020, 2100)]
        public int Annee { get; set; } = DateTime.Now.Year;

        [StringLength(100)]
        public string? ReferencePaiement { get; set; }

        /// <summary>
        /// Mode de paiement obligatoire. Valeurs acceptées : ESPECE, MOBILE_MONEY, CARTE_BANCAIRE (FlexPay), VIREMENT_BANCAIRE, CHEQUE, VIRTUAL_ACCOUNT
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ModePaiement { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Operateur { get; set; }

        [StringLength(20)]
        public string? StatutPaiement { get; set; }

        public int? SouscriptionPrestationId { get; set; }

        public decimal? MontantRecu { get; set; }

        public decimal? MontantAttendu { get; set; }

        [Required]
        public int DeviseId { get; set; }

        [StringLength(500)]
        public string? Observation { get; set; }

        public bool Statut { get; set; }
        
        // NOUVEAU : Validation personnalisée
        public bool IsValid()
        {
            return TypeCollecte switch
            {
                TypeCollecte.Frais => FraisId.HasValue && !CotisationAffilieId.HasValue && !SouscriptionPrestationId.HasValue,
                TypeCollecte.Souscription => SouscriptionPrestationId.HasValue && !FraisId.HasValue && !CotisationAffilieId.HasValue,
                TypeCollecte.Cotisation => CotisationAffilieId.HasValue && !FraisId.HasValue && !SouscriptionPrestationId.HasValue,
                _ => false
            };
        }
    }

    public class CollecteStatsDto
    {
        public decimal TotalMontant { get; set; }
        public decimal TotalMontantDevisePrincipale { get; set; }
        public int NombreCollectes { get; set; }
        public decimal MontantMoyen { get; set; }
        public Dictionary<string, decimal> MontantsParDevise { get; set; } = new();
        public Dictionary<string, int> CollectesParAgent { get; set; } = new();
        
        // NOUVEAU : Statistiques par type de collecte
        public Dictionary<TypeCollecte, decimal> MontantsParType { get; set; } = new();
        public Dictionary<TypeCollecte, int> NombreParType { get; set; } = new();
    }
    
    // NOUVEAU : DTOs spécialisés pour les frais
    public class CreateFraisCollecteDto
    {
        [Required]
        public int FraisId { get; set; }
        
        [Required]
        public int AffilieId { get; set; }

        [Required]
        public int? AgentId { get; set; }

        [Range(0.01, 999999999.99)]
        public decimal Montant { get; set; }

        // 🆕 Période de la collecte
        [Range(1, 12)]
        public int Mois { get; set; } = DateTime.Now.Month;
        
        [Range(2020, 2100)]
        public int Annee { get; set; } = DateTime.Now.Year;

        [Required]
        public int DeviseId { get; set; }

        [Required]
        [StringLength(20)]
        public string ModePaiement { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Observation { get; set; }
    }
    
    public class CreateSouscriptionCollecteDto
    {
        [Required]
        public int SouscriptionPrestationId { get; set; }
        
        [Required]
        public int AffilieId { get; set; }

        [Required]
        public int? AgentId { get; set; }

        [Range(0.01, 999999999.99)]
        public decimal Montant { get; set; }

        // 🆕 Période de la collecte
        [Range(1, 12)]
        public int Mois { get; set; } = DateTime.Now.Month;
        
        [Range(2020, 2100)]
        public int Annee { get; set; } = DateTime.Now.Year;

        [Required]
        public int DeviseId { get; set; }

        [Required]
        [StringLength(20)]
        public string ModePaiement { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Observation { get; set; }
    }

    public class CreateCotisationCollecteDto
    {
        [Required]
        public int CotisationAffilieId { get; set; }

        [Required]
        public int AffilieId { get; set; }

        [Required]
        public int? AgentId { get; set; }

        [Range(0.01, 999999999.99)]
        public decimal Montant { get; set; }

        [Range(1, 12)]
        public int Mois { get; set; } = DateTime.Now.Month;

        [Range(2020, 2100)]
        public int Annee { get; set; } = DateTime.Now.Year;

        [Required]
        public int DeviseId { get; set; }

        [Required]
        [StringLength(20)]
        public string ModePaiement { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Observation { get; set; }
    }
}
