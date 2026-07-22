-- Idempotent migration script: TypeAdhesions.DeviseId (FK obligatoire -> Devises.IdDevise)
-- DB cible: MariaDB/MySQL
--
-- Ce script:
-- 1) ajoute la colonne DeviseId si absente
-- 2) backfill les lignes existantes avec la devise principale (fallback devise active puis toute devise)
-- 3) crée l'index + FK si absents
-- 4) passe la colonne en NOT NULL lorsque toutes les lignes sont backfillées

START TRANSACTION;

-- 1) Ajouter la colonne DeviseId si elle n'existe pas
SET @has_deviseid_col := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TypeAdhesions'
      AND COLUMN_NAME = 'DeviseId'
);

SET @sql := IF(
    @has_deviseid_col = 0,
    'ALTER TABLE TypeAdhesions ADD COLUMN DeviseId INT NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 2) Backfill en priorité avec la devise principale active
UPDATE TypeAdhesions ta
JOIN (
    SELECT d.IdDevise
    FROM Devises d
    WHERE d.EstDevisePrincipale = TRUE
      AND d.Statut = TRUE
    ORDER BY d.IdDevise
    LIMIT 1
) principal ON 1 = 1
SET ta.DeviseId = principal.IdDevise
WHERE ta.DeviseId IS NULL;

-- 2.b) Fallback avec la première devise active si nécessaire
UPDATE TypeAdhesions ta
JOIN (
    SELECT d.IdDevise
    FROM Devises d
    WHERE d.Statut = TRUE
    ORDER BY d.IdDevise
    LIMIT 1
) active_devise ON 1 = 1
SET ta.DeviseId = active_devise.IdDevise
WHERE ta.DeviseId IS NULL;

-- 2.c) Dernier fallback avec la première devise disponible
UPDATE TypeAdhesions ta
JOIN (
    SELECT d.IdDevise
    FROM Devises d
    ORDER BY d.IdDevise
    LIMIT 1
) any_devise ON 1 = 1
SET ta.DeviseId = any_devise.IdDevise
WHERE ta.DeviseId IS NULL;

-- 2.d) Corriger les éventuelles valeurs orphelines (DeviseId sans devise correspondante)
UPDATE TypeAdhesions ta
LEFT JOIN Devises d ON d.IdDevise = ta.DeviseId
JOIN (
    SELECT COALESCE(
        (SELECT d1.IdDevise FROM Devises d1 WHERE d1.EstDevisePrincipale = TRUE AND d1.Statut = TRUE ORDER BY d1.IdDevise LIMIT 1),
        (SELECT d2.IdDevise FROM Devises d2 WHERE d2.Statut = TRUE ORDER BY d2.IdDevise LIMIT 1),
        (SELECT d3.IdDevise FROM Devises d3 ORDER BY d3.IdDevise LIMIT 1)
    ) AS FallbackDeviseId
) fb ON 1 = 1
SET ta.DeviseId = fb.FallbackDeviseId
WHERE ta.DeviseId IS NOT NULL
  AND d.IdDevise IS NULL
  AND fb.FallbackDeviseId IS NOT NULL;

-- 3) Créer l'index si absent
SET @has_index := (
    SELECT COUNT(*)
    FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TypeAdhesions'
      AND INDEX_NAME = 'IX_TypeAdhesions_DeviseId'
);

SET @sql := IF(
    @has_index = 0,
    'CREATE INDEX IX_TypeAdhesions_DeviseId ON TypeAdhesions(DeviseId)',
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
      AND TABLE_NAME = 'TypeAdhesions'
      AND CONSTRAINT_NAME = 'FK_TypeAdhesions_Devises_DeviseId'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql := IF(
    @has_fk = 0,
    'ALTER TABLE TypeAdhesions ADD CONSTRAINT FK_TypeAdhesions_Devises_DeviseId FOREIGN KEY (DeviseId) REFERENCES Devises(IdDevise) ON DELETE RESTRICT',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 5) Passer DeviseId en NOT NULL uniquement si aucune ligne n'est NULL
SET @null_count := (SELECT COUNT(*) FROM TypeAdhesions WHERE DeviseId IS NULL);
SET @is_nullable := (
    SELECT IS_NULLABLE
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'TypeAdhesions'
      AND COLUMN_NAME = 'DeviseId'
    LIMIT 1
);

SET @sql := IF(
    @null_count = 0 AND @is_nullable = 'YES',
    'ALTER TABLE TypeAdhesions MODIFY COLUMN DeviseId INT NOT NULL',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 6) Vérifications post-migration
SELECT COUNT(*) AS RemainingNullDeviseId
FROM TypeAdhesions
WHERE DeviseId IS NULL;

SELECT COUNT(*) AS RemainingOrphanDeviseId
FROM TypeAdhesions ta
LEFT JOIN Devises d ON d.IdDevise = ta.DeviseId
WHERE ta.DeviseId IS NOT NULL
  AND d.IdDevise IS NULL;

COMMIT;

