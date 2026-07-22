using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public interface IMatriculeGeneratorService
    {
        Task<string> GenerateMatriculeAsync(int categorieAgentId, CancellationToken ct = default);
    }

    public class MatriculeGeneratorService : IMatriculeGeneratorService
    {
        private readonly ProsocDbContext _db;
        private readonly Random _random = new Random();

        public MatriculeGeneratorService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<string> GenerateMatriculeAsync(int categorieAgentId, CancellationToken ct = default)
        {
            // Récupérer la catégorie pour obtenir le libellé
            var categorie = await _db.CategoriesAgents
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCategorieAgent == categorieAgentId, ct);

            if (categorie == null)
                throw new ArgumentException($"CatégorieAgent avec ID {categorieAgentId} introuvable");

            // Préfixe matricule : code court (AT, FI, …)
            var code = !string.IsNullOrWhiteSpace(categorie.Code)
                ? categorie.Code
                : categorie.LibelleCategorie;

            var prefix = code.Length >= 2
                ? code.Substring(0, 2).ToUpperInvariant()
                : code.ToUpperInvariant().PadRight(2, 'X');

            // Générer un matricule unique
            const int maxAttempts = 10;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var suffix = GenerateRandomDigits(9);
                var matricule = $"{prefix}{suffix}";

                // Vérifier l'unicité en base
                var exists = await _db.Agents
                    .AsNoTracking()
                    .AnyAsync(a => a.Matricule == matricule, ct);

                if (!exists)
                    return matricule;
            }

            // Si après 10 tentatives on n'a pas trouvé de matricule unique
            throw new InvalidOperationException($"Impossible de générer un matricule unique pour la catégorie {categorieAgentId} après {maxAttempts} tentatives");
        }

        private string GenerateRandomDigits(int length)
        {
            var digits = new char[length];
            for (int i = 0; i < length; i++)
            {
                digits[i] = (char)('0' + _random.Next(0, 10));
            }
            return new string(digits);
        }
    }
}
