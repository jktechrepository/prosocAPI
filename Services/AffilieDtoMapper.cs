using Prosoc.Utilities;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services;

public static class AffilieDtoMapper
{
    public static AffilieReadDto ToReadDto(Affilie a) => new()
    {
        IdAffilie = a.IdAffilie,
        CodeAdhesion = a.CodeAdhesion,
        Nom = a.Nom,
        Prenom = a.Prenom,
        Postnom = a.Postnom,
        NomComplet = a.NomComplet,
        DateNaissance = a.DateNaissance,
        Telephone = a.Telephone,
        EmailAffilie = a.EmailAffilie,
        ProvinceResidence = a.ProvinceResidence,
        CommuneResidence = a.CommuneResidence,
        QuartierResidence = a.QuartierResidence,
        AvenueResidence = a.AvenueResidence,
        NumeroResidence = a.NumeroResidence,
        CommuneActivite = a.CommuneActivite,
        QuartierActivite = a.QuartierActivite,
        AvenueActivite = a.AvenueActivite,
        NumeroActivite = a.NumeroActivite,
        HasPhoto = AffilieFichierHelper.ADesDonnees(a.PhotoData),
        HasCarteIdentite = AffilieFichierHelper.ADesDonnees(a.CarteIdentiteData),
        PhotoBase64 = AffilieFichierHelper.VersBase64(a.PhotoData),
        PhotoUrl = AffilieFichierHelper.VersBase64(a.PhotoData),
        CarteIdentiteBase64 = AffilieFichierHelper.VersBase64(a.CarteIdentiteData),
        PhotoContentType = a.PhotoContentType,
        CarteIdentiteContentType = a.CarteIdentiteContentType,
        DateCreation = a.DateCreation,
        DateModification = a.DateModification,
        Statut = a.Statut,
        Dependants = (a.Dependants ?? Enumerable.Empty<Dependant>())
            .OrderBy(d => d.IdDependant)
            .Select(DependantDtoMapper.ToReadDto)
            .ToList(),
        Antecedants = (a.Antecedants ?? Enumerable.Empty<Antecedant>())
            .OrderByDescending(x => x.DateCreation)
            .Select(x => x.ToReadDto())
            .ToList(),
        PersonneContact = MapPersonneContact(a.PersonneContact)
    };

    public static PersonneContactReadDto? MapPersonneContact(PersonneContact? p) =>
        p == null
            ? null
            : new PersonneContactReadDto
            {
                IdPersonneContact = p.IdPersonneContact,
                AffilieId = p.AffilieId,
                NomComplet = p.NomComplet,
                LienParente = p.LienParente,
                Adresse = p.Adresse,
                Statut = p.Statut
            };
}
