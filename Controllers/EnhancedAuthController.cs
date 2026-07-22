using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsocAPI.Models.DTOs.Authentication;
using ProsocAPI.Services;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnhancedAuthController : ControllerBase
    {
        private readonly EnhancedAuthService _authService;
        private readonly ILogger<EnhancedAuthController> _logger;

        public EnhancedAuthController(EnhancedAuthService authService, ILogger<EnhancedAuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Authentification enrichie avec support multi-canal (email/téléphone/username)
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthentificationResponse>> Login([FromBody] AuthentificationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthentificationResponse
                    {
                        Success = false,
                        Message = "Données invalides"
                    });
                }

                var response = await _authService.AuthenticateAsync(request);
                
                if (response == null || !response.Success)
                {
                    return BadRequest(response ?? new AuthentificationResponse
                    {
                        Success = false,
                        Message = "Échec de l'authentification"
                    });
                }

                _logger.LogInformation("Authentification réussie pour l'utilisateur {Identifier}", request.EmailOuTelephone);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'authentification pour {Identifier}", request.EmailOuTelephone);
                return StatusCode(500, new AuthentificationResponse
                {
                    Success = false,
                    Message = "Une erreur interne est survenue"
                });
            }
        }

        /// <summary>
        /// Rafraîchissement du token d'accès
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new RefreshTokenResponse
                    {
                        Success = false,
                        Message = "Données invalides"
                    });
                }

                var response = await _authService.RefreshTokenAsync(request);
                
                if (response == null || !response.Success)
                {
                    return BadRequest(response ?? new RefreshTokenResponse
                    {
                        Success = false,
                        Message = "Échec du rafraîchissement"
                    });
                }

                _logger.LogInformation("Token rafraîchi avec succès");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du rafraîchissement du token");
                return StatusCode(500, new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Une erreur interne est survenue"
                });
            }
        }

        /// <summary>
        /// Déconnexion (révocation de tous les tokens de l'utilisateur)
        /// </summary>
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("Utilisateur non identifié");
                }

                var success = await _authService.LogoutAsync(userId);
                
                if (success)
                {
                    _logger.LogInformation("Déconnexion réussie pour l'utilisateur {UserId}", userId);
                    return Ok(new { message = "Déconnexion réussie" });
                }
                else
                {
                    return BadRequest(new { message = "Erreur lors de la déconnexion" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la déconnexion");
                return this.TechnicalErrorResponse("Une erreur interne est survenue", ex);
            }
        }
    }
}
