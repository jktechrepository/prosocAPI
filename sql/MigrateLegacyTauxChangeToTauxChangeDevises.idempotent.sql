-- =============================================================================
-- Migration données : Devises.TauxChange (legacy) → TauxChangeDevises
-- Idempotent — réexécutable sans doublon.
--
-- Contexte : avant le module multidevise, le taux USD→CDF était stocké sur la
-- devise principale (Devises.TauxChange, ex. 2850 = 1 USD = 2850 CDF).
-- Ce script recopie ces valeurs dans TauxChangeDevises si la colonne legacy
-- existe encore et qu'aucun taux actif n'est déjà présent pour la paire.
--
-- Ordre recommandé en prod :
--   1. Scripts/SeedMultidevise.sql (configuration USD principale + taux par défaut)
--   2. Ce script (backfill depuis legacy si besoin)
--   3. sql/DropDeviseTauxChange.idempotent.sql (suppression colonne)
--   4. Déploiement API sans Devises.TauxChange
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateLegacyTauxChangeToTauxChangeDevises.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @db = DATABASE();

SET @legacy_col_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Devises' AND COLUMN_NAME = 'TauxChange'
);

SET @tcd_table_exists = (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'TauxChangeDevises'
);

-- Backfill : devise principale (TauxChange > 0) → chaque devise active non principale
SET @sql = IF(
    @legacy_col_exists > 0 AND @tcd_table_exists > 0,
    'INSERT INTO `TauxChangeDevises` (
        `DeviseSourceId`,
        `DeviseCibleId`,
        `Taux`,
        `DateEffet`,
        `Statut`,
        `DateCreation`
    )
    SELECT
        p.`IdDevise`,
        l.`IdDevise`,
        p.`TauxChange`,
        ''2020-01-01 00:00:00'',
        1,
        NOW()
    FROM `Devises` p
    INNER JOIN `Devises` l
        ON l.`EstDevisePrincipale` = 0
       AND l.`Statut` = 1
       AND l.`IdDevise` <> p.`IdDevise`
    WHERE p.`EstDevisePrincipale` = 1
      AND p.`Statut` = 1
      AND p.`TauxChange` > 0
      AND NOT EXISTS (
          SELECT 1
          FROM `TauxChangeDevises` t
          WHERE t.`DeviseSourceId` = p.`IdDevise`
            AND t.`DeviseCibleId` = l.`IdDevise`
            AND t.`Statut` = 1
      )',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

COMMIT;

SELECT
    CASE
        WHEN @legacy_col_exists = 0 THEN 'Colonne Devises.TauxChange absente — backfill ignoré (déjà migré).'
        WHEN @tcd_table_exists = 0 THEN 'Table TauxChangeDevises absente — exécuter AddMultideviseModule avant ce script.'
        ELSE 'Backfill legacy TauxChange → TauxChangeDevises terminé.'
    END AS Resultat;

-- Vérification (paires actives récentes)
SELECT
    s.`Code` AS Source,
    c.`Code` AS Cible,
    t.`Taux`,
    t.`DateEffet`,
    t.`Statut`
FROM `TauxChangeDevises` t
JOIN `Devises` s ON s.`IdDevise` = t.`DeviseSourceId`
JOIN `Devises` c ON c.`IdDevise` = t.`DeviseCibleId`
WHERE t.`Statut` = 1
ORDER BY t.`DateEffet` DESC, s.`Code`, c.`Code`
LIMIT 10;
