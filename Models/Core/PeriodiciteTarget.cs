using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProsocAPI.Models.Core
{
    [JsonConverter(typeof(PeriodiciteTargetConverter))]
    public enum PeriodiciteTarget
    {
        Journaliere = 1,
        Hebdomadaire = 2,
        Mensuelle = 3
    }

    public static class PeriodiciteTargetRegles
    {
        public static int GetNombreAdhesions(PeriodiciteTarget periodicite) => periodicite switch
        {
            PeriodiciteTarget.Journaliere => 5,
            PeriodiciteTarget.Hebdomadaire => 25,
            PeriodiciteTarget.Mensuelle => 100,
            _ => throw new ArgumentOutOfRangeException(nameof(periodicite), periodicite, null)
        };
    }

    public class PeriodiciteTargetConverter : JsonConverter<PeriodiciteTarget>
    {
        public override PeriodiciteTarget Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var num))
            {
                return num switch
                {
                    1 => PeriodiciteTarget.Journaliere,
                    2 => PeriodiciteTarget.Hebdomadaire,
                    3 => PeriodiciteTarget.Mensuelle,
                    _ => throw new JsonException($"Valeur PeriodiciteTarget invalide: {num}")
                };
            }

            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                throw new JsonException("Periodicite est requis.");

            return value.Trim().ToUpperInvariant() switch
            {
                "JOURNALIERE" or "JOURNALIER" or "1" => PeriodiciteTarget.Journaliere,
                "HEBDOMADAIRE" or "HEBDO" or "2" => PeriodiciteTarget.Hebdomadaire,
                "MENSUELLE" or "MENSUEL" or "3" => PeriodiciteTarget.Mensuelle,
                _ => throw new JsonException(
                    $"Valeur PeriodiciteTarget invalide: {value}. Valeurs valides: Journaliere, Hebdomadaire, Mensuelle")
            };
        }

        public override void Write(Utf8JsonWriter writer, PeriodiciteTarget value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
