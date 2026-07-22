namespace ProsocAPI.Exceptions
{
    public class AdhesionNotFoundException : Exception
    {
        public AdhesionNotFoundException(int id) : base($"Adhésion {id} non trouvée") { }
    }

    public class AdhesionNotInWaitingStateException : Exception
    {
        public AdhesionNotInWaitingStateException(int id) 
            : base($"Adhésion {id} n'est pas en état 'EN ATTENTE'") { }
    }

    public class AffilieDuplicateException : Exception
    {
        public AffilieDuplicateException(int existingAffilieId, string message) 
            : base(message) 
        { 
            ExistingAffilieId = existingAffilieId; 
        }
        
        public int ExistingAffilieId { get; }
    }

    public class AdresseAffilieIncompleteException : Exception
    {
        public AdresseAffilieIncompleteException(string message) : base(message) { }
    }
}
