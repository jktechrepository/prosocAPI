using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;
using Xunit;

namespace Prosoc.Tests.Unit.Services
{
    public class MatriculeGeneratorServiceTests
    {
        private static async Task<ProsocDbContext> CreateDbContextAsync(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<ProsocDbContext>()
                .UseSqlite(connection)
                .Options;

            var db = new ProsocDbContext(options);
            await db.Database.EnsureCreatedAsync();
            return db;
        }

        [Fact]
        public async Task GenerateMatriculeAsync_WithValidCategorieAgentId_ReturnsCorrectFormat()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            await using var db = await CreateDbContextAsync(connection);

            // Arrange
            var categorieId = 1;
            db.CategoriesAgents.Add(new CategorieAgent
            {
                IdCategorieAgent = categorieId,
                Code = "AG",
                LibelleCategorie = "Agent (AG)",
                Statut = true
            });
            await db.SaveChangesAsync();

            var service = new MatriculeGeneratorService(db);

            // Act
            var result = await service.GenerateMatriculeAsync(categorieId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(11, result.Length); // 2 caractères + 9 chiffres
            Assert.StartsWith("AG", result); // "Agent" -> "AG"
            Assert.True(result.Substring(2).All(char.IsDigit)); // Les 9 derniers caractères sont des chiffres
        }

        [Fact]
        public async Task GenerateMatriculeAsync_WithShortLibelle_ReturnsPaddedPrefix()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            await using var db = await CreateDbContextAsync(connection);

            // Arrange
            var categorieId = 1;
            db.CategoriesAgents.Add(new CategorieAgent
            {
                IdCategorieAgent = categorieId,
                Code = "A",
                LibelleCategorie = "A (A)",
                Statut = true
            });
            await db.SaveChangesAsync();

            var service = new MatriculeGeneratorService(db);

            // Act
            var result = await service.GenerateMatriculeAsync(categorieId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(11, result.Length);
            Assert.StartsWith("AX", result); // "A" -> "AX" (padded avec X)
        }

        [Fact]
        public async Task GenerateMatriculeAsync_WithNonExistentCategorieAgent_ThrowsArgumentException()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            await using var db = await CreateDbContextAsync(connection);

            // Arrange
            var categorieId = 999;
            var service = new MatriculeGeneratorService(db);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.GenerateMatriculeAsync(categorieId));
            
            Assert.Contains($"CatégorieAgent avec ID {categorieId} introuvable", exception.Message);
        }

        [Fact]
        public async Task GenerateMatriculeAsync_WithExistingMatricule_GeneratesUniqueOne()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            await using var db = await CreateDbContextAsync(connection);

            // Arrange
            var categorieId = 1;
            db.CategoriesAgents.Add(new CategorieAgent
            {
                IdCategorieAgent = categorieId,
                Code = "AG",
                LibelleCategorie = "Agent (AG)",
                Statut = true
            });
            db.Agents.Add(new Agent { NomComplet = "Existing", Matricule = "AG123456789", Phone = "000", Statut = true, DateCreation = DateTime.Now });
            await db.SaveChangesAsync();

            var service = new MatriculeGeneratorService(db);

            // Act
            var result = await service.GenerateMatriculeAsync(categorieId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(11, result.Length);
            Assert.StartsWith("AG", result);
            Assert.NotEqual("AG123456789", result); // Ne doit pas retourner le matricule existant
        }
    }
}
