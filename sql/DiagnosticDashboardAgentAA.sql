-- Diagnostic Dashboard Agent AA — remplacer @agentId si besoin (défaut 29)
SET @agentId := 29;

-- 1. Agent et utilisateurs
SELECT IdAgent, NomComplet, Matricule, Statut, ZoneSocialeId
FROM Agents WHERE IdAgent = @agentId;

SELECT IdUtilisateur, NomUtilisateur, AgentId, Statut
FROM Utilisateurs WHERE AgentId = @agentId;

-- 2. Filtre exact du service DashboardAgentAAService
SELECT COUNT(*) AS total,
       SUM(CASE WHEN StatutDossier IN ('VALIDÉ','VALIDE') THEN 1 ELSE 0 END) AS valides,
       SUM(CASE WHEN Statut = 1 THEN 1 ELSE 0 END) AS actives
FROM Adhesions WHERE AgentId = @agentId;

-- 3. Où sont les dossiers actifs
SELECT AgentId, StatutDossier, COUNT(*) AS nb
FROM Adhesions WHERE Statut = 1
GROUP BY AgentId, StatutDossier
ORDER BY nb DESC;

-- 4. Dépendants visibles dashboard
SELECT COUNT(DISTINCT d.IdDependant) AS dependants
FROM Dependants d
JOIN Adhesions ad ON ad.AffilieId = d.AffilieId
WHERE ad.AgentId = @agentId AND ad.Statut = 1 AND d.Statut = 1;

-- Candidats à affecter (dossiers non validés, autre agent)
SELECT ad.IdAdhesion, ad.AffilieId, ad.AgentId, ad.StatutDossier, af.CodeAdhesion
FROM Adhesions ad
JOIN Affilies af ON af.IdAffilie = ad.AffilieId
WHERE ad.Statut = 1
  AND ad.AgentId <> @agentId
  AND UPPER(TRIM(ad.StatutDossier)) IN ('EN ATTENTE', 'A', 'COMPLET')
ORDER BY ad.DateCreation DESC
LIMIT 20;
