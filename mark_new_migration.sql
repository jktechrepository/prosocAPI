-- Marquer la nouvelle migration InitialCreate comme appliquée
-- Ce script résout le problème des tables déjà existantes

INSERT IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
VALUES ('20260305104609_InitialCreate', '6.0.25');

-- Vérification
SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260305104609_InitialCreate';
