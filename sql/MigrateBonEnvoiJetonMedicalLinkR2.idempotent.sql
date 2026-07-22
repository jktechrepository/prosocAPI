-- Idempotent migration script R2: BonsEnvoi.JetonMedicalId obligatoire (NOT NULL)
-- Pré-requis: R1 déjà appliqué
-- DB cible: MariaDB/MySQL

START TRANSACTION;

-- 1) Re-backfill de sécurité
UPDATE BonsEnvoi b
INNER JOIN DemandesBonEnvoi d ON d.BonEnvoiId = b.IdBonEnvoi
SET b.JetonMedicalId = d.JetonMedicalId
WHERE b.JetonMedicalId IS NULL
  AND d.JetonMedicalId IS NOT NULL;

-- 2) Vérifier qu'il ne reste pas de BonsEnvoi sans Jeton
SET @null_count := (SELECT COUNT(*) FROM BonsEnvoi WHERE JetonMedicalId IS NULL);
SET @sql := IF(
    @null_count > 0,
    'SIGNAL SQLSTATE ''45000'' SET MESSAGE_TEXT = ''R2 aborted: BonsEnvoi sans JetonMedicalId''',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 3) Vérifier le caractère nullable de la colonne
SET @is_nullable := (
    SELECT IS_NULLABLE
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'BonsEnvoi'
      AND COLUMN_NAME = 'JetonMedicalId'
    LIMIT 1
);

-- 4) Passer la colonne en NOT NULL si nécessaire
SET @sql := IF(
    @is_nullable = 'YES',
    'ALTER TABLE BonsEnvoi MODIFY COLUMN JetonMedicalId INT NOT NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 5) Diagnostics
SELECT '=== DIAGNOSTICS R2 Bon<->Jeton ===' AS Section;
SELECT COUNT(*) AS NbBonsSansJeton
FROM BonsEnvoi
WHERE JetonMedicalId IS NULL;

COMMIT;

SELECT '✅ R2 appliqué: lien BonEnvoi.JetonMedicalId rendu obligatoire.' AS Resultat;
