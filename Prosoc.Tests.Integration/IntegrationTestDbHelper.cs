using Microsoft.EntityFrameworkCore;
using Prosoc.Data;

namespace Prosoc.Tests.Integration;

internal static class IntegrationTestDbHelper
{
    public static async Task<int> GetPrincipalDeviseIdAsync(ProsocDbContext db, CancellationToken ct = default)
    {
        return await db.Devises
            .Where(d => d.EstDevisePrincipale && d.Statut)
            .Select(d => d.IdDevise)
            .FirstAsync(ct);
    }
}
