using ProsocAPI.Models.Core;

namespace ProsocAPI.Models.Configuration
{
  public class PenaliteOptions
  {
    public const string SectionName = "Penalite";

    public bool ApplicationAutomatiqueActivee { get; set; } = true;

    public int DelaiGraceJours { get; set; } = 3;

    /// <summary>Code du frais catalogue pénalité retard cotisation (table Frais.Code).</summary>
    public string FraisPenaliteCode { get; set; } = FraisCodes.PenaliteRetardCotisation;

    public bool RetardCotisationActive { get; set; } = true;
  }
}
