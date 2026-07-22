-- Idempotent migration script R1: BonsEnvoi.JetonMedicalId (nullable + backfill + diagnostics)
-- DB cible: MariaDB/MySQL

START TRANSACTION;

-- 1) Ajouter la colonne JetonMedicalId si absente
SET @has_col := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'BonsEnvoi'
      AND COLUMN_NAME = 'JetonMedicalId'
);

SET @sql := IF(
    @has_col = 0,
    'ALTER TABLE BonsEnvoi ADD COLUMN JetonMedicalId INT NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 2) Backfill depuis DemandesBonEnvoi quand la paire Bon/Jeton existe
UPDATE BonsEnvoi b
INNER JOIN DemandesBonEnvoi d ON d.BonEnvoiId = b.IdBonEnvoi
SET b.JetonMedicalId = d.JetonMedicalId
WHERE b.JetonMedicalId IS NULL
  AND d.JetonMedicalId IS NOT NULL;

-- 3) Créer l'index unique si absent
SET @has_uq_index := (
    SELECT COUNT(*)
    FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'BonsEnvoi'
      AND INDEX_NAME = 'IX_BonsEnvoi_JetonMedicalId'
);

SET @sql := IF(
    @has_uq_index = 0,
    'CREATE UNIQUE INDEX IX_BonsEnvoi_JetonMedicalId ON BonsEnvoi(JetonMedicalId)',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 4) Ajouter la FK si absente
SET @has_fk := (
    SELECT COUNT(*)
    FROM information_schema.TABLE_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = DATABASE()
      AND TABLE_NAME = 'BonsEnvoi'
      AND CONSTRAINT_NAME = 'FK_BonsEnvoi_JetonsMedicaux_JetonMedicalId'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql := IF(
    @has_fk = 0,
    'ALTER TABLE BonsEnvoi ADD CONSTRAINT FK_BonsEnvoi_JetonsMedicaux_JetonMedicalId FOREIGN KEY (JetonMedicalId) REFERENCES JetonsMedicaux(IdJeton) ON DELETE RESTRICT',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 5) Diagnostics intégrité
SELECT '=== DIAGNOSTICS R1 Bon<->Jeton ===' AS Section;

SELECT COUNT(*) AS NbBonsSansJeton
FROM BonsEnvoi
WHERE JetonMedicalId IS NULL;

SELECT JetonMedicalId, COUNT(*) AS Occurrences
FROM BonsEnvoi
WHERE JetonMedicalId IS NOT NULL
GROUP BY JetonMedicalId
HAVING COUNT(*) > 1;

COMMIT;

SELECT '✅ R1 appliqué: lien direct BonEnvoi.JetonMedicalId ajouté/backfill.' AS Resultat;
