using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class DependantNiveau2Dto
    {
        public int? IdDependant { get; set; }

        [Required]
        [StringLength(200)]
        public string NomComplet { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LienParente { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Adresse { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? DateNaissance { get; set; }

        /// <summary>Obligatoire pour enfant 18–25 ans (certificat de scolarité, PDF ou image).</summary>
        public string? CertificatScolariteBase64 { get; set; }

        public string? CertificatScolariteContentType { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class PersonneContactCreateDto
    {
        [StringLength(200)]
        public string NomComplet { get; set; } = string.Empty;

        [StringLength(50)]
        public string LienParente { get; set; } = string.Empty;

        [StringLength(500)]
        public string Adresse { get; set; } = string.Empty;
    }

    public class PersonneContactNiveau2Dto : PersonneContactCreateDto
    {
    }

    /// <summary>Saisie niveau 2 par l'Agent Administratif (encodeur).</summary>
    public class AdhesionNiveau2EncodeurDto
    {
        [Required]
        public PersonneContactNiveau2Dto PersonneContact { get; set; } = new();

        public List<DependantNiveau2Dto> Dependants { get; set; } = new();

        /// <summary>Si true, le dossier passe au statut VALIDÉ après contrôle des 4 blocs « dossier complet ».</summary>
        public bool Valider { get; set; } = true;

        // --- Identité (optionnel : complète si manquant) ---
        [StringLength(100)]
        public string? Nom { get; set; }

        [StringLength(100)]
        public string? Prenom { get; set; }

        [StringLength(100)]
        public string? Postnom { get; set; }

        [StringLength(20)]
        public string? Telephone { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateNaissance { get; set; }

        // --- Adresse activité (optionnel à la saisie ; obligatoire pour valider) ---
        [StringLength(100)]
        public string? CommuneActivite { get; set; }

        [StringLength(100)]
        public string? QuartierActivite { get; set; }

        [StringLength(100)]
        public string? AvenueActivite { get; set; }

        [StringLength(50)]
        public string? NumeroActivite { get; set; }

        public string? PhotoBase64 { get; set; }

        [StringLength(100)]
        public string? PhotoContentType { get; set; }

        public string? CarteIdentiteBase64 { get; set; }

        [StringLength(100)]
        public string? CarteIdentiteContentType { get; set; }
    }

    public class PersonneContactReadDto
    {
        public int IdPersonneContact { get; set; }
        public int AffilieId { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string LienParente { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public bool Statut { get; set; }
    }

    public class AdhesionFicheEncodeurReadDto
    {
        public int IdAdhesion { get; set; }
        public string StatutDossier { get; set; } = string.Empty;
        public int AffilieId { get; set; }
        public string CodeAdhesion { get; set; } = string.Empty;
        public string NomCompletAffilie { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string? Postnom { get; set; }
        public string? Telephone { get; set; }
        public DateTime DateNaissance { get; set; }
        public string? CommuneActivite { get; set; }
        public string? QuartierActivite { get; set; }
        public string? AvenueActivite { get; set; }
        public string? NumeroActivite { get; set; }
        public bool HasPhoto { get; set; }
        public bool HasCarteIdentite { get; set; }
        public bool HasPersonneContact { get; set; }
        public bool IdentiteComplete { get; set; }
        public bool AdresseActiviteComplete { get; set; }
        public bool DossierComplet { get; set; }
        public List<DependantReadDto> Dependants { get; set; } = new();
        public PersonneContactReadDto? PersonneContact { get; set; }
    }

    public class AdhesionNiveau2EncodeurReadDto
    {
        public int IdAdhesion { get; set; }
        public string StatutDossier { get; set; } = string.Empty;
        public int AffilieId { get; set; }
        public List<DependantReadDto> Dependants { get; set; } = new();
        public PersonneContactReadDto PersonneContact { get; set; } = new();
    }
}
