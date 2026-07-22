using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class DependantReadDto
    {
        public int IdDependant { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Adresse { get; set; }
        public string LienParente { get; set; } = string.Empty;
        public int AffilieId { get; set; }
        public DateTime? DateNaissance { get; set; }
        public string? Telephone { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateModification { get; set; }
        public bool Statut { get; set; }
        public bool PossedeCertificatScolarite { get; set; }
        public string? CertificatScolariteBase64 { get; set; }
        public string? CertificatScolariteContentType { get; set; }
        public List<AntecedentReadDto> Antecedants { get; set; } = new();
    }

    public class DependantCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Adresse { get; set; }

        [StringLength(50)]
        public string LienParente { get; set; } = string.Empty;

        // 🆕 MODIFIÉ : AffilieId optionnel pour la création avec affilié
        public int? AffilieId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateNaissance { get; set; }

        /// <summary>Obligatoire pour enfant 18–25 ans (justificatif d'études).</summary>
        public string? CertificatScolariteBase64 { get; set; }

        public string? CertificatScolariteContentType { get; set; }
    }

    public class DependantUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Adresse { get; set; }

        [StringLength(50)]
        public string LienParente { get; set; } = string.Empty;

        public int? AffilieId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateNaissance { get; set; }

        public string? CertificatScolariteBase64 { get; set; }

        public string? CertificatScolariteContentType { get; set; }
    }
}
