using ProsocAPI.Models.DTOs.Statistiques;

namespace ProsocAPI.Services
{
    public interface IStatistiquesService
    {
        Task<StatistiquesGeneralesDto> GetGeneralesAsync(StatistiquesFiltresDto filtres, CancellationToken ct = default);
        Task<StatistiquesFinancieresDto> GetFinancieresAsync(StatistiquesFiltresDto filtres, CancellationToken ct = default);
        Task<StatistiquesOperationnellesDto> GetOperationnellesAsync(StatistiquesFiltresDto filtres, CancellationToken ct = default);
        Task<StatistiquesPerformanceDto> GetPerformanceAsync(StatistiquesFiltresDto filtres, CancellationToken ct = default);
        Task<StatistiquesConsolideesDto> GetConsolideesAsync(StatistiquesFiltresDto filtres, CancellationToken ct = default);
    }
}
