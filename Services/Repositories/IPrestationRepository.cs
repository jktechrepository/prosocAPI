using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IPrestationRepository
    {
        Task<List<Prestation>> GetAllAsync(CancellationToken ct = default);
        Task<Prestation?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Prestation>> GetByProduitMutuelAsync(int produitMutuelId, CancellationToken ct = default);
        Task<List<Prestation>> GetByProduitAssureurAsync(int produitAssureurId, CancellationToken ct = default);
        IQueryable<Prestation> GetGratuitesQueryable();
        Task<Prestation> CreateAsync(Prestation entity, CancellationToken ct = default);
        Task<Prestation?> UpdateAsync(int id, Prestation entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
