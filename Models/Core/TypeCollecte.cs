using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProsocAPI.Models.Core
{
    /// <summary>
    /// Définit les différents types de collecte possibles
    /// </summary>
    [JsonConverter(typeof(TypeCollecteConverter))]
    public enum TypeCollecte
    {
        /// <summary>
        /// Collecte liée à un frais (adhésion, carte membre, etc.)
        /// </summary>
        Frais = 1,
        
        /// <summary>
        /// Collecte liée à une souscription de prestation (mensuelle, régulière)
        /// </summary>
        Souscription = 2,

        /// <summary>
        /// Collecte liée à une cotisation affilié (mensuelle ou annuelle)
        /// </summary>
        Cotisation = 3
    }

    /// <summary>
    /// Convertisseur JSON pour gérer les chaînes et les nombres pour TypeCollecte
    /// </summary>
    public class TypeCollecteConverter : JsonConverter<TypeCollecte>
    {
        public override TypeCollecte Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return TypeCollecte.Frais; // Valeur par défaut

            return value.ToUpperInvariant() switch
            {
                "FRAIS" or "1" => TypeCollecte.Frais,
                "SOUSCRIPTION" or "2" => TypeCollecte.Souscription,
                "COTISATION" or "3" => TypeCollecte.Cotisation,
                _ => throw new JsonException($"Valeur TypeCollecte invalide: {value}. Valeurs valides: Frais, Souscription, Cotisation, 1, 2, 3")
            };
        }

        public override void Write(Utf8JsonWriter writer, TypeCollecte value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToUpperInvariant());
        }
    }
}
