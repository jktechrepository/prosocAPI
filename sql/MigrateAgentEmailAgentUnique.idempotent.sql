-- Idempotent cleanup + dedup before adding UNIQUE index on Agents.EmailAgent
-- DB: MariaDB/MySQL (Pomelo provider)
--
-- Rules:
-- - normalize: TRIM + lower-case
-- - empty string -> NULL
-- - allow multiple NULLs
-- - if duplicates exist after normalization, keep the smallest IdAgent and null out the others

START TRANSACTION;

-- 1) Normalize existing values (trim/lower), convert '' to NULL
UPDATE Agents
SET EmailAgent = NULL
WHERE EmailAgent IS NOT NULL AND TRIM(EmailAgent) = '';

UPDATE Agents
SET EmailAgent = LOWER(TRIM(EmailAgent))
WHERE EmailAgent IS NOT NULL;

-- 2) Report duplicates (should be empty before creating unique index)
SELECT
  LOWER(TRIM(EmailAgent)) AS EmailAgentNormalized,
  COUNT(*) AS Cnt,
  GROUP_CONCAT(IdAgent ORDER BY IdAgent) AS AgentIds
FROM Agents
WHERE EmailAgent IS NOT NULL
GROUP BY LOWER(TRIM(EmailAgent))
HAVING COUNT(*) > 1;

-- 3) Deterministic dedup: keep MIN(IdAgent), null others
UPDATE Agents a
JOIN (
  SELECT MIN(IdAgent) AS KeepId, LOWER(TRIM(EmailAgent)) AS EmailAgentNormalized
  FROM Agents
  WHERE EmailAgent IS NOT NULL
  GROUP BY LOWER(TRIM(EmailAgent))
  HAVING COUNT(*) > 1
) d
  ON LOWER(TRIM(a.EmailAgent)) = d.EmailAgentNormalized
SET a.EmailAgent = NULL
WHERE a.EmailAgent IS NOT NULL AND a.IdAgent <> d.KeepId;

-- 4) Re-check duplicates after cleanup (must return 0 rows)
SELECT
  LOWER(TRIM(EmailAgent)) AS EmailAgentNormalized,
  COUNT(*) AS Cnt,
  GROUP_CONCAT(IdAgent ORDER BY IdAgent) AS AgentIds
FROM Agents
WHERE EmailAgent IS NOT NULL
GROUP BY LOWER(TRIM(EmailAgent))
HAVING COUNT(*) > 1;

COMMIT;

