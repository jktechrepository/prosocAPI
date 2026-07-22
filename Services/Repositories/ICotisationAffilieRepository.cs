using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface ITarifCotisationRepository
    {
        Task<List<TarifCotisation>> GetAllAsync(CancellationToken ct = default);
        Task<TarifCotisation?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<TarifCotisation>> GetByTypeAdhesionIdAsync(int typeAdhesionId, CancellationToken ct = default);
        Task<List<TarifCotisation>> GetByAffilieIdAsync(int affilieId, CancellationToken ct = default);
        Task<TarifCotisation> CreateAsync(TarifCotisation entity, CancellationToken ct = default);
        Task<TarifCotisation?> UpdateAsync(int id, TarifCotisation entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }

    [Obsolete("Use ITarifCotisationRepository instead.")]
    public interface ICotisationAffilieRepository : ITarifCotisationRepository
    {
    }
}
