using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace Prosoc.Utilities
{
    public static class WalletAgentHelpers
    {
        public static WalletAgent? ResolveWallet(Agent agent, int? deviseId = null)
        {
            if (agent.Wallets == null || agent.Wallets.Count == 0)
                return null;

            if (deviseId is > 0)
                return agent.Wallets.FirstOrDefault(w => w.DeviseId == deviseId.Value);

            return agent.Wallets.FirstOrDefault(w => w.Devise?.EstDevisePrincipale == true)
                ?? agent.Wallets.First();
        }

        public static WalletAgentReadDto ToReadDto(WalletAgent w) => new()
        {
            IdWalletAgent = w.IdWalletAgent,
            AgentId = w.AgentId,
            DeviseId = w.DeviseId,
            DeviseCode = w.Devise?.Code,
            DeviseNom = w.Devise?.Nom,
            DeviseSymbole = w.Devise?.Symbole,
            SoldeCourant = w.SoldeCourant,
            SoldeDisponible = w.SoldeDisponible,
            DateCreation = w.DateCreation,
            DateModification = w.DateModification,
            Statut = w.Statut,
            AgentNom = w.Agent?.NomComplet,
            AgentMatricule = w.Agent?.Matricule
        };
    }
}
