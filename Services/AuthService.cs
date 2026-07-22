using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProsocAPI.Models.DTOs.Authentication;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUtilisateurRepository _users;
        private readonly IUserDeviceRepository _userDevices;
        private readonly IConfiguration _config;

        public AuthService(IUtilisateurRepository users, IUserDeviceRepository userDevices, IConfiguration config)
        {
            _users = users;
            _userDevices = userDevices;
            _config = config;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
        {
            var user = await _users.GetByNomUtilisateurAsync(request.NomUtilisateur, ct);
            if (user == null || !user.Statut)
                return null;

            var ok = BCrypt.Net.BCrypt.Verify(request.MotDePasse, user.MotDePasseHash);
            if (!ok)
                return null;

            // Gérer l'enregistrement/mise à jour du device si FCM token fourni
            if (!string.IsNullOrWhiteSpace(request.FcmToken))
            {
                try
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
                catch (Exception ex)
                {
                    // Logger l'erreur mais ne pas bloquer le login
                    // En production, vous pourriez vouloir logger cette erreur
                    Console.WriteLine($"Erreur lors de l'enregistrement du device: {ex.Message}");
                }
            }

            var jwtSection = _config.GetSection("Jwt");
            var secretKey = jwtSection["SecretKey"];
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var expirationMinutes = int.TryParse(jwtSection["ExpirationMinutes"], out var m) ? m : 120;

            if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
                throw new InvalidOperationException("JWT configuration missing (Jwt:SecretKey/Issuer/Audience)");

            var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.IdUtilisateur.ToString()),
                new("uid", user.IdUtilisateur.ToString()),
                new("username", user.NomUtilisateur)
            };

            // Ajouter les claims spécifiques au rôle
            if (user.AgentId.HasValue)
            {
                claims.Add(new Claim("AgentId", user.AgentId.Value.ToString()));
            }
            
            if (user.AffilieId.HasValue)
            {
                claims.Add(new Claim("AffilieId", user.AffilieId.Value.ToString()));
            }

            if (user.HopitalPartenaireId.HasValue)
            {
                claims.Add(new Claim("HopitalPartenaireId", user.HopitalPartenaireId.Value.ToString()));
            }

            var primaryRole = user.PrimaryRole;
            if (primaryRole != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, primaryRole.Nom));
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

            return new LoginResponseDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAtUtc = expiresAt,
                UtilisateurId = user.IdUtilisateur,
                NomUtilisateur = user.NomUtilisateur,
                Role = primaryRole?.Nom
            };
        }
    }
}
