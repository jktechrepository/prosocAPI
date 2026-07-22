using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using Prosoc.Data;
using Microsoft.EntityFrameworkCore;

namespace ProsocAPI.Utilities
{
    public static class TargetAgentRoleResolver
    {
        public static async Task<Role?> ResolveRoleByNomAsync(
            ProsocDbContext db,
            string roleNom,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(roleNom))
                return null;

            return await db.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Nom == roleNom.Trim() && r.Statut, ct);
        }

        public static async Task<int?> ResolveRoleIdForAgentAsync(
            ProsocDbContext db,
            int agentId,
            CancellationToken ct = default)
        {
            var roleId = await db.Utilisateurs
                .AsNoTracking()
                .Where(u => u.AgentId == agentId && u.RoleId != null)
                .Select(u => u.RoleId)
                .FirstOrDefaultAsync(ct);

            if (roleId.HasValue)
                return roleId;

            var roleNom = await db.Agents
                .AsNoTracking()
                .Where(a => a.IdAgent == agentId)
                .Select(a => a.RoleAgent)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrWhiteSpace(roleNom))
                return null;

            return await db.Roles
                .AsNoTracking()
                .Where(r => r.Nom == roleNom && r.Statut)
                .Select(r => (int?)r.IdRole)
                .FirstOrDefaultAsync(ct);
        }
    }
}
