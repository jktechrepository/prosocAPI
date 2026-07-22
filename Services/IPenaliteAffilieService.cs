using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
  public interface IPenaliteAffilieService
  {
    Task<List<PenaliteAffilie>> AppliquerPenalitesRetardCotisationAsync(DateTime date, CancellationToken ct = default);
    Task<PenaliteAffilie?> ProcessCollecteForPenaliteAsync(Collecte collecte, CancellationToken ct = default);
    Task<List<PenaliteAffilie>> GetByAffilieAsync(int affilieId, CancellationToken ct = default);
    Task<List<PenaliteAffilie>> GetByArriereAsync(int arrieresAffilieId, CancellationToken ct = default);
    Task<PenaliteAffilie> AnnulerPenaliteAsync(int id, string motifAnnulation, CancellationToken ct = default);
    Task<PenaliteResumeDto> GetResumeAsync(CancellationToken ct = default);
  }

  public class PenaliteResumeDto
  {
    public int TotalPenalites { get; set; }
    public int TotalAppliquees { get; set; }
    public int TotalPayees { get; set; }
    public int TotalAnnulees { get; set; }
    public decimal MontantTotalDu { get; set; }
    public int AffiliesConcernes { get; set; }
  }
}
