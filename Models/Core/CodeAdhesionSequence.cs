using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.Core
{
    public class CodeAdhesionSequence
    {
        [Key]
        [StringLength(20)]
        public string Prefix { get; set; } = string.Empty;

        public int NextValue { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.Now;

        public DateTime? DateModification { get; set; }
    }
}
