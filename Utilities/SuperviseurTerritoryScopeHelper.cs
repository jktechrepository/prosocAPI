using Microsoft.EntityFrameworkCore;
using Prosoc.Data;

namespace Prosoc.Utilities
{
    /// <summary>
    /// Périmètre Superviseur : tous les agents actifs des zones d'une commune titulaire.
    /// </summary>
    public static class SuperviseurTerritoryScopeHelper
    {
        public const string RoleName = "Superviseur";

        public static async Task<int?> GetCommuneIdForSuperviseurAsync(
            ProsocDbContext db,
            int agentId,
            CancellationToken ct = default)
        {
            return await db.Communes.AsNoTracking()
                .Where(c => c.SuperviseurAgentId == agentId)
                .Select(c => (int?)c.IdCommune)
                .FirstOrDefaultAsync(ct);
        }

        public static async Task<List<int>> GetAgentIdsDansCommuneAsync(
            ProsocDbContext db,
            int communeId,
            CancellationToken ct = default)
        {
            return await (
                from a in db.Agents.AsNoTracking()
                join z in db.ZonesSociales.AsNoTracking() on a.ZoneSocialeId equals z.IdZoneSociale
                where a.Statut && z.CommuneId == communeId
                select a.IdAgent
            ).Distinct().ToListAsync(ct);
        }
    }
}
