using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;

namespace ProsocAPI.Services
{
    public interface IAffilieConformiteService
    {
        Task<AffilieConformiteDto?> GetConformiteAffilieAsync(int affilieId, CancellationToken ct = default);

        Task<Dictionary<int, AffilieConformiteDto>> GetConformiteParAffiliesAsync(
            IEnumerable<int> affilieIds,
            CancellationToken ct = default);

        Task<PaginatedResponse<AffilieConformiteDto>> GetConformiteListeAsync(
            AffilieConformiteFiltreDto filtres,
            PaginationRequest pagination,
            CancellationToken ct = default);
    }
}
