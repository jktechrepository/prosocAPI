-- =============================================================================
-- Migration : Adhesions.UtilisateurId nullable (adhésion FlexPay anonyme)
-- =============================================================================
-- Parcours public without-affilie-paiement-electronique sans JWT :
-- UtilisateurId / Operateur collectes peuvent être null.
-- Idempotent.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateAdhesionUtilisateurIdNullable.idempotent.sql
-- =============================================================================

-- USE `prosoc_prod`;

START TRANSACTION;

SET @hasAdhesions := (
    SELECT COUNT(*) FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Adhesions'
);

SELECT
    CASE
        WHEN @hasAdhesions = 0 THEN '❌ ERREUR : table Adhesions introuvable'
        ELSE '✅ Table Adhesions'
    END AS Diagnostic;

-- Drop FK si présente
SET @fkName := (
    SELECT CONSTRAINT_NAME FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Adhesions'
      AND CONSTRAINT_NAME = 'FK_Adhesions_Utilisateurs_UtilisateurId'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
    LIMIT 1
);

SET @sql := IF(@hasAdhesions = 1 AND @fkName IS NOT NULL,
    'ALTER TABLE Adhesions DROP FOREIGN KEY FK_Adhesions_Utilisateurs_UtilisateurId',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Nullable
SET @isNullable := (
    SELECT IS_NULLABLE FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Adhesions'
      AND COLUMN_NAME = 'UtilisateurId'
    LIMIT 1
);

SET @sql := IF(@hasAdhesions = 1 AND @isNullable = 'NO',
    'ALTER TABLE Adhesions MODIFY COLUMN UtilisateurId INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Recréer FK
SET @fkExists := (
    SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Adhesions'
      AND CONSTRAINT_NAME = 'FK_Adhesions_Utilisateurs_UtilisateurId'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql := IF(@hasAdhesions = 1 AND @fkExists = 0,
    'ALTER TABLE Adhesions ADD CONSTRAINT FK_Adhesions_Utilisateurs_UtilisateurId FOREIGN KEY (UtilisateurId) REFERENCES Utilisateurs (IdUtilisateur) ON DELETE SET NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SELECT
    COLUMN_NAME,
    IS_NULLABLE,
    COLUMN_TYPE
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'Adhesions'
  AND COLUMN_NAME = 'UtilisateurId';

COMMIT;

SELECT '✅ Migration Adhesions.UtilisateurId nullable terminée.' AS Resultat;
