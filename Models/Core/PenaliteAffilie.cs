using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace ProsocAPI.Models.Core
{
  /// <summary>
  /// Pénalité appliquée à un affilié, liée à un arriéré source (ex. retard cotisation J+3).
  /// </summary>
  public class PenaliteAffilie
  {
    [Key]
    public int IdPenaliteAffilie { get; set; }

    public int AffilieId { get; set; }

    public int ArrieresAffilieId { get; set; }

    public int FraisId { get; set; }

    public TypePenalite TypePenalite { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Montant { get; set; }

    public int DeviseId { get; set; }

    public int JoursRetard { get; set; }

    [StringLength(500)]
    public string Motif { get; set; } = string.Empty;

    [StringLength(20)]
    public string Statut { get; set; } = PenaliteAffilieStatuts.Appliquee;

    [StringLength(500)]
    public string? MotifAnnulation { get; set; }

    public DateTime DateApplication { get; set; } = DateTime.Now;

    public DateTime? DatePaiement { get; set; }

    public DateTime? DateAnnulation { get; set; }

    public bool StatutActif { get; set; } = true;

    public DateTime DateCreation { get; set; } = DateTime.Now;

    public DateTime? DateModification { get; set; }

    [ForeignKey("AffilieId")]
    [JsonIgnore]
    [ValidateNever]
    public virtual Affilie Affilie { get; set; } = null!;

    [ForeignKey("ArrieresAffilieId")]
    [JsonIgnore]
    [ValidateNever]
    public virtual ArrieresAffilie ArrieresAffilie { get; set; } = null!;

    [ForeignKey("FraisId")]
    [JsonIgnore]
    [ValidateNever]
    public virtual Frais Frais { get; set; } = null!;

    [ForeignKey("DeviseId")]
    [JsonIgnore]
    [ValidateNever]
    public virtual Devise Devise { get; set; } = null!;

    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Collecte> Collectes { get; set; } = new List<Collecte>();

    [NotMapped]
    public bool EstDue => Statut == PenaliteAffilieStatuts.Appliquee;
  }
}
