-- =============================================================================
-- Retirer la catégorie agent « Super Admin (SP) »
-- =============================================================================
-- Le super administrateur est un rôle applicatif (Admin AD), pas une catégorie agent.
-- Idempotent : relançable sans erreur.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/RemoveCategorieAgentSp.idempotent.sql
-- =============================================================================

START TRANSACTION;

UPDATE Agents a
INNER JOIN CategoriesAgents c ON c.IdCategorieAgent = a.CategorieAgentId
SET a.CategorieAgentId = (
    SELECT IdCategorieAgent FROM CategoriesAgents WHERE UPPER(TRIM(Code)) = 'AD' LIMIT 1
)
WHERE UPPER(TRIM(c.Code)) = 'SP'
   OR c.LibelleCategorie IN ('Super Admin (SP)', 'SP');

DELETE FROM CategoriesAgents
WHERE UPPER(TRIM(Code)) = 'SP'
   OR LibelleCategorie IN ('Super Admin (SP)', 'SP');

SELECT IdCategorieAgent, Code, LibelleCategorie, Description
FROM CategoriesAgents
ORDER BY IdCategorieAgent;

COMMIT;
