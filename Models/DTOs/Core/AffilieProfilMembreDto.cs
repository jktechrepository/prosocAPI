namespace ProsocAPI.Models.DTOs.Core
{
    /// <summary>Profil membre : affilié + personne de contact + synthèse adhésion (sans liste globale).</summary>
    public class AffilieProfilMembreDto
    {
        public AffilieReadDto Affilie { get; set; } = new();
        public PersonneContactReadDto? PersonneContact { get; set; }
        public int? AdhesionId { get; set; }
        public string? StatutDossier { get; set; }
        public string? TypeAdhesion { get; set; }
    }
}
