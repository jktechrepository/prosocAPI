-- =============================================================================
-- Diagnostic : superviseur sans commune titulaire (HTTP 500/422 dashboard SP)
-- =============================================================================
-- Symptôme API :
--   GET /api/DashboardSuperviseur/indicateurs-performance/{id}
--   detail : "Superviseur {id} non titulaire d'une commune ..."
--
-- Usage UAT / prod :
--   mysql -h <host> -u <user> -p <database> < sql/DiagnoseSuperviseurCommuneTitulaire.idempotent.sql
--
-- Pour cibler un agent précis, définir @AgentId avant d'exécuter (ex. 52) :
--   SET @AgentId := 52;
-- =============================================================================

SET @AgentId := IFNULL(@AgentId, 52);

SELECT '=== 1) Agent cible (IdAgent, pas IdUtilisateur) ===' AS Section;

SELECT
    a.IdAgent,
    a.NomComplet,
    a.Matricule,
    a.ZoneSocialeId,
    a.Statut,
    z.Nom AS ZoneNom,
    z.CommuneId,
    c.Nom AS CommuneZoneNom
FROM Agents a
LEFT JOIN ZonesSociales z ON z.IdZoneSociale = a.ZoneSocialeId
LEFT JOIN Communes c ON c.IdCommune = z.CommuneId
WHERE a.IdAgent = @AgentId;

SELECT '=== 2) Utilisateur(s) lié(s) et rôle(s) JWT ===' AS Section;

SELECT
    u.IdUtilisateur,
    u.NomUtilisateur,
    u.AgentId,
    GROUP_CONCAT(DISTINCT r.Nom ORDER BY r.Nom SEPARATOR ', ') AS Roles
FROM Utilisateurs u
LEFT JOIN UserRoles ur ON ur.UtilisateurId = u.IdUtilisateur AND ur.Statut = 1
LEFT JOIN Roles r ON r.IdRole = ur.RoleId
WHERE u.AgentId = @AgentId OR u.IdUtilisateur = @AgentId
GROUP BY u.IdUtilisateur, u.NomUtilisateur, u.AgentId;

SELECT '=== 3) Commune(s) dont l''agent est superviseur titulaire (SuperviseurAgentId) ===' AS Section;

SELECT
    c.IdCommune,
    c.Nom,
    c.SuperviseurAgentId,
    c.Statut
FROM Communes c
WHERE c.SuperviseurAgentId = @AgentId;

SELECT '=== 4) Confusion IdUtilisateur vs IdAgent ? ===' AS Section;

SELECT
    CASE
        WHEN EXISTS (SELECT 1 FROM Agents WHERE IdAgent = @AgentId) THEN 'OK : @AgentId correspond à un IdAgent'
        WHEN EXISTS (SELECT 1 FROM Utilisateurs WHERE IdUtilisateur = @AgentId) THEN 'ATTENTION : @AgentId est un IdUtilisateur — Flutter doit passer Agents.IdAgent (JWT claim AgentId)'
        ELSE 'INCONNU : aucun agent ni utilisateur avec cet identifiant'
    END AS InterpretationId;

SELECT '=== 5) Agents actifs dans la commune de la zone de l''agent (périmètre SP une fois titulaire) ===' AS Section;

SELECT COUNT(DISTINCT a2.IdAgent) AS NbAgentsActifsCommune
FROM Agents a
INNER JOIN ZonesSociales z ON z.IdZoneSociale = a.ZoneSocialeId
INNER JOIN Agents a2 ON a2.ZoneSocialeId IN (
    SELECT z2.IdZoneSociale FROM ZonesSociales z2 WHERE z2.CommuneId = z.CommuneId
)
WHERE a.IdAgent = @AgentId
  AND a2.Statut = 1;

SELECT '=== Actions recommandées ===' AS Section;

SELECT
    CASE
        WHEN NOT EXISTS (SELECT 1 FROM Agents WHERE IdAgent = @AgentId)
            THEN 'Vérifier l''ID passé par le frontend (Agents.IdAgent, pas Utilisateurs.IdUtilisateur)'
        WHEN NOT EXISTS (SELECT 1 FROM Communes WHERE SuperviseurAgentId = @AgentId)
            THEN CONCAT(
                'Nommer SP titulaire : PUT /api/Commune/{communeId}/superviseur body {"agentId":', @AgentId, '} — voir sql/AssignSuperviseurCommuneTitulaire.idempotent.sql'
            )
        ELSE 'Configuration OK — si erreur persiste, vérifier déploiement API et zone active de l''agent'
    END AS Recommandation;
