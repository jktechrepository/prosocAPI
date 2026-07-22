using Prosoc.Utilities;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services;

public static class AgentDtoMapper
{
    public static AgentReadDto ToReadDto(Agent a) => new()
    {
        Id = a.IdAgent,
        NomComplet = a.NomComplet,
        Matricule = a.Matricule,
        Phone = a.Phone,
        EmailAgent = a.EmailAgent,
        Fonction = a.Fonction,
        RoleAgent = a.RoleAgent,
        PhotoUrl = a.PhotoUrl,
        DateCreation = a.DateCreation,
        DateModification = a.DateModification,
        Statut = a.Statut,
        ZoneSocialeId = a.ZoneSocialeId,
        ZoneSocialeNom = a.Zone?.Nom,
        CategorieAgentId = a.CategorieAgentId,
        CategorieAgentCode = a.CategorieAgent?.Code,
        CategorieAgentDescription = a.CategorieAgent?.Description,
        WalletId = WalletAgentHelpers.ResolveWallet(a)?.IdWalletAgent,
        WalletSolde = WalletAgentHelpers.ResolveWallet(a)?.SoldeCourant ?? 0,
        WalletCree = WalletAgentHelpers.ResolveWallet(a) != null,
        WalletVirtuelId = a.WalletVirtuel?.IdWalletVirtuelAgent,
        WalletVirtuelSolde = a.WalletVirtuel?.SoldeVirtuel ?? 0,
        WalletVirtuelCree = a.WalletVirtuel != null
    };
}
