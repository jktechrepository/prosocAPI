-- =============================================================================
-- Migration PRODUCTION : CategoriesAgents.Code + libellés affichés
-- =============================================================================
-- Objectif :
--   - ajouter la colonne Code (code court technique : AT, FI, …)
--   - conserver la logique métier (matricule, MAASH, filtres) sur Code
--   - aligner LibelleCategorie sur « Description (CODE) »
--   - ajuster les longueurs varchar (LibelleCategorie 200, Description 500)
--
-- Idempotent : relançable sans erreur si déjà appliqué.
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateCategorieAgentCode.production.idempotent.sql
-- =============================================================================

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- 0) Diagnostics
-- ---------------------------------------------------------------------------
SET @hasTable := (
    SELECT COUNT(*)
    FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CategoriesAgents'
);

SET @hasCode := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CategoriesAgents'
      AND COLUMN_NAME = 'Code'
);

SELECT
    CASE
        WHEN @hasTable = 0 THEN 'ERREUR : table CategoriesAgents introuvable'
        WHEN @hasCode > 0 THEN 'INFO : colonne Code déjà présente — migration données/libellés'
        ELSE 'INFO : ajout colonne Code en cours'
    END AS Diagnostic;

-- ---------------------------------------------------------------------------
-- 1) Ajouter Code si absent
-- ---------------------------------------------------------------------------
SET @sql := IF(@hasTable = 1 AND @hasCode = 0,
    'ALTER TABLE CategoriesAgents ADD COLUMN Code VARCHAR(10) NULL AFTER IdCategorieAgent',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasCode := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CategoriesAgents'
      AND COLUMN_NAME = 'Code'
);

-- ---------------------------------------------------------------------------
-- 2) Ajuster types colonnes (si encore longtext)
-- ---------------------------------------------------------------------------
SET @libelleType := (
    SELECT DATA_TYPE
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CategoriesAgents'
      AND COLUMN_NAME = 'LibelleCategorie'
    LIMIT 1
);

SET @sql := IF(@hasTable = 1 AND @libelleType = 'longtext',
    'ALTER TABLE CategoriesAgents
        MODIFY COLUMN LibelleCategorie VARCHAR(200) NOT NULL,
        MODIFY COLUMN Description VARCHAR(500) NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------------
-- 3) Remplir Code depuis LibelleCategorie existant
-- ---------------------------------------------------------------------------
UPDATE CategoriesAgents
SET Code = UPPER(TRIM(
    CASE
        WHEN LibelleCategorie LIKE '%(%)%'
            THEN SUBSTRING_INDEX(SUBSTRING_INDEX(LibelleCategorie, '(', -1), ')', 1)
        WHEN LibelleCategorie NOT LIKE '% %' AND CHAR_LENGTH(TRIM(LibelleCategorie)) <= 10
            THEN LibelleCategorie
        ELSE LibelleCategorie
    END
))
WHERE @hasCode > 0
  AND (Code IS NULL OR TRIM(Code) = '');

-- ---------------------------------------------------------------------------
-- 4) Aligner Description + LibelleCategorie
-- ---------------------------------------------------------------------------
UPDATE CategoriesAgents
SET Description = CASE UPPER(TRIM(Code))
        WHEN 'AT' THEN 'Agent de Terrain'
        WHEN 'AA' THEN 'Agent Administratif'
        WHEN 'AP' THEN 'Agent Percepteur'
        WHEN 'AS' THEN 'Agent Superviseur'
        WHEN 'CA' THEN 'Caissier'
        WHEN 'AH' THEN 'Agent Hôpital'
        WHEN 'FI' THEN 'Financier'
        WHEN 'IT' THEN 'Technicien'
        WHEN 'AD' THEN 'Admin'
        ELSE COALESCE(NULLIF(TRIM(Description), ''), UPPER(TRIM(Code)))
    END,
    LibelleCategorie = CONCAT(
        CASE UPPER(TRIM(Code))
            WHEN 'AT' THEN 'Agent de Terrain'
            WHEN 'AA' THEN 'Agent Administratif'
            WHEN 'AP' THEN 'Agent Percepteur'
            WHEN 'AS' THEN 'Agent Superviseur'
            WHEN 'CA' THEN 'Caissier'
            WHEN 'AH' THEN 'Agent Hôpital'
            WHEN 'FI' THEN 'Financier'
            WHEN 'IT' THEN 'Technicien'
            WHEN 'AD' THEN 'Admin'
            ELSE COALESCE(NULLIF(TRIM(Description), ''), UPPER(TRIM(Code)))
        END,
        ' (',
        UPPER(TRIM(Code)),
        ')'
    ),
    DateModification = NOW()
WHERE @hasCode > 0
  AND Code IS NOT NULL
  AND TRIM(Code) <> '';

-- ---------------------------------------------------------------------------
-- 5) Rendre Code NOT NULL si encore nullable
-- ---------------------------------------------------------------------------
SET @codeNullable := (
    SELECT IS_NULLABLE
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'CategoriesAgents'
      AND COLUMN_NAME = 'Code'
    LIMIT 1
);

SET @sql := IF(@hasTable = 1 AND @hasCode > 0 AND @codeNullable = 'YES',
    'ALTER TABLE CategoriesAgents MODIFY COLUMN Code VARCHAR(10) NOT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ---------------------------------------------------------------------------
-- 6) Retirer catégorie SP obsolète (Super Admin = rôle, pas catégorie agent)
-- ---------------------------------------------------------------------------
UPDATE Agents a
INNER JOIN CategoriesAgents c ON c.IdCategorieAgent = a.CategorieAgentId
SET a.CategorieAgentId = (
    SELECT IdCategorieAgent FROM CategoriesAgents WHERE UPPER(TRIM(Code)) = 'AD' LIMIT 1
)
WHERE @hasTable = 1
  AND (UPPER(TRIM(c.Code)) = 'SP' OR c.LibelleCategorie IN ('Super Admin (SP)', 'SP'));

DELETE FROM CategoriesAgents
WHERE @hasTable = 1
  AND (UPPER(TRIM(Code)) = 'SP' OR LibelleCategorie IN ('Super Admin (SP)', 'SP'));

-- ---------------------------------------------------------------------------
-- 7) Vérification
-- ---------------------------------------------------------------------------
SELECT
    IdCategorieAgent,
    Code,
    LibelleCategorie,
    Description
FROM CategoriesAgents
ORDER BY IdCategorieAgent;

COMMIT;
