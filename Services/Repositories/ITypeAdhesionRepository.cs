using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface ITypeAdhesionRepository
    {
        Task<List<TypeAdhesion>> GetAllAsync(CancellationToken ct = default);
        Task<TypeAdhesion?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<TypeAdhesion> CreateAsync(TypeAdhesion entity, CancellationToken ct = default);
        Task<TypeAdhesion?> UpdateAsync(int id, TypeAdhesion entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
