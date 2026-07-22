using System.ComponentModel.DataAnnotations;
using ProsocAPI.Models.Authentication;

namespace ProsocAPI.Models.DTOs.Authentication
{
    // ═══════════════════════════════════════════════════════════════════════════════════
    // 📋 REQUEST DTOs
    // ═══════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO pour la requête d'authentification (inspiré de Kenergie)
    /// Supporte email, téléphone ou username
    /// </summary>
    public class AuthentificationRequest
    {
        [Required(ErrorMessage = "L'email ou le téléphone est requis")]
        [MaxLength(200)]
        public string EmailOuTelephone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [MaxLength(200)]
        public string MotDePasse { get; set; } = string.Empty;

        // Informations du device (optionnelles)
        public string? FcmToken { get; set; }
        public string? DeviceType { get; set; }
        public string? DeviceModel { get; set; }
        public string? OsVersion { get; set; }
        public string? DeviceInfo { get; set; }
    }

    /// <summary>
    /// Request pour rafraîchir le token d'accès (format standard Kenergie/Kinde)
    /// </summary>
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Type de grant (fixe à "refresh_token" pour conformité OAuth2)
        /// </summary>
        public string GrantType { get; set; } = "refresh_token";

        /// <summary>
        /// Informations optionnelles sur le device pour tracking
        /// </summary>
        public string? DeviceInfo { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════════════
    // 📋 RESPONSE DTOs
    // ═══════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO pour la réponse d'authentification enrichie (inspiré de Kenergie)
    /// </summary>
    public class AuthentificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool DoitChangerMotDePasse { get; set; }
        public bool AcceptNotification { get; set; } = true;

        // Informations utilisateur
        public UtilisateurDto? Utilisateur { get; set; }
        public string? NomRole { get; set; }
        public List<string>? Permissions { get; set; }
        public RoleDto? PrimaryRole { get; set; }
        public List<RoleDto>? Roles { get; set; }
    }

    /// <summary>
    /// DTO pour la réponse de rafraîchissement de token (format standard Kenergie/Kinde)
    /// </summary>
    public class RefreshTokenResponse
    {
        /// <summary>
        /// Indique si le rafraîchissement a réussi
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message de statut
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Nouveau token d'accès JWT
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Nouveau refresh token (rotation automatique)
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Type de token (toujours "Bearer" pour conformité OAuth2)
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Durée de validité en secondes
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Date d'expiration UTC
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// DTO pour la demande de révocation de token
    /// </summary>
    public class RevokeTokenRequestDto
    {
        /// <summary>
        /// Refresh token à révoquer
        /// </summary>
        [Required(ErrorMessage = "Le refresh token est requis")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ChangerMotDePasseRequestDto
    {
        [Required]
        public int IdUtilisateur { get; set; }

        [Required]
        public string AncienMotDePasse { get; set; } = string.Empty;

        [Required]
        [MinLength(3, ErrorMessage = "Le nouveau mot de passe doit contenir au moins 3 caractères")]
        public string NouveauMotDePasse { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NouveauMotDePasse), ErrorMessage = "La confirmation du mot de passe ne correspond pas")]
        public string ConfirmerNouveauMotDePasse { get; set; } = string.Empty;
    }

    public class MotDePasseOublieRequestDto
    {
        [Required]
        public string EmailOuTelephone { get; set; } = string.Empty;
    }

    public class MotDePasseOublieConfirmerRequestDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(3, ErrorMessage = "Le nouveau mot de passe doit contenir au moins 3 caractères")]
        public string NouveauMotDePasse { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NouveauMotDePasse), ErrorMessage = "La confirmation du mot de passe ne correspond pas")]
        public string ConfirmerNouveauMotDePasse { get; set; } = string.Empty;
    }

    public class ReinitialiserUnRequestDto
    {
        [Required]
        public int IdUtilisateur { get; set; }
    }

    public class ReinitialiserMasseRequestDto
    {
        [Required]
        public int RoleId { get; set; }

        public int? EcoleId { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════════════
    // 👤 USER DTOs
    // ═══════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO pour les informations utilisateur (version allégée)
    /// </summary>
    public class UtilisateurDto
    {
        public int IdUtilisateur { get; set; }
        public string? ReferenceUtilisateur { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public string? NomUtilisateur { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Genre { get; set; }
        public bool Statut { get; set; }
        public DateTime DateCreation { get; set; }
        public bool IsConnecte { get; set; }
        public bool DoitChangerMotDePasse { get; set; }
        
        // Informations de liaison (style Kenergie)
        public int? AgentId { get; set; }
        public int? AffilieId { get; set; }
        public int? HopitalPartenaireId { get; set; }
        public int? AssureurId { get; set; }

        /// <summary>Agent gestionnaire du compte affilié (Adhesion.AgentId). Null si non affilié.</summary>
        public int? IdAgentGestionnaireCompte { get; set; }

        public string? NomAgentGestionnaireCompte { get; set; }

        public string? MatriculeAgentGestionnaireCompte { get; set; }
    }

    /// <summary>
    /// DTO pour les informations de rôle
    /// </summary>
    public class RoleDto
    {
        public int IdRole { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Niveau { get; set; }
        public bool Statut { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════════════
    // 🔧 TEST DTOs
    // ═══════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO pour les endpoints de test d'authentification
    /// </summary>
    public class AuthTestResponse
    {
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO pour les informations d'utilisateur dans les endpoints de test
    /// </summary>
    public class AuthTestUserInfo
    {
        public bool IsAuthenticated { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserRole { get; set; }
        public int? SocieteId { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsStaff { get; set; }
        public bool HasFinanceAccess { get; set; }
        public bool HasPedagogieAccess { get; set; }
        public int? AgentId { get; set; }
        public int? ClientId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO pour la réponse des endpoints protégés
    /// </summary>
    public class ProtectedEndpointResponse
    {
        public string Message { get; set; } = string.Empty;
        public AuthTestUserInfo User { get; set; } = new();
        public string? Note { get; set; }
    }
}
