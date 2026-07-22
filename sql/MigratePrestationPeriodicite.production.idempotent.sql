-- =============================================================================
-- Migration PRODUCTION : Prestations.Periodicite
-- =============================================================================
-- Objectif :
--   - ajouter Prestations.Periodicite (VARCHAR(20), default Mensuel)
--   - recuperer la periodicite depuis ProduitsMutuels/ProduitsAssureurs lies
--   - fallback Mensuel si aucune source
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigratePrestationPeriodicite.production.idempotent.sql
-- =============================================================================

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- 0) Diagnostics schema
-- ---------------------------------------------------------------------------
SET @hasPrestations := (
    SELECT COUNT(*)
    FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Prestations'
);

SET @hasPeriodicite := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Prestations'
      AND COLUMN_NAME = 'Periodicite'
);

SELECT
    CASE
        WHEN @hasPrestations = 0 THEN 'ERREUR : table Prestations introuvable'
        WHEN @hasPeriodicite > 0 THEN 'OK : colonne Periodicite deja presente'
        ELSE 'INFO : ajout colonne Periodicite en cours'
    END AS Diagnostic;

-- ---------------------------------------------------------------------------
-- 1) Ajouter colonne si absente
-- ---------------------------------------------------------------------------
SET @sql := IF(@hasPrestations = 1 AND @hasPeriodicite = 0,
    'ALTER TABLE Prestations ADD COLUMN Periodicite VARCHAR(20) NOT NULL DEFAULT ''Mensuel''',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------------
-- 2) Retro-propagation des valeurs
-- ---------------------------------------------------------------------------
UPDATE Prestations p
LEFT JOIN ProduitsMutuels pm ON pm.IdProduit = p.ProduitMutuelId
LEFT JOIN ProduitsAssureurs pa ON pa.IdProduit = p.ProduitAssureurId
SET p.Periodicite = COALESCE(NULLIF(pm.Periodicite, ''), NULLIF(pa.Periodicite, ''), 'Mensuel')
WHERE p.Periodicite IS NULL OR TRIM(p.Periodicite) = '' OR p.Periodicite = 'Mensuel';

-- ---------------------------------------------------------------------------
-- 3) Verification
-- ---------------------------------------------------------------------------
SELECT IdPrestation, NomPrestation, Periodicite, ProduitMutuelId, ProduitAssureurId
FROM Prestations
ORDER BY IdPrestation;

COMMIT;
