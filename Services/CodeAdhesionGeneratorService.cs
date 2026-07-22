using System.Data;
using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;

namespace ProsocAPI.Services
{
    public interface ICodeAdhesionGeneratorService
    {
        Task<string> GenerateCodeAdhesionAsync(string prefix, CancellationToken ct = default);
    }

    public class CodeAdhesionGeneratorService : ICodeAdhesionGeneratorService
    {
        private readonly ProsocDbContext _db;

        public CodeAdhesionGeneratorService(ProsocDbContext db)
        {
            _db = db;
        }

        public async Task<string> GenerateCodeAdhesionAsync(string prefix, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Le préfixe CodeAdhesion est requis", nameof(prefix));

            prefix = prefix.Trim();

            const int maxPerLetters = 9999;
            const int maxLetters = 26 * 26;
            const int maxValue = maxLetters * maxPerLetters;

            var nextValue = await GetNextSequenceValueAsync(prefix, ct);
            if (nextValue < 1 || nextValue > maxValue)
                throw new InvalidOperationException($"La séquence CodeAdhesion pour le préfixe '{prefix}' a dépassé la capacité maximale ({maxValue}).");

            var suffix = ToAlphaNumericSuffix(nextValue, maxPerLetters);
            return $"{prefix}{suffix}";
        }

        private async Task<int> GetNextSequenceValueAsync(string prefix, CancellationToken ct)
        {
            if ((_db.Database.ProviderName ?? string.Empty).Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
                return await GetNextSequenceValueSqliteAsync(prefix, ct);

            // Approche EF pure pour rester dans la transaction
            var existing = await _db.CodesAdhesionSequences
                .FirstOrDefaultAsync(x => x.Prefix == prefix, ct);

            if (existing == null)
            {
                _db.CodesAdhesionSequences.Add(new Models.Core.CodeAdhesionSequence
                {
                    Prefix = prefix,
                    NextValue = 1,
                    DateCreation = DateTime.Now
                });
                await _db.SaveChangesAsync(ct);
                return 1;
            }

            existing.NextValue += 1;
            existing.DateModification = DateTime.Now;
            await _db.SaveChangesAsync(ct);
            return existing.NextValue;
        }

        private async Task<int> GetNextSequenceValueSqliteAsync(string prefix, CancellationToken ct)
        {
            var existing = await _db.CodesAdhesionSequences
                .FirstOrDefaultAsync(x => x.Prefix == prefix, ct);

            if (existing == null)
            {
                _db.CodesAdhesionSequences.Add(new Models.Core.CodeAdhesionSequence
                {
                    Prefix = prefix,
                    NextValue = 1,
                    DateCreation = DateTime.Now
                });

                await _db.SaveChangesAsync(ct);
                return 1;
            }

            existing.NextValue += 1;
            existing.DateModification = DateTime.Now;
            await _db.SaveChangesAsync(ct);
            return existing.NextValue;
        }

        private static string ToAlphaNumericSuffix(int value, int maxPerLetters)
        {
            // value starts at 1
            var lettersIndex = (value - 1) / maxPerLetters; // 0..675
            var number = ((value - 1) % maxPerLetters) + 1; // 1..9999

            var first = lettersIndex / 26;
            var second = lettersIndex % 26;

            var letters = string.Concat((char)('A' + first), (char)('A' + second));
            return $"{letters}{number:0000}";
        }
    }
}
