using ProsocAPI.Models.Core;

namespace ProsocAPI.Models.DTOs.Core
{
    public static class AntecedentExtensions
    {
        public static AntecedentReadDto ToReadDto(this Antecedant antecedent)
        {
            return new AntecedentReadDto
            {
                IdAntecedant = antecedent.IdAntecedant,
                Description = antecedent.Description,
                AffilieId = antecedent.AffilieId,
                AffilieNom = antecedent.Affilie != null 
                    ? $"{antecedent.Affilie.Nom} {antecedent.Affilie.Prenom}".Trim()
                    : string.Empty,
                DependantId = antecedent.DependantId,
                DependantNom = antecedent.Dependant?.Nom,
                DateCreation = antecedent.DateCreation,
                DateModification = antecedent.DateModification,
                Statut = antecedent.Statut
            };
        }

        public static Antecedant ToEntity(this AntecedentCreateDto dto)
        {
            return new Antecedant
            {
                Description = dto.Description,
                AffilieId = dto.AffilieId,
                DependantId = dto.DependantId,
                Statut = dto.Statut,
                DateCreation = DateTime.Now
            };
        }

        public static Antecedant ToEntity(this AntecedentUpdateDto dto)
        {
            return new Antecedant
            {
                Description = dto.Description,
                AffilieId = dto.AffilieId,
                DependantId = dto.DependantId,
                Statut = dto.Statut,
                DateModification = DateTime.Now
            };
        }
    }
}
