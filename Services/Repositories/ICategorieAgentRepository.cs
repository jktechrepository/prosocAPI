using ProsocAPI.Models.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProsocAPI.Services.Repositories
{
    public interface ICategorieAgentRepository
    {
        Task<IEnumerable<CategorieAgent>> GetAllAsync(CancellationToken ct = default);
        Task<CategorieAgent?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<CategorieAgent?> GetByLibelleAsync(string libelle, CancellationToken ct = default);
        Task<CategorieAgent?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<IEnumerable<CategorieAgent>> GetByStatutAsync(bool statut, CancellationToken ct = default);
        Task<CategorieAgent> CreateAsync(CategorieAgent categorieAgent, CancellationToken ct = default);
        Task<bool> UpdateAsync(CategorieAgent categorieAgent, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<bool> ExistsByIdAsync(int id, CancellationToken ct = default);
        Task<bool> ExistsByLibelleAsync(string libelle, CancellationToken ct = default);
        Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
    }
}
