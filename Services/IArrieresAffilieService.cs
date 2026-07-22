using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
  public interface IArrieresAffilieService
  {
    Task<ArrieresAffilie> ProcessCollecteForArrieresAsync(Collecte collecte, CancellationToken ct = default);
    Task<List<ArrieresAffilie>> GetArrieresByAffilieAsync(int affilieId, CancellationToken ct = default);
    Task<List<ArrieresAffilie>> GetArrieresByPeriodeAsync(int mois, int annee, CancellationToken ct = default);
    Task<List<ArrieresAffilie>> GetArrieresByStatutPaiementAsync(string statutPaiement, CancellationToken ct = default);
    Task<ArrieresStatsDto> GetArrieresStatsByAffilieAsync(int affilieId, CancellationToken ct = default);
    Task<List<ArrieresAffilie>> GenerateArrieresForDateAsync(DateTime date, CancellationToken ct = default);
    Task<List<ArrieresAffilie>> GenerateMonthlyArrieresAsync(int mois, int annee, CancellationToken ct = default);
    Task<int> UpdateStatutsRetardAsync(CancellationToken ct = default);
    Task<ArrieresAffilie> UpdateStatutArriereAsync(int id, string statutPaiement, CancellationToken ct = default);
    Task<ArrieresResumeDto> GetArrieresResumeAsync(CancellationToken ct = default);
    Task<bool> DoitExecuterGenerationAutomatiqueAsync(CancellationToken ct = default);
    Task ExecuterGenerationAutomatiqueAsync(CancellationToken ct = default);
  }

  public class ArrieresStatsDto
  {
    public int AffilieId { get; set; }
    public string NomAffilie { get; set; } = string.Empty;
    public decimal TotalMontantAttendu { get; set; }
    public decimal TotalMontantPaye { get; set; }
    public decimal TotalRestantAPayer { get; set; }
    public int NombreArrieres { get; set; }
    public int NombreArrieresPayes { get; set; }
    public int NombreArrieresEnRetard { get; set; }
    public decimal TauxPaiementGlobal { get; set; }
    public DateTime DerniereMiseAJour { get; set; }
  }

  public class ArrieresResumeDto
  {
    public int TotalAffiliesAvecArrieres { get; set; }
    public decimal TotalMontantAttendu { get; set; }
    public decimal TotalMontantPaye { get; set; }
    public decimal TotalRestantAPayer { get; set; }
    public int TotalNombreArrieres { get; set; }
    public int TotalNombrePayes { get; set; }
    public int TotalNombreEnRetard { get; set; }
    public decimal TauxPaiementGlobal { get; set; }
    public List<ArrieresParMoisDto> ArrieresParMois { get; set; } = new();
    public List<ArrieresParStatutDto> ArrieresParStatut { get; set; } = new();
  }

  public class ArrieresParMoisDto
  {
    public string Periode { get; set; } = string.Empty;
    public decimal MontantAttendu { get; set; }
    public decimal MontantPaye { get; set; }
    public decimal RestantAPayer { get; set; }
    public int NombreArrieres { get; set; }
    public int NombrePayes { get; set; }
    public decimal TauxPaiement { get; set; }
  }

  public class ArrieresParStatutDto
  {
    public string StatutPaiement { get; set; } = string.Empty;
    public int NombreArrieres { get; set; }
    public decimal MontantTotal { get; set; }
    public decimal PourcentageTotal { get; set; }
  }
}
