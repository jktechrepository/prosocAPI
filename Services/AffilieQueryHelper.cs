using Microsoft.EntityFrameworkCore;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services;

public static class AffilieQueryHelper
{
    public static IQueryable<Affilie> WithAssociations(IQueryable<Affilie> query) =>
        query
            .AsSplitQuery()
            .Include(a => a.Dependants)
                .ThenInclude(d => d.Antecedants)
            .Include(a => a.Antecedants)
            .Include(a => a.PersonneContact);
}
