using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class ZoneSocialeReadDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public int CommuneId { get; set; }
        public string? CommuneNom { get; set; }
        public bool Statut { get; set; }
        public int? ChefEquipeAgentId { get; set; }
        public string? ChefEquipeNom { get; set; }
    }

    public class ZoneSocialeCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int CommuneId { get; set; }

        public bool Statut { get; set; } = true;
    }

    public class ZoneSocialeUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int CommuneId { get; set; }

        public bool Statut { get; set; } = true;
    }
}
