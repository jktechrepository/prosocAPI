-- ============================================================================
-- Migration: AddPhotoUrlToAgent
-- Date: 2026-03-10 16:18:00
-- Description: Ajout du champ PhotoUrl à la table Agents
-- ============================================================================

-- Script idempotent pour la production
-- Ajoute la colonne PhotoUrl à la table Agents si elle n'existe pas déjà

SET @exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                 WHERE TABLE_SCHEMA = DATABASE() 
                 AND TABLE_NAME = 'Agents' 
                 AND COLUMN_NAME = 'PhotoUrl');

-- Ajouter la colonne PhotoUrl seulement si elle n'existe pas
SET @sql = IF @exists = 0 THEN
    'ALTER TABLE `Agents` ADD COLUMN `PhotoUrl` VARCHAR(500) CHARACTER SET utf8mb4 NULL;'
ELSE
    'SELECT '' AS message;'; -- La colonne existe déjà
END;

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Mettre à jour la table des migrations pour marquer cette migration comme appliquée
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260310134920_AddPhotoUrlToAgent', '6.0.25');

-- Afficher le résultat
SELECT IF(@exists = 0, 
    'Colonne PhotoUrl ajoutée avec succès à la table Agents', 
    'La colonne PhotoUrl existe déjà dans la table Agents') AS Resultat;

-- ============================================================================
-- Instructions pour le rollback (si nécessaire) :
-- DROP COLUMN `PhotoUrl` FROM `Agents`;
-- DELETE FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310134920_AddPhotoUrlToAgent';
-- ============================================================================
