using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class SessionCaisseOuvrirDto
    {
        [Required]
        [Range(0, double.MaxValue)]
        public decimal SoldeOuverture { get; set; }
    }

    public class SessionCaisseCloturerDto
    {
        [Required]
        [Range(0, double.MaxValue)]
        public decimal SoldeReelCloture { get; set; }

        [StringLength(500)]
        public string? ObservationCloture { get; set; }
    }

    public class SessionCaisseReadDto
    {
        public int IdSessionCaisse { get; set; }
        public int UtilisateurId { get; set; }
        public decimal SoldeOuverture { get; set; }
        public decimal SoldeCourant { get; set; }
        public int DeviseId { get; set; }
        public string? DeviseCode { get; set; }
        public string Statut { get; set; } = string.Empty;
        public DateTime DateOuverture { get; set; }
        public DateTime? DateCloture { get; set; }
        public decimal? SoldeTheoriqueCloture { get; set; }
        public decimal? SoldeReelCloture { get; set; }
        public string? ObservationCloture { get; set; }
    }

    public class MouvementCaisseReadDto
    {
        public int IdMouvementCaisse { get; set; }
        public int SessionCaisseId { get; set; }
        public string TypeOperation { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public decimal Montant { get; set; }
        public int DeviseId { get; set; }
        public string? DeviseCode { get; set; }
        public DateTime DateOperation { get; set; }
        public int? CollecteId { get; set; }
        public int? DemandeRetraitId { get; set; }
        public int? JetonRetraitId { get; set; }
        public string? Description { get; set; }
    }

    public class SessionCaisseSoldeDto
    {
        public int IdSessionCaisse { get; set; }
        public decimal SoldeOuverture { get; set; }
        public decimal TotalEntrees { get; set; }
        public decimal TotalSorties { get; set; }
        public decimal SoldeCourant { get; set; }
        public string? DeviseCode { get; set; }
    }
}
