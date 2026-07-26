using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProsocAPI.Models.Core
{
    public class Adhesion
    {
        [Key]
        public int IdAdhesion { get; set; }
        
        [Required, StringLength(20)]
        /// <summary>Canon : <c>EN ATTENTE</c> | <c>VALIDÉ</c> (voir <see cref="AdhesionStatutDossierRegles"/>).</summary>
        public string StatutDossier { get; set; } = string.Empty;

        public int? AgentId { get; set; }

        public int AffilieId { get; set; }
        
        public int TypeAdhesionId { get; set; }
        
        public int? UtilisateurId { get; set; }
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        public DateTime? DateModification { get; set; }
        
        public bool Statut { get; set; } = true;

        
        [ForeignKey("TypeAdhesionId")]
        [JsonIgnore]
        [ValidateNever]
        public TypeAdhesion TypeAdhesion { get; set; } = null!;
        
        [ForeignKey("AgentId")]
        [InverseProperty("AdhesionsCrees")]
        [JsonIgnore]
        [ValidateNever]
        public Agent? AgentCreateur { get; set; }


        [ForeignKey("AffilieId")]
        [JsonIgnore]
        [ValidateNever]
        public Affilie Affilie { get; set; } = null!;

        // Navigation properties pour l'utilisateur (null = adhésion FlexPay anonyme)
        [ForeignKey("UtilisateurId")]
        [JsonIgnore]
        [ValidateNever]
        public virtual Models.Authentication.Utilisateur? Utilisateur { get; set; }

        // Navigation properties
    }
}
