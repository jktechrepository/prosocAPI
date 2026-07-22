using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IProvinceRepository
    {
        Task<List<Province>> GetAllAsync(CancellationToken ct = default);
        Task<Province?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Province>> GetActivesAsync(CancellationToken ct = default);
        Task<Province> CreateAsync(Province entity, CancellationToken ct = default);
        Task<Province?> UpdateAsync(int id, Province entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
