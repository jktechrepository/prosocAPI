using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface ICategorieAdhesionRepository
    {
        Task<List<CategorieAdhesion>> GetAllAsync(CancellationToken ct = default);
        Task<CategorieAdhesion?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<CategorieAdhesion>> GetActivesAsync(CancellationToken ct = default);
        Task<CategorieAdhesion> CreateAsync(CategorieAdhesion entity, CancellationToken ct = default);
        Task<CategorieAdhesion?> UpdateAsync(int id, CategorieAdhesion entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
