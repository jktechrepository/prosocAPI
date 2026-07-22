using System.ComponentModel.DataAnnotations;

namespace ProsocAPI.Models.DTOs.Core
{
    // DTO principal pour les statistiques du superviseur
    public class SuperviseurStatsDto
    {
        public int SuperviseurId { get; set; }
        public string NomSuperviseur { get; set; } = string.Empty;
        public int NombreAgentsDirects { get; set; }
        public int NombreAgentsTotal { get; set; } // Hiérarchie complète
        public decimal MontantTotalEquipe { get; set; }
        public decimal PerformanceMoyenneEquipe { get; set; }
        public decimal MontantTotalSuperviseur { get; set; }
        public int NombreTransactionsSuperviseur { get; set; }
        public decimal TauxSuccesEquipe { get; set; }
        public decimal ObjectifEquipe { get; set; }
        public decimal AtteinteObjectifEquipe { get; set; }
        public List<AgentPerformanceHierarchieDto> AgentsSupervises { get; set; } = new List<AgentPerformanceHierarchieDto>();
        public DateTime DerniereMiseAJour { get; set; } = DateTime.Now;
        public string? DevisePrincipaleCode { get; set; }
    }

    // DTO pour la performance d'un agent supervisé
    public class AgentPerformanceHierarchieDto
    {
        public int AgentId { get; set; }
        public string NomAgent { get; set; } = string.Empty;
        public decimal MontantTotal { get; set; }
        public int NombreTransactions { get; set; }
        public decimal MontantMoyen { get; set; }
        public decimal TauxSucces { get; set; }
        public decimal PerformanceMoyenne { get; set; }
        public decimal ObjectifPersonnel { get; set; }
        public decimal AtteinteObjectif { get; set; }
        public int RangEquipe { get; set; }
        public decimal Progression { get; set; }
        public DateTime DerniereActivite { get; set; }
        public int NombreJoursActifs { get; set; }
        public decimal MontantCommissions { get; set; }
        public decimal NetAPercevoir { get; set; }
    }

    // DTO pour la hiérarchie complète
    public class HierarchieSuperviseurDto
    {
        public int SuperviseurId { get; set; }
        public string NomSuperviseur { get; set; } = string.Empty;
        public int NiveauHierarchique { get; set; }
        public List<AgentHierarchieDto> AgentsSupervises { get; set; } = new List<AgentHierarchieDto>();
        public List<HierarchieSuperviseurDto> SousSuperviseurs { get; set; } = new List<HierarchieSuperviseurDto>();
        public int TotalAgentsDansHierarchie { get; set; }
        public decimal MontantTotalHierarchie { get; set; }
    }

    // DTO pour un agent dans la hiérarchie
    public class AgentHierarchieDto
    {
        public int AgentId { get; set; }
        public string NomAgent { get; set; } = string.Empty;
        public string Matricule { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal MontantTotal { get; set; }
        public int NombreTransactions { get; set; }
        public decimal PerformanceMoyenne { get; set; }
        public DateTime DateCreation { get; set; }
        public bool Statut { get; set; }
        public int NiveauHierarchique { get; set; }
        public string? CheminHierarchique { get; set; }
    }

    // DTO pour l'affectation d'un superviseur
    public class AffectationSuperviseurDto
    {
        public int AgentId { get; set; }
        public string NomAgent { get; set; } = string.Empty;
        public int? AncienSuperviseurId { get; set; }
        public string? AncienSuperviseurNom { get; set; }
        public int? NouveauSuperviseurId { get; set; }
        public string? NouveauSuperviseurNom { get; set; }
        public DateTime DateAffectation { get; set; } = DateTime.Now;
        public string RaisonAffectation { get; set; } = string.Empty;
        public string AffectePar { get; set; } = string.Empty;
        public bool EstActive { get; set; } = true;
        
        // Propriété pour compatibilité
        public int SuperviseurId => NouveauSuperviseurId ?? 0;
    }

    // DTO pour les objectifs d'équipe
    public class ObjectifEquipeDto
    {
        public int SuperviseurId { get; set; }
        public string NomSuperviseur { get; set; } = string.Empty;
        public string TypeObjectif { get; set; } = string.Empty; // Journalier, Hebdomadaire, Mensuel
        public decimal ObjectifMontant { get; set; }
        public decimal RealiseMontant { get; set; }
        public decimal AtteintePourcentage { get; set; }
        public decimal RestantMontant { get; set; }
        public DateTime DebutPeriode { get; set; }
        public DateTime FinPeriode { get; set; }
        public int NombreAgentsConcerne { get; set; }
        public decimal PerformanceMoyenneEquipe { get; set; }
        public bool EstAtteint { get; set; }
        public decimal ProgressionPrecedente { get; set; }
    }

