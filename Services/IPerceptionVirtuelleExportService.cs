using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services
{
    public interface IPerceptionVirtuelleExportService
    {
        Task<byte[]> ExportRapportAsync(
            DateTime? dateDebut,
            DateTime? dateFin,
            string? origine,
            string? statut,
            int? agentId,
            int? affilieId,
            CancellationToken ct = default);
    }
}
