using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class AdhesionUpdateWithAffilieDto
    {
        [Required]
        public AffilieForUpdateDto Affilie { get; set; } = new();

        [Required]
        public AdhesionForUpdateDto Adhesion { get; set; } = new();

        [Required]
        public List<SouscriptionPrestationForUpdateDto> Souscriptions { get; set; } = new();

        [Required]
        public List<DependantForUpdateDto> Dependents { get; set; } = new();
    }

    public class AffilieForUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        public DateTime DateNaissance { get; set; }

        [StringLength(20)]
        public string? Telephone { get; set; }

        [StringLength(100)]
        public string? Postnom { get; set; }

        [StringLength(100)]
        public string? ProvinceResidence { get; set; }

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

        public bool Statut { get; set; } = true;
    }

    public class AdhesionForUpdateDto
    {
        [Required]
        [StringLength(20)]
        public string StatutDossier { get; set; } = string.Empty; // 'EN ATTENTE', 'COMPLET', 'ACTIVE'
    }

    public class SouscriptionPrestationForUpdateDto
    {
        [Required]
        public int PrestationId { get; set; }

        public DateTime DateDebut { get; set; }

        public DateTime? DateFin { get; set; }

        public bool Statut { get; set; }
    }

    public class DependantForUpdateDto
    {
        public int? IdDependant { get; set; } // Null pour nouveau dépendant

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        public DateTime DateNaissance { get; set; }

        [StringLength(20)]
        public string? Telephone { get; set; }

        [StringLength(100)]
        public string? Postnom { get; set; }

        [StringLength(50)]
        public string? LienParente { get; set; }

        [Required]
        public string TypePieceIdentite { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string NumeroPieceIdentite { get; set; } = string.Empty;

        public bool Statut { get; set; } = true;
    }
}
