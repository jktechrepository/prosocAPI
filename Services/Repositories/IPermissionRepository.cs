using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Services.Repositories
{
    public interface IPermissionRepository
    {
        Task<List<Permission>> GetAllAsync(CancellationToken ct = default);
        Task<Permission?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Permission> CreateAsync(Permission entity, CancellationToken ct = default);
        Task<Permission?> UpdateAsync(int id, Permission entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
