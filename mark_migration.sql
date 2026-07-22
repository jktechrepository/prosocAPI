-- Marquer la migration InitialCreate comme appliquée
-- Ce script résout le problème des tables déjà existantes

INSERT IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
VALUES ('20260305094431_InitialCreate', '6.0.25');

-- Vérification
SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260305094431_InitialCreate';
