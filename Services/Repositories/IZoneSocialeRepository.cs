using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IZoneSocialeRepository
    {
        Task<List<ZoneSociale>> GetAllAsync(CancellationToken ct = default);
        Task<ZoneSociale?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<ZoneSociale>> GetByCommuneAsync(int communeId, CancellationToken ct = default);
        Task<ZoneSociale> CreateAsync(ZoneSociale entity, CancellationToken ct = default);
        Task<ZoneSociale?> UpdateAsync(int id, ZoneSociale entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
