using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Prosoc.Data;

namespace Prosoc
{
    public class ApplyMigrationTemp
    {
        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var services = new ServiceCollection();
            services.AddDbContext<ProsocDbContext>(options =>
                options.UseMySql(
                    configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(10, 6))));

            using var serviceProvider = services.BuildServiceProvider();
            using var context = serviceProvider.GetRequiredService<ProsocDbContext>();

            try
            {
                // Marquer la migration problématique comme appliquée
                var historyCheck = await context.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) 
                    VALUES ('20260226162138_AddCompletePermissionsSystem', '6.0.25')
                    ON DUPLICATE KEY UPDATE `MigrationId` = `MigrationId`");

                Console.WriteLine("Migration problématique marquée comme appliquée");

                // Appliquer notre migration CategorieAgent
                await context.Database.ExecuteSqlRawAsync(@"
                    ALTER TABLE `Agents` ADD COLUMN `CategorieAgentId` int NULL");

                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE `CategoriesAgents` (
                        `IdCategorieAgent` int NOT NULL AUTO_INCREMENT,
                        `LibelleCategorie` longtext CHARACTER SET utf8mb4 NOT NULL,
                        `Description` longtext CHARACTER SET utf8mb4 NULL,
                        `Statut` tinyint(1) NOT NULL,
                        CONSTRAINT `PK_CategoriesAgents` PRIMARY KEY (`IdCategorieAgent`)
                    ) CHARACTER SET=utf8mb4");

                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE INDEX `IX_Agents_CategorieAgentId` ON `Agents` (`CategorieAgentId`)");

                await context.Database.ExecuteSqlRawAsync(@"
                    ALTER TABLE `Agents` ADD CONSTRAINT `FK_Agents_CategoriesAgents_CategorieAgentId` 
                    FOREIGN KEY (`CategorieAgentId`) REFERENCES `CategoriesAgents` (`IdCategorieAgent`)");

                // Marquer notre migration comme appliquée
                await context.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) 
                    VALUES ('20260227161356_AddCategorieAgentFinal', '6.0.25')
                    ON DUPLICATE KEY UPDATE `MigrationId` = `MigrationId`");

                Console.WriteLine("Migration CategorieAgent appliquée avec succès !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
                Console.WriteLine($"Détails: {ex.InnerException?.Message}");
            }
        }
    }
}
