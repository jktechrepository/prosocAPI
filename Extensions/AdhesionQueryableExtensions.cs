using ProsocAPI.Models.Core;

namespace ProsocAPI.Extensions
{
    public static class AdhesionQueryableExtensions
    {
        public static IQueryable<Adhesion> ApplyAdhesionSearch(
            this IQueryable<Adhesion> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var term = search.Trim().ToLower();

            if (int.TryParse(term, out var id))
            {
                return query.Where(a => a.IdAdhesion == id || a.AffilieId == id);
            }

            return query.Where(a =>
                a.StatutDossier.ToLower().Contains(term)
                || a.Affilie.NomComplet.ToLower().Contains(term)
                || a.Affilie.CodeAdhesion.ToLower().Contains(term)
                || a.Affilie.Nom.ToLower().Contains(term)
                || a.Affilie.Prenom.ToLower().Contains(term)
                || (a.Affilie.Postnom != null && a.Affilie.Postnom.ToLower().Contains(term))
                || (a.Affilie.Telephone != null && a.Affilie.Telephone.ToLower().Contains(term))
                || (a.Affilie.EmailAffilie != null && a.Affilie.EmailAffilie.ToLower().Contains(term))
                || a.TypeAdhesion.Libelle.ToLower().Contains(term));
        }
    }
}
