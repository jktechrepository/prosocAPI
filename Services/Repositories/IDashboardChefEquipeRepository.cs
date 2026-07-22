using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IDashboardChefEquipeRepository
    {
        Task<ChefEquipeKpisDto> GetKpisAsync(int chefAgentId, CancellationToken ct = default);

        Task<List<ChefEquipeAgentResumeDto>> GetAgentsZoneAsync(int chefAgentId, CancellationToken ct = default);

        Task<AgentCommissionsResumeDto> GetMouvementsWalletAgentAsync(
            int chefAgentId,
            int targetAgentId,
            int limit,
            CancellationToken ct = default);

        Task<List<ChefEquipeCollecteResumeDto>> GetCollectesAgentAsync(
            int chefAgentId,
            int targetAgentId,
            int limit,
            CancellationToken ct = default);
    }
}
