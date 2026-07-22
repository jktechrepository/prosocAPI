using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Core;

namespace Prosoc.Tests.Unit.Helpers;

/// <summary>
/// Reproduit la logique de sql/MigrateLegacyTauxChangeToTauxChangeDevises.idempotent.sql
/// pour les tests unitaires (SQLite).
/// </summary>
internal static class LegacyTauxChangeBackfill
{
    public static async Task<bool> ColumnExistsAsync(ProsocDbContext db, CancellationToken ct = default)
    {
        await db.Database.OpenConnectionAsync(ct);
        try
        {
            await using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "PRAGMA table_info(Devises);";
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var name = reader.GetString(1);
                if (string.Equals(name, "TauxChange", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    public static async Task EnsureLegacyColumnAsync(ProsocDbContext db, CancellationToken ct = default)
    {
        if (await ColumnExistsAsync(db, ct))
            return;

        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE Devises ADD COLUMN TauxChange REAL NOT NULL DEFAULT 0;",
            ct);
    }

    public static async Task SetLegacyTauxAsync(ProsocDbContext db, int deviseId, decimal taux, CancellationToken ct = default)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Devises SET TauxChange = {taux} WHERE IdDevise = {deviseId};",
            ct);
    }

    public static async Task<int> ApplyAsync(ProsocDbContext db, CancellationToken ct = default)
    {
        if (!await ColumnExistsAsync(db, ct))
            return 0;

        var principale = await db.Devises
            .FirstOrDefaultAsync(d => d.EstDevisePrincipale && d.Statut, ct);
        if (principale == null)
            return 0;

        var tauxLegacy = await ReadLegacyTauxAsync(db, principale.IdDevise, ct);
        if (tauxLegacy <= 0)
            return 0;

        var cibles = await db.Devises
            .Where(d => !d.EstDevisePrincipale && d.Statut && d.IdDevise != principale.IdDevise)
            .ToListAsync(ct);

        var inserted = 0;
        foreach (var cible in cibles)
        {
            var exists = await db.TauxChangeDevises.AnyAsync(t =>
                t.DeviseSourceId == principale.IdDevise &&
                t.DeviseCibleId == cible.IdDevise &&
                t.Statut, ct);

            if (exists)
                continue;

            db.TauxChangeDevises.Add(new TauxChangeDevise
            {
                DeviseSourceId = principale.IdDevise,
                DeviseCibleId = cible.IdDevise,
                Taux = tauxLegacy,
                DateEffet = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Statut = true,
                DateCreation = DateTime.UtcNow
            });
            inserted++;
        }

        if (inserted > 0)
            await db.SaveChangesAsync(ct);

        return inserted;
    }

    private static async Task<decimal> ReadLegacyTauxAsync(ProsocDbContext db, int deviseId, CancellationToken ct)
    {
        await db.Database.OpenConnectionAsync(ct);
        try
        {
            await using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "SELECT TauxChange FROM Devises WHERE IdDevise = @id;";
            var param = cmd.CreateParameter();
            param.ParameterName = "@id";
            param.Value = deviseId;
            cmd.Parameters.Add(param);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}
