using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class ProvinceReadDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public bool Statut { get; set; }
        public int NombreCommunes { get; set; }
    }

    public class ProvinceCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        public bool Statut { get; set; } = true;
    }

    public class ProvinceUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        public bool Statut { get; set; } = true;
    }
}
