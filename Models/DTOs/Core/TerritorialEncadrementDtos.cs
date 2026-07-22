using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    public class TerritorialAffectationDto
    {
        [Range(1, int.MaxValue)]
        public int AgentId { get; set; }
    }

    public class TerritorialAffectationResultDto
    {
        public int TerritoryId { get; set; }
        public int? PreviousAgentId { get; set; }
        public string? PreviousAgentNom { get; set; }
        public int? NewAgentId { get; set; }
        public string? NewAgentNom { get; set; }
    }
}
