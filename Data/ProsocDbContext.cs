using Microsoft.EntityFrameworkCore;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.Authentication;
using System.ComponentModel.DataAnnotations;
using BCrypt.Net;

namespace Prosoc.Data
{
    public class ProsocDbContext : DbContext
    {
        public ProsocDbContext(DbContextOptions<ProsocDbContext> options) : base(options)
        {
        }

        // ═══════════════════════════════════════════════════════════════════════════════════
        // ✅ MODULE Affilie & ADHÉSION
        // ═══════════════════════════════════════════════════════════════════════════════════
        public DbSet<CategorieAdhesion> CategoriesAdhesions { get; set; } = null!;
        public DbSet<TypeAdhesion> TypeAdhesions { get; set; } = null!;
        public DbSet<TarifCotisation> TarifsCotisation { get; set; } = null!;
        [Obsolete("Use TarifsCotisation instead.")]
        public DbSet<TarifCotisation> CotisationsAffilie => TarifsCotisation;
        public DbSet<Adhesion> Adhesions { get; set; } = null!;
        public DbSet<Affilie> Affilies { get; set; } = null!;
        public DbSet<Dependant> Dependants { get; set; } = null!;
        public DbSet<PersonneContact> PersonnesContact { get; set; } = null!;
        public DbSet<Antecedant> Antecedants { get; set; } = null!;

        // ═══════════════════════════════════════════════════════════════════════════════════
        // ✅ MODULE AGENT & WALLET
        // ═══════════════════════════════════════════════════════════════════════════════════
        public DbSet<Agent> Agents { get; set; } = null!;
        public DbSet<CategorieAgent> CategoriesAgents { get; set; } = null!;
        public DbSet<WalletAgent> WalletsAgents { get; set; } = null!;
        public DbSet<WalletVirtuelAgent> WalletsVirtuelsAgents { get; set; } = null!;
        public DbSet<WalletVirtuelMouvement> WalletVirtuelMouvements { get; set; } = null!;
        public DbSet<WalletMouvement> WalletMouvements { get; set; } = null!;
        public DbSet<RetraitAgent> RetraitsAgents { get; set; } = null!;
        public DbSet<TargetAgent> TargetsAgents { get; set; } = null!;
        public DbSet<RetenueMaashAgent> RetenuesMaashAgents { get; set; } = null!;
        public DbSet<AgentBeneficiaireMaash> AgentBeneficiairesMaash { get; set; } = null!;

        // ═══════════════════════════════════════════════════════════════════════════════════
        // ✅ MODULE PRODUITS & PRESTATIONS
        // ═══════════════════════════════════════════════════════════════════════════════════
        public DbSet<ProduitMutuel> ProduitsMutuels { get; set; } = null!;
        public DbSet<ProduitAssureur> ProduitsAssureurs { get; set; } = null!;
        public DbSet<Assureur> Assureurs { get; set; } = null!;
        public DbSet<Prestation> Prestations { get; set; } = null!;
        public DbSet<SouscriptionPrestation> SouscriptionsPrestations { get; set; } = null!;
        public DbSet<BonEnvoi> BonsEnvoi { get; set; } = null!;

        // ═══════════════════════════════════════════════════════════════════════════════════
        // ✅ MODULE ARRIÉRÉS DE PAIEMENT
        // ═══════════════════════════════════════════════════════════════════════════════════
        public DbSet<ArrieresAffilie> ArrieresAffilie { get; set; } = null!;
        public DbSet<PenaliteAffilie> PenalitesAffilie { get; set; } = null!;

        // ═════════════════════════════════════════════════════════════════════════════════════════════════
        // ✅ MODULE DEMANDES DE BON D'ENVOI
        // ═══════════════════════════════════════════════════════════════════════════════════════════
        public DbSet<DemandeBonEnvoi> DemandesBonEnvoi { get; set; } = null!;

