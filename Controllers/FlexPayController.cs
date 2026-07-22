using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsocAPI.Models.DTOs.FlexPay;
using ProsocAPI.Services;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlexPayController : ControllerBase
    {
        private readonly IFlexPayCallbackService _callbackService;
        private readonly ILogger<FlexPayController> _logger;

        public FlexPayController(
            IFlexPayCallbackService callbackService,
            ILogger<FlexPayController> logger)
        {
            _callbackService = callbackService;
            _logger = logger;
        }

        /// <summary>
        /// Callback public FlexPay (sans JWT). Crée la collecte si code = 0.
        /// </summary>
        [HttpPost("callback")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(FlexPayCallbackProcessResultDto), 200)]
        public async Task<ActionResult<FlexPayCallbackProcessResultDto>> Callback(
            [FromBody] FlexPayCallbackDto callback,
            CancellationToken ct = default)
        {
            try
            {
                var raw = JsonSerializer.Serialize(callback);
                var headers = string.Join("; ", Request.Headers.Select(h => $"{h.Key}={h.Value}"));
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

                var result = await _callbackService.ProcessCallbackAsync(callback, raw, headers, ip, ct);
                return Ok(new { message = result.Message, result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur callback FlexPay");
                return this.TechnicalErrorResponse("Erreur interne callback FlexPay", ex);
            }
        }

        /// <summary>
        /// Secours : vérifie le statut chez FlexPay et finalise si succès.
        /// </summary>
        [HttpGet("verifier/{orderNumber}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(FlexPayCallbackProcessResultDto), 200)]
        public async Task<ActionResult<FlexPayCallbackProcessResultDto>> Verifier(
            string orderNumber,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _callbackService.VerifyAndFinalizeAsync(orderNumber, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur vérification FlexPay {OrderNumber}", orderNumber);
                return this.TechnicalErrorResponse("Erreur interne", ex);
            }
        }

        [HttpGet("approve")]
        [AllowAnonymous]
        public IActionResult Approve([FromQuery] string? orderNumber) =>
            Ok(new { message = "Paiement en cours de confirmation.", orderNumber });

        [HttpGet("cancel")]
        [AllowAnonymous]
        public IActionResult Cancel([FromQuery] string? orderNumber) =>
            Ok(new { message = "Paiement annulé.", orderNumber });

        [HttpGet("decline")]
        [AllowAnonymous]
        public IActionResult Decline([FromQuery] string? orderNumber) =>
            Ok(new { message = "Paiement refusé.", orderNumber });
    }
}
