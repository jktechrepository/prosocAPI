using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IJetonMedicalRepository
    {
        Task<List<JetonMedical>> GetAllAsync(CancellationToken ct = default);
        Task<JetonMedical?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<JetonMedical?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<List<JetonMedical>> GetByAffilieAsync(int affilieId, CancellationToken ct = default);
        Task<List<JetonMedical>> GetByHopitalAsync(int hopitalId, CancellationToken ct = default);
        Task<List<JetonMedical>> GetValidesAsync(CancellationToken ct = default);
        Task<List<JetonMedical>> GetExpiresAsync(CancellationToken ct = default);
        Task<List<JetonMedical>> GetUtilisesAsync(CancellationToken ct = default);
        Task<JetonMedical> CreateAsync(JetonMedical entity, CancellationToken ct = default);
        Task<JetonMedical?> UpdateAsync(int id, JetonMedical entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<bool> ValiderJetonAsync(string code, int hopitalId, CancellationToken ct = default);
        Task<bool> UtiliserJetonAsync(int id, string observation, CancellationToken ct = default);
        Task<JetonMedicalStatsDto> GetStatsAsync(DateTime date, CancellationToken ct = default);
        Task<List<JetonMedical>> GetJetonsAArchiverAsync(CancellationToken ct = default);
        Task<bool> ArchiverJetonsExpiresAsync(CancellationToken ct = default);
    }
}
