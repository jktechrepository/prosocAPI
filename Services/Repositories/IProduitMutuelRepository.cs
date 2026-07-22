using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IProduitMutuelRepository
    {
        Task<List<ProduitMutuel>> GetAllAsync(CancellationToken ct = default);
        Task<ProduitMutuel?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<ProduitMutuel>> GetActivesAsync(CancellationToken ct = default);
        Task<ProduitMutuel> CreateAsync(ProduitMutuel entity, CancellationToken ct = default);
        Task<ProduitMutuel?> UpdateAsync(int id, ProduitMutuel entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
