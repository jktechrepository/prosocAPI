using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsocAPI.Models.DTOs.Authentication;
using System.Security.Claims;

namespace ProsocAPI.Controllers
{
    public class AuthTestController : ControllerBase
    {
        private readonly ILogger<AuthTestController> _logger;

        public AuthTestController(ILogger<AuthTestController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Endpoint public de test pour vérifier que l'API fonctionne
        /// </summary>
        public IActionResult PublicEndpoint()
        {
            return Ok(new AuthTestResponse
            {
                Message = "API Prosoc fonctionne correctement",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Endpoint protégé pour tester l'authentification JWT
        /// </summary>
        public IActionResult ProtectedEndpoint()
        {
            var userInfo = GetUserInfo();
            
            return Ok(new ProtectedEndpointResponse
            {
                Message = "Authentification JWT réussie !",
                User = userInfo,
                Note = "Le middleware JWT a validé votre token avec succès"
            });
        }

        /// <summary>
        /// Endpoint pour vérifier les permissions de l'utilisateur
        /// </summary>
        public IActionResult PermissionsEndpoint()
        {
            var userInfo = GetUserInfo();
            
            return Ok(new ProtectedEndpointResponse
            {
                Message = "Permissions de l'utilisateur",
                User = userInfo
            });
        }

        /// <summary>
        /// Endpoint protégé par rôle spécifique (Admin/Super-Admin)
        /// </summary>
        public IActionResult AdminOnlyEndpoint()
        {
            var userInfo = GetUserInfo();
            
            return Ok(new ProtectedEndpointResponse
            {
                Message = "Accès administrateur autorisé !",
                User = userInfo,
                Note = "Cet endpoint nécessite les rôles Admin ou Super-Admin"
            });
        }

        /// <summary>
        /// Endpoint protégé par rôle Super-Admin uniquement
        /// </summary>
        public IActionResult SuperAdminOnlyEndpoint()
        {
            var userInfo = GetUserInfo();
            
            return Ok(new ProtectedEndpointResponse
            {
                Message = "Accès Super-Admin autorisé !",
                User = userInfo,
                Note = "Cet endpoint nécessite le rôle Super-Admin"
            });
        }

        /// <summary>
        /// Endpoint pour tester les informations du token JWT
        /// </summary>
        public IActionResult TokenInfoEndpoint()
        {
            var claims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value,
                ValueType = c.ValueType
            }).ToList();

            return Ok(new
            {
                Message = "Informations du token JWT",
                Timestamp = DateTime.UtcNow,
                Claims = claims,
                AuthenticationType = User.Identity?.AuthenticationType,
                IsAuthenticated = User.Identity?.IsAuthenticated,
                Name = User.Identity?.Name
            });
        }

        // ═══════════════════════════════════════════════════════════════════════════════════
        // 🔧 MÉTHODES PRIVÉES
        // ═══════════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Extraction des informations utilisateur depuis les claims JWT
        /// </summary>
        private AuthTestUserInfo GetUserInfo()
        {
            var userIdClaim = User.FindFirst("uid")?.Value;
            var userNameClaim = User.FindFirst("username")?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            var primaryRoleClaim = User.FindFirst("primaryRole")?.Value;

            int.TryParse(userIdClaim, out var userId);
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            return new AuthTestUserInfo
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                UserId = userId,
                UserName = userNameClaim,
                UserRole = roleClaim,
                IsSuperAdmin = roles.Contains("Super-Admin"),
                IsAdmin = roles.Contains("Admin") || roles.Contains("Super-Admin"),
                IsStaff = roles.Any(r => r != "Affilié"),
                HasFinanceAccess = roles.Contains("Admin") || roles.Contains("Super-Admin"),
                HasPedagogieAccess = roles.Contains("Super-Admin"),
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
