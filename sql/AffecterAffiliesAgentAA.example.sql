-- Exemple : affecter des affiliés à l'encodeur AA (agentId 29) pour alimenter le dashboard.
-- Prérequis : remplacer @agentId et la liste @affilieIds selon sql/DiagnosticDashboardAgentAA.sql
-- Attention : transfère aussi les collectes vers le nouvel agent (comme PUT /api/Agent/{id}/affecter-affilies).

SET @agentId := 29;
SET @affilieId1 := 1;
SET @affilieId2 := 2;

START TRANSACTION;

UPDATE Adhesions ad
SET ad.AgentId = @agentId, ad.DateModification = NOW(6)
WHERE ad.AffilieId IN (@affilieId1, @affilieId2) AND ad.Statut = 1;

UPDATE Collectes c
SET c.AgentId = @agentId
WHERE c.AffilieId IN (@affilieId1, @affilieId2);

COMMIT;

-- Vérification (doit être > 0 pour le dashboard AA)
SELECT COUNT(*) AS total FROM Adhesions WHERE AgentId = @agentId AND Statut = 1;
