-- =============================================================================
-- Seed : paramètres métier RetraitAgent (valeurs par défaut production)
-- =============================================================================
-- Idempotent : insère les lignes si absentes.
-- Aligné sur appsettings.json (Fenetre1 15-20, Fenetre2 7 jours, montant 5).
--
-- PRÉREQUIS : la table ParametresMetier doit exister.
--   Si erreur #1146 « table parametresmetier n'existe pas », exécuter D'ABORD :
--   sql/MigrateParametresMetier.idempotent.sql
--   (ou le script tout-en-un sql/DeployParametresMetierUat.idempotent.sql)
--
-- Ordre UAT complet :
--   1. sql/MigrateParametresMetier.idempotent.sql
--   2. sql/MigrateParametresMetierPermissions.idempotent.sql
--   3. sql/SeedParametresMetierRetraitAgent.idempotent.sql  (ce fichier)
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/SeedParametresMetierRetraitAgent.idempotent.sql
-- =============================================================================

START TRANSACTION;

INSERT INTO ParametresMetier (Code, ValeurJson, DateCreation)
SELECT
    'RETRAIT_AGENT',
    '{"fenetre1Debut":15,"fenetre1Fin":20,"fenetre2DerniersJours":7,"montantMinimumPartiel":5}',
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM ParametresMetier WHERE Code = 'RETRAIT_AGENT');

INSERT INTO ParametresMetier (Code, ValeurJson, DateCreation)
SELECT
    'AGENT_MAASH',
    '{"montantRetenueUsd":5,"deviseId":2,"codesCategoriesEligibles":["AT","AA","AP","AS","CA","FI","IT","AD"],"nomProduitMaash":"MAASH","retenueAutomatiqueActivee":true,"jourExecution":1,"heureExecution":2,"intervalleControleMinutes":60,"retenterEchecsQuotidiennement":true}',
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM ParametresMetier WHERE Code = 'AGENT_MAASH');

INSERT INTO ParametresMetier (Code, ValeurJson, DateCreation)
SELECT
    'ARRIERES',
    '{"generationAutomatiqueActivee":true,"heureExecution":0,"minuteExecution":30,"intervalleControleMinutes":600,"jourEcheanceMensuelle":1}',
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM ParametresMetier WHERE Code = 'ARRIERES');

INSERT INTO ParametresMetier (Code, ValeurJson, DateCreation)
SELECT
    'PENALITE',
    '{"applicationAutomatiqueActivee":true,"delaiGraceJours":3,"fraisPenaliteCode":"PENALITE_RETARD_COTISATION","retardCotisationActive":true}',
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM ParametresMetier WHERE Code = 'PENALITE');

COMMIT;

SELECT '✅ Paramètres métier seedés (RETRAIT_AGENT, AGENT_MAASH, ARRIERES, PENALITE).' AS Resultat;
