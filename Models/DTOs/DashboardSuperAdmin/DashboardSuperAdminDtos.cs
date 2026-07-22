using ProsocAPI.Models.DTOs.DashboardAdmin;

namespace ProsocAPI.Models.DTOs.DashboardSuperAdmin
{
    public class SuperAdminSystemKpisDto
    {
        public int TotalUtilisateursActifs { get; set; }
        public int TotalUtilisateursInactifs { get; set; }
        public int UtilisateursDoiventChangerMotDePasse { get; set; }
        public int TotalRoles { get; set; }
        public int TotalPermissionsActives { get; set; }
        public bool FlexPayMarchandConfigure { get; set; }
        public bool FlexPayMobileMoneyActif { get; set; }
        public bool FlexPayCarteBancaireActif { get; set; }
        public int CollectesFlexPayEnAttente { get; set; }
        public int CollectesFlexPayExpirees { get; set; }
        public int CollectesFlexPayEchec { get; set; }
    }

    public class UtilisateursParRoleDto
    {
        public string RoleNom { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public int NombreUtilisateurs { get; set; }
    }

    public class DashboardSuperAdminDto
    {
        public DashboardAdminKpisDto KpisAdmin { get; set; } = new();
        public SuperAdminSystemKpisDto KpisSysteme { get; set; } = new();
        public List<UtilisateursParRoleDto> UtilisateursParRole { get; set; } = new();
        public List<PerformanceAgentsDto> TopAgents { get; set; } = new();
        public List<CollecteEnAttenteDto> CollectesEnAttenteValidation { get; set; } = new();
        public DateTime DerniereMiseAJour { get; set; }
        /// <summary>Code ISO de la devise principale (ex. USD) pour les montants consolidés du dashboard.</summary>
        public string? DevisePrincipaleCode { get; set; }
    }
}
