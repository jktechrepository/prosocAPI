using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IProduitAssureurRepository
    {
        Task<List<ProduitAssureur>> GetAllAsync(CancellationToken ct = default);
        Task<ProduitAssureur?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<ProduitAssureur>> GetByAssureurAsync(int assureurId, CancellationToken ct = default);
        Task<List<ProduitAssureur>> GetActivesAsync(CancellationToken ct = default);
        Task<ProduitAssureur> CreateAsync(ProduitAssureur entity, CancellationToken ct = default);
        Task<ProduitAssureur?> UpdateAsync(int id, ProduitAssureur entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
