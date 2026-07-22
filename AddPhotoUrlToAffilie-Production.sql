-- ============================================================================
-- Migration: AddPhotoUrlToAffilie
-- Date: 2026-03-10 16:48:00
-- Description: Ajout du champ PhotoUrl à la table Affilies
-- ============================================================================

-- Script idempotent pour la production
-- Ajoute la colonne PhotoUrl à la table Affilies si elle n'existe pas déjà

SET @exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                 WHERE TABLE_SCHEMA = DATABASE() 
                 AND TABLE_NAME = 'Affilies' 
                 AND COLUMN_NAME = 'PhotoUrl');

-- Ajouter la colonne PhotoUrl seulement si elle n'existe pas
SET @sql = IF @exists = 0 THEN
    'ALTER TABLE `Affilies` ADD COLUMN `PhotoUrl` VARCHAR(500) CHARACTER SET utf8mb4 NULL;'
ELSE
    'SELECT '' AS message;'; -- La colonne existe déjà
END;

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Mettre à jour la table des migrations pour marquer cette migration comme appliquée
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260310164845_AddPhotoUrlToAffilie', '6.0.25');

-- Afficher le résultat
SELECT IF(@exists = 0, 
    'Colonne PhotoUrl ajoutée avec succès à la table Affilies', 
    'La colonne PhotoUrl existe déjà dans la table Affilies') AS Resultat;

-- ============================================================================
-- Instructions pour le rollback (si nécessaire) :
-- DROP COLUMN `PhotoUrl` FROM `Affilies`;
-- DELETE FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310164845_AddPhotoUrlToAffilie';
-- ============================================================================
