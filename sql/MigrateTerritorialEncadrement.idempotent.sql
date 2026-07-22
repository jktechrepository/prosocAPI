-- =============================================================================
-- Migration : encadrement territorial (ChefEquipeAgentId / SuperviseurAgentId)
-- =============================================================================
-- Idempotent : ne remplit les FK que si elles sont NULL et qu'un seul candidat existe.
-- =============================================================================

START TRANSACTION;

SET @ChefEquipeRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = "Chef d'équipe" AND Statut = 1 LIMIT 1
);

SET @SuperviseurRoleId := (
    SELECT IdRole FROM Roles WHERE Nom = "Superviseur" AND Statut = 1 LIMIT 1
);

-- Chef d'équipe par zone
UPDATE ZonesSociales z
INNER JOIN (
    SELECT a.ZoneSocialeId AS ZoneId, MIN(a.IdAgent) AS AgentId, COUNT(DISTINCT a.IdAgent) AS Cnt
    FROM Agents a
    INNER JOIN Utilisateurs u ON u.AgentId = a.IdAgent AND u.Statut = 1
    INNER JOIN UserRoles ur ON ur.UtilisateurId = u.IdUtilisateur AND ur.Statut = 1
    WHERE a.Statut = 1
      AND a.ZoneSocialeId IS NOT NULL
      AND ur.RoleId = @ChefEquipeRoleId
    GROUP BY a.ZoneSocialeId
    HAVING Cnt = 1
) candidats ON candidats.ZoneId = z.IdZoneSociale
SET z.ChefEquipeAgentId = candidats.AgentId
WHERE z.ChefEquipeAgentId IS NULL
  AND @ChefEquipeRoleId IS NOT NULL;

-- Superviseur par commune
UPDATE Communes c
INNER JOIN (
    SELECT z.CommuneId, MIN(a.IdAgent) AS AgentId, COUNT(DISTINCT a.IdAgent) AS Cnt
    FROM Agents a
    INNER JOIN ZonesSociales z ON z.IdZoneSociale = a.ZoneSocialeId
    INNER JOIN Utilisateurs u ON u.AgentId = a.IdAgent AND u.Statut = 1
    INNER JOIN UserRoles ur ON ur.UtilisateurId = u.IdUtilisateur AND ur.Statut = 1
    WHERE a.Statut = 1
      AND a.ZoneSocialeId IS NOT NULL
      AND ur.RoleId = @SuperviseurRoleId
    GROUP BY z.CommuneId
    HAVING Cnt = 1
) candidats ON candidats.CommuneId = c.IdCommune
SET c.SuperviseurAgentId = candidats.AgentId
WHERE c.SuperviseurAgentId IS NULL
  AND @SuperviseurRoleId IS NOT NULL;

-- NOTE: la colonne Agents.SuperviseurId (legacy) a été supprimée.

COMMIT;
