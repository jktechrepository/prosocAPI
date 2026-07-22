namespace ProsocAPI.Services
{
    public class AdhesionAlreadyExistsException : Exception
    {
        public AdhesionAlreadyExistsException(int affilieId)
            : base($"Une adhésion existe déjà pour l'affilié {affilieId}.")
        {
            AffilieId = affilieId;
        }

        public int AffilieId { get; }
    }
}
