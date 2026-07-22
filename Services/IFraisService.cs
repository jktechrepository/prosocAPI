using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public interface IFraisService
    {
        Task<Frais> CreateAsync(Frais frais);
        Task<Frais?> GetByIdAsync(int id);
        Task<Frais?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<Frais?> GetByIdWithCollectesAsync(int id); // NOUVEAU
        Task<List<Frais>> GetAllAsync();
        Task<List<Frais>> GetByDeviseAsync(int deviseId);
        Task<Frais> UpdateAsync(Frais frais);
        Task<bool> DeleteAsync(int id);
        Task<double> GetTotalByDeviseAsync(int deviseId);
        Task<List<Frais>> GetActiveByDeviseAsync(int deviseId);
        Task<bool> ExistsAsync(int id);
        
        // NOUVEAU : Méthodes pour les collectes associées
        Task<List<Collecte>> GetCollectesByFraisAsync(int fraisId);
        Task<double> GetTotalCollectesByFraisAsync(int fraisId);
        Task<int> GetCountCollectesByFraisAsync(int fraisId);
        Task<Dictionary<int, int>> GetCollectesStatsByFraisAsync();
    }
}
