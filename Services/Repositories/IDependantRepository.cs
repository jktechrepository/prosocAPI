using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IDependantRepository
    {
        Task<List<Dependant>> GetAllAsync(CancellationToken ct = default);
        Task<Dependant?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Dependant> CreateAsync(Dependant entity, CancellationToken ct = default);
        Task<Dependant?> UpdateAsync(int id, Dependant entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
