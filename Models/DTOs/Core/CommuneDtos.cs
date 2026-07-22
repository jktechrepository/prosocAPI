using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class CommuneReadDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public int ProvinceId { get; set; }
        public string? ProvinceNom { get; set; }
        public int NombreZones { get; set; }
        public int? SuperviseurAgentId { get; set; }
        public string? SuperviseurNom { get; set; }
    }

    public class CommuneCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int ProvinceId { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class CommuneUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int ProvinceId { get; set; }

        public bool Statut { get; set; } = true;
    }
}
