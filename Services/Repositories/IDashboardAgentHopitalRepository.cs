using ProsocAPI.Models.DTOs.DashboardAgentHopital;

namespace ProsocAPI.Services.Repositories
{
    public interface IDashboardAgentHopitalRepository
    {
        Task<HopitalKpisDto> GetKpisAsync(int hopitalPartenaireId, CancellationToken ct = default);

        Task<List<HopitalJetonEnAttenteDto>> GetJetonsEnAttenteAsync(
            int hopitalPartenaireId, int limit = 50, CancellationToken ct = default);

        Task<List<HopitalBonRecentDto>> GetBonsRecentsAsync(
            int hopitalPartenaireId, int limit = 50, CancellationToken ct = default);

        Task<List<HopitalPatientDto>> GetPatientsAsync(
            int hopitalPartenaireId, int limit = 50, CancellationToken ct = default);

        Task<List<HopitalDependantDto>> GetDependantsAsync(
            int hopitalPartenaireId, int limit = 100, CancellationToken ct = default);

        Task<List<HopitalAntecedentDto>> GetAntecedentsAsync(
            int hopitalPartenaireId, int limit = 100, CancellationToken ct = default);

        Task<List<HopitalRepartitionPrestationDto>> GetRepartitionPrestationsAsync(
            int hopitalPartenaireId, CancellationToken ct = default);

        Task<DashboardAgentHopitalDto> GetDashboardSummaryAsync(
            int hopitalPartenaireId, CancellationToken ct = default);
    }
}
