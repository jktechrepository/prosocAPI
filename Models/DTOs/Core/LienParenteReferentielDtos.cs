namespace ProsocAPI.Models.DTOs.Core
{
    public class LienParenteReferentielItemDto
    {
        public string Code { get; set; } = string.Empty;
        public string Libelle { get; set; } = string.Empty;
        public string Categorie { get; set; } = string.Empty;
    }

    public class LienParenteReferentielDto
    {
        public List<LienParenteReferentielItemDto> Liens { get; set; } = new();
        /// <summary>Codes déclenchant les règles d'âge enfant (certificat scolarité 18–25 ans).</summary>
        public string[] LiensEnfant { get; set; } = Array.Empty<string>();
        /// <summary>Codes conjoint (âge minimum 15 ans si date de naissance fournie).</summary>
        public string[] LiensConjoint { get; set; } = Array.Empty<string>();
    }
}
