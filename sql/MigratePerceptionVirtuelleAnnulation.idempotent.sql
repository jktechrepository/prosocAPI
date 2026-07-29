-- =============================================================================
-- Migration : annulation documentée PerceptionVirtuelle
-- =============================================================================
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigratePerceptionVirtuelleAnnulation.idempotent.sql
-- =============================================================================

START TRANSACTION;

-- StatutMetier (CONFIRMEE / ANNULEE)
SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PerceptionsVirtuelles' AND COLUMN_NAME = 'StatutMetier'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE PerceptionsVirtuelles ADD COLUMN StatutMetier VARCHAR(20) NOT NULL DEFAULT ''CONFIRMEE'' AFTER Observation',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

UPDATE PerceptionsVirtuelles
SET StatutMetier = 'CONFIRMEE'
WHERE StatutMetier IS NULL OR StatutMetier = '';

-- MotifAnnulation
SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PerceptionsVirtuelles' AND COLUMN_NAME = 'MotifAnnulation'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE PerceptionsVirtuelles ADD COLUMN MotifAnnulation VARCHAR(500) NULL AFTER StatutMetier',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- DateAnnulation
SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PerceptionsVirtuelles' AND COLUMN_NAME = 'DateAnnulation'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE PerceptionsVirtuelles ADD COLUMN DateAnnulation DATETIME(6) NULL AFTER MotifAnnulation',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- AnnuleParUtilisateurId
SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PerceptionsVirtuelles' AND COLUMN_NAME = 'AnnuleParUtilisateurId'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE PerceptionsVirtuelles ADD COLUMN AnnuleParUtilisateurId INT NULL AFTER DateAnnulation',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- FK AnnuleParUtilisateurId
SET @hasFk := (
    SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'PerceptionsVirtuelles'
      AND CONSTRAINT_NAME = 'FK_PerceptionsVirtuelles_Utilisateurs_AnnuleParUtilisateurId'
);
SET @sql := IF(@hasFk = 0,
    'ALTER TABLE PerceptionsVirtuelles
     ADD CONSTRAINT FK_PerceptionsVirtuelles_Utilisateurs_AnnuleParUtilisateurId
     FOREIGN KEY (AnnuleParUtilisateurId) REFERENCES Utilisateurs (IdUtilisateur)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Retirer UNIQUE CollecteId pour permettre re-confirmation après annulation
-- (lignes annulées conservées pour audit).
-- MySQL #1553 : l'index UNIQUE sert aussi la FK CollecteId → Collectes ;
-- il faut dropper la FK, remplacer l'index, puis recréer la FK.
SET @fkCollecte := (
    SELECT CONSTRAINT_NAME
    FROM information_schema.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'PerceptionsVirtuellesLignes'
      AND COLUMN_NAME = 'CollecteId'
      AND REFERENCED_TABLE_NAME = 'Collectes'
    LIMIT 1
);

SET @sql := IF(@fkCollecte IS NOT NULL,
    CONCAT('ALTER TABLE PerceptionsVirtuellesLignes DROP FOREIGN KEY `', @fkCollecte, '`'),
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasUnique := (
    SELECT COUNT(*) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'PerceptionsVirtuellesLignes'
      AND INDEX_NAME = 'IX_PerceptionsVirtuellesLignes_CollecteId'
      AND NON_UNIQUE = 0
);
SET @sql := IF(@hasUnique > 0,
    'ALTER TABLE PerceptionsVirtuellesLignes DROP INDEX IX_PerceptionsVirtuellesLignes_CollecteId',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasIdx := (
    SELECT COUNT(*) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'PerceptionsVirtuellesLignes'
      AND INDEX_NAME = 'IX_PerceptionsVirtuellesLignes_CollecteId'
);
SET @sql := IF(@hasIdx = 0,
    'CREATE INDEX IX_PerceptionsVirtuellesLignes_CollecteId ON PerceptionsVirtuellesLignes (CollecteId)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fkCollecteAfter := (
    SELECT CONSTRAINT_NAME
    FROM information_schema.KEY_COLUMN_USAGE
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'PerceptionsVirtuellesLignes'
      AND COLUMN_NAME = 'CollecteId'
      AND REFERENCED_TABLE_NAME = 'Collectes'
    LIMIT 1
);
SET @sql := IF(@fkCollecteAfter IS NULL,
    'ALTER TABLE PerceptionsVirtuellesLignes
     ADD CONSTRAINT FK_PerceptionsVirtuellesLignes_Collectes
     FOREIGN KEY (CollecteId) REFERENCES Collectes (IdCollecte)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

COMMIT;

SELECT '✅ Migration PerceptionVirtuelle annulation terminée.' AS Resultat;
