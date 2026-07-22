namespace ProsocAPI.Utilities
{
    public static class AdhesionAgentIdHelper
    {
        /// <summary>Adhésion en ligne (FlexPay) : null. Terrain : agent valide obligatoire.</summary>
        public static int? ResolveAdhesionAgentId(int? inputAgentId, bool isOnlineFlexPay)
        {
            if (isOnlineFlexPay)
                return null;

            if (!inputAgentId.HasValue || inputAgentId.Value <= 0)
                return null;

            return inputAgentId.Value;
        }

        /// <summary>Collecte : null tant qu'aucun gestionnaire AT n'est affecté.</summary>
        public static int? ResolveCollecteAgentId(int? adhesionAgentId) =>
            adhesionAgentId;

        public static bool IsTerrainAgentRequired(int? inputAgentId) =>
            inputAgentId.HasValue && inputAgentId.Value > 0;
    }
}