        // ═════════════════════════════════════════════════════════════════════════════════════════════════
        // ✅ MODULE RETRAITS AGENTS
        // ═════════════════════════════════════════════════════════════════════════════════════════════════
        public DbSet<DemandeRetraitAgent> DemandesRetraitAgents { get; set; } = null!;
        public DbSet<DemandeRechargeWalletVirtuel> DemandesRechargeWalletVirtuel { get; set; } = null!;
        public DbSet<JetonRetrait> JetonsRetraits { get; set; } = null!;
        public DbSet<SessionCaisse> SessionsCaisses { get; set; } = null!;
        public DbSet<MouvementCaisse> MouvementsCaisses { get; set; } = null!;
        public DbSet<PerceptionVirtuelle> PerceptionsVirtuelles { get; set; } = null!;
        public DbSet<PerceptionVirtuelleLigne> PerceptionsVirtuellesLignes { get; set; } = null!;

        // ═════════════════════════════════════════════════════════════════════════════════════════════════
        // ✅ MODULE JETONS MÉDICAUX
        // ═════════════════════════════════════════════════════════════════════════════════════════════════
        public DbSet<JetonMedical> JetonsMedicaux { get; set; } = null!;
        public DbSet<HopitalPartenaire> HopitalPartenaires { get; set; } = null!;

        public DbSet<CodeAdhesionSequence> CodesAdhesionSequences { get; set; } = null!;

        // ═══════════════════════════════════════════════════════════════════════════════════
        // ✅ MODULE GÉOGRAPHIQUE
        // ═══════════════════════════════════════════════════════════════════════════════════
        public DbSet<Province> Provinces { get; set; } = null!;
        public DbSet<Commune> Communes { get; set; } = null!;
        public DbSet<ZoneSociale> ZonesSociales { get; set; } = null!;

