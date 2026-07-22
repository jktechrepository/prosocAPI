using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IDeviseRepository
    {
        Task<List<Devise>> GetAllAsync(CancellationToken ct = default);
        Task<Devise?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Devise?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<List<Devise>> GetActivesAsync(CancellationToken ct = default);
        Task<Devise> CreateAsync(Devise entity, CancellationToken ct = default);
        Task<Devise?> UpdateAsync(int id, Devise entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
