using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services
{
    public interface ITerritorialEncadrementService
    {
        Task<TerritorialAffectationResultDto> AssignChefEquipeAsync(
            int zoneId,
            int agentId,
            int? assignedByUserId = null,
            CancellationToken ct = default);

        Task<TerritorialAffectationResultDto> ClearChefEquipeAsync(
            int zoneId,
            int? assignedByUserId = null,
            CancellationToken ct = default);

        Task<TerritorialAffectationResultDto> AssignSuperviseurAsync(
            int communeId,
            int agentId,
            int? assignedByUserId = null,
            CancellationToken ct = default);

        Task<TerritorialAffectationResultDto> ClearSuperviseurAsync(
            int communeId,
            int? assignedByUserId = null,
            CancellationToken ct = default);

        Task ReleaseTitularitesForAgentAsync(int agentId, CancellationToken ct = default);
    }
}
