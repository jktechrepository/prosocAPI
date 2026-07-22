-- Seed multidevise : USD devise principale + taux USD → CDF
-- Idempotent — peut être relancé sans doublon de taux actif identique.

-- 1. Symboles et devise principale
UPDATE Devises SET EstDevisePrincipale = 0 WHERE EstDevisePrincipale = 1;

UPDATE Devises
SET EstDevisePrincipale = 1,
    Symbole = '$',
    Statut = 1
WHERE Code = 'USD';

UPDATE Devises
SET Symbole = 'FC',
    EstDevisePrincipale = 0
WHERE Code = 'CDF';

-- 2. Taux de change USD → CDF (1 USD = 2850 CDF)
INSERT INTO TauxChangeDevises (
    DeviseSourceId,
    DeviseCibleId,
    Taux,
    DateEffet,
    Statut,
    DateCreation
)
SELECT
    s.IdDevise,
    c.IdDevise,
    2850.000000,
    '2020-01-01 00:00:00',
    1,
    NOW()
FROM Devises s
CROSS JOIN Devises c
WHERE s.Code = 'USD'
  AND c.Code = 'CDF'
  AND NOT EXISTS (
      SELECT 1
      FROM TauxChangeDevises t
      WHERE t.DeviseSourceId = s.IdDevise
        AND t.DeviseCibleId = c.IdDevise
        AND t.Taux = 2850.000000
        AND t.Statut = 1
  );

-- 3. Vérification
SELECT
    d.Code,
    d.Nom,
    d.Symbole,
    d.EstDevisePrincipale,
    d.Statut
FROM Devises d
WHERE d.Code IN ('USD', 'CDF')
ORDER BY d.EstDevisePrincipale DESC, d.Code;

SELECT
    s.Code AS Source,
    c.Code AS Cible,
    t.Taux,
    t.DateEffet,
    t.Statut
FROM TauxChangeDevises t
JOIN Devises s ON s.IdDevise = t.DeviseSourceId
JOIN Devises c ON c.IdDevise = t.DeviseCibleId
WHERE s.Code = 'USD' AND c.Code = 'CDF'
ORDER BY t.DateEffet DESC
LIMIT 5;
