using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface ICommuneRepository
    {
        Task<List<Commune>> GetAllAsync(CancellationToken ct = default);
        Task<Commune?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Commune>> GetByProvinceAsync(int provinceId, CancellationToken ct = default);
        Task<Commune> CreateAsync(Commune entity, CancellationToken ct = default);
        Task<Commune?> UpdateAsync(int id, Commune entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
