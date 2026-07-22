using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaiementAffilieTestController : ControllerBase
    {
        private readonly PaiementAffilieService _paiementAffilieService;
        private readonly ILogger<PaiementAffilieTestController> _logger;

        public PaiementAffilieTestController(
            PaiementAffilieService paiementAffilieService,
            ILogger<PaiementAffilieTestController> logger)
        {
            _paiementAffilieService = paiementAffilieService;
            _logger = logger;
        }

        /// <summary>
        /// Test simple - récupérer les souscriptions payables
        /// </summary>
        [HttpGet("test/souscriptions")]
        public async Task<ActionResult<List<object>>> GetTestSouscriptions()
        {
            try
            {
                // Simuler un affilié ID = 1 pour le test
                var affilieId = 1;
                
                var souscriptions = await _paiementAffilieService.GetSouscriptionsPayablesAsync(affilieId);
                
                var result = souscriptions.Select(sp => new
                {
                    Id = sp.IdSouscriptionPrestation,
                    NomPrestation = sp.Prestation?.NomPrestation,
                    Montant = sp.Prestation?.Description,
                    DateSouscription = sp.DateSouscription,
                    Statut = sp.Statut
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue : Erreur lors du test",
                    ex);
            }
        }

        /// <summary>
        /// Test simple - payer une souscription
        /// </summary>
        [HttpPost("test/paiement")]
        public async Task<ActionResult<object>> PostTestPaiement([FromBody] PayerSouscriptionDto dto)
        {
            try
            {
                // Simuler un affilié ID = 1 pour le test
                var affilieId = 1;
                
                var collecte = await _paiementAffilieService.PayerSouscriptionAsync(affilieId, dto);
                
                return Ok(new
                {
                    Message = "Paiement test réussi",
                    CollecteId = collecte.IdCollecte,
                    Montant = collecte.Montant,
                    Statut = collecte.StatutPaiement
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du test de paiement");
                return this.TechnicalErrorResponse("Erreur interne du serveur", ex);
            }
        }
    }
}
