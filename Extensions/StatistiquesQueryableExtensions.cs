using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Statistiques;

namespace ProsocAPI.Extensions
{
    public static class StatistiquesQueryableExtensions
    {
        public static IQueryable<Adhesion> AppliquerFiltresStatistiques(
            this IQueryable<Adhesion> query,
            StatistiquesFiltresDto filtres)
        {
            if (filtres.CategorieAdhesionId.HasValue)
            {
                var categorieId = filtres.CategorieAdhesionId.Value;
                query = query.Where(a => a.TypeAdhesion.CategorieAdhesionId == categorieId);
            }

            if (filtres.ZoneSocialeId.HasValue)
            {
                var zoneId = filtres.ZoneSocialeId.Value;
                query = query.Where(a => a.AgentCreateur != null && a.AgentCreateur.ZoneSocialeId == zoneId);
            }

            if (filtres.CommuneId.HasValue)
            {
                var communeId = filtres.CommuneId.Value;
                query = query.Where(a =>
                    a.AgentCreateur != null &&
                    a.AgentCreateur.Zone != null &&
                    a.AgentCreateur.Zone.CommuneId == communeId);
            }

            if (filtres.TypeAdhesionId.HasValue)
            {
                var typeId = filtres.TypeAdhesionId.Value;
                query = query.Where(a => a.TypeAdhesionId == typeId);
            }

            return query;
        }

        public static IQueryable<Collecte> AppliquerFiltresStatistiques(
            this IQueryable<Collecte> query,
            StatistiquesFiltresDto filtres)
        {
            if (filtres.ZoneSocialeId.HasValue)
            {
                var zoneId = filtres.ZoneSocialeId.Value;
                query = query.Where(c => c.Agent != null && c.Agent.ZoneSocialeId == zoneId);
            }

            if (filtres.CommuneId.HasValue)
            {
                var communeId = filtres.CommuneId.Value;
                query = query.Where(c =>
                    c.Agent != null &&
                    c.Agent.Zone != null &&
                    c.Agent.Zone.CommuneId == communeId);
            }

            if (filtres.TarifCotisationId.HasValue)
            {
                var tarifId = filtres.TarifCotisationId.Value;
                query = query.Where(c => c.CotisationAffilieId.HasValue && c.CotisationAffilieId.Value == tarifId);
            }

            return query;
        }

        public static IQueryable<ArrieresAffilie> AppliquerFiltresStatistiques(
            this IQueryable<ArrieresAffilie> query,
            StatistiquesFiltresDto filtres)
        {
            if (filtres.TarifCotisationId.HasValue)
            {
                var tarifId = filtres.TarifCotisationId.Value;
                query = query.Where(a => a.CotisationAffilieId.HasValue && a.CotisationAffilieId.Value == tarifId);
            }

            return query;
        }
    }
}
