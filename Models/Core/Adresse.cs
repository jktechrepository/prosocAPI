using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProsocAPI.Models.Core
{
    public abstract class Adresse
    {
        [StringLength(500)]
        public string? Rue { get; set; }
        
        [StringLength(200)]
        public string? Quartier { get; set; }
        
        [StringLength(100)]
        public string? Ville { get; set; }
        
        [StringLength(20)]
        public string? CodePostal { get; set; }
        
        [StringLength(50)]
        public string? Pays { get; set; } = "RD Congo";
        
        [StringLength(20)]
        public string? Telephone { get; set; }
        
        [StringLength(100)]
        public string? Email { get; set; }
    }
}
