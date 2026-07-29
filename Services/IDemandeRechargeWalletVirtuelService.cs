using System.Security.Claims;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;

namespace ProsocAPI.Services
{
    public interface IDemandeRechargeWalletVirtuelService
    {
        Task<PaginatedResponse<DemandeRechargeWalletVirtuelReadDto>> GetAllAsync(
            PaginationRequest request,
            string? statutDemande = null,
            CancellationToken ct = default);

        Task<List<DemandeRechargeWalletVirtuelReadDto>> GetEnAttenteAsync(CancellationToken ct = default);

        Task<DemandeRechargeWalletVirtuelReadDto?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<List<DemandeRechargeWalletVirtuelReadDto>> GetByAgentAsync(int agentId, CancellationToken ct = default);

        Task<DemandeRechargeWalletVirtuelOperationResultDto> CreerAsync(
            ClaimsPrincipal user,
            int demandeParUtilisateurId,
            DemandeRechargeWalletVirtuelCreateDto dto,
            CancellationToken ct = default);

        Task<DemandeRechargeWalletVirtuelOperationResultDto> ConfirmerAsync(
            ClaimsPrincipal user,
            int confirmeParUtilisateurId,
            int demandeId,
            CancellationToken ct = default);

        Task<DemandeRechargeWalletVirtuelOperationResultDto> RejeterAsync(
            int rejeteParUtilisateurId,
            int demandeId,
            DemandeRechargeWalletVirtuelRejeterDto dto,
            CancellationToken ct = default);
    }
}
