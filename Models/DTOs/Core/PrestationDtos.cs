using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class PrestationReadDto
    {
        public int Id { get; set; }
        public string NomPrestation { get; set; } = string.Empty;
        public string Periodicite { get; set; } = "Mensuel";
       
        public double? Montant { get; set; }
        
        public int? DeviseId { get; set; }
        public string? DeviseCode { get; set; }
        public string? Description { get; set; }
        public int? ProduitMutuelId { get; set; }
        public string? ProduitMutuelNom { get; set; }
        public int? ProduitAssureurId { get; set; }
        public string? ProduitAssureurNom { get; set; }
        public bool EstGratuit { get; set; }
    }

    public class PrestationCreateDto
    {
        [Required]
        [StringLength(200)]
        public string NomPrestation { get; set; } = string.Empty;

        public double? Montant { get; set; }
        
        public int? DeviseId { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(20)]
        public string? Periodicite { get; set; }

        public int? ProduitMutuelId { get; set; }
        public int? ProduitAssureurId { get; set; }
    }

    public class PrestationUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string NomPrestation { get; set; } = string.Empty;

        public double? Montant { get; set; }
        
        public int? DeviseId { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(20)]
        public string? Periodicite { get; set; }

        public int? ProduitMutuelId { get; set; }
        public int? ProduitAssureurId { get; set; }
    }
}
