namespace ProsocAPI.Models.DTOs.Statistiques
{
    public class StatistiquesFiltresDto
    {
        public int? CategorieAdhesionId { get; set; }
        public int? CommuneId { get; set; }
        public int? ZoneSocialeId { get; set; }
        public int? TypeAdhesionId { get; set; }
        public int? TarifCotisationId { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }
}
