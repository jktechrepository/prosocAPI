using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProsocAPI.Models.Core
{
    /// <summary>Configuration marchand FlexPay (singleton organisation).</summary>
    public class InfoPaiementMarchand
    {
        [Key]
        public int IdInfoPaiementMarchand { get; set; }

        [Required, MaxLength(100)]
        public string CodeMarchand { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string ApiToken { get; set; } = string.Empty;

        public bool ActifMobileMoney { get; set; } = true;

        public bool ActifCarteBancaire { get; set; } = true;

        public bool Statut { get; set; } = true;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public DateTime? DateModification { get; set; }
    }
}
