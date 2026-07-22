namespace ProsocAPI.Exceptions
{
    /// <summary>
    /// Levée lorsqu'un agent a le rôle Superviseur mais n'est titulaire d'aucune commune
    /// (<see cref="ProsocAPI.Models.Core.Commune.SuperviseurAgentId"/>).
    /// </summary>
    public class SuperviseurSansCommuneTitulaireException : Exception
    {
        public int SuperviseurAgentId { get; }

        public SuperviseurSansCommuneTitulaireException(int superviseurAgentId)
            : base(
                $"Superviseur {superviseurAgentId} non titulaire d'une commune : " +
                "la hiérarchie legacy Agent.SuperviseurId n'est plus supportée.")
        {
            SuperviseurAgentId = superviseurAgentId;
        }
    }
}
