using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class AntecedentSearchDto
    {
        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime? DateNaissanceDebut { get; set; }

        public DateTime? DateNaissanceFin { get; set; }
    }
}
