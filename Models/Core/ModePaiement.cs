using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProsocAPI.Models.Core
{
    [JsonConverter(typeof(ModePaiementConverter))]
    public enum ModePaiement
    {
        ESPECE = 1,
        MOBILE_MONEY = 2,
        CARTE_BANCAIRE = 3,
        VIREMENT_BANCAIRE = 4,
        CHEQUE = 5,
        VIRTUAL_ACCOUNT = 6
    }

    public class ModePaiementConverter : JsonConverter<ModePaiement>
    {
        public override ModePaiement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return ModePaiement.ESPECE;

            return value.ToUpperInvariant().Replace(" ", "_") switch
            {
                "ESPECE" or "1" => ModePaiement.ESPECE,
                "MOBILE_MONEY" or "2" => ModePaiement.MOBILE_MONEY,
                "CARTE_BANCAIRE" or "CARTE" or "3" => ModePaiement.CARTE_BANCAIRE,
                "VIREMENT_BANCAIRE" or "4" => ModePaiement.VIREMENT_BANCAIRE,
                "CHEQUE" or "5" => ModePaiement.CHEQUE,
                "VIRTUAL_ACCOUNT" or "COMPTE VIRTUEL" or "COMPTE_VIRTUEL" or "7" or "6" => ModePaiement.VIRTUAL_ACCOUNT,
                "ORANGE_MONEY" or "AIRTEL_MONEY" => ModePaiement.MOBILE_MONEY,
                _ => throw new JsonException(
                    $"Valeur ModePaiement invalide: {value}. Valeurs valides: ESPECE, MOBILE_MONEY, CARTE_BANCAIRE, VIREMENT_BANCAIRE, CHEQUE, VIRTUAL_ACCOUNT")
            };
        }

        public override void Write(Utf8JsonWriter writer, ModePaiement value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToUpperInvariant());
        }
    }

    public static class ModePaiementExtensions
    {
        public static string GetDisplayName(this ModePaiement mode) => mode switch
        {
            ModePaiement.ESPECE => "Espèces",
            ModePaiement.MOBILE_MONEY => "Mobile Money",
            ModePaiement.CARTE_BANCAIRE => "Carte bancaire",
            ModePaiement.VIREMENT_BANCAIRE => "Virement Bancaire",
            ModePaiement.CHEQUE => "Chèque",
            ModePaiement.VIRTUAL_ACCOUNT => "Compte Virtuel",
            _ => mode.ToString()
        };

        public static bool RequiresReference(this ModePaiement mode) =>
            mode != ModePaiement.VIRTUAL_ACCOUNT && mode != ModePaiement.ESPECE;

        public static bool IsFlexPay(this ModePaiement mode) =>
            mode is ModePaiement.MOBILE_MONEY or ModePaiement.CARTE_BANCAIRE;

        public static bool IsGuichetSync(this ModePaiement mode) =>
            mode is ModePaiement.ESPECE or ModePaiement.CHEQUE
                or ModePaiement.VIREMENT_BANCAIRE or ModePaiement.VIRTUAL_ACCOUNT;
    }
}
