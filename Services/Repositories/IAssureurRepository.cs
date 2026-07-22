using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IAssureurRepository
    {
        Task<List<Assureur>> GetAllAsync(CancellationToken ct = default);
        Task<Assureur?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Assureur>> GetActivesAsync(CancellationToken ct = default);
        Task<Assureur> CreateAsync(Assureur entity, CancellationToken ct = default);
        Task<Assureur?> UpdateAsync(int id, Assureur entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
