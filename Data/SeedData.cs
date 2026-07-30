using Microsoft.EntityFrameworkCore;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;
using ProsocAPI.Utilities;

namespace Prosoc.Data
{
    public class SeedData
    {
        public static async Task InitializeAsync(ProsocDbContext context, ILogger logger, bool forceReset = false)
        {
            try
            {
                var seedDemo = string.Equals(Environment.GetEnvironmentVariable("SEED_DEMO"), "true", StringComparison.OrdinalIgnoreCase);

                // S'assurer que la base de données est accessible
                try
                {
                    await context.Database.CanConnectAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Impossible de se connecter à la base de données");
                    return;
                }

                // Forcer le seed pour le debug (temporaire - PAS DE VÉRIFICATION)
                logger.LogInformation("Début du peuplement de la base de données...");
                
                // TEMPORAIRE : Désactiver la vérification pour forcer le seed
                
                // 1. Provinces (26 provinces de la RDC)
                if (!await context.Provinces.AnyAsync())
                {
                    var provinces = new[]
                    {
                        new Province { Nom = "Kinshasa", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Bandundu", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Bas-Congo", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Bas-Uele", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Équateur", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Haut-Katanga", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Haut-Lomami", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Haut-Uele", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Ituri", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Kasaï", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Kasaï-Central", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Kasaï-Oriental", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Kongo Central", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Kwango", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Kwilu", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Lomami", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Lualaba", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Maniema", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Mongala", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Nord-Kivu", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Nord-Ubangi", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Sud-Kivu", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Sud-Ubangi", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Tshopo", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Tshuapa", Statut = true, DateCreation = DateTime.Now },
                        new Province { Nom = "Tanganyika", Statut = true, DateCreation = DateTime.Now }
                    };
                    await context.Provinces.AddRangeAsync(provinces);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Provinces créées: {Count}", provinces.Length);
                }

                // 2. Devises
                if (!await context.Devises.AnyAsync())
                {
                    var devises = new[]
                    {
                        new Devise { Code = "CDF", Nom = "Franc Congolais", Symbole = "FC", EstDevisePrincipale = false, Statut = true, DateCreation = DateTime.Now },
                        new Devise { Code = "USD", Nom = "Dollar Américain", Symbole = "$", EstDevisePrincipale = true, Statut = true, DateCreation = DateTime.Now }
                    };
                    await context.Devises.AddRangeAsync(devises);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Devises créées: {Count}", devises.Length);
                }

                // 3. Catégories d'Adhésions
                if (!await context.CategoriesAdhesions.AnyAsync())
                {
                    var categoriesAdhesions = new[]
                    {
                        new CategorieAdhesion { IdCategorieAdhesion = 1, Libelle = "Particulier", Description = "Adhésion Particulier", Statut = true, DateCreation = DateTime.Now },
                        new CategorieAdhesion { IdCategorieAdhesion = 2, Libelle = "Entreprise", Description = "Adhésion Entreprise", Statut = true, DateCreation = DateTime.Now }
                    };
                    await context.CategoriesAdhesions.AddRangeAsync(categoriesAdhesions);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Catégories d'adhésions créées: {Count}", categoriesAdhesions.Length);
                }

                // 4. Types d'Adhésions
                if (!await context.TypeAdhesions.AnyAsync())
                {
                    var devisePrincipaleId = await context.Devises
                        .Where(d => d.EstDevisePrincipale && d.Statut)
                        .Select(d => (int?)d.IdDevise)
                        .FirstOrDefaultAsync();
                    if (!devisePrincipaleId.HasValue)
                        throw new InvalidOperationException("Impossible de créer TypeAdhesion: aucune devise principale active.");

                    var typeAdhesions = new[]
                    {
                        new TypeAdhesion { IdTypeAdhesion = 1, Libelle = "Solo", CategorieAdhesionId = 1, MaxDependants = 0,   Description = "Adhésion individuelle sans dépendants", Montant = 1.5m, DeviseId = devisePrincipaleId.Value, Statut = true, DateCreation = DateTime.Now },
                        new TypeAdhesion { IdTypeAdhesion = 2, Libelle = "F3",   CategorieAdhesionId = 1, MaxDependants = 2,   Description = "Adhésion familiale (titulaire + 2 personnes à charge)", Montant = 1.5m, DeviseId = devisePrincipaleId.Value, Statut = true, DateCreation = DateTime.Now },
                        new TypeAdhesion { IdTypeAdhesion = 3, Libelle = "F6",   CategorieAdhesionId = 1, MaxDependants = 5,   Description = "Adhésion familiale (titulaire + 5 personnes à charge)", Montant = 1.5m, DeviseId = devisePrincipaleId.Value, Statut = true, DateCreation = DateTime.Now },
                        new TypeAdhesion { IdTypeAdhesion = 4, Libelle = "ET",   CategorieAdhesionId = 2, MaxDependants = 100, Description = "Adhésion Entreprise jusqu'à 100 Agents", Montant = 1.5m, DeviseId = devisePrincipaleId.Value, Statut = true, DateCreation = DateTime.Now }
                    };
                    await context.TypeAdhesions.AddRangeAsync(typeAdhesions);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Types d'adhésions créés: {Count}", typeAdhesions.Length);
                }

                // 4. Catégories d'Agents
                if (!await context.CategoriesAgents.AnyAsync())
                {
                    var categoriesAgents = new[]
                    {
                        new CategorieAgent { Code = "AT", LibelleCategorie = "Agent de Terrain (AT)",    Description = "Agent de Terrain",    Statut = true, DateCreation = DateTime.Now },
                        new CategorieAgent { Code = "AA", LibelleCategorie = "Agent Administratif (AA)", Description = "Agent Administratif", Statut = true, DateCreation = DateTime.Now },
                        new CategorieAgent { Code = "AP", LibelleCategorie = "Agent Percepteur (AP)",    Description = "Agent Percepteur",    Statut = true, DateCreation = DateTime.Now },
                        new CategorieAgent { Code = "AS", LibelleCategorie = "Agent Superviseur (AS)",   Description = "Agent Superviseur",   Statut = true, DateCreation = DateTime.Now },
                        new CategorieAgent { Code = "CA", LibelleCategorie = "Caissier (CA)",            Description = "Caissier",            Statut = true, DateCreation = DateTime.Now },
                        new CategorieAgent { Code = "AH", LibelleCategorie = "Agent Hôpital (AH)",       Description = "Agent Hôpital",       Statut = true, DateCreation = DateTime.Now },
                        new CategorieAgent { Code = "FI", LibelleCategorie = "Financier (FI)",           Description = "Financier",           Statut = true, DateCreation = DateTime.Now },
                        new CategorieAgent { Code = "IT", LibelleCategorie = "Technicien (IT)",          Description = "Technicien",          Statut = true, DateCreation = DateTime.Now },
                        new CategorieAgent { Code = "AD", LibelleCategorie = "Admin (AD)",               Description = "Admin",               Statut = true, DateCreation = DateTime.Now }
                    };
                    await context.CategoriesAgents.AddRangeAsync(categoriesAgents);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Catégories d'agents créées: {Count}", categoriesAgents.Length);
                }

                // 6. Communes (24 communes de Kinshasa)
                var kinshasaProvince = await context.Provinces.FirstOrDefaultAsync(p => p.Nom == "Kinshasa");
                if (kinshasaProvince != null && !await context.Communes.AnyAsync())
                {
                    var communes = new[]
                    {
                        new Commune { Nom = "Gombe", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Lemba", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Matete", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Kalamu", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Kasa-Vubu", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Kintambo", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Kimbanseke", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Kinshasa", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Lingwala", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Limete", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Mont-Ngafula", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Ngaba", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Ngaliema", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Nsele", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Pikine", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Selembao", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Bandalungwa", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Barumbu", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Bumbu", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Kinkole", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Kisenso", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Kokolo", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Makala", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now },
                        new Commune { Nom = "Masina", ProvinceId = kinshasaProvince.IdProvince, Statut = true, DateCreation = DateTime.Now }
                    };
                    await context.Communes.AddRangeAsync(communes);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Communes créées: {Count}", communes.Length);
                }

                // 7. Zones Sociales (après les communes)
                var gombeCommune = await context.Communes.FirstOrDefaultAsync(c => c.Nom == "Gombe");
                if (gombeCommune != null && !await context.ZonesSociales.AnyAsync())
                {
                    var zonesSociales = new[]
                    {
                        new ZoneSociale { Nom = "Gombe-Centre", CommuneId = gombeCommune.IdCommune, Statut = true, DateCreation = DateTime.Now },
                        new ZoneSociale { Nom = "Gombe-Nord",   CommuneId = gombeCommune.IdCommune, Statut = true, DateCreation = DateTime.Now },
                        new ZoneSociale { Nom = "Gombe-Sud",    CommuneId = gombeCommune.IdCommune, Statut = true, DateCreation = DateTime.Now }
                    };
                    await context.ZonesSociales.AddRangeAsync(zonesSociales);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Zones sociales créées: {Count}", zonesSociales.Length);
                }

                // 8. Permissions
                if (!await context.Permissions.AnyAsync())
                {
                    var permissions = new[]
                    {
                        // Permissions Utilisateur
                        new Permission { Nom = "READ_USER",   Description = "Voir les utilisateurs",    DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_USER", Description = "Modifier un utilisateur",  DateCreation = DateTime.Now },
                        
                        

                        // Permissions Agent
                        new Permission { Nom = "CREATE_AGENT", Description = "Créer un agent",     DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_AGENT",   Description = "Voir les agents",    DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_AGENT", Description = "Modifier un agent",  DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_AGENT", Description = "Supprimer un agent", DateCreation = DateTime.Now },
                        
                        // Permissions Adhesion
                        new Permission { Nom = "CREATE_ADHESION", Description = "Créer une adhésion", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_ADHESION", Description = "Voir les adhésions",      DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_ADHESION", Description = "Modifier une adhésion", DateCreation = DateTime.Now },
                        new Permission { Nom = "ENCODE_ADHESION_NIVEAU_2", Description = "Encoder / valider le dossier adhésion niveau 2 (encodeur)", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_ADHESION", Description = "Supprimer une adhésion", DateCreation = DateTime.Now },
                        
                        // Permissions Assureur
                        new Permission { Nom = "CREATE_ASSUREUR", Description = "Créer un assureur", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_ASSUREUR", Description = "Voir les assureurs", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_ASSUREUR", Description = "Modifier un assureur", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_ASSUREUR", Description = "Supprimer un assureur", DateCreation = DateTime.Now },
                        
                        // Permissions Dépendant
                        new Permission { Nom = "CREATE_DEPENDANT", Description = "Créer un dépendant", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_DEPENDANT", Description = "Voir les dépendants", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_DEPENDANT", Description = "Modifier un dépendant", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_DEPENDANT", Description = "Supprimer un dépendant", DateCreation = DateTime.Now },
                        
                        // Permissions Antécédent
                        new Permission { Nom = "CREATE_ANTECEDENT", Description = "Créer un antécédent", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_ANTECEDENT", Description = "Voir les antécédents", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_ANTECEDENT", Description = "Modifier un antécédent", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_ANTECEDENT", Description = "Supprimer un antécédent", DateCreation = DateTime.Now },
                        
                       
                        // Permissions WalletAgent
                        new Permission { Nom = "CREATE_WALLET_AGENT", Description = "Créer un wallet agent", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_WALLET_AGENT", Description = "Voir les wallets agents", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_WALLET_AGENT", Description = "Modifier un wallet agent", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_WALLET_AGENT", Description = "Supprimer un wallet agent", DateCreation = DateTime.Now },
                        
                        // Permissions WalletVirtuelAgent
                        new Permission { Nom = "CREATE_WALLET_VIRTUEL", Description = "Créer un wallet virtuel", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_WALLET_VIRTUEL", Description = "Voir les wallets virtuels", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_WALLET_VIRTUEL", Description = "Modifier un wallet virtuel", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_WALLET_VIRTUEL", Description = "Supprimer un wallet virtuel", DateCreation = DateTime.Now },
                        
                        // Permissions WalletMouvement
                        new Permission { Nom = "CREATE_WALLET_MOVEMENT", Description = "Créer un mouvement de wallet", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_WALLET_MOVEMENT", Description = "Voir les mouvements de wallet", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_WALLET_MOVEMENT", Description = "Modifier un mouvement de wallet", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_WALLET_MOVEMENT", Description = "Supprimer un mouvement de wallet", DateCreation = DateTime.Now },
                        
                        // Permissions Transaction
                        new Permission { Nom = "CREATE_TRANSACTION", Description = "Effectuer une transaction", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_TRANSACTION", Description = "Voir les transactions", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_TRANSACTION", Description = "Modifier une transaction", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_TRANSACTION", Description = "Supprimer une transaction", DateCreation = DateTime.Now },
                        
                        // Permissions ProduitAssureur
                        new Permission { Nom = "CREATE_PRODUIT_ASSUREUR", Description = "Créer un produit assureur", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_PRODUIT_ASSUREUR", Description = "Voir les produits assureurs", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_PRODUIT_ASSUREUR", Description = "Modifier un produit assureur", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_PRODUIT_ASSUREUR", Description = "Supprimer un produit assureur", DateCreation = DateTime.Now },
                        
                        // Permissions ProduitMutuel
                        new Permission { Nom = "CREATE_PRODUIT_MUTUEL", Description = "Créer un produit mutuel", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_PRODUIT_MUTUEL", Description = "Voir les produits mutuels", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_PRODUIT_MUTUEL", Description = "Modifier un produit mutuel", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_PRODUIT_MUTUEL", Description = "Supprimer un produit mutuel", DateCreation = DateTime.Now },
                        
                        // Permissions Prestation
                        new Permission { Nom = "CREATE_PRESTATION", Description = "Créer une prestation", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_PRESTATION", Description = "Voir les prestations", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_PRESTATION", Description = "Modifier une prestation", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_PRESTATION", Description = "Supprimer une prestation", DateCreation = DateTime.Now },
                        
                        // Permissions Collecte
                        new Permission { Nom = "CREATE_COLLECTE", Description = "Créer une collecte", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_COLLECTE", Description = "Voir les collectes", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_COLLECTE", Description = "Modifier une collecte", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_COLLECTE", Description = "Supprimer une collecte", DateCreation = DateTime.Now },
                        
                        // Permissions Affilie (création via adhésion ; pas de suppression directe)
                        new Permission { Nom = "READ_AFFILIE", Description = "Voir les affiliés", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_AFFILIE", Description = "Modifier un affilié", DateCreation = DateTime.Now },
                        
                        // Permissions Géographiques
                        new Permission { Nom = "CREATE_PROVINCE", Description = "Créer une province", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_PROVINCE", Description = "Voir les provinces", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_PROVINCE", Description = "Modifier une province", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_PROVINCE", Description = "Supprimer une province", DateCreation = DateTime.Now },
                        
                        new Permission { Nom = "CREATE_COMMUNE", Description = "Créer une commune", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_COMMUNE", Description = "Voir les communes", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_COMMUNE", Description = "Modifier une commune", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_COMMUNE", Description = "Supprimer une commune", DateCreation = DateTime.Now },
                        
                        new Permission { Nom = "CREATE_ZONE_SOCIALE", Description = "Créer une zone sociale", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_ZONE_SOCIALE", Description = "Voir les zones sociales", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_ZONE_SOCIALE", Description = "Modifier une zone sociale", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_ZONE_SOCIALE", Description = "Supprimer une zone sociale", DateCreation = DateTime.Now },
                        
                        new Permission { Nom = "CREATE_DEVISE", Description = "Créer une devise", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_DEVISE", Description = "Voir les devises", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_DEVISE", Description = "Modifier une devise", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_DEVISE", Description = "Supprimer une devise", DateCreation = DateTime.Now },
                        new Permission { Nom = "CREATE_TAUX_CHANGE", Description = "Créer un taux de change", DateCreation = DateTime.Now },
                        
                        new Permission { Nom = "CREATE_CATEGORIE_ADHESION", Description = "Créer une catégorie d'adhésion", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_CATEGORIE_ADHESION", Description = "Voir les catégories d'adhésion", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_CATEGORIE_ADHESION", Description = "Modifier une catégorie d'adhésion", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_CATEGORIE_ADHESION", Description = "Supprimer une catégorie d'adhésion", DateCreation = DateTime.Now },
                        
                        new Permission { Nom = "CREATE_TYPE_ADHESION", Description = "Créer un type d'adhésion", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_TYPE_ADHESION", Description = "Voir les types d'adhésion", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_TYPE_ADHESION", Description = "Modifier un type d'adhésion", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_TYPE_ADHESION", Description = "Supprimer un type d'adhésion", DateCreation = DateTime.Now },
                        
                        new Permission { Nom = "CREATE_CATEGORIE_AGENT", Description = "Créer une catégorie d'agent", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_CATEGORIE_AGENT", Description = "Voir les catégories d'agents", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_CATEGORIE_AGENT", Description = "Modifier une catégorie d'agent", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_CATEGORIE_AGENT", Description = "Supprimer une catégorie d'agent", DateCreation = DateTime.Now },
                        
                        // Permissions Supervision
                        new Permission { Nom = "READ_HIERARCHIE", Description = "Voir la hiérarchie des agents", DateCreation = DateTime.Now },
                        new Permission { Nom = "MANAGE_SUPERVISION", Description = "Gérer la supervision d'équipe", DateCreation = DateTime.Now },
                        new Permission { Nom = "ACCESS_DASHBOARD_SUPERVISEUR", Description = "Accéder au dashboard superviseur", DateCreation = DateTime.Now },
                        new Permission { Nom = "MANAGE_OBJECTIFS", Description = "Gérer les objectifs d'équipe", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_TARGET_AGENT", Description = "Voir les objectifs / TargetAgent", DateCreation = DateTime.Now },
                        new Permission { Nom = "VALIDATE_PERFORMANCE", Description = "Valider les performances des agents", DateCreation = DateTime.Now },
                        
                        // Permissions Dashboard et Rapports
                        new Permission { Nom = "ACCESS_DASHBOARD_AGENT", Description = "Accéder au dashboard agent", DateCreation = DateTime.Now },
                        new Permission { Nom = "ACCESS_DASHBOARD_AFFILIE", Description = "Accéder au dashboard affilié", DateCreation = DateTime.Now },
                        new Permission { Nom = "ACCESS_DASHBOARD_ADMIN", Description = "Accéder au dashboard administrateur", DateCreation = DateTime.Now },
                        new Permission { Nom = "GENERATE_RAPPORT", Description = "Générer des rapports", DateCreation = DateTime.Now },
                        new Permission { Nom = "EXPORT_DATA", Description = "Exporter des données", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_STATISTIQUES", Description = "Consulter les statistiques", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_PARAMETRES_METIER", Description = "Consulter les paramètres métier", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_PARAMETRES_METIER", Description = "Modifier les paramètres métier", DateCreation = DateTime.Now },
                        
                        // Permissions Notifications
                        new Permission { Nom = "CREATE_NOTIFICATION", Description = "Créer une notification", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_NOTIFICATION", Description = "Voir les notifications", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_NOTIFICATION", Description = "Modifier une notification", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_NOTIFICATION", Description = "Supprimer une notification", DateCreation = DateTime.Now },
                        
                        // Permissions Bon d'envoi
                        new Permission { Nom = "CREATE_BON_ENVOI", Description = "Créer un bon d'envoi", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_BON_ENVOI", Description = "Voir les bons d'envoi", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_BON_ENVOI", Description = "Modifier un bon d'envoi", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_BON_ENVOI", Description = "Supprimer un bon d'envoi", DateCreation = DateTime.Now },
                        
                        // Permissions Frais
                        new Permission { Nom = "CREATE_FRAIS", Description = "Créer un FRAIS", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_FRAIS",   Description = "Voir les FRAIS", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_FRAIS", Description = "Modifier un FRAIS", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_FRAIS", Description = "Supprimer un FRAIS", DateCreation = DateTime.Now },

                        // Permissions espace affilié (workflow membre)
                        new Permission { Nom = "READ_COTISATION_AFFILIE", Description = "Consulter les cotisations affilié", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_ARRIERES_AFFILIE", Description = "Consulter ses arriérés de paiement", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_PENALITE_AFFILIE", Description = "Consulter ses pénalités de retard", DateCreation = DateTime.Now },
                        new Permission { Nom = "PAIEMENT_AFFILIE", Description = "Payer cotisations et souscriptions", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_SOUSCRIPTION_PRESTATION", Description = "Consulter ses souscriptions prestation", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_SOUSCRIPTION_PRESTATION", Description = "Modifier une souscription prestation", DateCreation = DateTime.Now },
                        new Permission { Nom = "DELETE_SOUSCRIPTION_PRESTATION", Description = "Supprimer une souscription prestation", DateCreation = DateTime.Now },
                        new Permission { Nom = "CREATE_DEMANDE_BON_ENVOI", Description = "Demander un bon d'envoi", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_DEMANDE_BON_ENVOI", Description = "Consulter ses demandes de bon d'envoi", DateCreation = DateTime.Now },
                        new Permission { Nom = "CONFIRM_DEMANDE_BON_ENVOI", Description = "Confirmer ou rejeter une demande de bon d'envoi", DateCreation = DateTime.Now },
                        new Permission { Nom = "SCAN_BON_ENVOI", Description = "Scanner et vérifier un QR de bon d'envoi", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_JETON_MEDICAL", Description = "Consulter ses jetons médicaux", DateCreation = DateTime.Now },
                        new Permission { Nom = "USE_JETON_MEDICAL", Description = "Valider et utiliser un jeton médical", DateCreation = DateTime.Now },
                        new Permission { Nom = "CREATE_HOPITAL_PARTENAIRE", Description = "Créer un hôpital partenaire", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_HOPITAL_PARTENAIRE", Description = "Consulter les hôpitaux partenaires", DateCreation = DateTime.Now },
                        new Permission { Nom = "UPDATE_HOPITAL_PARTENAIRE", Description = "Modifier un hôpital partenaire", DateCreation = DateTime.Now },
                        new Permission { Nom = "ACCESS_DASHBOARD_HOPITAL", Description = "Accéder au dashboard hôpital", DateCreation = DateTime.Now },
                        new Permission { Nom = "ACCESS_DASHBOARD_CAISSIER", Description = "Accéder au dashboard caissier", DateCreation = DateTime.Now },
                        new Permission { Nom = "OPEN_CAISSIER_SESSION", Description = "Ouvrir une session de caisse", DateCreation = DateTime.Now },
                        new Permission { Nom = "CLOSE_CAISSIER_SESSION", Description = "Clôturer une session de caisse", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_CAISSIER_SESSION", Description = "Consulter session et mouvements de caisse", DateCreation = DateTime.Now },
                        new Permission { Nom = "CREATE_DEMANDE_RETRAIT_AGENT", Description = "Créer une demande de retrait agent", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_DEMANDE_RETRAIT_AGENT", Description = "Consulter les demandes de retrait agent", DateCreation = DateTime.Now },
                        new Permission { Nom = "VALIDATE_DEMANDE_RETRAIT_AGENT", Description = "Valider une demande de retrait agent et générer le jeton", DateCreation = DateTime.Now },
                        new Permission { Nom = "CONFIRM_RETRAIT_AGENT", Description = "Payer un retrait agent au guichet", DateCreation = DateTime.Now },
                        new Permission { Nom = "MARQUER_PAYER_RETRAIT_AGENT", Description = "Marquer un retrait agent comme payé (jeton)", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_RETRAIT_AGENT", Description = "Accéder au module / menu retraits agent", DateCreation = DateTime.Now },
                        new Permission { Nom = "CREATE_DEMANDE_RECHARGE_WALLET_VIRTUEL", Description = "Créer une demande de recharge wallet virtuel", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_DEMANDE_RECHARGE_WALLET_VIRTUEL", Description = "Consulter les demandes de recharge wallet virtuel", DateCreation = DateTime.Now },
                        new Permission { Nom = "CONFIRM_DEMANDE_RECHARGE_WALLET_VIRTUEL", Description = "Confirmer ou rejeter une demande de recharge wallet virtuel", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_PERCEPTION_VIRTUAL", Description = "Consulter les collectes compte virtuel à percevoir", DateCreation = DateTime.Now },
                        new Permission { Nom = "CONFIRM_PERCEPTION_VIRTUAL", Description = "Confirmer la perception physique des collectes compte virtuel", DateCreation = DateTime.Now },
                        new Permission { Nom = "ACCESS_DASHBOARD_SUPERADMIN", Description = "Accéder au dashboard super administrateur", DateCreation = DateTime.Now },
                        new Permission { Nom = "ACCESS_DASHBOARD_ASSUREUR", Description = "Accéder au dashboard assureur", DateCreation = DateTime.Now },
                        new Permission { Nom = "ACCESS_DASHBOARD_AGENT_AA", Description = "Accéder au dashboard agent administratif (encodeur)", DateCreation = DateTime.Now },
                        new Permission { Nom = "ACCESS_DASHBOARD_CHEF_EQUIPE", Description = "Accéder au dashboard chef d'équipe", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_EQUIPE_ZONE", Description = "Consulter les agents AT de sa zone", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_EQUIPE_WALLET_MOVEMENT", Description = "Consulter les mouvements wallet des AT de sa zone", DateCreation = DateTime.Now },
                        new Permission { Nom = "READ_EQUIPE_COLLECTE", Description = "Consulter les collectes des AT de sa zone", DateCreation = DateTime.Now },
                        
                        // Permissions Système
                        new Permission { Nom = "MANAGE_SYSTEM", Description = "Gérer les paramètres système", DateCreation = DateTime.Now },
                        new Permission { Nom = "VIEW_LOGS", Description = "Voir les logs système", DateCreation = DateTime.Now },
                        new Permission { Nom = "MANAGE_BACKUP", Description = "Gérer les sauvegardes", DateCreation = DateTime.Now }
                    };
                    await context.Permissions.AddRangeAsync(permissions);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Permissions créées: {Count}", permissions.Length);
                }

                // 9. Rôles
                if (!await context.Roles.AnyAsync())
                {
                    var roles = new[]
                    {
                        new Role { Nom = "SuperAdmin",                 Code="SA",   Description = "Super administrateur",         Niveau = 0, Statut = true, DateCreation = DateTime.Now },
                        new Role { Nom = "Admin",                      Code="AD",   Description = "Administrateur système",       Niveau = 1, Statut = true, DateCreation = DateTime.Now },
                        new Role { Nom = "IT",                         Code="IT",   Description = "Technicien",                   Niveau = 2, Statut = true, DateCreation = DateTime.Now },
                        new Role { Nom = "Financier",                  Code="FI",   Description = "Financier",                    Niveau = 3, Statut = true, DateCreation = DateTime.Now },
                        new Role { Nom = "Caissier",                   Code="CA",   Description = "Caissier principal (guichet)", Niveau = 4, Statut = true, DateCreation = DateTime.Now },
                        new Role { Nom = "Superviseur",                Code="SP",   Description = "Superviseur d'équipe",         Niveau = 5, Statut = true, DateCreation = DateTime.Now },
                        new Role { Nom = "Chef d'équipe",              Code="CE",   Description = "Chef d'équipe de zone",        Niveau = 6, Statut = true, DateCreation = DateTime.Now },
                        new Role { Nom = "Percepteur",                 Code="PR",   Description =  "Superviseur d'équipe",        Niveau = 6, Statut = true, DateCreation = DateTime.Now },
                        new Role { Nom = "Agent (AT)",                 Code="AT",   Description = "Agent de Terrain",             Niveau = 7, Statut = true, DateCreation = DateTime.Now },
                        new Role { Nom = "Agent (AA)",                 Code="AA",   Description = "Agent Administratif",          Niveau = 8, Statut = true, DateCreation = DateTime.Now },
                        new Role { Nom = "Affilié",                    Code="AF",   Description = "Membre affilié",               Niveau = 9, Statut = true, DateCreation = DateTime.Now },
                        new Role { Nom = "Assureur",                   Code="AS",   Description = "Partenaire assureur",          Niveau = 10, Statut = true, DateCreation = DateTime.Now },
                        new Role { Nom = "Agent Hôpital",              Code="AH",   Description = "Personnel accueil hôpital",    Niveau = 11, Statut = true, DateCreation = DateTime.Now }

                    };
                    await context.Roles.AddRangeAsync(roles);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Rôles créés: {Count}", roles.Length);
                }
                
                // 10. Frais
                if (!await context.Frais.AnyAsync())
                {
                    var frais = new[]
                    {
                        new Frais { Code = FraisCodes.FraisAdhesion, Libelle = "Frais Adhesion", Montant = 1.5, DeviseId = 2, TauxCommission = 25m, Statut = true, DateCreation = DateTime.Now },
                        new Frais { Code = FraisCodes.CarteMembre, Libelle = "Achat Carte de Membre", Montant = 5, DeviseId = 2, TauxCommission = 25m, Statut = true, DateCreation = DateTime.Now },
                        new Frais { Code = FraisCodes.PenaliteRetardCotisation, Libelle = "Pénalité", Montant = 5, DeviseId = 2, TauxCommission = 0m, Periodicite = "Ponctuel", Statut = true, DateCreation = DateTime.Now }
                    };
                    await context.Frais.AddRangeAsync(frais);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Frais créés: {Count}", frais.Length);
                }

                // 10. Création des Agents (AVANT les utilisateurs)
                if (!await context.Agents.AnyAsync())
                {
                    var categorieSystem = await context.CategoriesAgents.FirstOrDefaultAsync(c => c.Code == "AD");
                    var gombeCentre = await context.ZonesSociales.FirstOrDefaultAsync(z => z.Nom == "Gombe-Centre");
                    var gombeNord = await context.ZonesSociales.FirstOrDefaultAsync(z => z.Nom == "Gombe-Nord");

                    // Créer d'abord les agents pour Admin et SuperAdmin
                    var adminSystemAgent = new Agent
                    {
                        NomComplet = "Admin Système",
                        Phone = "+243999999999",
                        EmailAgent = "admin@prosoc.cd",
                        Statut = true,
                        DateCreation = DateTime.Now,
                        CategorieAgentId = categorieSystem?.IdCategorieAgent ?? 1,
                        ZoneSocialeId = gombeCentre?.IdZoneSociale ?? 1,
                        Matricule = "ADMIN001"
                    };

                    var superAdminSystemAgent = new Agent
                    {
                        NomComplet = "Super Admin Système",
                        Phone = "+243888888888",
                        EmailAgent = "superadmin@prosoc.cd",
                        Statut = true,
                        DateCreation = DateTime.Now,
                        CategorieAgentId = categorieSystem?.IdCategorieAgent ?? 1,
                        ZoneSocialeId = gombeNord?.IdZoneSociale ?? 2,
                        Matricule = "SUPER001"
                    };
   

                    var agents = new[]
                    {
                        adminSystemAgent,
                        superAdminSystemAgent
                    };

                    await context.Agents.AddRangeAsync(agents);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Agents créés: {Count}", agents.Length);
                }

                // 11. Utilisateurs (après les agents)
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Admin");
                var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "SuperAdmin");
                var agentAtRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Agent (AT)");
                
                if (adminRole != null && superAdminRole != null && agentAtRole != null && !await context.Utilisateurs.AnyAsync())
                {
                    var adminSystemAgent = await context.Agents.FirstOrDefaultAsync(a => a.Matricule == "ADMIN001" || a.EmailAgent == "admin@prosoc.cd");
                    var superAdminSystemAgent = await context.Agents.FirstOrDefaultAsync(a => a.Matricule == "SUPER001" || a.EmailAgent == "superadmin@prosoc.cd");
                  //  var atSystemAgent1 = await context.Agents.FirstOrDefaultAsync(a => a.Matricule == "AT-001" || a.EmailAgent == "odedkangudja66@gmail.com");
                   // var atSystemAgent2 = await context.Agents.FirstOrDefaultAsync(a => a.Matricule == "AT-002" || a.EmailAgent == "garrykabeya294@gmail.com");
                    
                    
                    if (adminSystemAgent == null || superAdminSystemAgent == null )
                    {
                        logger.LogWarning("Agents système non trouvés; la création des utilisateurs système sera gérée par l'upsert.");
                        adminSystemAgent = adminSystemAgent ?? new Agent { IdAgent = 0 };
                        superAdminSystemAgent = superAdminSystemAgent ?? new Agent { IdAgent = 0 };
                       // atSystemAgent1 = atSystemAgent1 ?? new Agent { IdAgent = 0 };
                       // atSystemAgent2 = atSystemAgent2 ?? new Agent { IdAgent = 0 };
                    }

                    var users = new List<Utilisateur>
                    {
                        // Utilisateurs Admin/SuperAdmin liés à leurs agents
                        new Utilisateur
                        {
                            NomUtilisateur = "admin",
                            EmailUtilisateur = "admin@prosoc.cd",
                            DefaultUsername = "admin",
                            PhoneUtilisateur = "+243999999999",
                            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Admin"),
                            Statut = true,
                            RoleId = adminRole.IdRole,
                            AgentId = adminSystemAgent.IdAgent == 0 ? null : adminSystemAgent.IdAgent,
                            DateCreation = DateTime.Now,
                            DoitChangerMotDePasse = false
                        },
                        new Utilisateur
                        {
                            NomUtilisateur = "superadmin",
                            EmailUtilisateur = "superadmin@prosoc.cd",
                            PhoneUtilisateur = "+243888888888",
                            DefaultUsername = "superadmin",
                            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Super-Admin"),
                            Statut = true,
                            RoleId = superAdminRole.IdRole,
                            AgentId = superAdminSystemAgent.IdAgent == 0 ? null : superAdminSystemAgent.IdAgent,
                            DateCreation = DateTime.Now,
                            DoitChangerMotDePasse = false
                        }
                    };

                    // Ajouter les utilisateurs pour chaque agent (sauf Admin/SuperAdmin déjà créés)
                    
                    await context.Utilisateurs.AddRangeAsync(users);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Utilisateurs créés: {Count}", users.Count);
                }

                // 11.b Upsert des comptes système (Admin / SuperAdmin)
                // Permet de garantir la présence de ces comptes même si la base contient déjà d'autres utilisateurs.
                if (adminRole != null)
                {
                    var adminAgent = await context.Agents.FirstOrDefaultAsync(a => a.Matricule == "ADMIN001" || a.EmailAgent == "admin@prosoc.cd");
                    if (adminAgent == null)
                    {
                        var categorieForSystem = await context.CategoriesAgents.FirstOrDefaultAsync();
                        var zoneForAdmin = await context.ZonesSociales.FirstOrDefaultAsync();

                        adminAgent = new Agent
                        {
                            NomComplet = "Admin Système",
                            Phone = "+243999999997",
                            EmailAgent = "admin@prosoc.cd",
                            Statut = true,
                            DateCreation = DateTime.Now,
                            CategorieAgentId = categorieForSystem?.IdCategorieAgent,
                            ZoneSocialeId = zoneForAdmin.IdZoneSociale,
                            Matricule = "ADMIN001"
                        };

                        await context.Agents.AddAsync(adminAgent);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Agent système ADMIN001 créé (upsert)");
                    }

                    var adminCandidates = await context.Utilisateurs
                        .Where(u => u.EmailUtilisateur == "admin@prosoc.cd" || u.NomUtilisateur == "admin@prosoc.cd")
                        .ToListAsync();

                    var adminSystemUser = adminCandidates
                        .FirstOrDefault(u => u.EmailUtilisateur == "admin@prosoc.cd")
                        ?? adminCandidates.FirstOrDefault();

                    if (adminSystemUser == null)
                    {
                        adminSystemUser = new Utilisateur
                        {
                            NomUtilisateur = "admin",
                            EmailUtilisateur = "admin@prosoc.cd",
                            PhoneUtilisateur = "+243999999999",
                            DefaultUsername = "admin",
                            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Admin"),
                            Statut = true,
                            RoleId = adminRole.IdRole,
                            AgentId = adminAgent.IdAgent,
                            DateCreation = DateTime.Now,
                            DoitChangerMotDePasse = false
                        };

                        await context.Utilisateurs.AddAsync(adminSystemUser);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Compte admin@prosoc.cd créé (upsert)");
                    }
                    else
                    {
                        adminSystemUser.NomUtilisateur = "admin";
                        adminSystemUser.EmailUtilisateur = "admin@prosoc.cd";
                        adminSystemUser.PhoneUtilisateur = "+243999999997";
                        adminSystemUser.RoleId = adminRole.IdRole;

                        foreach (var other in adminCandidates.Where(u => u.IdUtilisateur != adminSystemUser.IdUtilisateur))
                        {
                            if (other.EmailUtilisateur == "admin@prosoc.cd")
                                other.EmailUtilisateur = null;
                        }

                        await context.SaveChangesAsync();
                        logger.LogInformation("Compte admin@prosoc.cd mis à jour (upsert)");
                    }
                }

                if (superAdminRole != null)
                {
                    var superAdminAgent = await context.Agents.FirstOrDefaultAsync(a => a.Matricule == "SUPER001" || a.EmailAgent == "superadmin@prosoc.cd");
                    if (superAdminAgent == null)
                    {
                        var categorieForSystem = await context.CategoriesAgents.FirstOrDefaultAsync();
                        var zoneForSuperAdmin = await context.ZonesSociales.FirstOrDefaultAsync();

                        superAdminAgent = new Agent
                        {
                            NomComplet = "Super Admin Système",
                            Phone = "+243888888888",
                            EmailAgent = "superadmin@prosoc.cd",
                            Statut = true,
                            DateCreation = DateTime.Now,
                            CategorieAgentId = categorieForSystem?.IdCategorieAgent,
                            ZoneSocialeId = zoneForSuperAdmin.IdZoneSociale,
                            Matricule = "SUPER001"
                        };

                        await context.Agents.AddAsync(superAdminAgent);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Agent système SUPER001 créé (upsert)");
                    }

                    var superAdminCandidates = await context.Utilisateurs
                        .Where(u => u.EmailUtilisateur == "superadmin@prosoc.cd" || u.NomUtilisateur == "superadmin@prosoc.cd")
                        .ToListAsync();

                    var superAdminSystemUser = superAdminCandidates
                        .FirstOrDefault(u => u.EmailUtilisateur == "superadmin@prosoc.cd")
                        ?? superAdminCandidates.FirstOrDefault();

                    if (superAdminSystemUser == null)
                    {
                        superAdminSystemUser = new Utilisateur
                        {
                            NomUtilisateur = "superadmin",
                            EmailUtilisateur = "superadmin@prosoc.cd",
                            PhoneUtilisateur = "+243888888888",
                            DefaultUsername = "superadmin",
                            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Super-Admin"),
                            Statut = true,
                            RoleId = superAdminRole.IdRole,
                            AgentId = superAdminAgent.IdAgent,
                            DateCreation = DateTime.Now,
                            DoitChangerMotDePasse = false
                        };

                        await context.Utilisateurs.AddAsync(superAdminSystemUser);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Compte superadmin@prosoc.cd créé (upsert)");
                    }
                    else
                    {
                        superAdminSystemUser.NomUtilisateur = "superadmin";
                        superAdminSystemUser.EmailUtilisateur = "superadmin@prosoc.cd";
                        superAdminSystemUser.PhoneUtilisateur = "+243888888888";
                        superAdminSystemUser.RoleId = superAdminRole.IdRole;

                        foreach (var other in superAdminCandidates.Where(u => u.IdUtilisateur != superAdminSystemUser.IdUtilisateur))
                        {
                            if (other.EmailUtilisateur == "superadmin@prosoc.cd")
                                other.EmailUtilisateur = null;
                        }

                        await context.SaveChangesAsync();
                        logger.LogInformation("Compte superadmin@prosoc.cd mis à jour (upsert)");
                    }
                }

                // 12. Attribution des permissions aux rôles
                var allPermissions = await context.Permissions.ToListAsync();
                var allRoles = await context.Roles.ToListAsync();

                if (!await context.RolePermissions.AnyAsync())
                {
                    var rolePermissions = new List<RolePermission>();

                    foreach (var role in allRoles)
                    {
                        // SuperAdmin — catalogue complet (toutes les permissions actives)
                        if (role.Nom == "SuperAdmin")
                        {
                            foreach (var permission in FilterPermissionsForSuperAdminRole(allPermissions))
                            {
                                rolePermissions.Add(new RolePermission
                                {
                                    RoleId = role.IdRole,
                                    PermissionId = permission.IdPermission,
                                    DateAttribution = DateTime.Now
                                });
                            }
                        }
                        // Admin — tout sauf suppressions et MANAGE_SYSTEM (réservé SuperAdmin)
                        else if (role.Nom == "Admin")
                        {
                            foreach (var permission in FilterPermissionsForAdminRole(allPermissions))
                            {
                                rolePermissions.Add(new RolePermission
                                {
                                    RoleId = role.IdRole,
                                    PermissionId = permission.IdPermission,
                                    DateAttribution = DateTime.Now
                                });
                            }
                        }
                        // IT — paramétrage technique, support et administration opérationnelle (sans DELETE métier)
                        else if (role.Nom == "IT")
                        {
                            var itPermissions = FilterPermissionsForItRole(allPermissions).ToList();

                            foreach (var permission in itPermissions)
                            {
                                rolePermissions.Add(new RolePermission
                                {
                                    RoleId = role.IdRole,
                                    PermissionId = permission.IdPermission,
                                    DateAttribution = DateTime.Now
                                });
                            }
                        }
                        // Financier : consultation et opérations financières (sans suppression ni admin système)
                        else if (role.Nom == "Financier")
                        {
                            var financierPermissions = FilterPermissionsForFinancierRole(allPermissions).ToList();

                            foreach (var permission in financierPermissions)
                            {
                                rolePermissions.Add(new RolePermission
                                {
                                    RoleId = role.IdRole,
                                    PermissionId = permission.IdPermission,
                                    DateAttribution = DateTime.Now
                                });
                            }
                        }
                        // Percepteur : guichet terrain (encaissement, adhésion)
                        else if (role.Nom == "Percepteur")
                        {
                            var percepteurPermissions = FilterPermissionsForPercepteurRole(allPermissions).ToList();

                            foreach (var permission in percepteurPermissions)
                            {
                                rolePermissions.Add(new RolePermission
                                {
                                    RoleId = role.IdRole,
                                    PermissionId = permission.IdPermission,
                                    DateAttribution = DateTime.Now
                                });
                            }
                        }
                        // Caissier principal — guichet + supervision, wallets et clôture
                        else if (role.Nom == "Caissier")
                        {
                            var caissierPermissions = FilterPermissionsForCaissierRole(allPermissions).ToList();

                            foreach (var permission in caissierPermissions)
                            {
                                rolePermissions.Add(new RolePermission
                                {
                                    RoleId = role.IdRole,
                                    PermissionId = permission.IdPermission,
                                    DateAttribution = DateTime.Now
                                });
                            }
                        }
                        // Superviseur — périmètre AT + supervision équipe (targets, performances, wallet virtuel)
                        else if (role.Nom == "Superviseur")
                        {
                            var supervisorPermissions = FilterPermissionsForSuperviseurRole(allPermissions).ToList();
                            
                            foreach (var permission in supervisorPermissions)
                            {
                                rolePermissions.Add(new RolePermission
                                {
                                    RoleId = role.IdRole,
                                    PermissionId = permission.IdPermission,
                                    DateAttribution = DateTime.Now
                                });
                            }
                        }
                        // Chef d'équipe — périmètre AT + lecture équipe de zone (sans pouvoirs superviseur)
                        else if (role.Nom == "Chef d'équipe")
                        {
                            var chefEquipePermissions = FilterPermissionsForChefEquipeRole(allPermissions).ToList();

                            foreach (var permission in chefEquipePermissions)
                            {
                                rolePermissions.Add(new RolePermission
                                {
                                    RoleId = role.IdRole,
                                    PermissionId = permission.IdPermission,
                                    DateAttribution = DateTime.Now
                                });
                            }
                        }
                        // Agent de terrain — liste blanche stricte (adhésion niv. 1, collecte, wallet, dashboard)
                        else if (role.Nom == "Agent (AT)")
                        {
                            var agentAtPermissions = FilterPermissionsForAgentAtRole(allPermissions).ToList();

                            foreach (var permission in agentAtPermissions)
                            {
                                rolePermissions.Add(new RolePermission
                                {
                                    RoleId = role.IdRole,
                                    PermissionId = permission.IdPermission,
                                    DateAttribution = DateTime.Now
                                });
                            }
                        }
                        // Agent administratif — périmètre élargi (encodeur niv. 2, dépendants, etc.)
                        else if (role.Nom == "Agent (AA)")
                        {
                            var agentAaPermissions = FilterPermissionsForAgentAaRole(allPermissions).ToList();

                            foreach (var permission in agentAaPermissions)
                            {
                                rolePermissions.Add(new RolePermission
                                {
                                    RoleId = role.IdRole,
                                    PermissionId = permission.IdPermission,
                                    DateAttribution = DateTime.Now
                                });
                            }
                        }
                        // Partenaire assureur — consultation dossiers, produits et prises en charge
                        else if (role.Nom == "Assureur")
                        {
                            var assureurPermissions = FilterPermissionsForAssureurRole(allPermissions).ToList();
                            
                            foreach (var permission in assureurPermissions)
                            {
                                rolePermissions.Add(new RolePermission
                                {
                                    RoleId = role.IdRole,
                                    PermissionId = permission.IdPermission,
                                    DateAttribution = DateTime.Now
                                });
                            }
                        }
                        // Membre affilié : espace personnel (profil, famille, paiements, soins)
                        else if (role.Nom == "Affilié")
                        {
                            var affiliePermissions = FilterPermissionsForAffilieRole(allPermissions).ToList();

                            foreach (var permission in affiliePermissions)
                            {
                                rolePermissions.Add(new RolePermission
                                {
                                    RoleId = role.IdRole,
                                    PermissionId = permission.IdPermission,
                                    DateAttribution = DateTime.Now
                                });
                            }
                        }
                        // Personnel hôpital partenaire — accueil, scan bon, jetons
                        else if (role.Nom == "Agent Hôpital")
                        {
                            var agentHopitalPermissions = FilterPermissionsForAgentHopitalRole(allPermissions).ToList();

                            foreach (var permission in agentHopitalPermissions)
                            {
                                rolePermissions.Add(new RolePermission
                                {
                                    RoleId = role.IdRole,
                                    PermissionId = permission.IdPermission,
                                    DateAttribution = DateTime.Now
                                });
                            }
                        }
                    }

                    await context.RolePermissions.AddRangeAsync(rolePermissions);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Attributions rôle-permission créées: {Count}", rolePermissions.Count);
                }

                // 13. Attribution des rôles aux utilisateurs (CRUCIAL pour les permissions)
                var adminUser = await context.Utilisateurs.FirstOrDefaultAsync(u => u.EmailUtilisateur == "admin@prosoc.cd");
                var superAdminUser = await context.Utilisateurs.FirstOrDefaultAsync(u => u.EmailUtilisateur == "superadmin@prosoc.cd");

                if (!await context.UserRoles.AnyAsync() && adminUser != null && superAdminUser != null && adminRole != null && superAdminRole != null && agentAtRole != null)
                {
                    var userRoles = new List<UserRole>
                    {
                        // Admin et SuperAdmin
                        new UserRole { UtilisateurId = adminUser.IdUtilisateur, RoleId = adminRole.IdRole, IsPrimary = true, Statut = true, DateAttribution = DateTime.Now },
                        new UserRole { UtilisateurId = superAdminUser.IdUtilisateur, RoleId = superAdminRole.IdRole, IsPrimary = true, Statut = true, DateAttribution = DateTime.Now }
                    };

                    // Attribution automatique des rôles pour les utilisateurs agents
                    var agentUsers = await context.Utilisateurs
                        .Where(u => u.AgentId.HasValue && u.RoleId == agentAtRole.IdRole)
                        .ToListAsync();

                    foreach (var agentUser in agentUsers)
                    {
                        userRoles.Add(new UserRole 
                        { 
                            UtilisateurId = agentUser.IdUtilisateur, 
                            RoleId = agentAtRole.IdRole, 
                            IsPrimary = true, 
                            Statut = true, 
                            DateAttribution = DateTime.Now 
                        });
                    }

                    await context.UserRoles.AddRangeAsync(userRoles);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Attributions utilisateur-rôle créées: {Count}", userRoles.Count);
                }

                if (seedDemo)
                {
                    await SeedDemoAsync(context, logger);
                }

                // Retrait CREATE_AFFILIE / DELETE_AFFILIE (affilié géré via adhésion)
                await RetireObsoleteAffilieCrudPermissionsAsync(context, logger);

                // 🔧 MIGRATION DES PERMISSIONS DEPENDANT/ASSUREUR (CRUCIAL)
                await MigrateDependantAssureurPermissionsAsync(context, logger);
                await MigrateAffilieRolePermissionsAsync(context, logger);
                await EnsureDashboardAssureurPermissionAsync(context, logger);
                await MigrateAssureurRolePermissionsAsync(context, logger);
                await MigrateAgentHopitalRolePermissionsAsync(context, logger);
                await EnsureReadStatistiquesPermissionAsync(context, logger);
                await EnsureReadTargetAgentPermissionAsync(context, logger);
                await EnsureCreateTauxChangePermissionAsync(context, logger);
                await EnsureSouscriptionPrestationWritePermissionsAsync(context, logger);
                await EnsureParametresMetierPermissionsAsync(context, logger);
                await MigrateFinancierRolePermissionsAsync(context, logger);
                await MigratePercepteurRolePermissionsAsync(context, logger);
                await EnsureDashboardCaissierPermissionAsync(context, logger);
                await EnsureCaisseSessionPermissionsAsync(context, logger);
                await EnsureDemandeRetraitAgentPermissionsAsync(context, logger);
                await EnsureDemandeRechargeWalletVirtuelPermissionsAsync(context, logger);
                await EnsureEncodeAdhesionNiveau2PermissionAsync(context, logger);
                await EnsurePerceptionVirtuellePermissionsAsync(context, logger);
                await MigrateCaissierRolePermissionsAsync(context, logger);
                await EnsureChefEquipePermissionsAsync(context, logger);
                await MigrateChefEquipeRolePermissionsAsync(context, logger);
                await MigrateTerritorialEncadrementAsync(context, logger);
                await MigrateCategorieAgentLibellesAsync(context, logger);

                await EnsureMultideviseConfigAsync(context, logger);
                await MigrateWalletAgentDeviseAsync(context, logger);
                await MigrateRetraitDevisePrincipaleAsync(context, logger);
                await MigrateBonEnvoiWorkflowPermissionsAsync(context, logger);
                await MigrateAgentAtRolePermissionsAsync(context, logger);
                await EnsureDashboardAgentAaPermissionAsync(context, logger);
                await MigrateAgentAaRolePermissionsAsync(context, logger);
                await MigrateSuperviseurRolePermissionsAsync(context, logger);
                await MigrateItRolePermissionsAsync(context, logger);
                await MigrateAdminRolePermissionsAsync(context, logger);
                await EnsureDashboardSuperAdminPermissionAsync(context, logger);
                await MigrateSuperAdminRolePermissionsAsync(context, logger);
                await MigrateRemoveUpdateCollectePermissionAsync(context, logger);
                await EnsureFraisCatalogueCodesAsync(context, logger);

                await SeedTargetAgentsAsync(context, logger);
                await EnsureSystemAdminAccessAsync(context, logger);

                logger.LogInformation("Peuplement de la base de données terminé avec succès !");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Une erreur est survenue lors du peuplement de la base de données");
                throw;
            }
        }

        /// <summary>
        /// Renseigne DeviseId sur les wallets agents existants (devise principale) et supprime l'index unique AgentId seul.
        /// </summary>
        private static async Task MigrateWalletAgentDeviseAsync(ProsocDbContext context, ILogger logger)
        {
            var principale = await context.Devises
                .FirstOrDefaultAsync(d => d.EstDevisePrincipale && d.Statut);

            if (principale == null)
            {
                logger.LogWarning("Migration WalletAgent DeviseId : devise principale introuvable.");
                return;
            }

            var sansDevise = await context.WalletsAgents
                .Where(w => w.DeviseId == 0)
                .ToListAsync();

            foreach (var wallet in sansDevise)
            {
                wallet.DeviseId = principale.IdDevise;
            }

            if (sansDevise.Count > 0)
            {
                await context.SaveChangesAsync();
                logger.LogInformation(
                    "Migration WalletAgent : DeviseId={DeviseId} ({Code}) appliqué à {Count} wallet(s).",
                    principale.IdDevise, principale.Code, sansDevise.Count);
            }

            var mouvements = await context.WalletMouvements
                .Where(m => m.DeviseId == 0)
                .Include(m => m.Wallet)
                .ToListAsync();

            foreach (var m in mouvements)
            {
                m.DeviseId = m.Wallet?.DeviseId > 0 ? m.Wallet.DeviseId : principale.IdDevise;
            }

            if (mouvements.Count > 0)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Migration WalletMouvement : DeviseId renseigné sur {Count} mouvement(s).", mouvements.Count);
            }
        }

        /// <summary>
        /// Aligne les soldes retrait (devise principale) : sync SoldeDisponible et transfert
        /// des soldes legacy sur wallets non principaux (idempotent, Source MIG_RETRAIT_DEVISE).
        /// Voir sql/MigrateRetraitDevisePrincipale.idempotent.sql pour exécution manuelle MySQL.
        /// </summary>
        public static async Task MigrateRetraitDevisePrincipaleAsync(ProsocDbContext context, ILogger logger)
        {
            const string migrationSource = "MIG_RETRAIT_DEVISE";

            try
            {
                var conversion = new DeviseConversionService(context);
                Devise principale;
                try
                {
                    principale = await conversion.GetDevisePrincipaleAsync();
                }
                catch (InvalidOperationException)
                {
                    logger.LogWarning("Migration retrait devise principale : devise principale introuvable.");
                    return;
                }

                var principalSynced = 0;
                var principalWallets = await context.WalletsAgents
                    .Where(w => w.Statut && w.DeviseId == principale.IdDevise && w.SoldeDisponible < w.SoldeCourant)
                    .ToListAsync();

                foreach (var wallet in principalWallets)
                {
                    wallet.SoldeDisponible = wallet.SoldeCourant;
                    wallet.DateModification = DateTime.Now;
                    principalSynced++;
                }

                if (principalSynced > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration retrait devise principale : SoldeDisponible synchronisé sur {Count} wallet(s) {Code}.",
                        principalSynced, principale.Code);
                }

                var migratedWallets = 0;
                var skippedNoTaux = 0;
                var sourceWallets = await context.WalletsAgents
                    .Include(w => w.Devise)
                    .Where(w => w.Statut && w.DeviseId != principale.IdDevise && w.SoldeCourant > 0)
                    .ToListAsync();

                foreach (var source in sourceWallets)
                {
                    var alreadyMigrated = await context.WalletMouvements.AnyAsync(m =>
                        m.WalletId == source.IdWalletAgent
                        && m.Source == migrationSource
                        && m.Statut);

                    if (alreadyMigrated)
                        continue;

                    decimal montantPrincipal;
                    try
                    {
                        (montantPrincipal, _) = await conversion.ConvertirAsync(
                            source.SoldeCourant,
                            source.DeviseId,
                            principale.IdDevise,
                            DateTime.Now);
                    }
                    catch (InvalidOperationException ex)
                    {
                        skippedNoTaux++;
                        logger.LogWarning(
                            ex,
                            "Migration retrait devise principale : taux absent pour wallet {WalletId} (agent {AgentId}, devise {DeviseCode}).",
                            source.IdWalletAgent, source.AgentId, source.Devise?.Code);
                        continue;
                    }

                    if (montantPrincipal <= 0)
                        continue;

                    var principalWallet = await context.WalletsAgents
                        .FirstOrDefaultAsync(w =>
                            w.AgentId == source.AgentId
                            && w.DeviseId == principale.IdDevise
                            && w.Statut);

                    if (principalWallet == null)
                    {
                        principalWallet = new WalletAgent
                        {
                            AgentId = source.AgentId,
                            DeviseId = principale.IdDevise,
                            SoldeCourant = 0,
                            SoldeDisponible = 0,
                            Statut = true,
                            DateCreation = DateTime.Now
                        };
                        context.WalletsAgents.Add(principalWallet);
                        await context.SaveChangesAsync();
                    }

                    var montantSource = source.SoldeCourant;
                    var sourceCode = source.Devise?.Code ?? source.DeviseId.ToString();

                    context.WalletMouvements.Add(new WalletMouvement
                    {
                        WalletId = source.IdWalletAgent,
                        DeviseId = source.DeviseId,
                        Montant = montantSource,
                        TypeOperation = "DEBIT",
                        Source = migrationSource,
                        Description = $"Migration retrait devise principale — transfert {sourceCode} vers {principale.Code}",
                        DateOperation = DateTime.Now,
                        Statut = true
                    });

                    source.SoldeCourant = 0;
                    source.SoldeDisponible = 0;
                    source.DateModification = DateTime.Now;

                    context.WalletMouvements.Add(new WalletMouvement
                    {
                        WalletId = principalWallet.IdWalletAgent,
                        DeviseId = principale.IdDevise,
                        Montant = montantPrincipal,
                        TypeOperation = "CREDIT",
                        Source = migrationSource,
                        Description = $"Migration retrait devise principale — reçu depuis wallet #{source.IdWalletAgent} ({sourceCode})",
                        DateOperation = DateTime.Now,
                        Statut = true
                    });

                    principalWallet.SoldeCourant += montantPrincipal;
                    principalWallet.SoldeDisponible += montantPrincipal;
                    principalWallet.DateModification = DateTime.Now;

                    migratedWallets++;
                }

                if (migratedWallets > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration retrait devise principale : {Count} wallet(s) non principal(aux) transféré(s) vers {Code}.",
                        migratedWallets, principale.Code);
                }

                if (skippedNoTaux > 0)
                {
                    logger.LogWarning(
                        "Migration retrait devise principale : {Count} wallet(s) ignoré(s) faute de taux de change.",
                        skippedNoTaux);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration retrait devise principale");
                throw;
            }
        }

        /// <summary>
        /// Permissions confirmation demande + scan QR pour agents / superviseurs.
        /// </summary>
        private static async Task MigrateBonEnvoiWorkflowPermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            var definitions = new (string Nom, string Description)[]
            {
                ("CONFIRM_DEMANDE_BON_ENVOI", "Confirmer ou rejeter une demande de bon d'envoi"),
                ("SCAN_BON_ENVOI", "Scanner et vérifier un QR de bon d'envoi"),
            };

            var created = 0;
            foreach (var (nom, desc) in definitions)
            {
                if (await context.Permissions.AnyAsync(p => p.Nom == nom))
                    continue;

                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = desc,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                created++;
            }

            if (created > 0)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Permissions bon d'envoi workflow : {Count} créée(s).", created);
            }

            var roleNoms = new[] { "Agent (AT)", "Agent (AA)", "Superviseur" };
            var roles = await context.Roles.Where(r => roleNoms.Contains(r.Nom)).ToListAsync();
            var permissionNoms = new[]
            {
                "CONFIRM_DEMANDE_BON_ENVOI",
                "SCAN_BON_ENVOI",
                "READ_DEMANDE_BON_ENVOI",
                "READ_BON_ENVOI"
            };
            var permissions = await context.Permissions
                .Where(p => permissionNoms.Contains(p.Nom) && p.Statut)
                .ToListAsync();

            var added = 0;
            foreach (var role in roles)
            {
                foreach (var permission in permissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == role.IdRole && rp.PermissionId == permission.IdPermission);
                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                    added++;
                }
            }

            if (added > 0)
            {
                await context.SaveChangesAsync();
                logger.LogInformation(
                    "Migration permissions bon d'envoi : {Added} attribution(s) rôle agent/superviseur.",
                    added);
            }
        }

        /// <summary>
        /// Codes métier sur Frais + frais pénalité retard cotisation (idempotent).
        /// </summary>
        private static async Task EnsureFraisCatalogueCodesAsync(ProsocDbContext context, ILogger logger)
        {
            var mappings = new (string Code, string LibelleContains)[]
            {
                (FraisCodes.FraisAdhesion, "Adhesion"),
                (FraisCodes.CarteMembre, "Carte"),
                (FraisCodes.PenaliteRetardCotisation, "Pénalit")
            };

            foreach (var (code, libellePart) in mappings)
            {
                var byCode = await context.Frais.FirstOrDefaultAsync(f => f.Code == code);
                if (byCode != null)
                    continue;

                var candidate = code == FraisCodes.PenaliteRetardCotisation
                    ? await context.Frais
                        .Where(f => !f.EstSupprime
                                    && (f.Libelle.Contains("énalit") || f.Libelle.Contains("enalit")))
                        .OrderBy(f => f.IdFrais)
                        .FirstOrDefaultAsync()
                    : await context.Frais
                        .Where(f => !f.EstSupprime && f.Libelle.Contains(libellePart))
                        .OrderBy(f => f.IdFrais)
                        .FirstOrDefaultAsync();

                if (candidate != null)
                {
                    candidate.Code = code;
                    candidate.DateModification = DateTime.Now;
                    logger.LogInformation("Frais Id={Id} : code {Code} assigné.", candidate.IdFrais, code);
                    continue;
                }

                if (code != FraisCodes.PenaliteRetardCotisation)
                    continue;

                var deviseId = await context.Devises
                    .Where(d => d.Statut && d.EstDevisePrincipale)
                    .Select(d => d.IdDevise)
                    .FirstOrDefaultAsync();

                if (deviseId == 0)
                {
                    deviseId = await context.Devises
                        .Where(d => d.Statut)
                        .Select(d => d.IdDevise)
                        .FirstOrDefaultAsync();
                }

                if (deviseId == 0)
                {
                    logger.LogWarning("EnsureFraisCatalogueCodes : aucune devise active pour créer le frais pénalité.");
                    continue;
                }

                context.Frais.Add(new Frais
                {
                    Code = FraisCodes.PenaliteRetardCotisation,
                    Libelle = "Pénalité",
                    Montant = 5,
                    DeviseId = deviseId,
                    TauxCommission = 0m,
                    Periodicite = "Ponctuel",
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                logger.LogInformation("Frais pénalité {Code} créé (devise {DeviseId}).", code, deviseId);
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// USD devise principale + taux USD→CDF initial (idempotent).
        /// </summary>
        private static async Task EnsureMultideviseConfigAsync(ProsocDbContext context, ILogger logger)
        {
            var usd = await context.Devises.FirstOrDefaultAsync(d => d.Code == "USD");
            var cdf = await context.Devises.FirstOrDefaultAsync(d => d.Code == "CDF");

            if (usd == null || cdf == null)
            {
                logger.LogWarning("Multidevise seed ignoré : devises USD/CDF absentes.");
                return;
            }

            var autresPrincipales = await context.Devises
                .Where(d => d.EstDevisePrincipale && d.IdDevise != usd.IdDevise)
                .ToListAsync();
            foreach (var d in autresPrincipales)
                d.EstDevisePrincipale = false;

            usd.EstDevisePrincipale = true;
            usd.Symbole ??= "$";
            usd.Statut = true;
            cdf.EstDevisePrincipale = false;
            cdf.Symbole ??= "FC";

            const decimal tauxUsdCdf = 2850m;
            var tauxExistant = await context.TauxChangeDevises.AnyAsync(t =>
                t.DeviseSourceId == usd.IdDevise &&
                t.DeviseCibleId == cdf.IdDevise &&
                t.Taux == tauxUsdCdf &&
                t.Statut);

            if (!tauxExistant)
            {
                context.TauxChangeDevises.Add(new TauxChangeDevise
                {
                    DeviseSourceId = usd.IdDevise,
                    DeviseCibleId = cdf.IdDevise,
                    Taux = tauxUsdCdf,
                    DateEffet = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                logger.LogInformation("Taux USD→CDF initial créé ({Taux}).", tauxUsdCdf);
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Configuration multidevise vérifiée (USD principale).");
        }

        /// <summary>
        /// Garantit que admin@prosoc.cd et superadmin@prosoc.cd ont leurs rôles et permissions
        /// (SuperAdmin : catalogue complet ; Admin : tout sauf DELETE_* et MANAGE_SYSTEM).
        /// </summary>
        private static async Task EnsureSystemAdminAccessAsync(ProsocDbContext context, ILogger logger)
        {
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Admin");
            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "SuperAdmin");
            var adminUser = await context.Utilisateurs.FirstOrDefaultAsync(u => u.EmailUtilisateur == "admin@prosoc.cd");
            var superAdminUser = await context.Utilisateurs.FirstOrDefaultAsync(u => u.EmailUtilisateur == "superadmin@prosoc.cd");
            var allPermissions = await context.Permissions.Where(p => p.Statut).ToListAsync();

            async Task EnsureUserRoleAsync(Utilisateur? user, Role? role)
            {
                if (user == null || role == null)
                    return;

                user.RoleId = role.IdRole;

                var userRole = await context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UtilisateurId == user.IdUtilisateur && ur.RoleId == role.IdRole);

                if (userRole == null)
                {
                    context.UserRoles.Add(new UserRole
                    {
                        UtilisateurId = user.IdUtilisateur,
                        RoleId = role.IdRole,
                        IsPrimary = true,
                        Statut = true,
                        DateAttribution = DateTime.Now,
                    });
                    logger.LogInformation("UserRole {Role} attribué à {Email}", role.Nom, user.EmailUtilisateur);
                }
                else
                {
                    userRole.Statut = true;
                    userRole.IsPrimary = true;
                }

                var rolePermissions = role.Nom == "SuperAdmin"
                    ? FilterPermissionsForSuperAdminRole(allPermissions).ToList()
                    : FilterPermissionsForAdminRole(allPermissions).ToList();

                foreach (var permission in rolePermissions)
                {
                    var exists = await context.RolePermissions
                        .AnyAsync(rp => rp.RoleId == role.IdRole && rp.PermissionId == permission.IdPermission);

                    if (!exists)
                    {
                        context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = role.IdRole,
                            PermissionId = permission.IdPermission,
                            DateAttribution = DateTime.Now,
                        });
                    }
                }

                var allowedIds = rolePermissions.Select(p => p.IdPermission).ToHashSet();
                var excess = await context.RolePermissions
                    .Where(rp => rp.RoleId == role.IdRole && !allowedIds.Contains(rp.PermissionId))
                    .ToListAsync();

                if (excess.Count > 0)
                {
                    context.RolePermissions.RemoveRange(excess);
                }
            }

            await EnsureUserRoleAsync(adminUser, adminRole);
            await EnsureUserRoleAsync(superAdminUser, superAdminRole);
            await context.SaveChangesAsync();
            logger.LogInformation("Accès système Admin/SuperAdmin vérifiés (UserRoles + permissions)");
        }

        /// <summary>Permissions du rôle Agent (AT) — adhésion niveau 1, collecte, wallet, dashboard terrain.</summary>
        private static IReadOnlyList<string> GetAgentAtRolePermissionNames() => new[]
        {
            // Adhésion / affilié (niveau 1 — création affilié via CREATE_ADHESION)
            "CREATE_ADHESION",
            "READ_ADHESION",
            "UPDATE_ADHESION",
            "READ_AFFILIE",
            "UPDATE_AFFILIE",
            "READ_DEPENDANT",
            // Collecte
            "CREATE_COLLECTE",
            "READ_COLLECTE",
            "READ_FRAIS",
            "READ_DEVISE",
            // Catalogue (lecture)
            "READ_PRESTATION",
            "READ_PRODUIT_MUTUEL",
            "READ_PRODUIT_ASSUREUR",
            "READ_SOUSCRIPTION_PRESTATION",
            "READ_TYPE_ADHESION",
            "READ_CATEGORIE_ADHESION",
            // Wallet / transactions / retraits
            "READ_WALLET_AGENT",
            "UPDATE_WALLET_AGENT",
            "READ_WALLET_VIRTUEL",
            "READ_WALLET_MOVEMENT",
            "CREATE_WALLET_MOVEMENT",
            "READ_TRANSACTION",
            "CREATE_TRANSACTION",
            "CREATE_DEMANDE_RETRAIT_AGENT",
            "READ_DEMANDE_RETRAIT_AGENT",
            // Dashboard agent
            "ACCESS_DASHBOARD_AGENT",
            // Bon d'envoi (workflow terrain)
            "READ_DEMANDE_BON_ENVOI",
            "CREATE_DEMANDE_BON_ENVOI",
            "CONFIRM_DEMANDE_BON_ENVOI",
            "SCAN_BON_ENVOI",
            "READ_BON_ENVOI",
            // Référentiels (lecture — zone de travail)
            "READ_ZONE_SOCIALE",
            "READ_COMMUNE",
            "READ_PROVINCE",
            "READ_CATEGORIE_AGENT",
            // Contexte opérationnel
            "READ_COTISATION_AFFILIE",
            "READ_NOTIFICATION"
        };

        private static IEnumerable<Permission> FilterPermissionsForAgentAtRole(IEnumerable<Permission> allPermissions)
        {
            var noms = GetAgentAtRolePermissionNames();
            return allPermissions.Where(p => noms.Contains(p.Nom));
        }

        /// <summary>
        /// Permissions du rôle Chef d'équipe — périmètre AT + lecture équipe de zone.
        /// </summary>
        private static IReadOnlyList<string> GetChefEquipeRolePermissionNames()
        {
            var noms = new List<string>(GetAgentAtRolePermissionNames())
            {
                "ACCESS_DASHBOARD_CHEF_EQUIPE",
                "READ_EQUIPE_ZONE",
                "READ_EQUIPE_WALLET_MOVEMENT",
                "READ_EQUIPE_COLLECTE"
            };

            return noms.Distinct(StringComparer.Ordinal).ToList();
        }

        private static IEnumerable<Permission> FilterPermissionsForChefEquipeRole(IEnumerable<Permission> allPermissions)
        {
            var noms = GetChefEquipeRolePermissionNames();
            return allPermissions.Where(p => noms.Contains(p.Nom));
        }

        private static async Task<Role?> EnsureChefEquipeRoleAsync(ProsocDbContext context, ILogger logger)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Chef d'équipe");
            if (role != null)
                return role;

            role = new Role
            {
                Nom = "Chef d'équipe",
                Code = "CE",
                Description = "Chef d'équipe de zone",
                Niveau = 6,
                Statut = true,
                DateCreation = DateTime.Now
            };
            context.Roles.Add(role);
            await context.SaveChangesAsync();
            logger.LogInformation("Rôle « Chef d'équipe » créé (IdRole = {RoleId}).", role.IdRole);
            return role;
        }

        private static async Task MigrateChefEquipeRolePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var chefEquipeRole = await EnsureChefEquipeRoleAsync(context, logger);
                if (chefEquipeRole == null)
                {
                    logger.LogWarning("Migration permissions Chef d'équipe : rôle introuvable.");
                    return;
                }

                var permissionNoms = GetChefEquipeRolePermissionNames();
                var allowedPermissions = await context.Permissions
                    .Where(p => permissionNoms.Contains(p.Nom) && p.Statut)
                    .ToListAsync();

                var allowedIds = allowedPermissions.Select(p => p.IdPermission).ToHashSet();

                var added = 0;
                foreach (var permission in allowedPermissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == chefEquipeRole.IdRole && rp.PermissionId == permission.IdPermission);

                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = chefEquipeRole.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                    added++;
                }

                var excessRolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == chefEquipeRole.IdRole && !allowedIds.Contains(rp.PermissionId))
                    .ToListAsync();

                var removed = excessRolePermissions.Count;
                if (removed > 0)
                    context.RolePermissions.RemoveRange(excessRolePermissions);

                if (added > 0 || removed > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration permissions Chef d'équipe : {Added} ajoutée(s), {Removed} retirée(s) (catalogue attendu : {Total}).",
                        added, removed, allowedPermissions.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration des permissions du rôle Chef d'équipe");
                throw;
            }
        }

        /// <summary>
        /// Renseigne ChefEquipeAgentId / SuperviseurAgentId à partir des rôles JWT existants (idempotent).
        /// </summary>
        public static async Task MigrateTerritorialEncadrementAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var ceRoleId = await context.Roles.AsNoTracking()
                    .Where(r => r.Nom == "Chef d'équipe" && r.Statut)
                    .Select(r => r.IdRole)
                    .FirstOrDefaultAsync();
                var spRoleId = await context.Roles.AsNoTracking()
                    .Where(r => r.Nom == "Superviseur" && r.Statut)
                    .Select(r => r.IdRole)
                    .FirstOrDefaultAsync();

                var modified = false;

                if (ceRoleId > 0)
                {
                    var zones = await context.ZonesSociales
                        .Where(z => z.ChefEquipeAgentId == null)
                        .ToListAsync();

                    foreach (var zone in zones)
                    {
                        var candidats = await (
                            from a in context.Agents.AsNoTracking()
                            join u in context.Utilisateurs.AsNoTracking() on a.IdAgent equals u.AgentId
                            join ur in context.UserRoles.AsNoTracking() on u.IdUtilisateur equals ur.UtilisateurId
                            where a.Statut && u.Statut && ur.Statut && ur.RoleId == ceRoleId
                                && a.ZoneSocialeId == zone.IdZoneSociale
                            select a.IdAgent
                        ).Distinct().ToListAsync();

                        if (candidats.Count == 1)
                        {
                            zone.ChefEquipeAgentId = candidats[0];
                            modified = true;
                        }
                        else if (candidats.Count > 1)
                        {
                            logger.LogWarning(
                                "Migration encadrement territorial : zone {ZoneId} ({Nom}) — {Count} candidats CE, ignorée.",
                                zone.IdZoneSociale, zone.Nom, candidats.Count);
                        }
                    }
                }

                if (spRoleId > 0)
                {
                    var communes = await context.Communes
                        .Where(c => c.SuperviseurAgentId == null)
                        .ToListAsync();

                    foreach (var commune in communes)
                    {
                        var candidats = await (
                            from a in context.Agents.AsNoTracking()
                            join z in context.ZonesSociales.AsNoTracking() on a.ZoneSocialeId equals z.IdZoneSociale
                            join u in context.Utilisateurs.AsNoTracking() on a.IdAgent equals u.AgentId
                            join ur in context.UserRoles.AsNoTracking() on u.IdUtilisateur equals ur.UtilisateurId
                            where a.Statut && u.Statut && ur.Statut && ur.RoleId == spRoleId
                                && z.CommuneId == commune.IdCommune
                            select a.IdAgent
                        ).Distinct().ToListAsync();

                        if (candidats.Count == 1)
                        {
                            commune.SuperviseurAgentId = candidats[0];
                            modified = true;
                        }
                        else if (candidats.Count > 1)
                        {
                            logger.LogWarning(
                                "Migration encadrement territorial : commune {CommuneId} ({Nom}) — {Count} candidats SP, ignorée.",
                                commune.IdCommune, commune.Nom, candidats.Count);
                        }
                    }
                }

                if (modified)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation("Migration encadrement territorial : FK ChefEquipe / Superviseur renseignées.");
                }

                // NOTE: Agent.SuperviseurId (legacy) a été retiré du modèle.
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration de l'encadrement territorial");
                throw;
            }
        }

        /// <summary>
        /// Permissions du rôle Superviseur — périmètre AT + gestion d'équipe
        /// (sans UPDATE_ADHESION / UPDATE_AFFILIE, sans CREATE/READ/UPDATE_ASSUREUR,
        /// sans CREATE_PRODUIT_ASSUREUR).
        /// </summary>
        private static IReadOnlyList<string> GetSuperviseurRolePermissionNames()
        {
            var noms = new List<string>(GetAgentAtRolePermissionNames())
            {
                // Agents sous supervision (consultation, désactivation)
                "READ_AGENT",
                "UPDATE_AGENT",
                // Hiérarchie & supervision
                "READ_HIERARCHIE",
                "MANAGE_SUPERVISION",
                "MANAGE_OBJECTIFS",
                "VALIDATE_PERFORMANCE",
                "ACCESS_DASHBOARD_SUPERVISEUR",
                // Validation demandes de retrait agent (génération jeton)
                "VALIDATE_DEMANDE_RETRAIT_AGENT",
                // Wallet virtuel agents (ajouter-solde / modifier-solde-wallet-agents)
                "UPDATE_WALLET_VIRTUEL",
                // Demandes de recharge wallet virtuel (jusqu'au plafond)
                "CREATE_DEMANDE_RECHARGE_WALLET_VIRTUEL",
                "READ_DEMANDE_RECHARGE_WALLET_VIRTUEL",
                "CONFIRM_DEMANDE_RECHARGE_WALLET_VIRTUEL",
                // Rapports performance équipe
                "GENERATE_RAPPORT",
                "EXPORT_DATA"
            };

            noms.RemoveAll(n => n is
                "UPDATE_ADHESION" or "UPDATE_AFFILIE" or
                "CREATE_ASSUREUR" or "READ_ASSUREUR" or "UPDATE_ASSUREUR" or
                "CREATE_PRODUIT_ASSUREUR");
            return noms.Distinct(StringComparer.Ordinal).ToList();
        }

        private static IEnumerable<Permission> FilterPermissionsForSuperviseurRole(IEnumerable<Permission> allPermissions)
        {
            var noms = GetSuperviseurRolePermissionNames();
            return allPermissions.Where(p => noms.Contains(p.Nom));
        }

        /// <summary>
        /// Aligne le rôle Superviseur sur la liste blanche et retire les permissions hors périmètre.
        /// </summary>
        private static async Task MigrateSuperviseurRolePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var superviseurRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Superviseur");
                if (superviseurRole == null)
                {
                    logger.LogWarning("Migration permissions Superviseur : rôle introuvable.");
                    return;
                }

                var permissionNoms = GetSuperviseurRolePermissionNames();
                var allowedPermissions = await context.Permissions
                    .Where(p => permissionNoms.Contains(p.Nom) && p.Statut)
                    .ToListAsync();

                var allowedIds = allowedPermissions.Select(p => p.IdPermission).ToHashSet();

                var added = 0;
                foreach (var permission in allowedPermissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == superviseurRole.IdRole && rp.PermissionId == permission.IdPermission);

                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = superviseurRole.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                    added++;
                }

                var excessRolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == superviseurRole.IdRole && !allowedIds.Contains(rp.PermissionId))
                    .ToListAsync();

                var removed = excessRolePermissions.Count;
                if (removed > 0)
                {
                    context.RolePermissions.RemoveRange(excessRolePermissions);
                }

                if (added > 0 || removed > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration permissions Superviseur : {Added} ajoutée(s), {Removed} retirée(s) (catalogue attendu : {Total}).",
                        added, removed, allowedPermissions.Count);
                }

                var missing = permissionNoms.Except(allowedPermissions.Select(p => p.Nom)).ToList();
                if (missing.Count > 0)
                {
                    logger.LogWarning(
                        "Permissions Superviseur absentes du catalogue : {Missing}",
                        string.Join(", ", missing));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration des permissions du rôle Superviseur");
                throw;
            }
        }

        /// <summary>
        /// Permissions du rôle Agent (AA) — périmètre AT + encodeur niveau 2 (dépendants, antécédents).
        /// </summary>
        private static IReadOnlyList<string> GetAgentAaRolePermissionNames()
        {
            var noms = new List<string>(GetAgentAtRolePermissionNames())
            {
                // Encodeur niveau 2 — personnes à charge
                "CREATE_DEPENDANT",
                "READ_DEPENDANT",
                "UPDATE_DEPENDANT",
                // Antécédents médicaux
                "CREATE_ANTECEDENT",
                "READ_ANTECEDENT",
                "UPDATE_ANTECEDENT",
                // Catalogue assureurs (consultation dossier)
                "READ_ASSUREUR",
                // Dashboard encodeur (remplace ACCESS_DASHBOARD_AGENT pour le rôle AA)
                "ACCESS_DASHBOARD_AGENT_AA",
                // Encodage / validation dossier niveau 2
                "ENCODE_ADHESION_NIVEAU_2"
            };

            noms.Remove("ACCESS_DASHBOARD_AGENT");

            return noms.Distinct(StringComparer.Ordinal).ToList();
        }

        private static IEnumerable<Permission> FilterPermissionsForAgentAaRole(IEnumerable<Permission> allPermissions)
        {
            var noms = GetAgentAaRolePermissionNames();
            return allPermissions.Where(p => noms.Contains(p.Nom));
        }

        /// <summary>
        /// Aligne le rôle Agent (AT) sur la liste blanche et retire les permissions hors périmètre.
        /// </summary>
        private static async Task MigrateAgentAtRolePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var atRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Agent (AT)");
                if (atRole == null)
                {
                    logger.LogWarning("Migration permissions Agent (AT) : rôle introuvable.");
                    return;
                }

                var permissionNoms = GetAgentAtRolePermissionNames();
                var allowedPermissions = await context.Permissions
                    .Where(p => permissionNoms.Contains(p.Nom) && p.Statut)
                    .ToListAsync();

                var allowedIds = allowedPermissions.Select(p => p.IdPermission).ToHashSet();

                var added = 0;
                foreach (var permission in allowedPermissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == atRole.IdRole && rp.PermissionId == permission.IdPermission);

                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = atRole.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                    added++;
                }

                var excessRolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == atRole.IdRole && !allowedIds.Contains(rp.PermissionId))
                    .ToListAsync();

                var removed = excessRolePermissions.Count;
                if (removed > 0)
                {
                    context.RolePermissions.RemoveRange(excessRolePermissions);
                }

                if (added > 0 || removed > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration permissions Agent (AT) : {Added} ajoutée(s), {Removed} retirée(s) (catalogue attendu : {Total}).",
                        added, removed, allowedPermissions.Count);
                }

                var missing = permissionNoms.Except(allowedPermissions.Select(p => p.Nom)).ToList();
                if (missing.Count > 0)
                {
                    logger.LogWarning(
                        "Permissions Agent (AT) absentes du catalogue : {Missing}",
                        string.Join(", ", missing));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration des permissions du rôle Agent (AT)");
                throw;
            }
        }

        /// <summary>
        /// Aligne le rôle Agent (AA) sur la liste blanche et retire les permissions hors périmètre.
        /// </summary>
        private static async Task MigrateAgentAaRolePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var aaRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Agent (AA)");
                if (aaRole == null)
                {
                    logger.LogWarning("Migration permissions Agent (AA) : rôle introuvable.");
                    return;
                }

                var permissionNoms = GetAgentAaRolePermissionNames();
                var allowedPermissions = await context.Permissions
                    .Where(p => permissionNoms.Contains(p.Nom) && p.Statut)
                    .ToListAsync();

                var allowedIds = allowedPermissions.Select(p => p.IdPermission).ToHashSet();

                var added = 0;
                foreach (var permission in allowedPermissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == aaRole.IdRole && rp.PermissionId == permission.IdPermission);

                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = aaRole.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                    added++;
                }

                var excessRolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == aaRole.IdRole && !allowedIds.Contains(rp.PermissionId))
                    .ToListAsync();

                var removed = excessRolePermissions.Count;
                if (removed > 0)
                {
                    context.RolePermissions.RemoveRange(excessRolePermissions);
                }

                if (added > 0 || removed > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration permissions Agent (AA) : {Added} ajoutée(s), {Removed} retirée(s) (catalogue attendu : {Total}).",
                        added, removed, allowedPermissions.Count);
                }

                var missing = permissionNoms.Except(allowedPermissions.Select(p => p.Nom)).ToList();
                if (missing.Count > 0)
                {
                    logger.LogWarning(
                        "Permissions Agent (AA) absentes du catalogue : {Missing}",
                        string.Join(", ", missing));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration des permissions du rôle Agent (AA)");
                throw;
            }
        }

        /// <summary>
        /// Indique si une permission est accordée au rôle SuperAdmin (catalogue complet actif, hors UPDATE_COLLECTE).
        /// </summary>
        private static bool IsSuperAdminPermissionAllowed(Permission permission) =>
            permission.Statut && permission.Nom != "UPDATE_COLLECTE";

        private static IEnumerable<Permission> FilterPermissionsForSuperAdminRole(IEnumerable<Permission> allPermissions) =>
            allPermissions.Where(IsSuperAdminPermissionAllowed);

        /// <summary>
        /// Aligne le rôle SuperAdmin sur le catalogue complet des permissions actives.
        /// </summary>
        private static async Task MigrateSuperAdminRolePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "SuperAdmin");
                if (superAdminRole == null)
                {
                    logger.LogWarning("Migration permissions SuperAdmin : rôle introuvable.");
                    return;
                }

                var allowedPermissions = await context.Permissions
                    .Where(p => p.Statut && p.Nom != "UPDATE_COLLECTE")
                    .ToListAsync();

                var allowedIds = allowedPermissions.Select(p => p.IdPermission).ToHashSet();

                var added = 0;
                foreach (var permission in allowedPermissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == superAdminRole.IdRole && rp.PermissionId == permission.IdPermission);

                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = superAdminRole.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                    added++;
                }

                var excessRolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == superAdminRole.IdRole && !allowedIds.Contains(rp.PermissionId))
                    .ToListAsync();

                var removed = excessRolePermissions.Count;
                if (removed > 0)
                {
                    context.RolePermissions.RemoveRange(excessRolePermissions);
                }

                if (added > 0 || removed > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration permissions SuperAdmin : {Added} ajoutée(s), {Removed} retirée(s) (catalogue attendu : {Total}).",
                        added, removed, allowedPermissions.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration des permissions du rôle SuperAdmin");
                throw;
            }
        }

        /// <summary>
        /// Indique si une permission est accordée au rôle Admin (pas de DELETE_*, pas de MANAGE_SYSTEM, pas UPDATE_COLLECTE).
        /// </summary>
        private static bool IsAdminPermissionAllowed(Permission permission) =>
            permission.Statut
            && !permission.Nom.StartsWith("DELETE_", StringComparison.Ordinal)
            && permission.Nom != "MANAGE_SYSTEM"
            && permission.Nom != "UPDATE_COLLECTE";

        private static IEnumerable<Permission> FilterPermissionsForAdminRole(IEnumerable<Permission> allPermissions) =>
            allPermissions.Where(IsAdminPermissionAllowed);

        /// <summary>
        /// Aligne le rôle Admin sur la politique « tout sauf DELETE et MANAGE_SYSTEM ».
        /// </summary>
        private static async Task MigrateAdminRolePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Admin");
                if (adminRole == null)
                {
                    logger.LogWarning("Migration permissions Admin : rôle introuvable.");
                    return;
                }

                var allowedPermissions = await context.Permissions
                    .Where(p => p.Statut
                                && !p.Nom.StartsWith("DELETE_")
                                && p.Nom != "MANAGE_SYSTEM"
                                && p.Nom != "UPDATE_COLLECTE")
                    .ToListAsync();

                var allowedIds = allowedPermissions.Select(p => p.IdPermission).ToHashSet();

                var added = 0;
                foreach (var permission in allowedPermissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == adminRole.IdRole && rp.PermissionId == permission.IdPermission);

                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRole.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                    added++;
                }

                var excessRolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == adminRole.IdRole && !allowedIds.Contains(rp.PermissionId))
                    .ToListAsync();

                var removed = excessRolePermissions.Count;
                if (removed > 0)
                {
                    context.RolePermissions.RemoveRange(excessRolePermissions);
                }

                if (added > 0 || removed > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration permissions Admin : {Added} ajoutée(s), {Removed} retirée(s) (catalogue attendu : {Total}).",
                        added, removed, allowedPermissions.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration des permissions du rôle Admin");
                throw;
            }
        }

        /// <summary>
        /// Retire UPDATE_COLLECTE de tous les rôles (aucun rôle ne peut modifier une collecte via PUT).
        /// </summary>
        private static async Task MigrateRemoveUpdateCollectePermissionAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var permission = await context.Permissions
                    .FirstOrDefaultAsync(p => p.Nom == "UPDATE_COLLECTE");

                if (permission == null)
                {
                    logger.LogWarning("Migration UPDATE_COLLECTE : permission introuvable.");
                    return;
                }

                var toRemove = await context.RolePermissions
                    .Where(rp => rp.PermissionId == permission.IdPermission)
                    .ToListAsync();

                if (toRemove.Count == 0)
                    return;

                context.RolePermissions.RemoveRange(toRemove);
                await context.SaveChangesAsync();
                logger.LogInformation(
                    "Migration UPDATE_COLLECTE : {Count} attribution(s) retirée(s) sur tous les rôles.",
                    toRemove.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors du retrait de UPDATE_COLLECTE");
                throw;
            }
        }

        /// <summary>
        /// Permissions du rôle IT — paramétrage catalogue, référentiels, support et technique.
        /// (Frais, FlexPay marchand, notifications, mobile : contrôles par rôle sur les contrôleurs dédiés.)
        /// </summary>
        private static IReadOnlyList<string> GetItRolePermissionNames() => new[]
        {
            // Système (hors MANAGE_SYSTEM réservé Admin/SuperAdmin)
            "VIEW_LOGS",
            "MANAGE_BACKUP",
            // Comptes & agents
            "READ_USER",
            "UPDATE_USER",
            "CREATE_AGENT",
            "READ_AGENT",
            "UPDATE_AGENT",
            // Référentiels géographiques & agent
            "CREATE_PROVINCE",
            "READ_PROVINCE",
            "UPDATE_PROVINCE",
            "CREATE_COMMUNE",
            "READ_COMMUNE",
            "UPDATE_COMMUNE",
            "CREATE_ZONE_SOCIALE",
            "READ_ZONE_SOCIALE",
            "UPDATE_ZONE_SOCIALE",
            "CREATE_DEVISE",
            "READ_DEVISE",
            "UPDATE_DEVISE",
            "CREATE_TAUX_CHANGE",
            "CREATE_CATEGORIE_AGENT",
            "READ_CATEGORIE_AGENT",
            "UPDATE_CATEGORIE_AGENT",
            "CREATE_CATEGORIE_ADHESION",
            "READ_CATEGORIE_ADHESION",
            "UPDATE_CATEGORIE_ADHESION",
            "CREATE_TYPE_ADHESION",
            "READ_TYPE_ADHESION",
            "UPDATE_TYPE_ADHESION",
            // Catalogue produits & prestations
            "CREATE_PRODUIT_ASSUREUR",
            "READ_PRODUIT_ASSUREUR",
            "UPDATE_PRODUIT_ASSUREUR",
            "CREATE_PRODUIT_MUTUEL",
            "READ_PRODUIT_MUTUEL",
            "UPDATE_PRODUIT_MUTUEL",
            "READ_PRESTATION",
            "CREATE_FRAIS",
            "READ_FRAIS",
            "UPDATE_FRAIS",
            "CREATE_ASSUREUR",
            "READ_ASSUREUR",
            "UPDATE_ASSUREUR",
            // Hôpitaux partenaires (référentiel)
            "CREATE_HOPITAL_PARTENAIRE",
            "READ_HOPITAL_PARTENAIRE",
            "UPDATE_HOPITAL_PARTENAIRE",
            // Notifications & files techniques
            "CREATE_NOTIFICATION",
            "READ_NOTIFICATION",
            "UPDATE_NOTIFICATION",
            "DELETE_NOTIFICATION",
            // Consultation opérationnelle (support / diagnostic)
            "READ_COLLECTE",
            "READ_ADHESION",
            "READ_AFFILIE",
            "READ_TRANSACTION",
            "READ_SOUSCRIPTION_PRESTATION",
            "READ_BON_ENVOI",
            "READ_DEMANDE_BON_ENVOI",
            "CONFIRM_DEMANDE_BON_ENVOI",
            "READ_HIERARCHIE",
            // Corrections données membres
            "CREATE_DEPENDANT",
            "READ_DEPENDANT",
            "UPDATE_DEPENDANT",
            "CREATE_ANTECEDENT",
            "READ_ANTECEDENT",
            "UPDATE_ANTECEDENT",
            // Wallets (lecture + correction support)
            "READ_WALLET_AGENT",
            "UPDATE_WALLET_AGENT",
            "READ_WALLET_VIRTUEL",
            "UPDATE_WALLET_VIRTUEL",
            "READ_WALLET_MOVEMENT",
            // Dashboard admin & exports techniques
            "ACCESS_DASHBOARD_ADMIN",
            "GENERATE_RAPPORT",
            "EXPORT_DATA",
            "READ_PARAMETRES_METIER",
            "UPDATE_PARAMETRES_METIER"
        };

        private static IEnumerable<Permission> FilterPermissionsForItRole(IEnumerable<Permission> allPermissions)
        {
            var noms = GetItRolePermissionNames();
            return allPermissions.Where(p => noms.Contains(p.Nom));
        }

        /// <summary>
        /// Aligne le rôle IT sur la liste blanche et retire les permissions hors périmètre.
        /// </summary>
        private static async Task MigrateItRolePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var itRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "IT");
                if (itRole == null)
                {
                    logger.LogWarning("Migration permissions IT : rôle introuvable.");
                    return;
                }

