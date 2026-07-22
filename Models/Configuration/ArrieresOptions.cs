namespace ProsocAPI.Models.Configuration
{
    public class ArrieresOptions
    {
        public const string SectionName = "Arrieres";

        public bool GenerationAutomatiqueActivee { get; set; } = true;

        public int HeureExecution { get; set; } = 0;

        public int MinuteExecution { get; set; } = 30;

        public int IntervalleControleMinutes { get; set; } = 600;

        public int JourEcheanceMensuelle { get; set; } = 1;
    }
}
