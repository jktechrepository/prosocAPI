-- =============================================================================
-- Migration schéma : suppression Devises.TauxChange (legacy)
-- Aligné sur EF : DropDeviseTauxChange (20260610125844)
-- Idempotent — réexécutable sans erreur.
--
-- Prérequis :
--   - TauxChangeDevises peuplée (SeedMultidevise.sql + MigrateLegacyTauxChange…)
--   - Vérifier : SELECT COUNT(*) FROM TauxChangeDevises WHERE Statut = 1;  -- > 0
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/DropDeviseTauxChange.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @db = DATABASE();

SET @col_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Devises' AND COLUMN_NAME = 'TauxChange'
);

SET @sql = IF(
    @col_exists > 0,
    'ALTER TABLE `Devises` DROP COLUMN `TauxChange`',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260610125844_DropDeviseTauxChange', '6.0.25'
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260610125844_DropDeviseTauxChange'
);

COMMIT;

SELECT
    CASE
        WHEN @col_exists > 0 THEN 'Colonne Devises.TauxChange supprimée.'
        ELSE 'Colonne Devises.TauxChange déjà absente.'
    END AS Resultat;