    // DTO pour le rapport de performance d'équipe
    public class RapportPerformanceEquipeDto
    {
        public int SuperviseurId { get; set; }
        public string NomSuperviseur { get; set; } = string.Empty;
        public DateTime DebutPeriode { get; set; }
        public DateTime FinPeriode { get; set; }
        public int NombreAgents { get; set; }
        public decimal MontantTotalEquipe { get; set; }
        public decimal MontantMoyenParAgent { get; set; }
        public int TotalTransactionsEquipe { get; set; }
        public decimal TauxSuccesEquipe { get; set; }
        public decimal ObjectifEquipe { get; set; }
        public decimal AtteinteObjectifEquipe { get; set; }
        public List<AgentPerformanceHierarchieDto> PerformancesAgents { get; set; } = new List<AgentPerformanceHierarchieDto>();
        public decimal CroissanceParRapportPrecedent { get; set; }
        public int RangParmiSuperviseurs { get; set; }
        public string CommentairePerformance { get; set; } = string.Empty;
        public DateTime DateGenerationRapport { get; set; } = DateTime.Now;
    }

    // DTO pour la comparaison des équipes
    public class ComparaisonEquipesDto
    {
        public List<SuperviseurStatsDto> Equipes { get; set; } = new List<SuperviseurStatsDto>();
        public DateTime DebutPeriode { get; set; }
        public DateTime FinPeriode { get; set; }
        public decimal MontantTotalGeneral { get; set; }
        public int NombreAgentsTotal { get; set; }
        public decimal PerformanceMoyenneGenerale { get; set; }
        public SuperviseurStatsDto MeilleureEquipe { get; set; }
        public SuperviseurStatsDto EquipeMoinsPerformante { get; set; }
        public decimal EcartPerformance { get; set; }
        public int NombreEquipesComparees { get; set; }
    }

    // DTO pour les tendances de l'équipe
    public class TendanceEquipeDto
    {
        public int SuperviseurId { get; set; }
        public string NomSuperviseur { get; set; } = string.Empty;
        public string Periode { get; set; } = string.Empty; // Format YYYY-MM
        public decimal MontantPeriode { get; set; }
        public int NombreAgentsPeriode { get; set; }
        public decimal PerformanceMoyennePeriode { get; set; }
        public decimal TauxSuccesPeriode { get; set; }
        public decimal Croissance { get; set; }
        public decimal ObjectifPeriode { get; set; }
        public decimal AtteinteObjectifPeriode { get; set; }
        public int NombreTransactionsPeriode { get; set; }
        public decimal MontantCommissionsPeriode { get; set; }
    }

    // DTO pour le dashboard superviseur complet
    public class DashboardSuperviseurDto
    {
        public SuperviseurStatsDto StatsSuperviseur { get; set; }
        public List<AgentPerformanceDto> TopAgents { get; set; } = new List<AgentPerformanceDto>();
        public List<TendanceEquipeDto> TendancesEquipe { get; set; } = new List<TendanceEquipeDto>();
        public List<ObjectifEquipeDto> ObjectifsEquipe { get; set; } = new List<ObjectifEquipeDto>();
        public RapportPerformanceEquipeDto RapportPerformance { get; set; }
        public HierarchieSuperviseurDto HierarchieComplete { get; set; }
        public List<AffectationSuperviseurDto> AffectationsRecentes { get; set; } = new List<AffectationSuperviseurDto>();
        public DateTime DerniereMiseAJour { get; set; } = DateTime.Now;
        public decimal MontantTotalHierarchie { get; set; }
        public int NombreTotalAgentsHierarchie { get; set; }
        public decimal PerformanceMoyenneHierarchie { get; set; }
        /// <summary>Code ISO de la devise principale (ex. USD) pour les montants consolidés.</summary>
        public string? DevisePrincipaleCode { get; set; }
    }

    // DTO pour les permissions du superviseur
    public class PermissionSuperviseurDto
    {
        public int SuperviseurId { get; set; }
        public string NomSuperviseur { get; set; } = string.Empty;
        public bool PeutVoirTousAgents { get; set; }
        public bool PeutModifierAgents { get; set; }
        public bool PeutAssignerObjectifs { get; set; }
        public bool PeutVoirRapports { get; set; }
        public bool PeutExporterDonnees { get; set; }
        public List<int> AgentsAccessibles { get; set; } = new List<int>();
        public List<string> PermissionsSpecifiques { get; set; } = new List<string>();
        public DateTime DateModificationPermissions { get; set; } = DateTime.Now;
    }

    // DTO pour l'activité du superviseur
    public class ActiviteSuperviseurDto
    {
        public int SuperviseurId { get; set; }
        public string NomSuperviseur { get; set; } = string.Empty;
        public DateTime DateActivite { get; set; }
        public int NombreAgentsConnectes { get; set; }
        public int NombreTransactionsEquipe { get; set; }
        public decimal MontantTransactionsEquipe { get; set; }
        public decimal TauxSuccesEquipe { get; set; }
        public int NombreNouveauxAgents { get; set; }
        public int NombreAgentsDesactives { get; set; }
        public List<string> ActionsRealisees { get; set; } = new List<string>();
        public decimal TempsMoyenResponse { get; set; }
        public int NombreAlertes { get; set; }
        public int NombreProblemesResolus { get; set; }
    }
}
