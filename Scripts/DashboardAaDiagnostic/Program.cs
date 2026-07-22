using MySqlConnector;

var agentId = 29;
if (int.TryParse(Environment.GetEnvironmentVariable("AGENT_ID"), out var envAgentId))
    agentId = envAgentId;

var connStr = args.Length > 0
    ? args[0]
    : Environment.GetEnvironmentVariable("PROSOC_CONNECTION")
      ?? "Server=localhost;Port=3306;Database=dev-prosocdb;User=kansa;Password=kansa2025;CharSet=utf8mb4;SslMode=none;";

Console.WriteLine($"=== Diagnostic Dashboard Agent AA (agentId={agentId}) ===");
Console.WriteLine($"Connection: {MaskConnectionString(connStr)}");
Console.WriteLine();

try
{
    await using var conn = new MySqlConnection(connStr);
    await conn.OpenAsync();

    await RunQueryAsync(conn, "1. Agent",
        "SELECT IdAgent, NomComplet, Matricule, Statut, ZoneSocialeId FROM Agents WHERE IdAgent = @agentId",
        new MySqlParameter("@agentId", agentId));

    await RunQueryAsync(conn, "1b. Utilisateurs liés",
        "SELECT IdUtilisateur, NomUtilisateur, AgentId, Statut FROM Utilisateurs WHERE AgentId = @agentId",
        new MySqlParameter("@agentId", agentId));

    var adhesionCount = await RunScalarAsync<long>(conn,
        "SELECT COUNT(*) FROM Adhesions WHERE AgentId = @agentId",
        new MySqlParameter("@agentId", agentId));

    await RunQueryAsync(conn, "2. Adhésions agent (filtre service)",
        @"SELECT COUNT(*) AS total,
               SUM(CASE WHEN StatutDossier IN ('VALIDÉ','VALIDE') THEN 1 ELSE 0 END) AS valides,
               SUM(CASE WHEN Statut = 1 THEN 1 ELSE 0 END) AS actives
        FROM Adhesions WHERE AgentId = @agentId",
        new MySqlParameter("@agentId", agentId));

    await RunQueryAsync(conn, "3. Répartition adhésions actives par agent",
        @"SELECT AgentId, StatutDossier, COUNT(*) AS nb
        FROM Adhesions WHERE Statut = 1
        GROUP BY AgentId, StatutDossier
        ORDER BY nb DESC
        LIMIT 20");

    await RunQueryAsync(conn, "4. Dépendants (affiliés AgentId)",
        @"SELECT COUNT(DISTINCT d.IdDependant) AS dependants
        FROM Dependants d
        JOIN Adhesions ad ON ad.AffilieId = d.AffilieId
        WHERE ad.AgentId = @agentId AND ad.Statut = 1 AND d.Statut = 1",
        new MySqlParameter("@agentId", agentId));

    if (adhesionCount == 0)
    {
        Console.WriteLine();
        Console.WriteLine(">>> CAUSE PROBABLE : aucune adhésion avec AgentId = {0}.", agentId);
        Console.WriteLine(">>> Action : PUT /api/Agent/{0}/affecter-affilies avec affilieIds ciblés.", agentId);
        Console.WriteLine();

        await RunQueryAsync(conn, "Candidats EN ATTENTE (autres agents)",
            @"SELECT ad.IdAdhesion, ad.AffilieId, ad.AgentId, ad.StatutDossier, af.CodeAdhesion, af.NomComplet
            FROM Adhesions ad
            JOIN Affilies af ON af.IdAffilie = ad.AffilieId
            WHERE ad.Statut = 1
              AND ad.AgentId <> @agentId
              AND UPPER(TRIM(ad.StatutDossier)) IN ('EN ATTENTE', 'A', 'COMPLET')
            ORDER BY ad.DateCreation DESC
            LIMIT 10",
            new MySqlParameter("@agentId", agentId));
    }
    else
    {
        Console.WriteLine();
        Console.WriteLine(">>> Adhésions trouvées pour agent {0} : le dashboard devrait renvoyer des KPIs > 0.", agentId);
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"ERREUR connexion / requête : {ex.Message}");
    Console.Error.WriteLine("Exécutez manuellement sql/DiagnosticDashboardAgentAA.sql si MySQL est indisponible.");
    Environment.ExitCode = 1;
}

static string MaskConnectionString(string cs)
{
    var parts = cs.Split(';', StringSplitOptions.RemoveEmptyEntries);
    return string.Join(';', parts.Select(p =>
        p.TrimStart().StartsWith("Password=", StringComparison.OrdinalIgnoreCase)
            ? "Password=***"
            : p));
}

static async Task RunQueryAsync(MySqlConnection conn, string title, string sql, params MySqlParameter[] parameters)
{
    Console.WriteLine($"--- {title} ---");
    await using var cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddRange(parameters);
    await using var reader = await cmd.ExecuteReaderAsync();
    var fieldCount = reader.FieldCount;
    if (fieldCount == 0)
    {
        Console.WriteLine("(aucune colonne)");
        Console.WriteLine();
        return;
    }

    var headers = Enumerable.Range(0, fieldCount).Select(i => reader.GetName(i)).ToArray();
    Console.WriteLine(string.Join(" | ", headers));
    Console.WriteLine(new string('-', headers.Sum(h => h.Length + 3)));

    var rowCount = 0;
    while (await reader.ReadAsync())
    {
        var values = Enumerable.Range(0, fieldCount)
            .Select(i => reader.IsDBNull(i) ? "NULL" : reader.GetValue(i)?.ToString() ?? "")
            .ToArray();
        Console.WriteLine(string.Join(" | ", values));
        rowCount++;
    }

    if (rowCount == 0)
        Console.WriteLine("(0 ligne)");
    Console.WriteLine();
}

static async Task<T> RunScalarAsync<T>(MySqlConnection conn, string sql, params MySqlParameter[] parameters)
{
    await using var cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddRange(parameters);
    var result = await cmd.ExecuteScalarAsync();
    return result is T t ? t : (T)Convert.ChangeType(result!, typeof(T));
}
