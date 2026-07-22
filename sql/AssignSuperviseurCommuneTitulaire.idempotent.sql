-- =============================================================================
-- Affectation superviseur titulaire de commune (SuperviseurAgentId)
-- =============================================================================
-- Méthode recommandée (API Admin / IT) :
--
--   PUT /api/Commune/{communeId}/superviseur
--   Authorization: Bearer <token Admin|SuperAdmin|IT>
--   Content-Type: application/json
--
--   { "agentId": 52 }
--
-- Prérequis métier :
--   - L'agent doit être rattaché à une zone de la commune cible (Agents.ZoneSocialeId)
--   - L'API synchronise aussi le rôle JWT « Superviseur » sur l'utilisateur lié
--
-- Usage diagnostic + affectation SQL d'urgence (à valider métier) :
--   SET @AgentId := 52;
--   SET @CommuneId := <id_commune>;
--   mysql ... < sql/AssignSuperviseurCommuneTitulaire.idempotent.sql
-- =============================================================================

SET @AgentId := IFNULL(@AgentId, 52);
SET @CommuneId := IFNULL(@CommuneId, NULL);

SELECT '=== 1) Pré-contrôles agent / commune / zone ===' AS Section;

SELECT
    a.IdAgent,
    a.NomComplet,
    a.ZoneSocialeId,
    z.CommuneId AS CommuneDeLaZoneAgent,
    @CommuneId AS CommuneCible
FROM Agents a
LEFT JOIN ZonesSociales z ON z.IdZoneSociale = a.ZoneSocialeId
WHERE a.IdAgent = @AgentId;

SELECT
    c.IdCommune,
    c.Nom,
    c.SuperviseurAgentId AS SuperviseurActuel,
    prev.NomComplet AS SuperviseurActuelNom
FROM Communes c
LEFT JOIN Agents prev ON prev.IdAgent = c.SuperviseurAgentId
WHERE @CommuneId IS NOT NULL AND c.IdCommune = @CommuneId;

SELECT '=== 2) Affectation SQL (uniquement si @CommuneId est défini) ===' AS Section;

-- Bloque si l'agent n'est pas dans une zone de la commune cible
UPDATE Communes c
INNER JOIN Agents a ON a.IdAgent = @AgentId AND a.Statut = 1
INNER JOIN ZonesSociales z ON z.IdZoneSociale = a.ZoneSocialeId AND z.CommuneId = c.IdCommune
SET c.SuperviseurAgentId = @AgentId
WHERE @CommuneId IS NOT NULL
  AND c.IdCommune = @CommuneId
  AND (c.SuperviseurAgentId IS NULL OR c.SuperviseurAgentId <> @AgentId);

SELECT ROW_COUNT() AS LignesMisesAJour;

SELECT '=== 3) Vérification post-affectation ===' AS Section;

SELECT
    c.IdCommune,
    c.Nom,
    c.SuperviseurAgentId,
    sp.NomComplet AS SuperviseurNom
FROM Communes c
LEFT JOIN Agents sp ON sp.IdAgent = c.SuperviseurAgentId
WHERE c.SuperviseurAgentId = @AgentId
   OR (@CommuneId IS NOT NULL AND c.IdCommune = @CommuneId);

SELECT '=== 4) Retest API ===' AS Section;

SELECT CONCAT(
    'GET /api/DashboardSuperviseur/indicateurs-performance/', @AgentId,
    ' — doit retourner 200 après affectation'
) AS EtapeSuivante;
