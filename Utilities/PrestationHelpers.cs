using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace Prosoc.Utilities
{
    public static class PrestationHelpers
    {
        public static bool EstGratuite(Prestation p) =>
            p.ProduitMutuel?.EstGratuit == true || p.ProduitAssureur?.EstGratuit == true;

        public static IQueryable<Prestation> FilterGratuitesActives(IQueryable<Prestation> query) =>
            query.Where(p => p.Statut
                && ((p.ProduitMutuel != null && p.ProduitMutuel.EstGratuit && p.ProduitMutuel.Statut)
                    || (p.ProduitAssureur != null && p.ProduitAssureur.EstGratuit && p.ProduitAssureur.Statut)));

        public static PrestationReadDto ToReadDto(Prestation p) => new()
        {
            Id = p.IdPrestation,
            NomPrestation = p.NomPrestation,
            Description = p.Description,
            Periodicite = p.Periodicite,
            Montant = (double?)p.Montant,
            DeviseId = p.DeviseId,
            DeviseCode = p.Devise?.Code,
            ProduitMutuelId = p.ProduitMutuelId,
            ProduitMutuelNom = p.ProduitMutuel?.Nom,
            ProduitAssureurId = p.ProduitAssureurId,
            ProduitAssureurNom = p.ProduitAssureur?.Nom,
            EstGratuit = EstGratuite(p)
        };
    }
}
