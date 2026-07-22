-- =============================================================================
-- Migration : Prestations.Periodicite
-- =============================================================================
-- Aligne sur EF migration 20260625092924_AddPrestationPeriodicite
-- Objectif :
--   - ajouter la colonne Periodicite sur Prestations (VARCHAR(20), default Mensuel)
--   - retro-propager depuis ProduitMutuel / ProduitAssureur quand lie
--   - fallback Mensuel sinon
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigratePrestationPeriodicite.idempotent.sql
-- =============================================================================

START TRANSACTION;

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

SET @sql := IF(@hasPrestations = 1 AND @hasPeriodicite = 0,
    'ALTER TABLE Prestations ADD COLUMN Periodicite VARCHAR(20) NOT NULL DEFAULT ''Mensuel''',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

UPDATE Prestations p
LEFT JOIN ProduitsMutuels pm ON pm.IdProduit = p.ProduitMutuelId
LEFT JOIN ProduitsAssureurs pa ON pa.IdProduit = p.ProduitAssureurId
SET p.Periodicite = COALESCE(NULLIF(pm.Periodicite, ''), NULLIF(pa.Periodicite, ''), 'Mensuel')
WHERE p.Periodicite IS NULL OR TRIM(p.Periodicite) = '' OR p.Periodicite = 'Mensuel';

SELECT IdPrestation, NomPrestation, Periodicite, ProduitMutuelId, ProduitAssureurId
FROM Prestations
ORDER BY IdPrestation;

COMMIT;
