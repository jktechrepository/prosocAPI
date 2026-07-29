-- =============================================================================
-- Migration : permission ENCODE_ADHESION_NIVEAU_2 (rôle Agent AA)
-- =============================================================================
-- Endpoint : PUT /api/Adhesion/{id}/niveau-2-encodeur
-- Idempotent : crée la permission si absente, l'attribue à Agent (AA).
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateEncodeAdhesionNiveau2Permission.idempotent.sql
--
-- Après exécution : déconnecter / reconnecter les comptes Agent (AA) (JWT).
-- =============================================================================

START TRANSACTION;

INSERT INTO Permissions (Nom, Description, Categorie, Action, Statut, DateCreation)
SELECT
    'ENCODE_ADHESION_NIVEAU_2',
    'Encoder / valider le dossier adhésion niveau 2 (encodeur)',
    'ADHESION',
    'ENCODE',
    1,
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE Nom = 'ENCODE_ADHESION_NIVEAU_2');

SET @PermId := (
    SELECT IdPermission FROM Permissions
    WHERE Nom = 'ENCODE_ADHESION_NIVEAU_2' AND Statut = 1
    LIMIT 1
);

SET @AaRoleId := (SELECT IdRole FROM Roles WHERE Nom = 'Agent (AA)' LIMIT 1);

INSERT INTO RolePermissions (RoleId, PermissionId, DateAttribution)
SELECT @AaRoleId, @PermId, NOW()
WHERE @AaRoleId IS NOT NULL
  AND @PermId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions rp
      WHERE rp.RoleId = @AaRoleId AND rp.PermissionId = @PermId
  );

COMMIT;

SELECT
    CASE
        WHEN @AaRoleId IS NULL THEN '❌ Rôle Agent (AA) introuvable'
        WHEN @PermId IS NULL THEN '❌ Permission ENCODE_ADHESION_NIVEAU_2 introuvable'
        ELSE '✅ Permission ENCODE_ADHESION_NIVEAU_2 migrée pour Agent (AA).'
    END AS Resultat;
