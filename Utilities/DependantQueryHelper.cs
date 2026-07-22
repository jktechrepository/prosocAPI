using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;

namespace Prosoc.Utilities;

public static class DependantQueryHelper
{
    public static IQueryable<Dependant> GetByAffilieQuery(ProsocDbContext db, int affilieId) =>
        db.Dependants
            .AsNoTracking()
            .Include(d => d.Affilie)
            .Include(d => d.Antecedants)
                .ThenInclude(a => a.Affilie)
            .Where(d => d.AffilieId == affilieId);
}
