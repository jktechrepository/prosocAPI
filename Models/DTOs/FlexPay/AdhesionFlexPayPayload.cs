using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Models.DTOs.FlexPay
{
  public class AdhesionFlexPayPayload
  {
    public AdhesionWithAffilieCreateDto Input { get; set; } = new();

    public int? UtilisateurId { get; set; }

    public int DevisePaiementId { get; set; }
  }
}
