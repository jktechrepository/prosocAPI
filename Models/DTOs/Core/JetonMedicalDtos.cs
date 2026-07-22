using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class JetonMedicalReadDto
    {
        public int IdJeton { get; set; }
        public int AffilieId { get; set; }
        public string? AffilieNom { get; set; }
        public string CodeJeton { get; set; } = string.Empty;
        public DateTime DateEmission { get; set; }
        public DateTime? DateUtilisation { get; set; }
        public DateTime? DateExpiration { get; set; }
        public bool EstValide { get; set; }
        public bool EstUtilise { get; set; }
        public int? HopitalPartenaireId { get; set; }
        public string? HopitalPartenaireNom { get; set; }
        public int? BonEnvoiId { get; set; }
        public string? BonEnvoiNumero { get; set; }
        public string? Observation { get; set; }
        public DateTime DateCreation { get; set; }
        public bool Statut { get; set; }
    }

    public class JetonMedicalCreateDto
    {
        [Required]
        public int AffilieId { get; set; }
        
        [Required]
        public int HopitalPartenaireId { get; set; }
        
        [StringLength(500)]
        public string? Observation { get; set; }
        
        public DateTime? DateExpiration { get; set; }
    }

    public class JetonMedicalValidationDto
    {
        [Required]
        [StringLength(20)]
        public string CodeJeton { get; set; } = string.Empty;
        
        [Required]
        public int HopitalPartenaireId { get; set; }
        
        public string? ObservationUtilisation { get; set; }
    }

    public class JetonMedicalUtilisationDto
    {
        [Required]
        public int IdJeton { get; set; }
        
        [Required]
        [StringLength(20)]
        public string CodeJeton { get; set; } = string.Empty;
        
        [Required]
        public int HopitalPartenaireId { get; set; }
        
        [StringLength(500)]
        public string? ObservationUtilisation { get; set; }
    }

    public class JetonMedicalStatsDto
    {
        public int TotalEmis { get; set; }
        public int TotalUtilises { get; set; }
        public int TotalValides { get; set; }
        public int TotalExpires { get; set; }
        public decimal TauxUtilisation { get; set; }
        public DateTime DateStats { get; set; }
    }
}