        // ═══════════════════════════════════════════════════════════════════════════════════
        // ✅ MODULE FINANCIER
        // ═══════════════════════════════════════════════════════════════════════════════════
        public DbSet<Collecte> Collectes { get; set; } = null!;
        public DbSet<Devise> Devises { get; set; } = null!;
        public DbSet<TauxChangeDevise> TauxChangeDevises { get; set; } = null!;
        public DbSet<InfoPaiementMarchand> InfoPaiementsMarchand { get; set; } = null!;
        public DbSet<CollecteEnAttente> CollectesEnAttente { get; set; } = null!;
        public DbSet<PaiementHold> PaiementHolds { get; set; } = null!;
        public DbSet<TransactionFlexPay> TransactionsFlexPay { get; set; } = null!;
        public DbSet<CallbackFlexPay> CallbacksFlexPay { get; set; } = null!;
        public DbSet<Frais> Frais { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        // ═══════════════════════════════════════════════════════════════════════════════════
        // ✅ MODULE UTILISATEUR & NOTIFICATIONS
        // ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════
        public DbSet<UserNotificationPreferences> UserNotificationPreferences { get; set; } = null!;
        public DbSet<Utilisateur> Utilisateurs { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        
        // Ajout pour les types de notifications
        public DbSet<NotificationType> NotificationTypes { get; set; } = null!;
        
        // Ajout pour les fonctionnalités mobiles
        public DbSet<ProsocAPI.Models.Mobile.MobileAppConfig> MobileAppConfigs { get; set; } = null!;
        public DbSet<ProsocAPI.Models.Mobile.MobileUserSession> MobileUserSessions { get; set; } = null!;
        public DbSet<ProsocAPI.Models.Mobile.MobileSyncData> MobileSyncData { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public DbSet<UserPermission> UserPermissions { get; set; } = null!;
        public DbSet<UserDevice> UserDevices { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

        // ═══════════════════════════════════════════════════════════════════════════════════
        // ✅ PARAMÈTRES MÉTIER (config éditable Admin/IT)
        // ═══════════════════════════════════════════════════════════════════════════════════
        public DbSet<ParametreMetier> ParametresMetier { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CodeAdhesionSequence>()
                .HasKey(x => x.Prefix);

            modelBuilder.Entity<Adhesion>()
                .HasOne(a => a.Affilie)
                .WithOne(a => a.Adhesion)
                .HasForeignKey<Adhesion>(a => a.AffilieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Adhesion>()
                .HasIndex(a => a.AffilieId)
                .IsUnique();

            modelBuilder.Entity<Adhesion>()
                .HasOne(a => a.Utilisateur)
                .WithMany()
                .HasForeignKey(a => a.UtilisateurId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Collecte>()
                .HasOne(c => c.Agent)
                .WithMany(a => a.Collectes)
                .HasForeignKey(c => c.AgentId)
                .IsRequired(false);

            modelBuilder.Entity<Collecte>()
                .HasIndex(c => c.ReferencePaiement)
                .IsUnique();

            modelBuilder.Entity<Collecte>()
                .HasOne(c => c.SouscriptionPrestationRef)
                .WithMany()
                .HasForeignKey(c => c.SouscriptionPrestationId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Collecte>()
                .HasOne(c => c.OperateurUtilisateur)
                .WithMany()
                .HasForeignKey(c => c.OperateurUtilisateurId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Agent>()
                .HasIndex(a => a.Matricule)
                .IsUnique();

            modelBuilder.Entity<Agent>()
                .HasIndex(a => a.EmailAgent)
                .IsUnique();

            modelBuilder.Entity<Utilisateur>()
                .HasIndex(u => u.EmailUtilisateur)
                .IsUnique();

            modelBuilder.Entity<Utilisateur>()
                .HasIndex(u => u.PhoneUtilisateur)
                .IsUnique();

            modelBuilder.Entity<Utilisateur>()
                .HasOne(u => u.HopitalPartenaire)
                .WithMany()
                .HasForeignKey(u => u.HopitalPartenaireId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Utilisateur>()
                .HasOne(u => u.Assureur)
                .WithMany()
                .HasForeignKey(u => u.AssureurId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Agent>()
                .HasOne(a => a.Zone)
                .WithMany(z => z.Agents)
                .HasForeignKey(a => a.ZoneSocialeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ZoneSociale>()
                .HasOne(z => z.ChefEquipe)
                .WithMany()
                .HasForeignKey(z => z.ChefEquipeAgentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ZoneSociale>()
                .HasIndex(z => z.ChefEquipeAgentId)
                .IsUnique();

            modelBuilder.Entity<Commune>()
                .HasOne(c => c.Superviseur)
                .WithMany()
                .HasForeignKey(c => c.SuperviseurAgentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Commune>()
                .HasIndex(c => c.SuperviseurAgentId)
                .IsUnique();

            modelBuilder.Entity<TypeAdhesion>()
                .HasOne(t => t.CategorieAdhesion)
                .WithMany(c => c.TypeAdhesions)
                .HasForeignKey(t => t.CategorieAdhesionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TypeAdhesion>()
                .HasOne(t => t.Devise)
                .WithMany()
                .HasForeignKey(t => t.DeviseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TarifCotisation>()
                .ToTable("TarifsCotisation")
                .HasOne(c => c.TypeAdhesion)
                .WithMany(t => t.CotisationsAffilie)
                .HasForeignKey(c => c.TypeAdhesionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TarifCotisation>()
                .HasIndex(c => new { c.TypeAdhesionId, c.Periodicite })
                .IsUnique();

            modelBuilder.Entity<TarifCotisation>()
                .HasIndex(c => c.LibelleTarifCotisationNormalized)
                .IsUnique();

            modelBuilder.Entity<TarifCotisation>()
                .HasOne(c => c.Devise)
                .WithMany(d => d.TarifsCotisation)
                .HasForeignKey(c => c.DeviseId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🆕 Configuration de la relation entre ProduitMutuel et Devise
            modelBuilder.Entity<ProduitMutuel>()
                .HasOne(pm => pm.Devise)
                .WithMany(d => d.ProduitsMutuels)
                .HasForeignKey(pm => pm.DeviseId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🆕 Configuration de la relation entre ProduitAssureur et Devise
            modelBuilder.Entity<ProduitAssureur>()
                .HasOne(pa => pa.Devise)
                .WithMany(d => d.ProduitsAssureurs)
                .HasForeignKey(pa => pa.DeviseId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🆕 Configuration de la relation entre Frais et Devise
            modelBuilder.Entity<Frais>()
                .HasOne(f => f.Devise)
                .WithMany(d => d.Frais)
                .HasForeignKey(f => f.DeviseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Frais>()
                .HasIndex(f => f.Code)
                .IsUnique();

            // 🆕 Configuration de la relation entre Collecte et Frais
            modelBuilder.Entity<Collecte>()
                .HasOne(c => c.Frais)
                .WithMany(f => f.Collectes)
                .HasForeignKey(c => c.FraisId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Collecte>()
                .HasOne(c => c.CotisationAffilie)
                .WithMany(ca => ca.Collectes)
                .HasForeignKey(c => c.CotisationAffilieId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prestation>()
                .Property(p => p.Periodicite)
                .HasDefaultValue("Mensuel");

            modelBuilder.Entity<Collecte>()
                .HasOne(c => c.ArrieresAffilie)
                .WithMany(a => a.Collectes)
                .HasForeignKey(c => c.ArrieresAffilieId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ArrieresAffilie>()
                .HasIndex(a => new
                {
                    a.AffilieId,
                    a.TypeObligation,
                    a.Mois,
                    a.Annee,
                    a.FraisId,
                    a.SouscriptionPrestationId,
                    a.CotisationAffilieId
                })
                .IsUnique();

            modelBuilder.Entity<ArrieresAffilie>()
                .HasOne(a => a.Frais)
                .WithMany()
                .HasForeignKey(a => a.FraisId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ArrieresAffilie>()
                .HasOne(a => a.SouscriptionPrestation)
                .WithMany()
                .HasForeignKey(a => a.SouscriptionPrestationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ArrieresAffilie>()
                .HasOne(a => a.CotisationAffilie)
                .WithMany()
                .HasForeignKey(a => a.CotisationAffilieId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ArrieresAffilie>()
                .HasOne(a => a.Devise)
                .WithMany()
                .HasForeignKey(a => a.DeviseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PenaliteAffilie>()
                .HasIndex(p => new { p.ArrieresAffilieId, p.TypePenalite })
                .IsUnique();

            modelBuilder.Entity<PenaliteAffilie>()
                .HasOne(p => p.Affilie)
                .WithMany()
                .HasForeignKey(p => p.AffilieId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PenaliteAffilie>()
                .HasOne(p => p.ArrieresAffilie)
                .WithMany()
                .HasForeignKey(p => p.ArrieresAffilieId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PenaliteAffilie>()
                .HasOne(p => p.Frais)
                .WithMany()
                .HasForeignKey(p => p.FraisId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PenaliteAffilie>()
                .HasOne(p => p.Devise)
                .WithMany()
                .HasForeignKey(p => p.DeviseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Collecte>()
                .HasOne(c => c.PenaliteAffilie)
                .WithMany(p => p.Collectes)
                .HasForeignKey(c => c.PenaliteAffilieId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TauxChangeDevise>()
                .HasOne(t => t.DeviseSource)
                .WithMany()
                .HasForeignKey(t => t.DeviseSourceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TauxChangeDevise>()
                .HasOne(t => t.DeviseCible)
                .WithMany()
                .HasForeignKey(t => t.DeviseCibleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TauxChangeDevise>()
                .HasIndex(t => new { t.DeviseSourceId, t.DeviseCibleId, t.DateEffet });

            modelBuilder.Entity<Collecte>()
                .HasOne(c => c.DevisePrincipale)
                .WithMany()
                .HasForeignKey(c => c.DevisePrincipaleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Collecte>()
                .HasOne(c => c.DeviseTarif)
                .WithMany()
                .HasForeignKey(c => c.DeviseTarifId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PaiementHold>()
                .HasOne(h => h.CollecteEnAttente)
                .WithMany()
                .HasForeignKey(h => h.IdCollecteEnAttente)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CallbackFlexPay>()
                .HasOne(c => c.Transaction)
                .WithMany()
                .HasForeignKey(c => c.IdTransaction)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PersonneContact>()
                .HasOne(p => p.Affilie)
                .WithOne(a => a.PersonneContact)
                .HasForeignKey<PersonneContact>(p => p.AffilieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PersonneContact>()
                .HasIndex(p => p.AffilieId)
                .IsUnique();

            modelBuilder.Entity<RetenueMaashAgent>()
                .HasIndex(r => new { r.AgentId, r.Annee, r.Mois })
                .IsUnique();

            modelBuilder.Entity<RetenueMaashAgent>()
                .HasOne(r => r.Agent)
                .WithMany()
                .HasForeignKey(r => r.AgentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RetenueMaashAgent>()
                .HasOne(r => r.WalletMouvement)
                .WithMany()
                .HasForeignKey(r => r.WalletMouvementId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AgentBeneficiaireMaash>()
                .HasOne(b => b.Agent)
                .WithMany()
                .HasForeignKey(b => b.AgentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WalletAgent>()
                .HasOne(w => w.Agent)
                .WithMany(a => a.Wallets)
                .HasForeignKey(w => w.AgentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WalletAgent>()
                .HasOne(w => w.Devise)
                .WithMany()
                .HasForeignKey(w => w.DeviseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WalletAgent>()
                .HasIndex(w => new { w.AgentId, w.DeviseId })
                .IsUnique();

            modelBuilder.Entity<WalletMouvement>()
                .HasOne(m => m.Devise)
                .WithMany()
                .HasForeignKey(m => m.DeviseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WalletVirtuelAgent>()
                .HasOne(w => w.Devise)
                .WithMany()
                .HasForeignKey(w => w.DeviseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WalletVirtuelAgent>()
                .HasIndex(w => w.DeviseId);

            modelBuilder.Entity<WalletVirtuelMouvement>()
                .HasOne(m => m.WalletVirtuel)
                .WithMany(w => w.Mouvements)
                .HasForeignKey(m => m.WalletVirtuelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WalletVirtuelMouvement>()
                .HasIndex(m => new { m.WalletVirtuelId, m.DateOperation });

            modelBuilder.Entity<WalletVirtuelMouvement>()
                .HasOne(m => m.Devise)
                .WithMany()
                .HasForeignKey(m => m.DeviseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WalletVirtuelMouvement>()
                .HasOne(m => m.OperateurUtilisateur)
                .WithMany()
                .HasForeignKey(m => m.OperateurUtilisateurId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WalletVirtuelMouvement>()
                .HasIndex(m => m.OperateurUtilisateurId);

            modelBuilder.Entity<DemandeBonEnvoi>()
                .HasOne(d => d.Agent)
                .WithMany()
                .HasForeignKey(d => d.AgentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<BonEnvoi>()
                .HasOne(b => b.JetonMedical)
                .WithOne(j => j.BonEnvoiLie)
                .HasForeignKey<BonEnvoi>(b => b.JetonMedicalId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BonEnvoi>()
                .HasIndex(b => b.JetonMedicalId)
                .IsUnique();

            modelBuilder.Entity<TargetAgent>()
                .HasOne(t => t.Role)
                .WithMany()
                .HasForeignKey(t => t.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TargetAgent>()
                .HasIndex(t => new { t.RoleId, t.Periodicite });

            modelBuilder.Entity<SessionCaisse>()
                .HasOne(s => s.Utilisateur)
                .WithMany()
                .HasForeignKey(s => s.UtilisateurId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SessionCaisse>()
                .HasOne(s => s.Devise)
                .WithMany()
                .HasForeignKey(s => s.DeviseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SessionCaisse>()
                .HasIndex(s => new { s.UtilisateurId, s.Statut });

            modelBuilder.Entity<MouvementCaisse>()
                .HasOne(m => m.SessionCaisse)
                .WithMany(s => s.Mouvements)
                .HasForeignKey(m => m.SessionCaisseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MouvementCaisse>()
                .HasOne(m => m.Utilisateur)
                .WithMany()
                .HasForeignKey(m => m.UtilisateurId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MouvementCaisse>()
                .HasOne(m => m.Collecte)
                .WithMany()
                .HasForeignKey(m => m.CollecteId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MouvementCaisse>()
                .HasOne(m => m.DemandeRetrait)
                .WithMany()
                .HasForeignKey(m => m.DemandeRetraitId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MouvementCaisse>()
                .HasOne(m => m.JetonRetrait)
                .WithMany()
                .HasForeignKey(m => m.JetonRetraitId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MouvementCaisse>()
                .HasOne(m => m.WalletMouvement)
                .WithMany()
                .HasForeignKey(m => m.WalletMouvementId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MouvementCaisse>()
                .HasOne(m => m.PerceptionVirtuelle)
                .WithMany()
                .HasForeignKey(m => m.PerceptionVirtuelleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DemandeRetraitAgent>()
                .HasOne(d => d.OperateurPaiement)
                .WithMany()
                .HasForeignKey(d => d.OperateurPaiementUtilisateurId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DemandeRetraitAgent>()
                .HasOne(d => d.WalletMouvement)
                .WithMany()
                .HasForeignKey(d => d.WalletMouvementId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DemandeRechargeWalletVirtuel>()
                .HasOne(d => d.Agent)
                .WithMany()
                .HasForeignKey(d => d.AgentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DemandeRechargeWalletVirtuel>()
                .HasOne(d => d.DemandePar)
                .WithMany()
                .HasForeignKey(d => d.DemandeParUtilisateurId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DemandeRechargeWalletVirtuel>()
                .HasOne(d => d.ConfirmePar)
                .WithMany()
                .HasForeignKey(d => d.ConfirmeParUtilisateurId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DemandeRechargeWalletVirtuel>()
                .HasOne(d => d.RejetePar)
                .WithMany()
                .HasForeignKey(d => d.RejeteParUtilisateurId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DemandeRechargeWalletVirtuel>()
                .HasOne(d => d.WalletVirtuelMouvement)
                .WithMany()
                .HasForeignKey(d => d.WalletVirtuelMouvementId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DemandeRechargeWalletVirtuel>()
                .HasIndex(d => new { d.AgentId, d.StatutDemande });

            modelBuilder.Entity<JetonRetrait>()
                .HasOne(j => j.OperateurUtilisateur)
                .WithMany()
                .HasForeignKey(j => j.OperateurUtilisateurId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Collecte>()
                .HasOne(c => c.PercepteurUtilisateur)
                .WithMany()
                .HasForeignKey(c => c.PercepteurUtilisateurId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Collecte>()
                .HasOne(c => c.PerceptionVirtuelle)
                .WithMany()
                .HasForeignKey(c => c.PerceptionVirtuelleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PerceptionVirtuelleLigne>()
                .HasIndex(l => l.CollecteId);

            modelBuilder.Entity<PerceptionVirtuelleLigne>()
                .HasOne(l => l.PerceptionVirtuelle)
                .WithMany(p => p.Lignes)
                .HasForeignKey(l => l.PerceptionVirtuelleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PerceptionVirtuelleLigne>()
                .HasOne(l => l.Collecte)
                .WithMany()
                .HasForeignKey(l => l.CollecteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PerceptionVirtuelleLigne>()
                .HasOne(l => l.Agent)
                .WithMany()
                .HasForeignKey(l => l.AgentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PerceptionVirtuelleLigne>()
                .HasOne(l => l.WalletVirtuelMouvement)
                .WithMany()
                .HasForeignKey(l => l.WalletVirtuelMouvementId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PerceptionVirtuelle>()
                .HasOne(p => p.Agent)
                .WithMany()
                .HasForeignKey(p => p.AgentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PerceptionVirtuelle>()
                .HasOne(p => p.PercepteurUtilisateur)
                .WithMany()
                .HasForeignKey(p => p.PercepteurUtilisateurId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PerceptionVirtuelle>()
                .HasOne(p => p.AnnuleParUtilisateur)
                .WithMany()
                .HasForeignKey(p => p.AnnuleParUtilisateurId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PerceptionVirtuelle>()
                .HasOne(p => p.Devise)
                .WithMany()
                .HasForeignKey(p => p.DeviseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParametreMetier>()
                .HasIndex(p => p.Code)
                .IsUnique();

            modelBuilder.Entity<ParametreMetier>()
                .HasOne(p => p.ModifiePar)
                .WithMany()
                .HasForeignKey(p => p.ModifieParUtilisateurId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
