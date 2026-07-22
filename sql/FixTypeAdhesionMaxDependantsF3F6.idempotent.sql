-- =============================================================================
-- Correction MaxDependants F3 / F6
-- F3 : famille de 3 personnes assurées → max 2 dépendants
-- F6 : famille de 6 personnes assurées → max 5 dépendants
-- Idempotent — réexécutable sans erreur.
--
-- Vérification avant déploiement :
--   SELECT Libelle, MaxDependants, Description FROM TypeAdhesions WHERE Libelle IN ('F3', 'F6');
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/FixTypeAdhesionMaxDependantsF3F6.idempotent.sql
-- =============================================================================

START TRANSACTION;

UPDATE `TypeAdhesions`
SET
    `MaxDependants` = 2,
    `Description` = 'Adhésion familiale (titulaire + 2 personnes à charge)',
    `DateModification` = NOW()
WHERE `Libelle` = 'F3'
  AND (`MaxDependants` <> 2 OR `Description` IS NULL OR `Description` NOT LIKE '%2 personnes%');

UPDATE `TypeAdhesions`
SET
    `MaxDependants` = 5,
    `Description` = 'Adhésion familiale (titulaire + 5 personnes à charge)',
    `DateModification` = NOW()
WHERE `Libelle` = 'F6'
  AND (`MaxDependants` <> 5 OR `Description` IS NULL OR `Description` NOT LIKE '%5 personnes%');

COMMIT;

-- Contrôle post-migration :
-- SELECT Libelle, MaxDependants, Description FROM TypeAdhesions WHERE Libelle IN ('F3', 'F6');