                var permissionNoms = GetItRolePermissionNames();
                var allowedPermissions = await context.Permissions
                    .Where(p => permissionNoms.Contains(p.Nom) && p.Statut)
                    .ToListAsync();

                var allowedIds = allowedPermissions.Select(p => p.IdPermission).ToHashSet();

                var added = 0;
                foreach (var permission in allowedPermissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == itRole.IdRole && rp.PermissionId == permission.IdPermission);

                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = itRole.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                    added++;
                }

                var excessRolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == itRole.IdRole && !allowedIds.Contains(rp.PermissionId))
                    .ToListAsync();

                var removed = excessRolePermissions.Count;
                if (removed > 0)
                {
                    context.RolePermissions.RemoveRange(excessRolePermissions);
                }

                if (added > 0 || removed > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration permissions IT : {Added} ajoutée(s), {Removed} retirée(s) (catalogue attendu : {Total}).",
                        added, removed, allowedPermissions.Count);
                }

                var missing = permissionNoms.Except(allowedPermissions.Select(p => p.Nom)).ToList();
                if (missing.Count > 0)
                {
                    logger.LogWarning(
                        "Permissions IT absentes du catalogue : {Missing}",
                        string.Join(", ", missing));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration des permissions du rôle IT");
                throw;
            }
        }

        /// <summary>
        /// Permissions du rôle Financier — flux financiers, wallets agents, collectes, rapports.
        /// (Dashboard financier : contrôle par rôle sur DashboardFinancierController.)
        /// </summary>
        private static IReadOnlyList<string> GetFinancierRolePermissionNames() => new[]
        {
            // Agents & wallets (commissions, retraits, Maash)
            "READ_AGENT",
            "READ_WALLET_AGENT",
            "UPDATE_WALLET_AGENT",
            "READ_WALLET_VIRTUEL",
            "READ_WALLET_MOVEMENT",
            "CREATE_WALLET_MOVEMENT",
            // Transactions & collectes
            "READ_COLLECTE",
            "CREATE_COLLECTE",
            "READ_TRANSACTION",
            "CREATE_TRANSACTION",
            "UPDATE_TRANSACTION",
            // Catalogue financier
            "CREATE_FRAIS",
            "READ_FRAIS",
            "UPDATE_FRAIS",
            "READ_DEVISE",
            "CREATE_DEVISE",
            "CREATE_TAUX_CHANGE",
            // Adhésion & affilié (flux guichet)
            "CREATE_ADHESION",
            "READ_ADHESION",
            "UPDATE_ADHESION",
            "READ_AFFILIE",
            "READ_DEPENDANT",
            "READ_TYPE_ADHESION",
            "READ_CATEGORIE_ADHESION",
            "READ_COTISATION_AFFILIE",
            // Contexte membres (consultation)
            "READ_SOUSCRIPTION_PRESTATION",
            "UPDATE_SOUSCRIPTION_PRESTATION",
            "DELETE_SOUSCRIPTION_PRESTATION",
            "READ_ARRIERES_AFFILIE",
            "READ_PENALITE_AFFILIE",
            "READ_PRESTATION",
            "READ_PRODUIT_MUTUEL",
            "CREATE_PRODUIT_MUTUEL",
            "UPDATE_PRODUIT_MUTUEL",
            "READ_PRODUIT_ASSUREUR",
            "CREATE_PRODUIT_ASSUREUR",
            "UPDATE_PRODUIT_ASSUREUR",
            // Géographie & hiérarchie (filtres rapports)
            "READ_PROVINCE",
            "READ_COMMUNE",
            "READ_ZONE_SOCIALE",
            "READ_CATEGORIE_AGENT",
            "READ_HIERARCHIE",
            // Rapports & statistiques
            "GENERATE_RAPPORT",
            "EXPORT_DATA",
            "READ_STATISTIQUES",
            // Objectifs agents (consultation seule)
            "READ_TARGET_AGENT",
            // Perception compte virtuel (consultation / réconciliation)
            "READ_PERCEPTION_VIRTUAL",
            "CONFIRM_PERCEPTION_VIRTUAL",
            // Retrait agent — marquer payé (jeton) + menu module
            "MARQUER_PAYER_RETRAIT_AGENT",
            "READ_RETRAIT_AGENT",
            // Notifications
            "READ_NOTIFICATION"
        };

        private static IEnumerable<Permission> FilterPermissionsForFinancierRole(IEnumerable<Permission> allPermissions)
        {
            var noms = GetFinancierRolePermissionNames();
            return allPermissions.Where(p => noms.Contains(p.Nom));
        }

        /// <summary>
        /// Aligne le rôle Financier sur la liste blanche et retire les permissions hors périmètre.
        /// </summary>
        private static async Task MigrateFinancierRolePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var financierRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Financier");
                if (financierRole == null)
                {
                    logger.LogWarning("Migration permissions Financier : rôle « Financier » introuvable.");
                    return;
                }

                var permissionNoms = GetFinancierRolePermissionNames();
                var allowedPermissions = await context.Permissions
                    .Where(p => permissionNoms.Contains(p.Nom) && p.Statut)
                    .ToListAsync();

                var allowedIds = allowedPermissions.Select(p => p.IdPermission).ToHashSet();

                var added = 0;
                foreach (var permission in allowedPermissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == financierRole.IdRole && rp.PermissionId == permission.IdPermission);

                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = financierRole.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                    added++;
                }

                var excessRolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == financierRole.IdRole && !allowedIds.Contains(rp.PermissionId))
                    .ToListAsync();

                var removed = excessRolePermissions.Count;
                if (removed > 0)
                {
                    context.RolePermissions.RemoveRange(excessRolePermissions);
                }

                if (added > 0 || removed > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration permissions Financier : {Added} ajoutée(s), {Removed} retirée(s) (catalogue attendu : {Total}).",
                        added, removed, allowedPermissions.Count);
                }

                var missing = permissionNoms.Except(allowedPermissions.Select(p => p.Nom)).ToList();
                if (missing.Count > 0)
                {
                    logger.LogWarning(
                        "Permissions financier absentes du catalogue : {Missing}",
                        string.Join(", ", missing));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration des permissions du rôle Financier");
                throw;
            }
        }

        /// <summary>
        /// Permissions du rôle Percepteur — guichet terrain (encaissement, adhésion, perception VA, paiement retrait jeton).
        /// </summary>
        private static IReadOnlyList<string> GetPercepteurRolePermissionNames() => new[]
        {
            // Adhésion & affilié (guichet)
            "CREATE_ADHESION",
            "READ_ADHESION",
            "UPDATE_ADHESION",
            "READ_AFFILIE",
            "UPDATE_AFFILIE",
            // Collectes & transactions
            "CREATE_COLLECTE",
            "READ_COLLECTE",
            "CREATE_TRANSACTION",
            "READ_TRANSACTION",
            "UPDATE_TRANSACTION",
            // Catalogue & tarification
            "READ_FRAIS",
            "READ_DEVISE",
            "READ_PRESTATION",
            "READ_PRODUIT_MUTUEL",
            "READ_PRODUIT_ASSUREUR",
            "READ_SOUSCRIPTION_PRESTATION",
            "READ_TYPE_ADHESION",
            "READ_CATEGORIE_ADHESION",
            "READ_COTISATION_AFFILIE",
            // Contexte agent (affectation collecte / adhésion)
            "READ_AGENT",
            "READ_CATEGORIE_AGENT",
            // Famille (consultation)
            "READ_DEPENDANT",
            // Référentiels géographiques (saisie adresse)
            "READ_PROVINCE",
            "READ_COMMUNE",
            "READ_ZONE_SOCIALE",
            // Notifications
            "READ_NOTIFICATION",
            // Bons d'envoi (workflow guichet)
            "READ_DEMANDE_BON_ENVOI",
            "READ_BON_ENVOI",
            "CONFIRM_DEMANDE_BON_ENVOI",
            // Perception compte virtuel AT
            "READ_PERCEPTION_VIRTUAL",
            "CONFIRM_PERCEPTION_VIRTUAL",
            // Paiement retrait commission agent (jeton) + session caisse terrain
            "OPEN_CAISSIER_SESSION",
            "CLOSE_CAISSIER_SESSION",
            "READ_CAISSIER_SESSION",
            "READ_DEMANDE_RETRAIT_AGENT",
            "VALIDATE_DEMANDE_RETRAIT_AGENT",
            "CONFIRM_RETRAIT_AGENT",
            "MARQUER_PAYER_RETRAIT_AGENT",
            "READ_RETRAIT_AGENT"
        };

        private static IEnumerable<Permission> FilterPermissionsForPercepteurRole(IEnumerable<Permission> allPermissions)
        {
            var noms = GetPercepteurRolePermissionNames();
            return allPermissions.Where(p => noms.Contains(p.Nom));
        }

        /// <summary>
        /// Permissions du rôle Caissier — caissier principal (périmètre Percepteur + supervision guichet).
        /// </summary>
        private static IReadOnlyList<string> GetCaissierRolePermissionNames()
        {
            var noms = new List<string>(GetPercepteurRolePermissionNames())
            {
                // Wallets guichet (réconciliation, mouvements)
                "READ_WALLET_AGENT",
                "UPDATE_WALLET_AGENT",
                "READ_WALLET_MOVEMENT",
                "CREATE_WALLET_MOVEMENT",
                // Obligations membres au guichet
                "READ_ARRIERES_AFFILIE",
                "READ_PENALITE_AFFILIE",
                "READ_ANTECEDENT",
                // Bons d'envoi (validation guichet principal)
                "READ_BON_ENVOI",
                "READ_DEMANDE_BON_ENVOI",
                "CONFIRM_DEMANDE_BON_ENVOI",
                "OPEN_CAISSIER_SESSION",
                "CLOSE_CAISSIER_SESSION",
                "READ_CAISSIER_SESSION",
                "CREATE_DEMANDE_RETRAIT_AGENT",
                "READ_DEMANDE_RETRAIT_AGENT",
                "VALIDATE_DEMANDE_RETRAIT_AGENT",
                "CONFIRM_RETRAIT_AGENT",
                "MARQUER_PAYER_RETRAIT_AGENT",
                "READ_RETRAIT_AGENT",
                // Clôture & rapports
                "GENERATE_RAPPORT",
                "EXPORT_DATA",
                "READ_STATISTIQUES",
                "UPDATE_NOTIFICATION",
                "ACCESS_DASHBOARD_CAISSIER"
            };

            return noms.Distinct(StringComparer.Ordinal).ToList();
        }

        private static async Task EnsureReadStatistiquesPermissionAsync(ProsocDbContext context, ILogger logger)
        {
            const string nom = "READ_STATISTIQUES";
            if (!await context.Permissions.AnyAsync(p => p.Nom == nom))
            {
                var (categorie, action) = ParsePermissionCategorieAndAction(nom);
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = "Consulter les statistiques",
                    Categorie = categorie,
                    Action = action,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                await context.SaveChangesAsync();
                logger.LogInformation("Permission {Permission} créée.", nom);
            }
        }

        private static async Task EnsureReadTargetAgentPermissionAsync(ProsocDbContext context, ILogger logger)
        {
            const string nom = "READ_TARGET_AGENT";
            if (!await context.Permissions.AnyAsync(p => p.Nom == nom))
            {
                var (categorie, action) = ParsePermissionCategorieAndAction(nom);
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = "Voir les objectifs / TargetAgent",
                    Categorie = categorie,
                    Action = action,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                await context.SaveChangesAsync();
                logger.LogInformation("Permission {Permission} créée.", nom);
            }
        }

        private static async Task EnsureCreateTauxChangePermissionAsync(ProsocDbContext context, ILogger logger)
        {
            const string nom = "CREATE_TAUX_CHANGE";
            if (!await context.Permissions.AnyAsync(p => p.Nom == nom))
            {
                var (categorie, action) = ParsePermissionCategorieAndAction(nom);
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = "Créer un taux de change",
                    Categorie = categorie,
                    Action = action,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                await context.SaveChangesAsync();
                logger.LogInformation("Permission {Permission} créée.", nom);
            }
        }

        private static async Task EnsureSouscriptionPrestationWritePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            foreach (var (nom, description) in new[]
            {
                ("UPDATE_SOUSCRIPTION_PRESTATION", "Modifier une souscription prestation"),
                ("DELETE_SOUSCRIPTION_PRESTATION", "Supprimer une souscription prestation")
            })
            {
                if (await context.Permissions.AnyAsync(p => p.Nom == nom))
                    continue;

                var (categorie, action) = ParsePermissionCategorieAndAction(nom);
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = description,
                    Categorie = categorie,
                    Action = action,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                await context.SaveChangesAsync();
                logger.LogInformation("Permission {Permission} créée.", nom);
            }
        }

        private static async Task EnsureParametresMetierPermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            foreach (var (nom, description) in new[]
            {
                ("READ_PARAMETRES_METIER", "Consulter les paramètres métier"),
                ("UPDATE_PARAMETRES_METIER", "Modifier les paramètres métier")
            })
            {
                if (await context.Permissions.AnyAsync(p => p.Nom == nom))
                    continue;

                var (categorie, action) = ParsePermissionCategorieAndAction(nom);
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = description,
                    Categorie = categorie,
                    Action = action,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                await context.SaveChangesAsync();
                logger.LogInformation("Permission {Permission} créée.", nom);
            }

            var permissionNoms = new[] { "READ_PARAMETRES_METIER", "UPDATE_PARAMETRES_METIER" };
            var permissions = await context.Permissions
                .Where(p => permissionNoms.Contains(p.Nom) && p.Statut)
                .ToListAsync();

            foreach (var roleNom in new[] { "Admin", "IT" })
            {
                var role = await context.Roles.FirstOrDefaultAsync(r => r.Nom == roleNom);
                if (role == null)
                    continue;

                foreach (var permission in permissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == role.IdRole && rp.PermissionId == permission.IdPermission);
                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureDashboardCaissierPermissionAsync(ProsocDbContext context, ILogger logger)
        {
            const string nom = "ACCESS_DASHBOARD_CAISSIER";
            if (!await context.Permissions.AnyAsync(p => p.Nom == nom))
            {
                var (categorie, action) = ParsePermissionCategorieAndAction(nom);
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = "Accéder au dashboard caissier",
                    Categorie = categorie,
                    Action = action,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                await context.SaveChangesAsync();
                logger.LogInformation("Permission {Permission} créée.", nom);
            }
        }

        private static async Task EnsureCaisseSessionPermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            var permissions = new (string Nom, string Description)[]
            {
                ("OPEN_CAISSIER_SESSION", "Ouvrir une session de caisse"),
                ("CLOSE_CAISSIER_SESSION", "Clôturer une session de caisse"),
                ("READ_CAISSIER_SESSION", "Consulter session et mouvements de caisse"),
                ("CONFIRM_RETRAIT_AGENT", "Payer un retrait agent au guichet")
            };

            foreach (var (nom, description) in permissions)
            {
                if (await context.Permissions.AnyAsync(p => p.Nom == nom))
                    continue;

                var (categorie, action) = ParsePermissionCategorieAndAction(nom);
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = description,
                    Categorie = categorie,
                    Action = action,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                logger.LogInformation("Permission {Permission} créée.", nom);
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureDemandeRetraitAgentPermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            var permissions = new (string Nom, string Description)[]
            {
                ("CREATE_DEMANDE_RETRAIT_AGENT", "Créer une demande de retrait agent"),
                ("READ_DEMANDE_RETRAIT_AGENT", "Consulter les demandes de retrait agent"),
                ("VALIDATE_DEMANDE_RETRAIT_AGENT", "Valider une demande de retrait agent et générer le jeton"),
                ("MARQUER_PAYER_RETRAIT_AGENT", "Marquer un retrait agent comme payé (jeton)"),
                ("READ_RETRAIT_AGENT", "Accéder au module / menu retraits agent")
            };

            foreach (var (nom, description) in permissions)
            {
                if (await context.Permissions.AnyAsync(p => p.Nom == nom))
                    continue;

                var (categorie, action) = ParsePermissionCategorieAndAction(nom);
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = description,
                    Categorie = categorie,
                    Action = action,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                logger.LogInformation("Permission {Permission} créée.", nom);
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureDemandeRechargeWalletVirtuelPermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            var permissionDefs = new (string Nom, string Description)[]
            {
                ("CREATE_DEMANDE_RECHARGE_WALLET_VIRTUEL", "Créer une demande de recharge wallet virtuel"),
                ("READ_DEMANDE_RECHARGE_WALLET_VIRTUEL", "Consulter les demandes de recharge wallet virtuel"),
                ("CONFIRM_DEMANDE_RECHARGE_WALLET_VIRTUEL", "Confirmer ou rejeter une demande de recharge wallet virtuel")
            };

            foreach (var (nom, description) in permissionDefs)
            {
                if (await context.Permissions.AnyAsync(p => p.Nom == nom))
                    continue;

                var (categorie, action) = ParsePermissionCategorieAndAction(nom);
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = description,
                    Categorie = categorie,
                    Action = action,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                logger.LogInformation("Permission {Permission} créée.", nom);
            }

            await context.SaveChangesAsync();

            var permissionNoms = permissionDefs.Select(p => p.Nom).ToArray();
            var permissions = await context.Permissions
                .Where(p => permissionNoms.Contains(p.Nom) && p.Statut)
                .ToListAsync();

            foreach (var roleNom in new[] { "Admin", "Superviseur" })
            {
                var role = await context.Roles.FirstOrDefaultAsync(r => r.Nom == roleNom);
                if (role == null)
                    continue;

                foreach (var permission in permissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == role.IdRole && rp.PermissionId == permission.IdPermission);
                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureEncodeAdhesionNiveau2PermissionAsync(ProsocDbContext context, ILogger logger)
        {
            const string nom = "ENCODE_ADHESION_NIVEAU_2";
            if (!await context.Permissions.AnyAsync(p => p.Nom == nom))
            {
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = "Encoder / valider le dossier adhésion niveau 2 (encodeur)",
                    Categorie = "ADHESION",
                    Action = "ENCODE",
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                await context.SaveChangesAsync();
                logger.LogInformation("Permission {Permission} créée.", nom);
            }

            var permission = await context.Permissions.FirstAsync(p => p.Nom == nom && p.Statut);
            var aaRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Agent (AA)");
            if (aaRole == null)
                return;

            var exists = await context.RolePermissions.AnyAsync(rp =>
                rp.RoleId == aaRole.IdRole && rp.PermissionId == permission.IdPermission);
            if (exists)
                return;

            context.RolePermissions.Add(new RolePermission
            {
                RoleId = aaRole.IdRole,
                PermissionId = permission.IdPermission,
                DateAttribution = DateTime.Now
            });
            await context.SaveChangesAsync();
            logger.LogInformation("Permission {Permission} attribuée au rôle Agent (AA).", nom);
        }

        private static async Task EnsurePerceptionVirtuellePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            var permissions = new (string Nom, string Description)[]
            {
                ("READ_PERCEPTION_VIRTUAL", "Consulter les collectes compte virtuel à percevoir"),
                ("CONFIRM_PERCEPTION_VIRTUAL", "Confirmer la perception physique des collectes compte virtuel")
            };

            foreach (var (nom, description) in permissions)
            {
                if (await context.Permissions.AnyAsync(p => p.Nom == nom))
                    continue;

                var (categorie, action) = ParsePermissionCategorieAndAction(nom);
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = description,
                    Categorie = categorie,
                    Action = action,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                logger.LogInformation("Permission {Permission} créée.", nom);
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureDashboardSuperAdminPermissionAsync(ProsocDbContext context, ILogger logger)
        {
            const string nom = "ACCESS_DASHBOARD_SUPERADMIN";
            if (!await context.Permissions.AnyAsync(p => p.Nom == nom))
            {
                var (categorie, action) = ParsePermissionCategorieAndAction(nom);
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = "Accéder au dashboard super administrateur",
                    Categorie = categorie,
                    Action = action,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                await context.SaveChangesAsync();
                logger.LogInformation("Permission {Permission} créée.", nom);
            }
        }

        private static async Task EnsureDashboardAssureurPermissionAsync(ProsocDbContext context, ILogger logger)
        {
            const string nom = "ACCESS_DASHBOARD_ASSUREUR";
            if (!await context.Permissions.AnyAsync(p => p.Nom == nom))
            {
                var (categorie, action) = ParsePermissionCategorieAndAction(nom);
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = "Accéder au dashboard assureur",
                    Categorie = categorie,
                    Action = action,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                await context.SaveChangesAsync();
                logger.LogInformation("Permission {Permission} créée.", nom);
            }
        }

        private static async Task EnsureDashboardAgentAaPermissionAsync(ProsocDbContext context, ILogger logger)
        {
            const string nom = "ACCESS_DASHBOARD_AGENT_AA";
            if (!await context.Permissions.AnyAsync(p => p.Nom == nom))
            {
                var (categorie, action) = ParsePermissionCategorieAndAction(nom);
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = "Accéder au dashboard agent administratif (encodeur)",
                    Categorie = categorie,
                    Action = action,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                await context.SaveChangesAsync();
                logger.LogInformation("Permission {Permission} créée.", nom);
            }
        }

        private static async Task EnsureChefEquipePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            var definitions = new[]
            {
                (Nom: "ACCESS_DASHBOARD_CHEF_EQUIPE", Description: "Accéder au dashboard chef d'équipe"),
                (Nom: "READ_EQUIPE_ZONE", Description: "Consulter les agents AT de sa zone"),
                (Nom: "READ_EQUIPE_WALLET_MOVEMENT", Description: "Consulter les mouvements wallet des AT de sa zone"),
                (Nom: "READ_EQUIPE_COLLECTE", Description: "Consulter les collectes des AT de sa zone")
            };

            var created = 0;
            foreach (var (nom, description) in definitions)
            {
                if (await context.Permissions.AnyAsync(p => p.Nom == nom))
                    continue;

                var (categorie, action) = ParsePermissionCategorieAndAction(nom);
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = description,
                    Categorie = categorie,
                    Action = action,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                created++;
            }

            if (created > 0)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Permissions Chef d'équipe : {Count} créée(s).", created);
            }
        }

        private static IEnumerable<Permission> FilterPermissionsForCaissierRole(IEnumerable<Permission> allPermissions)
        {
            var noms = GetCaissierRolePermissionNames();
            return allPermissions.Where(p => noms.Contains(p.Nom));
        }

        /// <summary>
        /// Aligne le rôle Percepteur sur la liste blanche et retire les excès.
        /// </summary>
        private static async Task MigratePercepteurRolePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var percepteurRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Percepteur");
                if (percepteurRole == null)
                {
                    logger.LogWarning("Migration permissions Percepteur : rôle « Percepteur » introuvable.");
                    return;
                }

                var permissionNoms = GetPercepteurRolePermissionNames();
                var allowedPermissions = await context.Permissions
                    .Where(p => permissionNoms.Contains(p.Nom) && p.Statut)
                    .ToListAsync();

                var allowedIds = allowedPermissions.Select(p => p.IdPermission).ToHashSet();

                var added = 0;
                foreach (var permission in allowedPermissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == percepteurRole.IdRole && rp.PermissionId == permission.IdPermission);

                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = percepteurRole.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                    added++;
                }

                var excessRolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == percepteurRole.IdRole && !allowedIds.Contains(rp.PermissionId))
                    .ToListAsync();

                var removed = excessRolePermissions.Count;
                if (removed > 0)
                {
                    context.RolePermissions.RemoveRange(excessRolePermissions);
                }

                if (added > 0 || removed > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration permissions Percepteur : {Added} ajoutée(s), {Removed} retirée(s) (catalogue attendu : {Total}).",
                        added, removed, allowedPermissions.Count);
                }

                var missing = permissionNoms.Except(allowedPermissions.Select(p => p.Nom)).ToList();
                if (missing.Count > 0)
                {
                    logger.LogWarning(
                        "Permissions percepteur absentes du catalogue : {Missing}",
                        string.Join(", ", missing));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration des permissions du rôle Percepteur");
                throw;
            }
        }

        /// <summary>
        /// Aligne le rôle Caissier (principal) sur la liste blanche et retire les excès.
        /// </summary>
        private static async Task MigrateCaissierRolePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var caissierRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Caissier");
                if (caissierRole == null)
                {
                    logger.LogWarning("Migration permissions Caissier : rôle « Caissier » introuvable.");
                    return;
                }

                var permissionNoms = GetCaissierRolePermissionNames();
                var allowedPermissions = await context.Permissions
                    .Where(p => permissionNoms.Contains(p.Nom) && p.Statut)
                    .ToListAsync();

                var allowedIds = allowedPermissions.Select(p => p.IdPermission).ToHashSet();

                var added = 0;
                foreach (var permission in allowedPermissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == caissierRole.IdRole && rp.PermissionId == permission.IdPermission);

                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = caissierRole.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                    added++;
                }

                var excessRolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == caissierRole.IdRole && !allowedIds.Contains(rp.PermissionId))
                    .ToListAsync();

                var removed = excessRolePermissions.Count;
                if (removed > 0)
                {
                    context.RolePermissions.RemoveRange(excessRolePermissions);
                }

                if (added > 0 || removed > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration permissions Caissier : {Added} ajoutée(s), {Removed} retirée(s) (catalogue attendu : {Total}).",
                        added, removed, allowedPermissions.Count);
                }

                var missing = permissionNoms.Except(allowedPermissions.Select(p => p.Nom)).ToList();
                if (missing.Count > 0)
                {
                    logger.LogWarning(
                        "Permissions caissier absentes du catalogue : {Missing}",
                        string.Join(", ", missing));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration des permissions du rôle Caissier");
                throw;
            }
        }

        /// <summary>Définitions des permissions dédiées au workflow affilié (créées si absentes en BDD).</summary>
        private static IReadOnlyList<(string Nom, string Description)> GetAffilieWorkflowPermissionDefinitions() => new[]
        {
            ("ACCESS_DASHBOARD_AFFILIE", "Accéder au dashboard affilié"),
            ("READ_COTISATION_AFFILIE", "Consulter les cotisations affilié"),
            ("READ_ARRIERES_AFFILIE", "Consulter ses arriérés de paiement"),
            ("READ_PENALITE_AFFILIE", "Consulter ses pénalités de retard"),
            ("PAIEMENT_AFFILIE", "Payer cotisations et souscriptions"),
            ("READ_SOUSCRIPTION_PRESTATION", "Consulter ses souscriptions prestation"),
            ("CREATE_DEMANDE_BON_ENVOI", "Demander un bon d'envoi"),
            ("READ_DEMANDE_BON_ENVOI", "Consulter ses demandes de bon d'envoi"),
            ("READ_JETON_MEDICAL", "Consulter ses jetons médicaux")
        };

        /// <summary>Permissions du rôle Affilié — espace membre (profil, famille, paiements, soins).</summary>
        /// <remarks>CREATE_AFFILIE est volontairement exclue: la création d'affilié passe par le flux d'adhésion.</remarks>
        private static IReadOnlyList<string> GetAffilieRolePermissionNames() => new[]
        {
            // Profil (lecture via /api/Affilie/mon-profil ; pas de liste READ_AFFILIE / READ_ADHESION)
            "UPDATE_AFFILIE",
            // Famille & santé
            "READ_DEPENDANT",
            // CREATE/UPDATE dependant retirés pour le rôle affilié
            "READ_ANTECEDENT",
            // CREATE/UPDATE antécédent retirés pour le rôle affilié
            // Catalogue (lecture)
            "READ_PRODUIT_MUTUEL",
            "READ_PRODUIT_ASSUREUR",
            "READ_PRESTATION",
            "READ_TYPE_ADHESION",
            "READ_CATEGORIE_ADHESION",
            "READ_DEVISE",
            "READ_FRAIS",
            "READ_PROVINCE",
            "READ_COMMUNE",
            // Cotisations, obligations, paiements
            "READ_COTISATION_AFFILIE",
            "READ_ARRIERES_AFFILIE",
            "READ_PENALITE_AFFILIE",
            "READ_SOUSCRIPTION_PRESTATION",
            "READ_COLLECTE",
            "CREATE_COLLECTE",
            "READ_TRANSACTION",
            "PAIEMENT_AFFILIE",
            // Soins & bons (demande côté membre ; émission réservée aux agents)
            "READ_BON_ENVOI",
            "CREATE_DEMANDE_BON_ENVOI",
            "READ_DEMANDE_BON_ENVOI",
            "READ_JETON_MEDICAL",
            // Notifications & tableau de bord
            "READ_NOTIFICATION",
            // UPDATE_NOTIFICATION retiré pour le rôle affilié
            "ACCESS_DASHBOARD_AFFILIE"
        };

        private static async Task EnsureAffilieWorkflowPermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            var created = 0;
            foreach (var (nom, description) in GetAffilieWorkflowPermissionDefinitions())
            {
                if (await context.Permissions.AnyAsync(p => p.Nom == nom))
                    continue;

                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = description,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                created++;
            }

            if (created > 0)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Permissions workflow affilié : {Count} créée(s).", created);
            }
        }

        private static IEnumerable<Permission> FilterPermissionsForAffilieRole(IEnumerable<Permission> allPermissions)
        {
            var noms = GetAffilieRolePermissionNames();
            return allPermissions.Where(p => noms.Contains(p.Nom));
        }

        /// <summary>
        /// Permissions du rôle Assureur — portail partenaire (lecture réseau assurance, rapports).
        /// </summary>
        private static IReadOnlyList<string> GetAssureurRolePermissionNames() => new[]
        {
            // Organisation & catalogue partenaire
            "READ_ASSUREUR",
            "READ_PRODUIT_ASSUREUR",
            "READ_PRESTATION",
            // Souscriptions & flux financiers liés aux produits assureur
            "READ_SOUSCRIPTION_PRESTATION",
            "READ_COLLECTE",
            "READ_TRANSACTION",
            // Dossiers membres (prise en charge)
            "READ_AFFILIE",
            "READ_ADHESION",
            "READ_DEPENDANT",
            "READ_ANTECEDENT",
            // Workflow soins (consultation réseau assurance)
            "READ_BON_ENVOI",
            "READ_DEMANDE_BON_ENVOI",
            "READ_JETON_MEDICAL",
            // Référentiels (filtres / contexte)
            "READ_DEVISE",
            "READ_FRAIS",
            "READ_TYPE_ADHESION",
            "READ_CATEGORIE_ADHESION",
            "READ_PROVINCE",
            "READ_COMMUNE",
            // Rapports partenaire
            "GENERATE_RAPPORT",
            "EXPORT_DATA",
            // Notifications
            "READ_NOTIFICATION",
            "ACCESS_DASHBOARD_ASSUREUR"
        };

        private static IEnumerable<Permission> FilterPermissionsForAssureurRole(IEnumerable<Permission> allPermissions)
        {
            var noms = GetAssureurRolePermissionNames();
            return allPermissions.Where(p => noms.Contains(p.Nom));
        }

        private static async Task<Role?> EnsureAssureurRoleAsync(ProsocDbContext context, ILogger logger)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Assureur");
            if (role != null)
                return role;

            role = new Role
            {
                Nom = "Assureur",
                Code = "AS",
                Description = "Partenaire assureur",
                Niveau = 10,
                Statut = true,
                DateCreation = DateTime.Now
            };
            context.Roles.Add(role);
            await context.SaveChangesAsync();
            logger.LogInformation("Rôle « Assureur » créé (IdRole = {RoleId}).", role.IdRole);
            return role;
        }

        /// <summary>
        /// Aligne le rôle Assureur sur la liste blanche et retire les permissions hors périmètre.
        /// </summary>
        private static async Task MigrateAssureurRolePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var assureurRole = await EnsureAssureurRoleAsync(context, logger);
                if (assureurRole == null)
                {
                    logger.LogWarning("Migration permissions Assureur : rôle introuvable.");
                    return;
                }

                var permissionNoms = GetAssureurRolePermissionNames();
                var allowedPermissions = await context.Permissions
                    .Where(p => permissionNoms.Contains(p.Nom) && p.Statut)
                    .ToListAsync();

                var allowedIds = allowedPermissions.Select(p => p.IdPermission).ToHashSet();

                var added = 0;
                foreach (var permission in allowedPermissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == assureurRole.IdRole && rp.PermissionId == permission.IdPermission);

                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = assureurRole.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                    added++;
                }

                var excessRolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == assureurRole.IdRole && !allowedIds.Contains(rp.PermissionId))
                    .ToListAsync();

                var removed = excessRolePermissions.Count;
                if (removed > 0)
                {
                    context.RolePermissions.RemoveRange(excessRolePermissions);
                }

                if (added > 0 || removed > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration permissions Assureur : {Added} ajoutée(s), {Removed} retirée(s) (catalogue attendu : {Total}).",
                        added, removed, allowedPermissions.Count);
                }

                var missing = permissionNoms.Except(allowedPermissions.Select(p => p.Nom)).ToList();
                if (missing.Count > 0)
                {
                    logger.LogWarning(
                        "Permissions assureur absentes du catalogue : {Missing}",
                        string.Join(", ", missing));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration des permissions du rôle Assureur");
                throw;
            }
        }

        /// <summary>Définitions des permissions dédiées au portail hôpital (créées si absentes en BDD).</summary>
        private static IReadOnlyList<(string Nom, string Description)> GetAgentHopitalWorkflowPermissionDefinitions() => new[]
        {
            ("USE_JETON_MEDICAL", "Valider et utiliser un jeton médical"),
            ("READ_HOPITAL_PARTENAIRE", "Consulter les hôpitaux partenaires"),
            ("ACCESS_DASHBOARD_HOPITAL", "Accéder au dashboard hôpital")
        };

        /// <summary>Permissions du rôle Agent Hôpital — accueil partenaire (scan bon, jetons, dossier patient).</summary>
        private static IReadOnlyList<string> GetAgentHopitalRolePermissionNames() => new[]
        {
            "SCAN_BON_ENVOI",
            "READ_BON_ENVOI",
            "READ_DEMANDE_BON_ENVOI",
            "READ_JETON_MEDICAL",
            "USE_JETON_MEDICAL",
            "READ_AFFILIE",
            "READ_ADHESION",
            "READ_DEPENDANT",
            "READ_ANTECEDENT",
            "READ_PRESTATION",
            "READ_PRODUIT_MUTUEL",
            "READ_PRODUIT_ASSUREUR",
            "READ_HOPITAL_PARTENAIRE",
            "ACCESS_DASHBOARD_HOPITAL",
            "READ_NOTIFICATION",
            "UPDATE_NOTIFICATION"
        };

        private static IEnumerable<Permission> FilterPermissionsForAgentHopitalRole(IEnumerable<Permission> allPermissions)
        {
            var noms = GetAgentHopitalRolePermissionNames();
            return allPermissions.Where(p => noms.Contains(p.Nom));
        }

        private static async Task EnsureAgentHopitalWorkflowPermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            var created = 0;
            foreach (var (nom, description) in GetAgentHopitalWorkflowPermissionDefinitions())
            {
                if (await context.Permissions.AnyAsync(p => p.Nom == nom))
                    continue;

                var (categorie, action) = ParsePermissionCategorieAndAction(nom);
                context.Permissions.Add(new Permission
                {
                    Nom = nom,
                    Description = description,
                    Categorie = categorie,
                    Action = action,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                created++;
            }

            if (created > 0)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Permissions workflow Agent Hôpital : {Count} créée(s).", created);
            }
        }

        /// <summary>Ex. READ_AFFILIE → (AFFILIE, READ), ACCESS_DASHBOARD_HOPITAL → (DASHBOARD_HOPITAL, ACCESS).</summary>
        private static (string Categorie, string Action) ParsePermissionCategorieAndAction(string nom)
        {
            var separator = nom.IndexOf('_');
            if (separator <= 0)
                return (nom, nom);

            return (nom[(separator + 1)..], nom[..separator]);
        }

        private static async Task<Role?> EnsureAgentHopitalRoleAsync(ProsocDbContext context, ILogger logger)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Agent Hôpital");
            if (role != null)
                return role;

            role = new Role
            {
                Nom = "Agent Hôpital",
                Code = "AH",
                Description = "Personnel accueil hôpital",
                Niveau = 11,
                Statut = true,
                DateCreation = DateTime.Now
            };
            context.Roles.Add(role);
            await context.SaveChangesAsync();
            logger.LogInformation("Rôle « Agent Hôpital » créé (IdRole = {RoleId}).", role.IdRole);
            return role;
        }

        /// <summary>
        /// Aligne le rôle Agent Hôpital sur la liste blanche et retire les permissions hors périmètre.
        /// </summary>
        private static async Task MigrateAgentHopitalRolePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var agentHopitalRole = await EnsureAgentHopitalRoleAsync(context, logger);
                if (agentHopitalRole == null)
                {
                    logger.LogWarning("Migration permissions Agent Hôpital : rôle introuvable.");
                    return;
                }

                await EnsureAgentHopitalWorkflowPermissionsAsync(context, logger);

                var permissionNoms = GetAgentHopitalRolePermissionNames();
                var allowedPermissions = await context.Permissions
                    .Where(p => permissionNoms.Contains(p.Nom) && p.Statut)
                    .ToListAsync();

                var allowedIds = allowedPermissions.Select(p => p.IdPermission).ToHashSet();

                var added = 0;
                foreach (var permission in allowedPermissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == agentHopitalRole.IdRole && rp.PermissionId == permission.IdPermission);

                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = agentHopitalRole.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                    added++;
                }

                var excessRolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == agentHopitalRole.IdRole && !allowedIds.Contains(rp.PermissionId))
                    .ToListAsync();

                var removed = excessRolePermissions.Count;
                if (removed > 0)
                {
                    context.RolePermissions.RemoveRange(excessRolePermissions);
                }

                if (added > 0 || removed > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration permissions Agent Hôpital : {Added} ajoutée(s), {Removed} retirée(s) (catalogue attendu : {Total}).",
                        added, removed, allowedPermissions.Count);
                }

                var missing = permissionNoms.Except(allowedPermissions.Select(p => p.Nom)).ToList();
                if (missing.Count > 0)
                {
                    logger.LogWarning(
                        "Permissions Agent Hôpital absentes du catalogue : {Missing}",
                        string.Join(", ", missing));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration des permissions du rôle Agent Hôpital");
                throw;
            }
        }

        /// <summary>
        /// Aligne le rôle Affilié sur la liste blanche et retire les permissions hors périmètre.
        /// </summary>
        private static async Task MigrateAffilieRolePermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var affilieRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Affilié");
                if (affilieRole == null)
                {
                    logger.LogWarning("Migration permissions Affilié : rôle « Affilié » introuvable.");
                    return;
                }

                await EnsureAffilieWorkflowPermissionsAsync(context, logger);

                var permissionNoms = GetAffilieRolePermissionNames();
                var allowedPermissions = await context.Permissions
                    .Where(p => permissionNoms.Contains(p.Nom) && p.Statut)
                    .ToListAsync();

                var allowedIds = allowedPermissions.Select(p => p.IdPermission).ToHashSet();

                var added = 0;
                foreach (var permission in allowedPermissions)
                {
                    var exists = await context.RolePermissions.AnyAsync(rp =>
                        rp.RoleId == affilieRole.IdRole && rp.PermissionId == permission.IdPermission);

                    if (exists)
                        continue;

                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = affilieRole.IdRole,
                        PermissionId = permission.IdPermission,
                        DateAttribution = DateTime.Now
                    });
                    added++;
                }

                var excessRolePermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == affilieRole.IdRole && !allowedIds.Contains(rp.PermissionId))
                    .ToListAsync();

                var removed = excessRolePermissions.Count;
                if (removed > 0)
                {
                    context.RolePermissions.RemoveRange(excessRolePermissions);
                }

                if (added > 0 || removed > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration permissions Affilié : {Added} ajoutée(s), {Removed} retirée(s) (catalogue attendu : {Total}).",
                        added, removed, allowedPermissions.Count);
                }

                var missing = permissionNoms.Except(allowedPermissions.Select(p => p.Nom)).ToList();
                if (missing.Count > 0)
                {
                    logger.LogWarning(
                        "Permissions affilié absentes du catalogue : {Missing}",
                        string.Join(", ", missing));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration des permissions du rôle Affilié");
                throw;
            }
        }

        private static readonly string[] RetiredAffiliePermissionNames = { "CREATE_AFFILIE", "DELETE_AFFILIE" };

        /// <summary>
        /// Désactive CREATE_AFFILIE et DELETE_AFFILIE (création affilié via flux adhésion uniquement).
        /// </summary>
        private static async Task RetireObsoleteAffilieCrudPermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                var permissions = await context.Permissions
                    .Where(p => RetiredAffiliePermissionNames.Contains(p.Nom))
                    .ToListAsync();

                if (permissions.Count == 0)
                    return;

                var permissionIds = permissions.Select(p => p.IdPermission).ToList();

                var rolePermissions = await context.RolePermissions
                    .Where(rp => permissionIds.Contains(rp.PermissionId))
                    .ToListAsync();

                if (rolePermissions.Count > 0)
                    context.RolePermissions.RemoveRange(rolePermissions);

                var userPermissions = await context.UserPermissions
                    .Where(up => permissionIds.Contains(up.PermissionId))
                    .ToListAsync();

                if (userPermissions.Count > 0)
                    context.UserPermissions.RemoveRange(userPermissions);

                foreach (var permission in permissions)
                    permission.Statut = false;

                await context.SaveChangesAsync();

                logger.LogInformation(
                    "Permissions affilié obsolètes retirées : {RemovedRole} attribution(s) rôle, {RemovedUser} attribution(s) utilisateur, permissions désactivées : {Names}",
                    rolePermissions.Count,
                    userPermissions.Count,
                    string.Join(", ", RetiredAffiliePermissionNames));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors du retrait des permissions CREATE_AFFILIE / DELETE_AFFILIE");
                throw;
            }
        }

        /// <summary>
        /// Migration des permissions DEPENDANT et ASSUREUR
        /// </summary>
        private static async Task MigrateDependantAssureurPermissionsAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("Début de la migration des permissions DEPENDANT/ASSUREUR");

                // Étape 1: Ajouter les permissions manquantes
                var dependantAssureurPermissions = new[]
                {
                    new Permission { Nom = "CREATE_DEPENDANT", Description = "Créer un dépendant", Categorie = "DEPENDANT", Action = "CREATE", Statut = true, DateCreation = DateTime.Now },
                    new Permission { Nom = "READ_DEPENDANT", Description = "Voir les dépendants", Categorie = "DEPENDANT", Action = "READ", Statut = true, DateCreation = DateTime.Now },
                    new Permission { Nom = "UPDATE_DEPENDANT", Description = "Modifier un dépendant", Categorie = "DEPENDANT", Action = "UPDATE", Statut = true, DateCreation = DateTime.Now },
                    new Permission { Nom = "DELETE_DEPENDANT", Description = "Supprimer un dépendant", Categorie = "DEPENDANT", Action = "DELETE", Statut = true, DateCreation = DateTime.Now },
                    new Permission { Nom = "CREATE_ASSUREUR", Description = "Créer un assureur", Categorie = "ASSUREUR", Action = "CREATE", Statut = true, DateCreation = DateTime.Now },
                    new Permission { Nom = "READ_ASSUREUR", Description = "Voir les assureurs", Categorie = "ASSUREUR", Action = "READ", Statut = true, DateCreation = DateTime.Now },
                    new Permission { Nom = "UPDATE_ASSUREUR", Description = "Modifier un assureur", Categorie = "ASSUREUR", Action = "UPDATE", Statut = true, DateCreation = DateTime.Now },
                    new Permission { Nom = "DELETE_ASSUREUR", Description = "Supprimer un assureur", Categorie = "ASSUREUR", Action = "DELETE", Statut = true, DateCreation = DateTime.Now }
                };

                foreach (var permission in dependantAssureurPermissions)
                {
                    var existing = await context.Permissions
                        .FirstOrDefaultAsync(p => p.Nom == permission.Nom);

                    if (existing == null)
                    {
                        context.Permissions.Add(permission);
                        logger.LogInformation("Permission ajoutée: {Permission}", permission.Nom);
                    }
                }

                await context.SaveChangesAsync();

                // Étape 2: Attribuer les permissions aux rôles cibles (hors AT / Superviseur — listes blanches dédiées)
                var targetRoles = await context.Roles
                    .Where(r => r.Nom == "IT" || r.Nom == "Agent (AA)")
                    .ToListAsync();

                var targetPermissions = await context.Permissions
                    .Where(p => p.Nom.Contains("DEPENDANT") || p.Nom.Contains("ASSUREUR") || p.Nom.Contains("ANTECEDENT"))
                    .ToListAsync();

                foreach (var role in targetRoles)
                {
                    foreach (var permission in targetPermissions)
                    {
                        var existing = await context.RolePermissions
                            .FirstOrDefaultAsync(rp => rp.RoleId == role.IdRole && rp.PermissionId == permission.IdPermission);

                        if (existing == null)
                        {
                            var rolePermission = new RolePermission
                            {
                                RoleId = role.IdRole,
                                PermissionId = permission.IdPermission,
                                DateAttribution = DateTime.Now
                            };

                            context.RolePermissions.Add(rolePermission);
                            logger.LogInformation("Permission '{Permission}' attribuée au rôle '{Role}'", permission.Nom, role.Nom);
                        }
                    }
                }

                await context.SaveChangesAsync();

                // Étape 3: Vérification finale
                var finalCount = await context.RolePermissions
                    .Include(rp => rp.Role)
                    .Include(rp => rp.Permission)
                    .Where(rp => (rp.Role!.Nom == "IT" || rp.Role!.Nom == "Agent (AA)") &&
                                  (rp.Permission!.Nom.Contains("DEPENDANT") || rp.Permission!.Nom.Contains("ASSUREUR")))
                    .CountAsync();

                logger.LogInformation("Migration terminée: {Count} permissions DEPENDANT/ASSUREUR trouvées", finalCount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration des permissions DEPENDANT/ASSUREUR");
                throw;
            }
        }

        private static async Task SeedDemoAsync(ProsocDbContext context, ILogger logger)
        {
            var gombeNord = await context.ZonesSociales.FirstOrDefaultAsync(z => z.Nom == "Gombe-Nord");
            var categorieAt = await context.CategoriesAgents.FirstOrDefaultAsync(c => c.Code == "AT");

            if (gombeNord == null || categorieAt == null)
            {
                return;
            }

            var exists = await context.Agents.AnyAsync(a => a.Matricule == "AG003" || a.EmailAgent == "jk@prosoc.cd");
            if (exists)
            {
                return;
            }

            var agent = new Agent
            {
                NomComplet = "Jonathan Kalambayi",
                Phone = "+243812726582",
                EmailAgent = "jk@prosoc.cd",
                Statut = true,
                DateCreation = DateTime.Now,
                CategorieAgentId = categorieAt.IdCategorieAgent,
                ZoneSocialeId = gombeNord.IdZoneSociale,
                Matricule = "AG003"
            };

            await context.Agents.AddAsync(agent);
            await context.SaveChangesAsync();
            logger.LogInformation("Demo seed: Agent créé: {Matricule}", agent.Matricule);
        }

        /// <summary>Objectifs KPI par rôle applicatif (workflow AT : 5 / 25 / 100 adhésions).</summary>
        private static async Task SeedTargetAgentsAsync(ProsocDbContext context, ILogger logger)
        {
            var atRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Agent (AT)");
            if (atRole == null)
                return;

            var defaults = new[]
            {
                (PeriodiciteTarget.Journaliere, "Objectif adhésions AT — journalier", 5),
                (PeriodiciteTarget.Hebdomadaire, "Objectif adhésions AT — hebdomadaire", 25),
                (PeriodiciteTarget.Mensuelle, "Objectif adhésions AT — mensuel", 100)
            };

            var added = 0;
            foreach (var (periodicite, libelle, nombre) in defaults)
            {
                var exists = await context.TargetsAgents.AnyAsync(t =>
                    t.RoleId == atRole.IdRole && t.Periodicite == periodicite && t.Statut);
                if (exists)
                    continue;

                await context.TargetsAgents.AddAsync(new TargetAgent
                {
                    RoleId = atRole.IdRole,
                    LibelleTarget = libelle,
                    Periodicite = periodicite,
                    Nombre = nombre,
                    Statut = true,
                    DateCreation = DateTime.Now
                });
                added++;
            }

            if (added > 0)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Objectifs TargetAgent AT seedés: {Count}", added);
            }
        }

        /// <summary>
        /// Aligne Code + LibelleCategorie affiché (« Description (CODE) ») pour toutes les catégories agents.
        /// </summary>
        private static async Task MigrateCategorieAgentLibellesAsync(ProsocDbContext context, ILogger logger)
        {
            try
            {
                await RemoveObsoleteCategorieAgentSpAsync(context, logger);

                var standardDescriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["AT"] = "Agent de Terrain",
                    ["AA"] = "Agent Administratif",
                    ["AP"] = "Agent Percepteur",
                    ["AS"] = "Agent Superviseur",
                    ["CA"] = "Caissier",
                    ["AH"] = "Agent Hôpital",
                    ["FI"] = "Financier",
                    ["IT"] = "Technicien",
                    ["AD"] = "Admin"
                };

                var categories = await context.CategoriesAgents.ToListAsync();
                var updated = 0;

                foreach (var categorie in categories)
                {
                    var code = !string.IsNullOrWhiteSpace(categorie.Code)
                        ? categorie.Code.Trim().ToUpperInvariant()
                        : CategorieAgentLibelleHelper.ExtractCodeFromLibelle(categorie.LibelleCategorie)
                          ?? categorie.LibelleCategorie.Trim().ToUpperInvariant();

                    var description = !string.IsNullOrWhiteSpace(categorie.Description)
                        ? categorie.Description.Trim()
                        : standardDescriptions.GetValueOrDefault(code) ?? code;

                    var libelle = CategorieAgentLibelleHelper.BuildLibelle(description, code);

                    if (string.Equals(categorie.Code, code, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(categorie.Description, description, StringComparison.Ordinal)
                        && string.Equals(categorie.LibelleCategorie, libelle, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    categorie.Code = code;
                    categorie.Description = description;
                    categorie.LibelleCategorie = libelle;
                    categorie.DateModification = DateTime.Now;
                    updated++;
                }

                if (updated > 0)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(
                        "Migration catégories agents : {Count} libellé(s) aligné(s) sur le format « Description (CODE) ».",
                        updated);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la migration des libellés CategorieAgent");
                throw;
            }
        }

        /// <summary>
        /// Retire la catégorie agent SP (« Super Admin ») : le super admin est un rôle applicatif (AD), pas une catégorie.
        /// </summary>
        private static async Task RemoveObsoleteCategorieAgentSpAsync(ProsocDbContext context, ILogger logger)
        {
            var categorieSp = await context.CategoriesAgents
                .Include(c => c.Agents)
                .FirstOrDefaultAsync(c =>
                    c.Code == "SP"
                    || c.LibelleCategorie == "Super Admin (SP)"
                    || c.LibelleCategorie == "SP");

            if (categorieSp == null)
                return;

            if (categorieSp.Agents.Any())
            {
                var categorieAd = await context.CategoriesAgents.FirstOrDefaultAsync(c => c.Code == "AD");
                if (categorieAd == null)
                {
                    logger.LogWarning(
                        "Catégorie agent SP (Id={Id}) non supprimée : {Count} agent(s) lié(s) et catégorie AD introuvable.",
                        categorieSp.IdCategorieAgent,
                        categorieSp.Agents.Count);
                    return;
                }

                foreach (var agent in categorieSp.Agents)
                    agent.CategorieAgentId = categorieAd.IdCategorieAgent;

                logger.LogInformation(
                    "{Count} agent(s) réaffecté(s) de la catégorie SP vers AD avant suppression.",
                    categorieSp.Agents.Count);
            }

            context.CategoriesAgents.Remove(categorieSp);
            await context.SaveChangesAsync();
            logger.LogInformation("Catégorie agent « Super Admin (SP) » retirée.");
        }
    }
}
