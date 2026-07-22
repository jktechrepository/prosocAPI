-- =============================================================================
-- Normalisation des numéros de téléphone (RDC → +243XXXXXXXXX)
-- =============================================================================
-- Aligne la base sur PhoneNumberHelper (C#) :
--   0812345678      → +243812345678
--   243812345678    → +243812345678
--   +243 81 234 56 78 → +243812345678
--   812345678 (9 chiffres) → +243812345678
--
-- Tables : Utilisateurs, Agents, Affilies, Dependants
-- Idempotent : les numéros déjà au format +243XXXXXXXXX ne sont pas modifiés.
--
-- ⚠️  Utilisateurs.PhoneUtilisateur a un index UNIQUE : les doublons après
--     normalisation sont listés et exclus de la mise à jour (CanUpdate = 0).
--
-- Vérification avant déploiement :
--   SELECT PhoneUtilisateur, COUNT(*) FROM Utilisateurs
--   WHERE PhoneUtilisateur IS NOT NULL GROUP BY PhoneUtilisateur HAVING COUNT(*) > 1;
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/NormalizePhoneNumbers.idempotent.sql
-- =============================================================================

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- Fonction inline : chiffres seuls
-- ---------------------------------------------------------------------------
DROP TEMPORARY TABLE IF EXISTS tmp_normalize_utilisateurs;
CREATE TEMPORARY TABLE tmp_normalize_utilisateurs (
    IdUtilisateur   INT NOT NULL PRIMARY KEY,
    OldPhone        VARCHAR(30) NULL,
    Digits          VARCHAR(20) NULL,
    NewPhone        VARCHAR(30) NULL,
    CanUpdate       TINYINT(1) NOT NULL DEFAULT 0,
    ConflictReason  VARCHAR(200) NULL
);

INSERT INTO tmp_normalize_utilisateurs (IdUtilisateur, OldPhone, Digits, NewPhone)
SELECT
    u.IdUtilisateur,
    u.PhoneUtilisateur,
    REGEXP_REPLACE(TRIM(u.PhoneUtilisateur), '[^0-9]', '') AS Digits,
    CASE
        WHEN LENGTH(REGEXP_REPLACE(TRIM(u.PhoneUtilisateur), '[^0-9]', '')) = 10
             AND LEFT(REGEXP_REPLACE(TRIM(u.PhoneUtilisateur), '[^0-9]', ''), 1) = '0'
            THEN CONCAT('+243', SUBSTRING(REGEXP_REPLACE(TRIM(u.PhoneUtilisateur), '[^0-9]', ''), 2, 9))
        WHEN LENGTH(REGEXP_REPLACE(TRIM(u.PhoneUtilisateur), '[^0-9]', '')) = 12
             AND LEFT(REGEXP_REPLACE(TRIM(u.PhoneUtilisateur), '[^0-9]', ''), 3) = '243'
            THEN CONCAT('+', REGEXP_REPLACE(TRIM(u.PhoneUtilisateur), '[^0-9]', ''))
        WHEN LENGTH(REGEXP_REPLACE(TRIM(u.PhoneUtilisateur), '[^0-9]', '')) = 9
             AND LEFT(REGEXP_REPLACE(TRIM(u.PhoneUtilisateur), '[^0-9]', ''), 1) <> '0'
            THEN CONCAT('+243', REGEXP_REPLACE(TRIM(u.PhoneUtilisateur), '[^0-9]', ''))
        ELSE NULL
    END AS NewPhone
FROM Utilisateurs u
WHERE u.PhoneUtilisateur IS NOT NULL
  AND TRIM(u.PhoneUtilisateur) <> '';

-- Candidats à la mise à jour
UPDATE tmp_normalize_utilisateurs
SET CanUpdate = 1
WHERE NewPhone IS NOT NULL
  AND NewPhone <> TRIM(OldPhone)
  AND OldPhone NOT REGEXP '^\\+243[0-9]{9}$';

-- Doublons internes (plusieurs utilisateurs → même NewPhone)
UPDATE tmp_normalize_utilisateurs t
INNER JOIN (
    SELECT NewPhone
    FROM tmp_normalize_utilisateurs
    WHERE CanUpdate = 1 AND NewPhone IS NOT NULL
    GROUP BY NewPhone
    HAVING COUNT(*) > 1
) d ON d.NewPhone = t.NewPhone
SET t.CanUpdate = 0,
    t.ConflictReason = 'Doublon après normalisation (plusieurs utilisateurs)';

-- Conflit avec un utilisateur déjà au numéro cible
UPDATE tmp_normalize_utilisateurs t
INNER JOIN Utilisateurs u
    ON u.PhoneUtilisateur = t.NewPhone
   AND u.IdUtilisateur <> t.IdUtilisateur
SET t.CanUpdate = 0,
    t.ConflictReason = CONCAT('Conflit avec utilisateur Id=', u.IdUtilisateur)
WHERE t.CanUpdate = 1;

SELECT '--- Utilisateurs : aperçu avant migration ---' AS Etape;
SELECT
    IdUtilisateur,
    OldPhone,
    NewPhone,
    CanUpdate,
    ConflictReason
FROM tmp_normalize_utilisateurs
WHERE NewPhone IS NOT NULL
ORDER BY CanUpdate ASC, IdUtilisateur;

SELECT COUNT(*) AS NbUtilisateursAMigrer
FROM tmp_normalize_utilisateurs
WHERE CanUpdate = 1;

SELECT COUNT(*) AS NbUtilisateursEnConflit
FROM tmp_normalize_utilisateurs
WHERE ConflictReason IS NOT NULL;

UPDATE Utilisateurs u
INNER JOIN tmp_normalize_utilisateurs t ON t.IdUtilisateur = u.IdUtilisateur
SET u.PhoneUtilisateur = t.NewPhone
WHERE t.CanUpdate = 1;

SELECT ROW_COUNT() AS NbUtilisateursMisAJour;

-- ---------------------------------------------------------------------------
-- Agents.Phone
-- ---------------------------------------------------------------------------
SELECT '--- Agents ---' AS Etape;

UPDATE Agents a
SET a.Phone = CASE
        WHEN LENGTH(REGEXP_REPLACE(TRIM(a.Phone), '[^0-9]', '')) = 10
             AND LEFT(REGEXP_REPLACE(TRIM(a.Phone), '[^0-9]', ''), 1) = '0'
            THEN CONCAT('+243', SUBSTRING(REGEXP_REPLACE(TRIM(a.Phone), '[^0-9]', ''), 2, 9))
        WHEN LENGTH(REGEXP_REPLACE(TRIM(a.Phone), '[^0-9]', '')) = 12
             AND LEFT(REGEXP_REPLACE(TRIM(a.Phone), '[^0-9]', ''), 3) = '243'
            THEN CONCAT('+', REGEXP_REPLACE(TRIM(a.Phone), '[^0-9]', ''))
        WHEN LENGTH(REGEXP_REPLACE(TRIM(a.Phone), '[^0-9]', '')) = 9
             AND LEFT(REGEXP_REPLACE(TRIM(a.Phone), '[^0-9]', ''), 1) <> '0'
            THEN CONCAT('+243', REGEXP_REPLACE(TRIM(a.Phone), '[^0-9]', ''))
        ELSE a.Phone
    END
WHERE a.Phone IS NOT NULL
  AND TRIM(a.Phone) <> ''
  AND a.Phone NOT REGEXP '^\\+243[0-9]{9}$'
  AND (
        (LENGTH(REGEXP_REPLACE(TRIM(a.Phone), '[^0-9]', '')) = 10
         AND LEFT(REGEXP_REPLACE(TRIM(a.Phone), '[^0-9]', ''), 1) = '0')
     OR (LENGTH(REGEXP_REPLACE(TRIM(a.Phone), '[^0-9]', '')) = 12
         AND LEFT(REGEXP_REPLACE(TRIM(a.Phone), '[^0-9]', ''), 3) = '243')
     OR (LENGTH(REGEXP_REPLACE(TRIM(a.Phone), '[^0-9]', '')) = 9
         AND LEFT(REGEXP_REPLACE(TRIM(a.Phone), '[^0-9]', ''), 1) <> '0')
  );

SELECT ROW_COUNT() AS NbAgentsMisAJour;

-- ---------------------------------------------------------------------------
-- Affilies.Telephone
-- ---------------------------------------------------------------------------
SELECT '--- Affilies ---' AS Etape;

UPDATE Affilies a
SET a.Telephone = CASE
        WHEN LENGTH(REGEXP_REPLACE(TRIM(a.Telephone), '[^0-9]', '')) = 10
             AND LEFT(REGEXP_REPLACE(TRIM(a.Telephone), '[^0-9]', ''), 1) = '0'
            THEN CONCAT('+243', SUBSTRING(REGEXP_REPLACE(TRIM(a.Telephone), '[^0-9]', ''), 2, 9))
        WHEN LENGTH(REGEXP_REPLACE(TRIM(a.Telephone), '[^0-9]', '')) = 12
             AND LEFT(REGEXP_REPLACE(TRIM(a.Telephone), '[^0-9]', ''), 3) = '243'
            THEN CONCAT('+', REGEXP_REPLACE(TRIM(a.Telephone), '[^0-9]', ''))
        WHEN LENGTH(REGEXP_REPLACE(TRIM(a.Telephone), '[^0-9]', '')) = 9
             AND LEFT(REGEXP_REPLACE(TRIM(a.Telephone), '[^0-9]', ''), 1) <> '0'
            THEN CONCAT('+243', REGEXP_REPLACE(TRIM(a.Telephone), '[^0-9]', ''))
        ELSE a.Telephone
    END
WHERE a.Telephone IS NOT NULL
  AND TRIM(a.Telephone) <> ''
  AND a.Telephone NOT REGEXP '^\\+243[0-9]{9}$'
  AND (
        (LENGTH(REGEXP_REPLACE(TRIM(a.Telephone), '[^0-9]', '')) = 10
         AND LEFT(REGEXP_REPLACE(TRIM(a.Telephone), '[^0-9]', ''), 1) = '0')
     OR (LENGTH(REGEXP_REPLACE(TRIM(a.Telephone), '[^0-9]', '')) = 12
         AND LEFT(REGEXP_REPLACE(TRIM(a.Telephone), '[^0-9]', ''), 3) = '243')
     OR (LENGTH(REGEXP_REPLACE(TRIM(a.Telephone), '[^0-9]', '')) = 9
         AND LEFT(REGEXP_REPLACE(TRIM(a.Telephone), '[^0-9]', ''), 1) <> '0')
  );

SELECT ROW_COUNT() AS NbAffiliesMisAJour;

-- ---------------------------------------------------------------------------
-- Dependants.Telephone (optionnel, cohérence données)
-- ---------------------------------------------------------------------------
SELECT '--- Dependants ---' AS Etape;

UPDATE Dependants d
SET d.Telephone = CASE
        WHEN LENGTH(REGEXP_REPLACE(TRIM(d.Telephone), '[^0-9]', '')) = 10
             AND LEFT(REGEXP_REPLACE(TRIM(d.Telephone), '[^0-9]', ''), 1) = '0'
            THEN CONCAT('+243', SUBSTRING(REGEXP_REPLACE(TRIM(d.Telephone), '[^0-9]', ''), 2, 9))
        WHEN LENGTH(REGEXP_REPLACE(TRIM(d.Telephone), '[^0-9]', '')) = 12
             AND LEFT(REGEXP_REPLACE(TRIM(d.Telephone), '[^0-9]', ''), 3) = '243'
            THEN CONCAT('+', REGEXP_REPLACE(TRIM(d.Telephone), '[^0-9]', ''))
        WHEN LENGTH(REGEXP_REPLACE(TRIM(d.Telephone), '[^0-9]', '')) = 9
             AND LEFT(REGEXP_REPLACE(TRIM(d.Telephone), '[^0-9]', ''), 1) <> '0'
            THEN CONCAT('+243', REGEXP_REPLACE(TRIM(d.Telephone), '[^0-9]', ''))
        ELSE d.Telephone
    END
WHERE d.Telephone IS NOT NULL
  AND TRIM(d.Telephone) <> ''
  AND d.Telephone NOT REGEXP '^\\+243[0-9]{9}$'
  AND (
        (LENGTH(REGEXP_REPLACE(TRIM(d.Telephone), '[^0-9]', '')) = 10
         AND LEFT(REGEXP_REPLACE(TRIM(d.Telephone), '[^0-9]', ''), 1) = '0')
     OR (LENGTH(REGEXP_REPLACE(TRIM(d.Telephone), '[^0-9]', '')) = 12
         AND LEFT(REGEXP_REPLACE(TRIM(d.Telephone), '[^0-9]', ''), 3) = '243')
     OR (LENGTH(REGEXP_REPLACE(TRIM(d.Telephone), '[^0-9]', '')) = 9
         AND LEFT(REGEXP_REPLACE(TRIM(d.Telephone), '[^0-9]', ''), 1) <> '0')
  );

SELECT ROW_COUNT() AS NbDependantsMisAJour;

-- Resynchroniser Agent → Utilisateur si écart résiduel
UPDATE Utilisateurs u
INNER JOIN Agents a ON a.IdAgent = u.AgentId
SET u.PhoneUtilisateur = a.Phone
WHERE u.AgentId IS NOT NULL
  AND a.Phone IS NOT NULL
  AND TRIM(a.Phone) <> ''
  AND a.Phone REGEXP '^\\+243[0-9]{9}$'
  AND (u.PhoneUtilisateur IS NULL OR u.PhoneUtilisateur <> a.Phone)
  AND NOT EXISTS (
        SELECT 1
        FROM Utilisateurs u2
        WHERE u2.PhoneUtilisateur = a.Phone
          AND u2.IdUtilisateur <> u.IdUtilisateur
  );

SELECT ROW_COUNT() AS NbUtilisateursResynchronisesDepuisAgent;

-- Resynchroniser Affilie → Utilisateur si écart résiduel
UPDATE Utilisateurs u
INNER JOIN Affilies af ON af.IdAffilie = u.AffilieId
SET u.PhoneUtilisateur = af.Telephone
WHERE u.AffilieId IS NOT NULL
  AND af.Telephone IS NOT NULL
  AND TRIM(af.Telephone) <> ''
  AND af.Telephone REGEXP '^\\+243[0-9]{9}$'
  AND (u.PhoneUtilisateur IS NULL OR u.PhoneUtilisateur <> af.Telephone)
  AND NOT EXISTS (
        SELECT 1
        FROM Utilisateurs u2
        WHERE u2.PhoneUtilisateur = af.Telephone
          AND u2.IdUtilisateur <> u.IdUtilisateur
  );

SELECT ROW_COUNT() AS NbUtilisateursResynchronisesDepuisAffilie;

DROP TEMPORARY TABLE IF EXISTS tmp_normalize_utilisateurs;

COMMIT;

SELECT '✅ Normalisation téléphones terminée. Vérifiez NbUtilisateursEnConflit si > 0.' AS Resultat;

-- Contrôle post-migration :
-- SELECT PhoneUtilisateur, COUNT(*) FROM Utilisateurs WHERE PhoneUtilisateur IS NOT NULL
--   AND PhoneUtilisateur NOT REGEXP '^\\+243[0-9]{9}$' GROUP BY PhoneUtilisateur;
-- SELECT Phone FROM Agents WHERE Phone IS NOT NULL AND Phone NOT REGEXP '^\\+243[0-9]{9}$' LIMIT 20;
