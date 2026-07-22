using ProsocAPI.Models.DTOs.DashboardAgentAa;

namespace ProsocAPI.Services.Repositories
{
    public interface IDashboardAgentAARepository
    {
        Task<AgentAaKpisDto> GetKpisAsync(int agentId, CancellationToken ct = default);

        Task<List<AgentAaDossierDto>> GetDossiersATraiterAsync(
            int agentId, int limit = 50, CancellationToken ct = default);

        Task<List<AgentAaDependantRecentDto>> GetDependantsRecentsAsync(
            int agentId, int limit = 50, CancellationToken ct = default);

        Task<List<AgentAaAntecedentRecentDto>> GetAntecedentsRecentsAsync(
            int agentId, int limit = 50, CancellationToken ct = default);

        Task<List<AgentAaRepartitionStatutDto>> GetRepartitionStatutsAsync(
            int agentId, CancellationToken ct = default);

        Task<DashboardAgentAaDto> GetDashboardSummaryAsync(int agentId, CancellationToken ct = default);
    }
}
