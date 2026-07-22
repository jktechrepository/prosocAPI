-- Idempotent migration: add LibelleTarifCotisation to TarifsCotisation
-- DB: MariaDB/MySQL

START TRANSACTION;

SET @has_column := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TarifsCotisation'
      AND COLUMN_NAME = 'LibelleTarifCotisation'
);

SET @sql := IF(
    @has_column = 0,
    'ALTER TABLE TarifsCotisation ADD COLUMN LibelleTarifCotisation VARCHAR(255) NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'TarifsCotisation'
  AND COLUMN_NAME = 'LibelleTarifCotisation';

COMMIT;

