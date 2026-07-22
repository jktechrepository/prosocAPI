using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IAntecedentRepository
    {
        Task<List<Antecedant>> GetAllAsync(CancellationToken ct = default);
        Task<Antecedant?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Antecedant>> GetByAffilieAsync(int affilieId, CancellationToken ct = default);
        Task<List<Antecedant>> GetActifsAsync(CancellationToken ct = default);
        Task<Antecedant> CreateAsync(Antecedant entity, CancellationToken ct = default);
        Task<Antecedant?> UpdateAsync(int id, Antecedant entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
