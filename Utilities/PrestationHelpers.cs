using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace Prosoc.Utilities
{
    public static class PrestationHelpers
    {
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
            ProduitAssureurNom = p.ProduitAssureur?.Nom
        };
    }
}
