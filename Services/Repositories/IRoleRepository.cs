using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Services.Repositories
{
    public interface IRoleRepository
    {
        Task<List<Role>> GetAllAsync(CancellationToken ct = default);
        Task<Role?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Role> CreateAsync(Role entity, CancellationToken ct = default);
        Task<Role?> UpdateAsync(int id, Role entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
