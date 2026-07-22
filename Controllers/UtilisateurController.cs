using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.DTOs.Authentication;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services;
using ProsocAPI.Services.Repositories;
using ProsocAPI.Utilities;
using Prosoc.Data;
using Prosoc.Utilities;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UtilisateurController : BaseApiController
    {
        private readonly IUtilisateurRepository _repo;
        private readonly EnhancedAuthService _authService;
        private readonly ProsocDbContext _db;

        public UtilisateurController(
            IUtilisateurRepository repo,
            EnhancedAuthService authService,
            ProsocDbContext db,
            IPaginationService paginationService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<UtilisateurController> logger)
            : base(paginationService, paginationOptions, logger)
        {
            _repo = repo;
            _authService = authService;
            _db = db;
        }

        // ═══════════════════════════════════════════════════════════════════════════════════
        // 🔐 ENDPOINTS D'AUTHENTIFICATION (PUBLICS - SANS AUTHORIZE)
        // ═══════════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Authentification enrichie avec support multi-canal (email/téléphone/username)
        /// </summary>
        /// 
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<UtilisateurReadDto>>> GetAll(
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.Utilisateurs
                    .Include(u => u.Role)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(ToReadDto).ToList();

                var paginatedDtos = new PaginatedResponse<UtilisateurReadDto>
                {
                    Data = dtos,
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasNextPage = result.HasNextPage,
                    HasPreviousPage = result.HasPreviousPage
                };

                return Ok(paginatedDtos);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des utilisateurs paginés",
                    ex);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UtilisateurDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var item = await _repo.GetByIdAsync(id, ct);
            return item == null ? NotFound() : Ok(await ToUtilisateurDtoAsync(item, ct));
        }

        [HttpPost]
        public async Task<ActionResult<UtilisateurReadDto>> Create([FromBody] UtilisateurCreateDto input, CancellationToken ct)
        {
            var roleError = await ValidateAgentHopitalAccountAsync(input.RoleId, input.HopitalPartenaireId, ct);
            if (roleError != null)
                return BadRequest(new { message = roleError });

            roleError = await ValidateAssureurAccountAsync(input.RoleId, input.AssureurId, ct);
            if (roleError != null)
                return BadRequest(new { message = roleError });

            var entity = new Utilisateur
            {
                NomUtilisateur = input.NomUtilisateur,
                EmailUtilisateur = input.EmailUtilisateur,
                PhoneUtilisateur = input.PhoneUtilisateur,
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(input.MotDePasse),
                Statut = input.Statut,
                RoleId = input.RoleId,
                AgentId = input.AgentId,
                AffilieId = input.AffilieId,
                HopitalPartenaireId = input.HopitalPartenaireId,
                AssureurId = input.AssureurId,
                DoitChangerMotDePasse = false
            };

            var created = await _repo.CreateAsync(entity, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.IdUtilisateur }, ToReadDto(created));
        }

        /// <summary>
        /// Crée un compte personnel d'accueil pour un hôpital partenaire (rôle Agent Hôpital).
        /// </summary>
        [HttpPost("agent-hopital")]
        [Authorize(Roles = "Admin,IT,SuperAdmin")]
        public async Task<ActionResult<UtilisateurReadDto>> CreateAgentHopital(
            [FromBody] AgentHopitalUtilisateurCreateDto input,
            CancellationToken ct)
        {
            var role = await _db.Roles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Nom == CurrentUserHopitalResolver.AgentHopitalRoleName, ct);
            if (role == null)
                return BadRequest(new { message = "Rôle « Agent Hôpital » introuvable." });

            var hopitalExists = await _db.HopitalPartenaires
                .AnyAsync(h => h.IdHopital == input.HopitalPartenaireId && h.Statut, ct);
            if (!hopitalExists)
                return BadRequest(new { message = "Hôpital partenaire introuvable ou inactif." });

            if (!string.IsNullOrWhiteSpace(input.EmailUtilisateur)
                && await _repo.ExistsByEmailAsync(input.EmailUtilisateur, ct))
            {
                return Conflict(new { message = "Un utilisateur avec cet email existe déjà." });
            }

            var entity = new Utilisateur
            {
                NomUtilisateur = input.NomUtilisateur,
                EmailUtilisateur = input.EmailUtilisateur,
                PhoneUtilisateur = input.PhoneUtilisateur,
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(input.MotDePasse),
                Statut = input.Statut,
                RoleId = role.IdRole,
                HopitalPartenaireId = input.HopitalPartenaireId,
                DoitChangerMotDePasse = true
            };

            var created = await _repo.CreateAsync(entity, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.IdUtilisateur }, ToReadDto(created));
        }

        /// <summary>
        /// Crée un compte portail pour un partenaire assureur (rôle Assureur).
        /// </summary>
        [HttpPost("utilisateur-assureur")]
        [Authorize(Roles = "Admin,IT,SuperAdmin")]
        public async Task<ActionResult<UtilisateurReadDto>> CreateUtilisateurAssureur(
            [FromBody] AssureurUtilisateurCreateDto input,
            CancellationToken ct)
        {
            var role = await _db.Roles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Nom == CurrentUserAssureurResolver.AssureurRoleName, ct);
            if (role == null)
                return BadRequest(new { message = "Rôle « Assureur » introuvable." });

            var assureurExists = await _db.Assureurs
                .AnyAsync(a => a.IdAssureur == input.AssureurId && a.Statut, ct);
            if (!assureurExists)
                return BadRequest(new { message = "Assureur partenaire introuvable ou inactif." });

            if (!string.IsNullOrWhiteSpace(input.EmailUtilisateur)
                && await _repo.ExistsByEmailAsync(input.EmailUtilisateur, ct))
            {
                return Conflict(new { message = "Un utilisateur avec cet email existe déjà." });
            }

            var entity = new Utilisateur
            {
                NomUtilisateur = input.NomUtilisateur,
                EmailUtilisateur = input.EmailUtilisateur,
                PhoneUtilisateur = input.PhoneUtilisateur,
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(input.MotDePasse),
                Statut = input.Statut,
                RoleId = role.IdRole,
                AssureurId = input.AssureurId,
                DoitChangerMotDePasse = true
            };

            var created = await _repo.CreateAsync(entity, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.IdUtilisateur }, ToReadDto(created));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<UtilisateurReadDto>> Update([FromRoute] int id, [FromBody] UtilisateurUpdateDto input, CancellationToken ct)
        {
            var roleError = await ValidateAgentHopitalAccountAsync(input.RoleId, input.HopitalPartenaireId, ct);
            if (roleError != null)
                return BadRequest(new { message = roleError });

            roleError = await ValidateAssureurAccountAsync(input.RoleId, input.AssureurId, ct);
            if (roleError != null)
                return BadRequest(new { message = roleError });

            var entity = new Utilisateur
            {
                NomUtilisateur = input.NomUtilisateur,
                EmailUtilisateur = input.EmailUtilisateur,
                PhoneUtilisateur = input.PhoneUtilisateur,
                Statut = input.Statut,
                RoleId = input.RoleId,
                AgentId = input.AgentId,
                AffilieId = input.AffilieId,
                HopitalPartenaireId = input.HopitalPartenaireId,
                AssureurId = input.AssureurId
            };

            var updated = await _repo.UpdateAsync(id, entity, ct);
            return updated == null ? NotFound() : Ok(ToReadDto(updated));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var ok = await _repo.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }
        
                // ═══════════════════════════════════════════════════════════════════════════════════
        // �👤 ENDPOINTS CRUD UTILISATEURS (PROTÉGÉS)
        // ═════════════════════════════════════════════════════════════════════════════════

        [HttpGet("email")]
        public async Task<ActionResult<UtilisateurReadDto>> GetByEmail([FromQuery] string email, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email requis" });

            var item = await _repo.GetByEmailAsync(email, ct);
            return item == null ? NotFound() : Ok(ToReadDto(item));
        }

        [HttpGet("role/{roleId:int}")]
        public async Task<ActionResult<List<UtilisateurReadDto>>> GetByRole([FromRoute] int roleId, CancellationToken ct)
        {
            var items = await _repo.GetByRoleAsync(roleId, ct);
            return Ok(items.Select(ToReadDto).ToList());
        }

        [HttpGet("statut/{statut:bool}")]
        public async Task<ActionResult<List<UtilisateurReadDto>>> GetByStatut([FromRoute] bool statut, CancellationToken ct)
        {
            var items = await _repo.GetByStatutAsync(statut, ct);
            return Ok(items.Select(ToReadDto).ToList());
        }

        [HttpGet("exists/{id:int}")]
        public async Task<ActionResult> ExistsById([FromRoute] int id, CancellationToken ct)
        {
            var exists = await _repo.ExistsByIdAsync(id, ct);
            return Ok(new { exists });
        }

        [HttpGet("exists/email/{email}")]
        public async Task<ActionResult> ExistsByEmail([FromRoute] string email, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email requis" });

            var exists = await _repo.ExistsByEmailAsync(email, ct);
            return Ok(new { exists });
        }

        [HttpGet("societe/{idSociete:int}")]
        public ActionResult GetBySociete([FromRoute] int idSociete)
        {
            return BadRequest(new { message = "Filtre société non supporté dans ce modèle (IdSociete introuvable dans Utilisateur/Agent/Affilié)" });
        }

        [HttpPost("login")]
        [AllowAnonymous]
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

                return Ok(response);
            }
            catch (Exception ex)
            {
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
        [AllowAnonymous]
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

                return Ok(response);
            }
            catch (Exception ex)
            {
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
        [Authorize]
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
                    return Ok(new { message = "Déconnexion réussie" });
                }
                else
                {
                    return BadRequest(new { message = "Erreur lors de la déconnexion" });
                }
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse("Une erreur interne est survenue", ex);
            }
        }

        /// <summary>
        /// Révoquer un refresh token spécifique
        /// </summary>
        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<ActionResult> RevokeToken([FromBody] RevokeTokenRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Données invalides" });
                }

                var userIdClaim = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("Utilisateur non identifié");
                }

                var success = await _authService.RevokeTokenAsync(request.RefreshToken, userId);
                
                if (success)
                {
                    return Ok(new { message = "Token révoqué avec succès" });
                }
                else
                {
                    return BadRequest(new { message = "Token invalide ou non trouvé" });
                }
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse("Une erreur interne est survenue", ex);
            }
        }

        /// <summary>
        /// Révoquer TOUS les refresh tokens de l'utilisateur connecté
        /// </summary>
        [HttpPost("revoke-all-tokens")]
        [Authorize]
        public async Task<ActionResult> RevokeAllTokens()
        {
            try
            {
                var userIdClaim = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("Utilisateur non identifié");
                }

                var success = await _authService.RevokeAllUserTokensAsync(userId);
                
                if (success)
                {
                    return Ok(new { message = "Tous les tokens ont été révoqués avec succès" });
                }
                else
                {
                    return BadRequest(new { message = "Erreur lors de la révocation des tokens" });
                }
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse("Une erreur interne est survenue", ex);
            }
        }

        [HttpPost("changer_mot_de_passe")]
        [AllowAnonymous]
        public async Task<ActionResult> ChangerMotDePasse([FromBody] ChangerMotDePasseRequestDto request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ok = await _authService.ChangePasswordAsync(request.IdUtilisateur, request.AncienMotDePasse, request.NouveauMotDePasse, ct);
            if (!ok)
                return BadRequest(new { message = "Ancien mot de passe incorrect ou utilisateur non trouvé" });

            return Ok(new { message = "Mot de passe changé avec succès" });
        }

        [HttpPost("mot-de-passe-oublie")]
        [AllowAnonymous]
        public async Task<ActionResult> MotDePasseOublie([FromBody] MotDePasseOublieRequestDto request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (ok, token) = await _authService.CreatePasswordResetTokenAsync(request.EmailOuTelephone, ct);
            if (!ok || string.IsNullOrWhiteSpace(token))
                return NotFound(new { message = "Utilisateur non trouvé" });

            return Ok(new { message = "Token de réinitialisation généré", token });
        }

        [HttpPost("mot-de-passe-oublie/confirmer")]
        [AllowAnonymous]
        public async Task<ActionResult> MotDePasseOublieConfirmer([FromBody] MotDePasseOublieConfirmerRequestDto request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ok = await _authService.ConfirmPasswordResetAsync(request.Token, request.NouveauMotDePasse, ct);
            if (!ok)
                return BadRequest(new { message = "Token invalide ou expiré" });

            return Ok(new { message = "Mot de passe réinitialisé avec succès" });
        }

        [HttpPut("toggle-statut/{id:int}")]
        [Authorize(Roles = "Admin,Super-Admin")]
        public async Task<ActionResult> ToggleStatut([FromRoute] int id, CancellationToken ct)
        {
            var statut = await _authService.ToggleUserStatusAsync(id, ct);
            if (statut == null)
                return NotFound(new { message = "Utilisateur non trouvé" });

            return Ok(new { message = "Statut utilisateur mis à jour", statut = statut.Value });
        }

        [HttpPost("reinitialiser-un")]
        [Authorize(Roles = "Admin,Super-Admin")]
        public async Task<ActionResult> ReinitialiserUn([FromBody] ReinitialiserUnRequestDto request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ok = await _authService.ResetPasswordForUserAsync(request.IdUtilisateur, "123456", ct);
            if (!ok)
                return NotFound(new { message = "Utilisateur non trouvé" });

            return Ok(new { message = "Mot de passe réinitialisé avec succès" });
        }

        [HttpPost("reinitialiser-masse")]
        [Authorize(Roles = "Admin,Super-Admin")]
        public async Task<ActionResult> ReinitialiserMasse([FromBody] ReinitialiserMasseRequestDto request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.EcoleId.HasValue)
                return BadRequest(new { message = "Filtre école non supporté dans ce modèle (EcoleId introuvable dans les entités actuelles)" });

            var count = await _authService.ResetPasswordBulkByRoleAsync(request.RoleId, "123456", ct);
            return Ok(new { message = "Réinitialisation de masse terminée", count });
        }

        // ═══════════════════════════════════════════════════════════════════════════════════
        // 👥 GESTION DES RÔLES UTILISATEUR (PROTÉGÉS)
        // ═══════════════════════════════════════════════════════════════════════════════════

        [HttpGet("{id:int}/roles")]
        public async Task<ActionResult<List<RoleReadDto>>> GetUserRoles([FromRoute] int id, CancellationToken ct)
        {
            var roles = await _repo.GetUserRolesEntitiesAsync(id, ct);
            return Ok(roles.Select(r => new RoleReadDto
            {
                IdRole = r.IdRole,
                Nom = r.Nom,
                Description = r.Description,
                Niveau = r.Niveau,
                Statut = r.Statut,
                DateCreation = r.DateCreation
            }).ToList());
        }

        [HttpPost("{id:int}/roles/{roleId:int}")]
        public async Task<ActionResult> AddRoleToUser([FromRoute] int id, [FromRoute] int roleId, CancellationToken ct)
        {
            var assignedByUserIdClaim = User.FindFirst("uid")?.Value;
            int? assignedByUserId = null;
            if (!string.IsNullOrWhiteSpace(assignedByUserIdClaim) && int.TryParse(assignedByUserIdClaim, out var parsed))
                assignedByUserId = parsed;

            var ok = await _repo.AddRoleToUserAsync(id, roleId, assignedByUserId, isPrimary: false, ct);
            if (!ok)
                return NotFound(new { message = "Utilisateur ou rôle non trouvé" });

            return Ok(new { message = "Rôle ajouté avec succès" });
        }

        [HttpDelete("{id:int}/roles/{roleId:int}")]
        public async Task<ActionResult> RemoveRoleFromUser([FromRoute] int id, [FromRoute] int roleId, CancellationToken ct)
        {
            var ok = await _repo.RemoveRoleFromUserAsync(id, roleId, ct);
            if (!ok)
                return BadRequest(new { message = "Impossible de retirer ce rôle (introuvable ou dernier rôle actif)" });

            return Ok(new { message = "Rôle retiré avec succès" });
        }

        [HttpPut("{id:int}/roles/{roleId:int}/primary")]
        public async Task<ActionResult> SetPrimaryRole([FromRoute] int id, [FromRoute] int roleId, CancellationToken ct)
        {
            var ok = await _repo.SetPrimaryRoleAsync(id, roleId, ct);
            if (!ok)
                return NotFound(new { message = "Rôle non trouvé pour cet utilisateur" });

            return Ok(new { message = "Rôle principal mis à jour avec succès" });
        }



        

        private static UtilisateurReadDto ToReadDto(Utilisateur u)
        {
            return new UtilisateurReadDto
            {
                IdUtilisateur = u.IdUtilisateur,
                ReferenceUtilisateur = u.ReferenceUtilisateur,
                NomUtilisateur = u.NomUtilisateur,
                EmailUtilisateur = u.EmailUtilisateur,
                PhoneUtilisateur = u.PhoneUtilisateur,
                Statut = u.Statut,
                RoleId = u.RoleId,
                AgentId = u.AgentId,
                AffilieId = u.AffilieId,
                HopitalPartenaireId = u.HopitalPartenaireId,
                AssureurId = u.AssureurId,
                DateCreation = u.DateCreation
            };
        }

        private async Task<string?> ValidateAssureurAccountAsync(int? roleId, int? assureurId, CancellationToken ct)
        {
            if (!roleId.HasValue)
                return null;

            var roleNom = await _db.Roles.AsNoTracking()
                .Where(r => r.IdRole == roleId.Value)
                .Select(r => r.Nom)
                .FirstOrDefaultAsync(ct);

            if (roleNom != CurrentUserAssureurResolver.AssureurRoleName)
                return null;

            if (!assureurId.HasValue || assureurId.Value <= 0)
                return "AssureurId est obligatoire pour le rôle Assureur.";

            var assureurExists = await _db.Assureurs
                .AnyAsync(a => a.IdAssureur == assureurId.Value && a.Statut, ct);

            return assureurExists
                ? null
                : "Assureur partenaire introuvable ou inactif.";
        }

        private async Task<string?> ValidateAgentHopitalAccountAsync(int? roleId, int? hopitalPartenaireId, CancellationToken ct)
        {
            if (!roleId.HasValue)
                return null;

            var roleNom = await _db.Roles.AsNoTracking()
                .Where(r => r.IdRole == roleId.Value)
                .Select(r => r.Nom)
                .FirstOrDefaultAsync(ct);

            if (roleNom != CurrentUserHopitalResolver.AgentHopitalRoleName)
                return null;

            if (!hopitalPartenaireId.HasValue || hopitalPartenaireId.Value <= 0)
                return "HopitalPartenaireId est obligatoire pour le rôle Agent Hôpital.";

            var hopitalExists = await _db.HopitalPartenaires
                .AnyAsync(h => h.IdHopital == hopitalPartenaireId.Value && h.Statut, ct);

            return hopitalExists
                ? null
                : "Hôpital partenaire introuvable ou inactif.";
        }

        private async Task<UtilisateurDto> ToUtilisateurDtoAsync(Utilisateur u, CancellationToken ct = default)
        {
            var dto = new UtilisateurDto
            {
                IdUtilisateur = u.IdUtilisateur,
                ReferenceUtilisateur = u.ReferenceUtilisateur?.ToString() ?? string.Empty,
                NomComplet = u.NomUtilisateur,
                NomUtilisateur = u.NomUtilisateur,
                Email = u.EmailUtilisateur ?? (LooksLikeEmail(u.NomUtilisateur) ? u.NomUtilisateur : null),
                Telephone = u.PhoneUtilisateur ?? (LooksLikePhoneNumber(u.NomUtilisateur) ? u.NomUtilisateur : null),
                PhotoUrl = null,
                Genre = null,
                Statut = u.Statut,
                DateCreation = u.DateCreation,
                IsConnecte = u.IsConnecte,
                DoitChangerMotDePasse = u.DoitChangerMotDePasse,
                AgentId = u.AgentId,
                AffilieId = u.AffilieId,
                HopitalPartenaireId = u.HopitalPartenaireId,
                AssureurId = u.AssureurId
            };

            await UtilisateurGestionnaireHelper.EnrichGestionnaireAffilieAsync(dto, _db, u.AffilieId, ct: ct);
            return dto;
        }

        private static bool LooksLikeEmail(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value.Contains('@');
        }

        private static bool LooksLikePhoneNumber(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var trimmed = value.Trim();
            if (trimmed.StartsWith("+"))
                trimmed = trimmed[1..];

            if (trimmed.Length < 8)
                return false;

            return trimmed.All(char.IsDigit);
        }

        /// <summary>
        /// Récupère les utilisateurs avec filtres avancés
        /// </summary>
        [HttpPost("advanced")]
        public async Task<ActionResult<ExtendedPaginatedResponse<UtilisateurReadDto>>> GetUtilisateursAdvanced(
            [FromBody] AdvancedPaginationRequest request)
        {
            try
            {
                // Construire la requête de base
                var query = _db.Utilisateurs
                    .Include(u => u.Role)
                    .AsQueryable();

                // Appliquer les filtres de base
                if (request.FilterList != null && request.FilterList.Any())
                {
                    foreach (var filter in request.FilterList)
                    {
                        switch (filter.Field.ToLower())
                        {
                            case "roleid":
                                if (filter.Operator == "eq")
                                    query = query.Where(u => u.RoleId == int.Parse(filter.Value));
                                break;
                            case "statut":
                                if (filter.Operator == "eq")
                                    query = query.Where(u => u.Statut == bool.Parse(filter.Value));
                                break;
                            case "nomutilisateur":
                                if (filter.Operator == "contains")
                                    query = query.Where(u => u.NomUtilisateur.Contains(filter.Value));
                                else if (filter.Operator == "eq")
                                    query = query.Where(u => u.NomUtilisateur == filter.Value);
                                break;
                            case "emailutilisateur":
                                if (filter.Operator == "contains")
                                    query = query.Where(u => u.EmailUtilisateur != null && u.EmailUtilisateur.Contains(filter.Value));
                                else if (filter.Operator == "eq")
                                    query = query.Where(u => u.EmailUtilisateur == filter.Value);
                                break;
                            case "phoneutilisateur":
                                if (filter.Operator == "contains")
                                    query = query.Where(u => u.PhoneUtilisateur != null && u.PhoneUtilisateur.Contains(filter.Value));
                                else if (filter.Operator == "eq")
                                    query = query.Where(u => u.PhoneUtilisateur == filter.Value);
                                break;
                            case "agentid":
                                if (filter.Operator == "eq")
                                    query = query.Where(u => u.AgentId == int.Parse(filter.Value));
                                break;
                            case "affilieid":
                                if (filter.Operator == "eq")
                                    query = query.Where(u => u.AffilieId == int.Parse(filter.Value));
                                break;
                            case "isconnecte":
                                if (filter.Operator == "eq")
                                    query = query.Where(u => u.IsConnecte == bool.Parse(filter.Value));
                                break;
                            case "doitchangermotdepasse":
                                if (filter.Operator == "eq")
                                    query = query.Where(u => u.DoitChangerMotDePasse == bool.Parse(filter.Value));
                                break;
                            case "datecreation":
                                if (filter.Operator == "eq")
                                    query = query.Where(u => u.DateCreation.Date == DateTime.Parse(filter.Value).Date);
                                else if (filter.Operator == "gt")
                                    query = query.Where(u => u.DateCreation > DateTime.Parse(filter.Value));
                                else if (filter.Operator == "lt")
                                    query = query.Where(u => u.DateCreation < DateTime.Parse(filter.Value));
                                break;
                        }
                    }
                }

                // Appliquer la pagination
                var response = await _paginationService.CreateExtendedPaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs
                var utilisateurDtos = response.Data.Select(ToReadDto).ToList();
                
                // Créer une nouvelle réponse avec les DTOs
                var dtoResponse = new ExtendedPaginatedResponse<UtilisateurReadDto>
                {
                    Data = utilisateurDtos,
                    CurrentPage = response.CurrentPage,
                    PageSize = response.PageSize,
                    TotalItems = response.TotalItems,
                    TotalPages = response.TotalPages,
                    HasNextPage = response.HasNextPage,
                    HasPreviousPage = response.HasPreviousPage,
                    AppliedFilters = request.FilterList?.Select(f => $"{f.Field} {f.Operator} {f.Value}").ToList() ?? new(),
                    AppliedSorting = $"{request.SortBy} {request.SortDirection}"
                };

                return Ok(dtoResponse);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des utilisateurs avancés",
                    ex);
            }
        }

        /// <summary>
        /// Récupère les utilisateurs par rôle avec pagination
        /// </summary>
        [HttpGet("by-role/{roleId}")]
        public async Task<ActionResult<PaginatedResponse<UtilisateurReadDto>>> GetByRole(
            int roleId, 
            [FromQuery] PaginationRequest request)
        {
            try
            {
                var query = _db.Utilisateurs
                    .Include(u => u.Role)
                    .Where(u => u.RoleId == roleId)
                    .AsQueryable();

                var result = await _paginationService.CreatePaginatedResponseAsync(query, request);

                // Mapper les entités vers les DTOs et créer une nouvelle réponse
                var dtos = result.Data.Select(ToReadDto).ToList();

                var paginatedDtos = new PaginatedResponse<UtilisateurReadDto>
                {
                    Data = dtos,
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalItems,
                    TotalPages = result.TotalPages,
                    HasNextPage = result.HasNextPage,
                    HasPreviousPage = result.HasPreviousPage
                };

                return Ok(paginatedDtos);
            }
            catch (Exception ex)
            {
                return this.TechnicalErrorResponse(
                    "Une erreur technique est survenue lors de la récupération des utilisateurs pour le rôle ",
                    ex);
            }
        }
    }
}
