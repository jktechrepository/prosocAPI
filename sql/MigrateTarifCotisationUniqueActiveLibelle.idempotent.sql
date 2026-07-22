-- Idempotent migration: unique normalized active libelle for TarifsCotisation
-- Rule: uniqueness applies only to active tariffs via normalized key.

START TRANSACTION;

-- 1) Add normalized column if missing
SET @has_column := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TarifsCotisation'
      AND COLUMN_NAME = 'LibelleTarifCotisationNormalized'
);

SET @sql := IF(
    @has_column = 0,
    'ALTER TABLE TarifsCotisation ADD COLUMN LibelleTarifCotisationNormalized VARCHAR(255) NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 2) Backfill normalized key for active rows only
UPDATE TarifsCotisation
SET LibelleTarifCotisationNormalized = CASE
    WHEN Statut = 1
         AND LibelleTarifCotisation IS NOT NULL
         AND TRIM(LibelleTarifCotisation) <> ''
    THEN LOWER(TRIM(LibelleTarifCotisation))
    ELSE NULL
END;

-- 3) Diagnostic duplicates among active normalized labels
SELECT
    LibelleTarifCotisationNormalized AS LibelleNormalized,
    COUNT(*) AS Cnt,
    GROUP_CONCAT(IdCotisationAffilie ORDER BY IdCotisationAffilie) AS TarifIds
FROM TarifsCotisation
WHERE LibelleTarifCotisationNormalized IS NOT NULL
GROUP BY LibelleTarifCotisationNormalized
HAVING COUNT(*) > 1;

-- 4) Create unique index only if no duplicates remain
SET @dup_count := (
    SELECT COUNT(*)
    FROM (
        SELECT LibelleTarifCotisationNormalized
        FROM TarifsCotisation
        WHERE LibelleTarifCotisationNormalized IS NOT NULL
        GROUP BY LibelleTarifCotisationNormalized
        HAVING COUNT(*) > 1
    ) d
);

SET @has_index := (
    SELECT COUNT(*)
    FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TarifsCotisation'
      AND INDEX_NAME = 'IX_TarifsCotisation_LibelleTarifCotisationNormalized'
);

SET @sql := IF(
    @dup_count = 0 AND @has_index = 0,
    'CREATE UNIQUE INDEX IX_TarifsCotisation_LibelleTarifCotisationNormalized ON TarifsCotisation(LibelleTarifCotisationNormalized)',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SELECT @dup_count AS DuplicateNormalizedActiveLibelles;

COMMIT;

