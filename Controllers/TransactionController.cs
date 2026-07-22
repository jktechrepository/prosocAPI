using Microsoft.AspNetCore.Mvc;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpPost("collecte/{agentId}")]
        public async Task<ActionResult<TransactionResponseDto>> Collecte(int agentId, [FromBody] CollecteDto dto)
        {
            try
            {
                var result = await _transactionService.ProcessCollecte(agentId, dto);
                
                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la collecte pour l'agent {AgentId}", agentId);
                return StatusCode(500, new TransactionResponseDto
                {
                    Success = false,
                    Message = "Erreur serveur lors de la collecte"
                });
            }
        }

        [HttpPost("retrait/{agentId}")]
        public async Task<ActionResult<TransactionResponseDto>> Retrait(int agentId, [FromBody] RetraitDto dto)
        {
            try
            {
                var result = await _transactionService.ProcessRetrait(agentId, dto);
                
                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du retrait pour l'agent {AgentId}", agentId);
                return StatusCode(500, new TransactionResponseDto
                {
                    Success = false,
                    Message = "Erreur serveur lors du retrait"
                });
            }
        }

        [HttpPost("bonus/{agentId}")]
        public async Task<ActionResult<TransactionResponseDto>> Bonus(int agentId, [FromBody] BonusDto dto)
        {
            try
            {
                var result = await _transactionService.ProcessBonus(agentId, dto);
                
                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du bonus pour l'agent {AgentId}", agentId);
                return StatusCode(500, new TransactionResponseDto
                {
                    Success = false,
                    Message = "Erreur serveur lors du bonus"
                });
            }
        }

        [HttpPost("commission/{agentId}")]
        public async Task<ActionResult<TransactionResponseDto>> Commission(int agentId, [FromBody] CommissionDto dto)
        {
            try
            {
                var result = await _transactionService.ProcessCommission(agentId, dto);
                
                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la commission pour l'agent {AgentId}", agentId);
                return StatusCode(500, new TransactionResponseDto
                {
                    Success = false,
                    Message = "Erreur serveur lors de la commission"
                });
            }
        }

        [HttpGet("solde/{agentId}")]
        public async Task<ActionResult<decimal>> GetSolde(int agentId)
        {
            try
            {
                var solde = await _transactionService.GetSoldeActuel(agentId);
                return Ok(solde);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du solde pour l'agent {AgentId}", agentId);
                return StatusCode(500, 0);
            }
        }
    }
}
