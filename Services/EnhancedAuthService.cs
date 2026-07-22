using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.DTOs.Authentication;
using ProsocAPI.Services.Repositories;
using ProsocAPI.Utilities;
using Prosoc.Data;

namespace ProsocAPI.Services
{
    public class EnhancedAuthService : IAuthService
    {
        private readonly IUtilisateurRepository _users;
        private readonly IUserDeviceRepository _userDevices;
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly IConfiguration _config;
        private readonly ILogger<EnhancedAuthService> _logger;
        private readonly ProsocDbContext _context;

        public EnhancedAuthService(
            IUtilisateurRepository users, 
            IUserDeviceRepository userDevices,
            IRefreshTokenRepository refreshTokens,
            IConfiguration config,
            ILogger<EnhancedAuthService> logger,
            ProsocDbContext context)
        {
            _users = users;
            _userDevices = userDevices;
            _refreshTokens = refreshTokens;
            _config = config;
            _logger = logger;
            _context = context;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string ancienMotDePasse, string nouveauMotDePasse, CancellationToken ct = default)
        {
            var user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.IdUtilisateur == userId, ct);
            if (user == null)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(ancienMotDePasse, user.MotDePasseHash))
                return false;

            user.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(nouveauMotDePasse);
            user.DoitChangerMotDePasse = false;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<(bool ok, string? token)> CreatePasswordResetTokenAsync(string emailOuTelephone, CancellationToken ct = default)
        {
            var user = await FindUserByMultipleWays(emailOuTelephone, ct);
            if (user == null)
                return (false, null);

            var token = Guid.NewGuid().ToString("N");
            var expiresAt = DateTime.UtcNow.AddMinutes(30);

            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                UtilisateurId = user.IdUtilisateur,
                Token = token,
                DateCreation = DateTime.UtcNow,
                DateExpiration = expiresAt
            });

            await _context.SaveChangesAsync(ct);
            return (true, token);
        }

        public async Task<bool> ConfirmPasswordResetAsync(string token, string nouveauMotDePasse, CancellationToken ct = default)
        {
            var reset = await _context.PasswordResetTokens
                .Include(x => x.Utilisateur)
                .FirstOrDefaultAsync(x => x.Token == token, ct);

            if (reset == null)
                return false;

            if (reset.Utilise || reset.EstExpire)
                return false;

            if (reset.Utilisateur == null)
                return false;

            reset.Utilisateur.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(nouveauMotDePasse);
            reset.Utilisateur.DoitChangerMotDePasse = false;
            reset.DateUtilisation = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool?> ToggleUserStatusAsync(int userId, CancellationToken ct = default)
        {
            var user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.IdUtilisateur == userId, ct);
            if (user == null)
                return null;

            user.Statut = !user.Statut;
            await _context.SaveChangesAsync(ct);
            return user.Statut;
        }

        public async Task<bool> ResetPasswordForUserAsync(int userId, string defaultPassword, CancellationToken ct = default)
        {
            var user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.IdUtilisateur == userId, ct);
            if (user == null)
                return false;

            user.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
            user.DoitChangerMotDePasse = true;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> ResetPasswordBulkByRoleAsync(int roleId, string defaultPassword, CancellationToken ct = default)
        {
            // On cible les utilisateurs ayant ce rôle actif (UserRoles.Statut = true)
            var userIds = await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.RoleId == roleId && ur.Statut)
                .Select(ur => ur.UtilisateurId)
                .Distinct()
                .ToListAsync(ct);

            if (userIds.Count == 0)
                return 0;

            var users = await _context.Utilisateurs
                .Where(u => userIds.Contains(u.IdUtilisateur))
                .ToListAsync(ct);

            foreach (var u in users)
            {
                u.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
                u.DoitChangerMotDePasse = true;
            }

            await _context.SaveChangesAsync(ct);
            return users.Count;
        }

        /// <summary>
        /// Authentification enrichie avec support multi-canal (email/téléphone/username)
        /// </summary>
        public async Task<AuthentificationResponse?> AuthenticateAsync(AuthentificationRequest request, CancellationToken ct = default)
        {
            try
            {
                // 1. Recherche de l'utilisateur par ordre de priorité
                var user = await FindUserByMultipleWays(request.EmailOuTelephone, ct);
                if (user == null || !user.Statut)
                {
                    _logger.LogWarning("Tentative de connexion échouée pour {Identifier}", request.EmailOuTelephone);
                    return new AuthentificationResponse
                    {
                        Success = false,
                        Message = "Email/Telephone ou mot de passe incorrect"
                    };
                }

                // 2. Validation du mot de passe
                if (!BCrypt.Net.BCrypt.Verify(request.MotDePasse, user.MotDePasseHash))
                {
                    _logger.LogWarning("Mot de passe incorrect pour l'utilisateur {UserId}", user.IdUtilisateur);
                    return new AuthentificationResponse
                    {
                        Success = false,
                        Message = "Email/Telephone ou mot de passe incorrect"
                    };
                }

                // 3. Gestion du device FCM
                if (!string.IsNullOrWhiteSpace(request.FcmToken))
                {
                    await _userDevices.RegisterOrUpdateDeviceAsync(
                        user.IdUtilisateur,
                        request.FcmToken,
                        request.DeviceType,
                        request.DeviceModel,
                        request.OsVersion,
                        ct
                    );
                }

                // 4. Génération des tokens
                var (accessToken, refreshToken) = await GenerateTokensAsync(user, request.DeviceInfo, ct);

                // 5. Construction de la réponse enrichie
                return await BuildAuthResponse(user, accessToken, refreshToken, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'authentification pour {Identifier}", request.EmailOuTelephone);
                return new AuthentificationResponse
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de l'authentification"
                };
            }
        }

        /// <summary>
        /// Rafraîchissement du token d'accès
        /// </summary>
        public async Task<RefreshTokenResponse?> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default)
        {
            try
            {
                // 1. Validation du refresh token
                var refreshToken = await _refreshTokens.GetByTokenAsync(request.RefreshToken, ct);
                if (refreshToken == null || !refreshToken.EstActif)
                {
                    _logger.LogWarning("Tentative de rafraîchissement avec token invalide ou expiré");
                    return new RefreshTokenResponse
                    {
                        Success = false,
                        Message = "Refresh token invalide ou expiré"
                    };
                }

                // 2. Récupération de l'utilisateur
                var user = await _users.GetByIdAsync(refreshToken.UtilisateurId, ct);
                if (user == null || !user.Statut)
                {
                    _logger.LogWarning("Utilisateur {UserId} non trouvé ou inactif", refreshToken.UtilisateurId);
                    return new RefreshTokenResponse
                    {
                        Success = false,
                        Message = "Utilisateur non trouvé"
                    };
                }

                // 3. Génération des nouveaux tokens d'abord
                var (accessToken, newRefreshToken) = await GenerateTokensAsync(user, request.DeviceInfo, ct);

                // 4. Révocation de l'ancien refresh token seulement après succès
                await _refreshTokens.RevokeAsync(refreshToken.IdRefreshToken, ct);

                // 5. Construction de la réponse
                return new RefreshTokenResponse
                {
                    Success = true,
                    Message = "Token rafraîchi avec succès",
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = GetTokenExpirationSeconds(),
                    ExpiresAt = DateTime.UtcNow.AddSeconds(GetTokenExpirationSeconds())
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du rafraîchissement du token");
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Une erreur est survenue lors du rafraîchissement"
                };
            }
        }

        /// <summary>
        /// Déconnexion (révocation de tous les tokens)
        /// </summary>
        public async Task<bool> LogoutAsync(int userId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Tentative de déconnexion pour l'utilisateur {UserId}", userId);
                var success = await _refreshTokens.RevokeAllForUserAsync(userId, ct);
                
                if (success)
                {
                    _logger.LogInformation("Déconnexion réussie pour l'utilisateur {UserId}", userId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Échec de la déconnexion pour l'utilisateur {UserId}", userId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la déconnexion de l'utilisateur {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Révoquer un refresh token spécifique
        /// </summary>
        public async Task<bool> RevokeTokenAsync(string refreshToken, int userId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Tentative de révocation du token pour l'utilisateur {UserId}", userId);
                
                // Vérifier que le token appartient bien à l'utilisateur
                var token = await _refreshTokens.GetByTokenAsync(refreshToken, ct);
                if (token == null || token.UtilisateurId != userId)
                {
                    _logger.LogWarning("Token non trouvé ou n'appartenant pas à l'utilisateur {UserId}", userId);
                    return false;
                }

                var success = await _refreshTokens.RevokeAsync(token.IdRefreshToken, ct);
                
                if (success)
                {
                    _logger.LogInformation("Token révoqué avec succès pour l'utilisateur {UserId}", userId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Échec de la révocation du token pour l'utilisateur {UserId}", userId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la révocation du token pour l'utilisateur {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Révoquer TOUS les refresh tokens d'un utilisateur
        /// </summary>
        public async Task<bool> RevokeAllUserTokensAsync(int userId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Tentative de révocation de tous les tokens pour l'utilisateur {UserId}", userId);
                var success = await _refreshTokens.RevokeAllForUserAsync(userId, ct);
                
                if (success)
                {
                    _logger.LogInformation("Tous les tokens révoqués avec succès pour l'utilisateur {UserId}", userId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Échec de la révocation de tous les tokens pour l'utilisateur {UserId}", userId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la révocation de tous les tokens pour l'utilisateur {UserId}", userId);
                return false;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════════════
        // 🔧 MÉTHODES PRIVÉES
        // ═══════════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Recherche utilisateur par email, téléphone ou username
        /// </summary>
        private async Task<Utilisateur?> FindUserByMultipleWays(string identifier, CancellationToken ct)
        {
            identifier = identifier.Trim();

            // 1. Essai par email
            if (IsValidEmail(identifier))
            {
                var user = await _users.GetByEmailAsync(identifier, ct);
                if (user != null) return user;
            }

            // 2. Essai par téléphone
            if (IsValidPhoneNumber(identifier))
            {
                var user = await _users.GetByTelephoneAsync(identifier, ct);
                if (user != null) return user;
            }

            // 3. Essai par username (NomUtilisateur ou DefaultUsername)
            var userByUsername = await _users.GetByNomUtilisateurAsync(identifier, ct);
            if (userByUsername != null) return userByUsername;

            // 4. Essai par DefaultUsername
            return await _users.GetByDefaultUsernameAsync(identifier, ct);
        }

        /// <summary>
        /// Validation du format email
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var mail = new System.Net.Mail.MailAddress(email);
                return mail.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validation du format téléphone
        /// </summary>
        private static bool IsValidPhoneNumber(string phone) =>
            PhoneNumberHelper.IsValidPhone(phone);

        /// <summary>
        /// Génération des tokens JWT et Refresh
        /// </summary>
        private async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(Utilisateur user, string? deviceInfo, CancellationToken ct)
        {
            // 1. Génération du JWT Access Token
            var accessToken = await GenerateJwtTokenAsync(user, ct);

            // 2. Génération du Refresh Token
            var rawRefreshToken = RefreshTokenService.GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                UtilisateurId = user.IdUtilisateur,
                TokenHash = RefreshTokenService.HashToken(rawRefreshToken), // Hasher le token pour le stockage
                DateExpiration = DateTime.UtcNow.AddDays(7),
                DeviceInfo = deviceInfo
            };
            await _refreshTokens.CreateAsync(refreshTokenEntity, ct);

            return (accessToken, rawRefreshToken); // Retourner le token original (non hashé)
        }

        /// <summary>
        /// Construction de la réponse d'authentification enrichie
        /// </summary>
        private async Task<AuthentificationResponse> BuildAuthResponse(Utilisateur user, string accessToken, string refreshToken, CancellationToken ct)
        {
            var permissions = await GetUserPermissionsAsync(user.IdUtilisateur, ct);
            var userRoles = await _users.GetUserRolesAsync(user.IdUtilisateur);
            var primaryRole = userRoles.FirstOrDefault(ur => ur.IsPrimary);

            return new AuthentificationResponse
            {
                Success = true,
                Message = "Authentification réussie",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = GetTokenExpirationSeconds(),
                ExpiresAt = DateTime.UtcNow.AddSeconds(GetTokenExpirationSeconds()),
                DoitChangerMotDePasse = user.DoitChangerMotDePasse,
                AcceptNotification = true, // TODO: Récupérer depuis profil utilisateur
                Utilisateur = await BuildUtilisateurDtoAsync(user, ct),
                NomRole = primaryRole?.Role?.Nom,
                Permissions = permissions.Select(p => p.Nom).ToList(),
                PrimaryRole = primaryRole?.Role != null ? new RoleDto
                {
                    IdRole = primaryRole.Role.IdRole,
                    Nom = primaryRole.Role.Nom,
                    Description = primaryRole.Role.Description,
                    Niveau = primaryRole.Role.Niveau ?? 999,
                    Statut = primaryRole.Role.Statut
                } : null,
                Roles = userRoles.Where(ur => ur.Role != null).Select(ur => new RoleDto
                {
                    IdRole = ur.Role.IdRole,
                    Nom = ur.Role.Nom,
                    Description = ur.Role.Description,
                    Niveau = ur.Role.Niveau ?? 999,
                    Statut = ur.Role.Statut
                }).ToList()
            };
        }

        /// <summary>
        /// Génération du JWT Token
        /// </summary>
        private async Task<string> GenerateJwtTokenAsync(Utilisateur user, CancellationToken ct = default)
        {
            var jwtSection = _config.GetSection("Jwt");
            var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var expirationSeconds = GetTokenExpirationSeconds();

            var expiresAt = DateTime.UtcNow.AddSeconds(expirationSeconds);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.IdUtilisateur.ToString()),
                new("uid", user.IdUtilisateur.ToString()),
                new("UserId", user.IdUtilisateur.ToString()),
                new("username", user.NomUtilisateur ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            if (user.AgentId.HasValue)
                claims.Add(new Claim("AgentId", user.AgentId.Value.ToString()));

            if (user.AffilieId.HasValue)
                claims.Add(new Claim("AffilieId", user.AffilieId.Value.ToString()));

            if (user.HopitalPartenaireId.HasValue)
                claims.Add(new Claim("HopitalPartenaireId", user.HopitalPartenaireId.Value.ToString()));

            if (user.AssureurId.HasValue)
                claims.Add(new Claim("AssureurId", user.AssureurId.Value.ToString()));

            // Ajout des rôles
            var userRoles = await _users.GetUserRolesAsync(user.IdUtilisateur);
            foreach (var userRole in userRoles)
            {
                if (userRole.Role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Nom));
                    if (userRole.IsPrimary)
                    {
                        claims.Add(new Claim("primaryRole", userRole.Role.Nom));
                    }
                }
            }

            // Claims permission (utilisés par BaseApiController.HasPermission)
            var permissions = await GetUserPermissionsAsync(user.IdUtilisateur, ct);
            foreach (var permission in permissions)
            {
                if (!string.IsNullOrWhiteSpace(permission.Nom))
                    claims.Add(new Claim("permission", permission.Nom));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Obtenir la durée d'expiration en secondes (standard OAuth2)
        /// </summary>
        private int GetTokenExpirationSeconds()
        {
            var jwtSection = _config.GetSection("Jwt");
            return int.TryParse(jwtSection["ExpirationMinutes"], out var minutes) ? minutes * 60 : 300; // 5 minutes par défaut
        }

        /// <summary>
        /// Obtenir les permissions de l'utilisateur (via les rôles + permissions directes)
        /// </summary>
        private async Task<List<Permission>> GetUserPermissionsAsync(int userId, CancellationToken ct)
        {
            _logger.LogInformation("🔍 Récupération des permissions pour l'utilisateur {UserId}", userId);

            // 1. Récupérer les IDs des rôles (UserRoles + rôle principal Utilisateur.RoleId)
            var userRoleIds = await _context.UserRoles
                .Where(ur => ur.UtilisateurId == userId && ur.Statut)
                .Select(ur => ur.RoleId)
                .ToListAsync(ct);

            var primaryRoleId = await _context.Utilisateurs
                .Where(u => u.IdUtilisateur == userId)
                .Select(u => u.RoleId)
                .FirstOrDefaultAsync(ct);

            if (primaryRoleId.HasValue && !userRoleIds.Contains(primaryRoleId.Value))
                userRoleIds.Add(primaryRoleId.Value);

            _logger.LogInformation("✅ Rôles trouvés pour l'utilisateur {UserId}: {RoleIds}", userId, string.Join(", ", userRoleIds));

            // 2. Récupérer les permissions via les rôles (avec Include explicite)
            var rolePermissions = await _context.RolePermissions
                .Where(rp => userRoleIds.Contains(rp.RoleId))
                .Include(rp => rp.Permission)
                .Where(rp => rp.Permission != null && rp.Permission.Statut)
                .Select(rp => rp.Permission!)
                .ToListAsync(ct);

            _logger.LogInformation("✅ Permissions via rôles: {Count} permissions trouvées", rolePermissions.Count);

            // 3. Récupérer les permissions directement assignées à l'utilisateur
            var directPermissions = await _context.UserPermissions
                .Where(up => up.UtilisateurId == userId)
                .Include(up => up.Permission)
                .Where(up => up.Permission != null && up.Permission.Statut)
                .Select(up => up.Permission!)
                .ToListAsync(ct);

            _logger.LogInformation("✅ Permissions directes: {Count} permissions trouvées", directPermissions.Count);

            // 4. Combiner et dédoublonner les permissions
            var allPermissions = rolePermissions.Concat(directPermissions)
                .GroupBy(p => p.IdPermission)
                .Select(g => g.First())
                .ToList();

            _logger.LogInformation("✅ Total permissions uniques: {Count} - [{Permissions}]", 
                allPermissions.Count, 
                string.Join(", ", allPermissions.Select(p => p.Nom)));

            return allPermissions;
        }

        /// <summary>
        /// Construire le DTO utilisateur
        /// </summary>
        private async Task<UtilisateurDto> BuildUtilisateurDtoAsync(Utilisateur user, CancellationToken ct = default)
        {
            var dto = new UtilisateurDto
            {
                IdUtilisateur = user.IdUtilisateur,
                ReferenceUtilisateur = user.ReferenceUtilisateur?.ToString() ?? string.Empty,
                NomComplet = user.NomUtilisateur,
                NomUtilisateur = user.NomUtilisateur,
                Email = user.EmailUtilisateur,
                Telephone = user.PhoneUtilisateur,
                PhotoUrl = null,
                Genre = null,
                Statut = user.Statut,
                DateCreation = user.DateCreation,
                IsConnecte = user.IsConnecte,
                DoitChangerMotDePasse = user.DoitChangerMotDePasse,
                AgentId = user.AgentId,
                AffilieId = user.AffilieId,
                HopitalPartenaireId = user.HopitalPartenaireId,
                AssureurId = user.AssureurId
            };

            await UtilisateurGestionnaireHelper.EnrichGestionnaireAffilieAsync(
                dto, _context, user.AffilieId, _logger, ct);

            return dto;
        }

        /// <summary>
        /// Construire le DTO rôle
        /// </summary>
        private static RoleDto BuildRoleDto(Role? role)
        {
            if (role == null) return null!;
            
            return new RoleDto
            {
                IdRole = role.IdRole,
                Nom = role.Nom,
                Description = role.Description,
                Niveau = role.Niveau ?? 999,
                Statut = role.Statut
            };
        }

        /// <summary>
        /// Construire la liste des DTOs rôles
        /// </summary>
        private static List<RoleDto> BuildRolesDto(Utilisateur user)
        {
            return user.UserRoles
                .Where(ur => ur.Role != null)
                .Select(ur => BuildRoleDto(ur.Role))
                .ToList();
        }

        // ═══════════════════════════════════════════════════════════════════════════════════
        // 🔄 COMPATIBILITÉ AVEC L'ANCIENNE INTERFACE
        // ═══════════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Maintien de la compatibilité avec l'ancienne méthode LoginAsync
        /// </summary>
        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
        {
            // Conversion vers le nouveau format
            var authRequest = new AuthentificationRequest
            {
                EmailOuTelephone = request.NomUtilisateur,
                MotDePasse = request.MotDePasse,
                FcmToken = request.FcmToken,
                DeviceType = request.DeviceType,
                DeviceModel = request.DeviceModel,
                OsVersion = request.OsVersion
            };

            var response = await AuthenticateAsync(authRequest, ct);
            if (!response.Success || response.AccessToken == null)
                return null;

            return new LoginResponseDto
            {
                AccessToken = response.AccessToken,
                ExpiresAtUtc = response.ExpiresAt ?? DateTime.UtcNow,
                UtilisateurId = response.Utilisateur?.IdUtilisateur ?? 0,
                NomUtilisateur = response.Utilisateur?.NomUtilisateur ?? string.Empty,
                Role = response.PrimaryRole?.Nom
            };
        }
    }
}
