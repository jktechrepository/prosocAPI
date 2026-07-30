using System.ComponentModel.DataAnnotations;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Models.DTOs.Core
{
    public class AdhesionReadDto
    {
        public int Id { get; set; }
        public string StatutDossier { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }
        public int AffilieId { get; set; }
        public int TypeAdhesionId { get; set; }
        public int? AgentId { get; set; }
    }

    public class AdhesionWithAffilieReadDto
    {
        public int Id { get; set; }
        public string StatutDossier { get; set; } = string.Empty;
        public int TypeAdhesionId { get; set; }
        public string TypeAdhesionLibelle { get; set; } = string.Empty;
        public int? AgentId { get; set; }
        public string AgentNom { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }
        public int AffilieId { get; set; }
        public string CodeAdhesion { get; set; } = string.Empty;
        public string? CommuneActivite { get; set; }
        public string? QuartierActivite { get; set; }
        public string? AvenueActivite { get; set; }
        public string? NumeroActivite { get; set; }
        public AffilieReadDto Affilie { get; set; } = new();
        public List<SouscriptionPrestationReadDto> Souscriptions { get; set; } = new();
        public List<CollecteReadDto> Collectes { get; set; } = new();
        
        // 🆕 Ajout des dépendants
        public List<DependantReadDto> Dependants { get; set; } = new();
        
        // ✅ NOUVEAU : Ajout des antécédents
        public List<AntecedantReadDto> Antecedants { get; set; } = new();

        public PersonneContactReadDto? PersonneContact { get; set; }
    }

    public class AdhesionCreateDto
    {
        [Required]
        [StringLength(20)]
        public string StatutDossier { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int AffilieId { get; set; }

        [Range(1, int.MaxValue)]
        public int TypeAdhesionId { get; set; }

        [Range(1, int.MaxValue)]
        public int AgentId { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class AdhesionUpdateDto
    {
        [Required]
        [StringLength(20)]
        public string StatutDossier { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int AffilieId { get; set; }

        [Range(1, int.MaxValue)]
        public int TypeAdhesionId { get; set; }

        [Range(1, int.MaxValue)]
        public int AgentId { get; set; }

        public bool Statut { get; set; }
    }

    // 🆕 DTO intégré pour collecte avec souscription embarquée
    public class CollecteAvecSouscriptionDto
    {
        // 🆕 Souscription intégrée (optionnelle) - utilise le DTO existant défini plus bas
        public SouscriptionPrestationCreateDto? Souscription { get; set; }
        
        // Champs de la collecte
        [Required]
        public TypeCollecte TypeCollecte { get; set; }
        
        public int? FraisId { get; set; }

        public int? CotisationAffilieId { get; set; }

        [Range(0.01, 999999999.99)]
        public decimal Montant { get; set; }

        // 🆕 Période de la collecte
        [Range(1, 12)]
        public int Mois { get; set; } = DateTime.Now.Month;
        
        [Range(2020, 2100)]
        public int Annee { get; set; } = DateTime.Now.Year;

        [StringLength(100)]
        public string? ReferencePaiement { get; set; }

        [StringLength(20)]
        public string? ModePaiement { get; set; }

        [StringLength(50)]
        public string? Operateur { get; set; }

        [StringLength(20)]
        public string? StatutPaiement { get; set; }

        public decimal? MontantRecu { get; set; }
        public decimal? MontantAttendu { get; set; }
        
        [Required]
        public int DeviseId { get; set; }

        [StringLength(500)]
        public string? Observation { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class AdhesionWithAffilieCreateDto
    {
        [StringLength(20)]
        public string? CodeAdhesion { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Postnom { get; set; }

        public DateTime DateNaissance { get; set; }

        [StringLength(20)]
        public string? Telephone { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string? EmailAffilie { get; set; }

        [Required]
        [StringLength(100)]
        public string ProvinceResidence { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CommuneResidence { get; set; }

        [StringLength(100)]
        public string? QuartierResidence { get; set; }

        [StringLength(100)]
        public string? AvenueResidence { get; set; }

        [StringLength(50)]
        public string? NumeroResidence { get; set; }

        [StringLength(100)]
        public string? CommuneActivite { get; set; }

        [StringLength(100)]
        public string? QuartierActivite { get; set; }

        [StringLength(100)]
        public string? AvenueActivite { get; set; }

        [StringLength(50)]
        public string? NumeroActivite { get; set; }

       
        public string? PhotoBase64 { get; set; } = string.Empty;

       
        [StringLength(100)]
        public string? PhotoContentType { get; set; } = string.Empty;

    
        public string? CarteIdentiteBase64 { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CarteIdentiteContentType { get; set; } = string.Empty;

        // ❌ SUPPRIMÉ : public List<SouscriptionPrestationCreateDto> Souscriptions { get; set; } = new();
        // Utiliser Collectes[].Souscription à la place

        public bool AffilieStatut { get; set; } = true;

        [Required]
        [StringLength(20)]
        public string StatutDossier { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int TypeAdhesionId { get; set; }

        /// <summary>Agent AT gestionnaire. Null ou 0 pour adhésion en ligne (FlexPay).</summary>
        public int? AgentId { get; set; }

        public bool AdhesionStatut { get; set; } = true;

        [Required]
        public List<CollecteAvecSouscriptionDto> Collectes { get; set; } = new();

        // Ajout des dépendants
        public List<DependantCreateDto> Dependants { get; set; } = new();
        
        // ✅ NOUVEAU : Ajout des antécédents
        public List<AntecedantCreateDto> Antecedants { get; set; } = new();

        public PersonneContactCreateDto? PersonneContact { get; set; }
    }

    /// <summary>Création adhésion niveau 1 via multipart/form-data (payload JSON + fichiers binaires).</summary>
    public class AdhesionWithAffilieMultipartRequest
    {
        [Required]
        public string Payload { get; set; } = string.Empty;
    }

    /// <summary>
    /// Création d'adhésion + affilié avec paiement électronique (FlexPay) via endpoint dédié.
    /// </summary>
    public class AdhesionWithAffiliePaiementElectroniqueCreateDto
    {
        [Required]
        public AdhesionWithAffilieCreateDto Adhesion { get; set; } = new();

        /// <summary>Mode FlexPay obligatoire: MOBILE_MONEY ou CARTE_BANCAIRE.</summary>
        [Required]
        [StringLength(20)]
        public string ModePaiement { get; set; } = string.Empty;

        /// <summary>Téléphone du compte Mobile Money (obligatoire si MOBILE_MONEY).</summary>
        [StringLength(20)]
        public string? TelephonePaiement { get; set; }

        /// <summary>Devise de paiement FlexPay (doit être identique sur toutes les collectes).</summary>
        [Range(1, int.MaxValue)]
        public int DevisePaiementId { get; set; }
    }

    public class CollecteInAdhesionCreateDto
    {
        // 🆕 Type de collecte (obligatoire)
        [Required]
        public TypeCollecte TypeCollecte { get; set; } = TypeCollecte.Souscription;
        
        // 🆕 Relation avec Frais (optionnel)
        public int? FraisId { get; set; }

        public int? CotisationAffilieId { get; set; }

        [Range(0.01, 999999999.99)]
        public decimal Montant { get; set; }

        // 🆕 Période de la collecte
        [Range(1, 12)]
        public int Mois { get; set; } = DateTime.Now.Month;
        
        [Range(2020, 2100)]
        public int Annee { get; set; } = DateTime.Now.Year;

        [StringLength(100)]
        public string? ReferencePaiement { get; set; }

        [StringLength(20)]
        public string? ModePaiement { get; set; }

        [StringLength(50)]
        public string? Operateur { get; set; }

        [StringLength(20)]
        public string? StatutPaiement { get; set; }

        public int? SouscriptionPrestationId { get; set; }

        public decimal? MontantRecu { get; set; }

        public decimal? MontantAttendu { get; set; }

        [Range(1, int.MaxValue)]
        public int DeviseId { get; set; }

        [StringLength(500)]
        public string? Observation { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class SouscriptionPrestationCreateDto
    {
        [Range(1, int.MaxValue)]
        public int PrestationId { get; set; }

        public DateTime? DateSouscription { get; set; }

        public bool Statut { get; set; } = true;
    }

    /// <summary>
    /// Paiement initial lors de la création d'une souscription (POST /api/SouscriptionPrestation).
    /// </summary>
    public class SouscriptionPrestationCollecteCreateDto
    {
        /// <summary>Agent guichet. Si 0, repli sur l'agent référent de l'adhésion.</summary>
        public int AgentId { get; set; }

        [Required]
        [Range(0.01, 999999999.99)]
        public decimal Montant { get; set; }

        [Required]
        [Range(1, 12)]
        public int Mois { get; set; } = DateTime.UtcNow.Month;

        [Required]
        [Range(2020, 2100)]
        public int Annee { get; set; } = DateTime.UtcNow.Year;

        [Required]
        public int DeviseId { get; set; }

        /// <summary>ESPECE, VIRTUAL_ACCOUNT, CHEQUE, VIREMENT_BANCAIRE (synchrone). FlexPay : POST /paiement-electronique.</summary>
        [Required]
        [StringLength(20)]
        public string ModePaiement { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ReferencePaiement { get; set; }

        [StringLength(500)]
        public string? Observation { get; set; }

        public decimal? MontantRecu { get; set; }

        public decimal? MontantAttendu { get; set; }

        [StringLength(20)]
        public string? StatutPaiement { get; set; }

        public bool Statut { get; set; } = true;
    }

    /// <summary>
    /// Souscription à une prestation avec paiement de la première période (body POST /api/SouscriptionPrestation).
    /// </summary>
    public class SouscriptionPrestationAchatCreateDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int PrestationId { get; set; }

        public DateTime? DateSouscription { get; set; }

        public bool Statut { get; set; } = true;

        [Required]
        public SouscriptionPrestationCollecteCreateDto Collecte { get; set; } = null!;
    }

    /// <summary>
    /// Achat d'une nouvelle souscription via FlexPay (POST /api/SouscriptionPrestation/paiement-electronique).
    /// </summary>
    public class SouscriptionPrestationPaiementElectroniqueCreateDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int AffilieId { get; set; }

        /// <summary>Mode FlexPay obligatoire : MOBILE_MONEY ou CARTE_BANCAIRE.</summary>
        [Required]
        [StringLength(20)]
        public string ModePaiement { get; set; } = string.Empty;

        /// <summary>Téléphone Mobile Money (obligatoire si MOBILE_MONEY).</summary>
        [StringLength(20)]
        public string? TelephonePaiement { get; set; }

        /// <summary>Devise de paiement FlexPay (doit correspondre à Achat.Collecte.DeviseId).</summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int DevisePaiementId { get; set; }

        [Required]
        public SouscriptionPrestationAchatCreateDto Achat { get; set; } = null!;
    }

    public class SouscriptionPrestationAchatReadDto
    {
        public SouscriptionPrestationReadDto Souscription { get; set; } = null!;
        public CollecteReadDto Collecte { get; set; } = null!;
    }

    public class SouscriptionPrestationReadDto
    {
        public int Id { get; set; }
        public int AffilieId { get; set; }
        public string? AffilieNom { get; set; }
        public string? AffiliePrenom { get; set; }
        public int PrestationId { get; set; }
        public string? PrestationNom { get; set; }
        public string? PrestationDescription { get; set; }
        public DateTime DateSouscription { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }
        public int NombreCollectes { get; set; }
        public decimal TotalCollectes { get; set; }
    }

    public class SouscriptionPrestationUpdateDto
    {
        [Required]
        public int AffilieId { get; set; }

        [Required]
        public int PrestationId { get; set; }

        public bool Statut { get; set; }
    }

    public class AdhesionEnLigneSansGestionnaireDto
    {
        public int IdAdhesion { get; set; }
        public int IdAffilie { get; set; }
        public string CodeAdhesion { get; set; } = string.Empty;
        public string NomComplet { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string? EmailAffilie { get; set; }
        public string? ProvinceResidence { get; set; }
        public string TypeAdhesion { get; set; } = string.Empty;
        public string StatutDossier { get; set; } = string.Empty;
        public DateTime DateAdhesion { get; set; }
        public string? ModePaiementAdhesion { get; set; }
    }

    public class SouscriptionPrestationStatsDto
    {
        public int NombreTotalSouscriptions { get; set; }
        public int NombreSouscriptionsActives { get; set; }
        public decimal TotalMontantCollectes { get; set; }
        public Dictionary<string, int> SouscriptionsParPrestation { get; set; } = new();
        public Dictionary<string, int> SouscriptionsParAffilie { get; set; } = new();
    }
}
