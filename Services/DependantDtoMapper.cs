using Prosoc.Utilities;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services;

public static class DependantDtoMapper
{
    public static DependantReadDto ToReadDto(Dependant d) => new()
    {
        IdDependant = d.IdDependant,
        Nom = d.Nom,
        Adresse = d.Adresse,
        LienParente = d.LienParente,
        AffilieId = d.AffilieId,
        DateNaissance = d.DateNaissance,
        Telephone = d.Telephone,
        DateCreation = d.DateCreation,
        DateModification = d.DateModification,
        Statut = d.Statut,
        PossedeCertificatScolarite = AffilieFichierHelper.ADesDonnees(d.CertificatScolariteData),
        CertificatScolariteBase64 = AffilieFichierHelper.VersBase64(d.CertificatScolariteData),
        CertificatScolariteContentType = d.CertificatScolariteContentType,
        Antecedants = (d.Antecedants ?? Enumerable.Empty<Antecedant>())
            .OrderByDescending(a => a.DateCreation)
            .Select(a => a.ToReadDto())
            .ToList()
    };
}
