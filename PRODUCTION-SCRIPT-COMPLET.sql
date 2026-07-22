-- =====================================================
-- SCRIPT DE MIGRATION PRODUCTION PROSOC - SYSTÈME JETONS MÉDICAUX
-- =====================================================
-- Auteur: Cascade AI Assistant
-- Date: 10/03/2026
-- Version: 1.0
-- Base de données: MariaDB 10.6+
-- =====================================================

-- ⚠️  IMPORTANT: Exécuter ce script sur une base de données sauvegardée
-- ⚠️  Vérifier que vous avez les permissions nécessaires
-- ⚠️  Ce script est idempotent - peut être exécuté plusieurs fois

START TRANSACTION;

-- =====================================================
-- 1. CRÉATION TABLE HÔPITAUX PARTENAIRES
-- =====================================================

CREATE TABLE IF NOT EXISTS `HopitalPartenaires` (
    `IdHopital` int NOT NULL AUTO_INCREMENT,
    `Nom` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Adresse` varchar(500) CHARACTER SET utf8mb4 NULL,
    `Telephone` varchar(100) CHARACTER SET utf8mb4 NULL,
    `Email` varchar(200) CHARACTER SET utf8mb4 NULL,
    `ContactPersonne` varchar(100) CHARACTER SET utf8mb4 NULL,
    `CodeAcces` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Niveau` varchar(20) CHARACTER SET utf8mb4 NULL,
    `EstActif` tinyint(1) NOT NULL DEFAULT TRUE,
    `ServicesOfferts` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `PlafondJournalier` decimal(18,2) NULL,
    `DateCreation` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL DEFAULT TRUE,
    CONSTRAINT `PK_HopitalPartenaires` PRIMARY KEY (`IdHopital`)
) ENGINE=InnoDB CHARACTER SET=utf8mb4;

-- Index pour optimisation
CREATE INDEX IF NOT EXISTS `IX_HopitalPartenaires_CodeAcces` ON `HopitalPartenaires` (`CodeAcces`);
CREATE INDEX IF NOT EXISTS `IX_HopitalPartenaires_EstActif` ON `HopitalPartenaires` (`EstActif`);
CREATE INDEX IF NOT EXISTS `IX_HopitalPartenaires_Statut` ON `HopitalPartenaires` (`Statut`);

-- =====================================================
-- 2. CRÉATION TABLE JETONS MÉDICAUX
-- =====================================================

CREATE TABLE IF NOT EXISTS `JetonsMedicaux` (
    `IdJeton` int NOT NULL AUTO_INCREMENT,
    `AffilieId` int NOT NULL,
    `CodeJeton` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `DateEmission` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `DateUtilisation` datetime(6) NULL,
    `DateExpiration` datetime(6) NULL,
    `EstValide` tinyint(1) NOT NULL DEFAULT TRUE,
    `EstUtilise` tinyint(1) NOT NULL DEFAULT FALSE,
    `HopitalPartenaireId` int NULL,
    `Observation` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DateCreation` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL DEFAULT TRUE,
    CONSTRAINT `PK_JetonsMedicaux` PRIMARY KEY (`IdJeton`),
    CONSTRAINT `UQ_JetonsMedicaux_CodeJeton` UNIQUE (`CodeJeton`)
) ENGINE=InnoDB CHARACTER SET=utf8mb4;

-- Index pour optimisation
CREATE INDEX IF NOT EXISTS `IX_JetonsMedicaux_AffilieId` ON `JetonsMedicaux` (`AffilieId`);
CREATE INDEX IF NOT EXISTS `IX_JetonsMedicaux_CodeJeton` ON `JetonsMedicaux` (`CodeJeton`);
CREATE INDEX IF NOT EXISTS `IX_JetonsMedicaux_HopitalPartenaireId` ON `JetonsMedicaux` (`HopitalPartenaireId`);
CREATE INDEX IF NOT EXISTS `IX_JetonsMedicaux_EstValide` ON `JetonsMedicaux` (`EstValide`);
CREATE INDEX IF NOT EXISTS `IX_JetonsMedicaux_EstUtilise` ON `JetonsMedicaux` (`EstUtilise`);
CREATE INDEX IF NOT EXISTS `IX_JetonsMedicaux_DateExpiration` ON `JetonsMedicaux` (`DateExpiration`);

-- =====================================================
-- 3. CRÉATION TABLE DEMANDES DE BON D'ENVOI
-- =====================================================

CREATE TABLE IF NOT EXISTS `DemandesBonEnvoi` (
    `IdDemande` int NOT NULL AUTO_INCREMENT,
    `AffilieId` int NOT NULL,
    `PrestationId` int NOT NULL,
    `TypeDemande` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `MotifDemande` varchar(500) CHARACTER SET utf8mb4 NULL,
    `AgentId` int NOT NULL,
    `ObservationAgent` varchar(500) CHARACTER SET utf8mb4 NULL,
    `DateDemande` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `DateValidation` datetime(6) NULL,
    `StatutDemande` varchar(20) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'EN_ATTENTE',
    `BonEnvoiId` int NULL,
    `JetonMedicalId` int NULL,
    `DateCreation` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `DateModification` datetime(6) NULL,
    `Statut` tinyint(1) NOT NULL DEFAULT TRUE,
    CONSTRAINT `PK_DemandesBonEnvoi` PRIMARY KEY (`IdDemande`)
) ENGINE=InnoDB CHARACTER SET=utf8mb4;

-- Index pour optimisation
CREATE INDEX IF NOT EXISTS `IX_DemandesBonEnvoi_AffilieId` ON `DemandesBonEnvoi` (`AffilieId`);
CREATE INDEX IF NOT EXISTS `IX_DemandesBonEnvoi_AgentId` ON `DemandesBonEnvoi` (`AgentId`);
CREATE INDEX IF NOT EXISTS `IX_DemandesBonEnvoi_PrestationId` ON `DemandesBonEnvoi` (`PrestationId`);
CREATE INDEX IF NOT EXISTS `IX_DemandesBonEnvoi_StatutDemande` ON `DemandesBonEnvoi` (`StatutDemande`);
CREATE INDEX IF NOT EXISTS `IX_DemandesBonEnvoi_BonEnvoiId` ON `DemandesBonEnvoi` (`BonEnvoiId`);
CREATE INDEX IF NOT EXISTS `IX_DemandesBonEnvoi_JetonMedicalId` ON `DemandesBonEnvoi` (`JetonMedicalId`);
CREATE INDEX IF NOT EXISTS `IX_DemandesBonEnvoi_DateDemande` ON `DemandesBonEnvoi` (`DateDemande`);

-- =====================================================
-- 4. AJOUT DES CONTRAINTES FOREIGN KEY
-- =====================================================

-- Contraintes pour JetonsMedicaux
ALTER TABLE `JetonsMedicaux` 
ADD CONSTRAINT IF NOT EXISTS `FK_JetonsMedicaux_Affilies_AffilieId` 
FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE;

ALTER TABLE `JetonsMedicaux` 
ADD CONSTRAINT IF NOT EXISTS `FK_JetonsMedicaux_HopitalPartenaires_HopitalPartenaireId` 
FOREIGN KEY (`HopitalPartenaireId`) REFERENCES `HopitalPartenaires` (`IdHopital`);

-- Contraintes pour DemandesBonEnvoi
ALTER TABLE `DemandesBonEnvoi` 
ADD CONSTRAINT IF NOT EXISTS `FK_DemandesBonEnvoi_Affilies_AffilieId` 
FOREIGN KEY (`AffilieId`) REFERENCES `Affilies` (`IdAffilie`) ON DELETE CASCADE;

ALTER TABLE `DemandesBonEnvoi` 
ADD CONSTRAINT IF NOT EXISTS `FK_DemandesBonEnvoi_Agents_AgentId` 
FOREIGN KEY (`AgentId`) REFERENCES `Agents` (`IdAgent`) ON DELETE CASCADE;

ALTER TABLE `DemandesBonEnvoi` 
ADD CONSTRAINT IF NOT EXISTS `FK_DemandesBonEnvoi_Prestations_PrestationId` 
FOREIGN KEY (`PrestationId`) REFERENCES `Prestations` (`Id`) ON DELETE CASCADE;

ALTER TABLE `DemandesBonEnvoi` 
ADD CONSTRAINT IF NOT EXISTS `FK_DemandesBonEnvoi_BonsEnvoi_BonEnvoiId` 
FOREIGN KEY (`BonEnvoiId`) REFERENCES `BonsEnvoi` (`IdBonEnvoi`);

ALTER TABLE `DemandesBonEnvoi` 
ADD CONSTRAINT IF NOT EXISTS `FK_DemandesBonEnvoi_JetonsMedicaux_JetonMedicalId` 
FOREIGN KEY (`JetonMedicalId`) REFERENCES `JetonsMedicaux` (`IdJeton`);

-- =====================================================
-- 5. MISE À JOUR DES TABLES EXISTANTES
-- =====================================================

-- Ajouter la navigation vers JetonsMedicaux dans Affilies (si nécessaire)
ALTER TABLE `Affilies` 
ADD COLUMN IF NOT EXISTS `DateModification` datetime(6) NULL;

-- =====================================================
-- 6. CRÉATION TABLE DE SUIVI DES MIGRATIONS
-- =====================================================

CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB CHARACTER SET=utf8mb4;

-- =====================================================
-- 7. INSERTION DES MIGRATIONS
-- =====================================================

INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) 
VALUES ('20260310183713_AddJetonMedicalSystem', '6.0.25');

INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) 
VALUES ('20260310193335_AddDemandeBonEnvoiSystem', '6.0.25');

-- =====================================================
-- 8. DONNÉES DE DÉMONSTRATION (OPTIONNEL)
-- =====================================================

-- Insertion d'hôpitaux partenaires de démonstration
INSERT IGNORE INTO `HopitalPartenaires` 
(`IdHopital`, `Nom`, `Adresse`, `Telephone`, `Email`, `ContactPersonne`, `CodeAcces`, `Niveau`, `EstActif`, `ServicesOfferts`, `PlafondJournalier`, `DateCreation`, `Statut`) 
VALUES 
(1, 'Hôpital Général de Kinshasa', 'Avenue de la Paix, Kinshasa', '+243812345678', 'contact@hgk.cd', 'Dr. Mukendi', 'HOPKIN001', 'Tertiaire', TRUE, 'Urgence, Chirurgie, Pédiatrie', 5000.00, NOW(), TRUE),
(2, 'Clinique Médical la Source', 'Boulevard du 30 Juin, Kinshasa', '+243812345679', 'info@cliniquelasource.cd', 'Dr. Kalonji', 'CLLSRC002', 'Secondaire', TRUE, 'Consultation, Laboratoire, Imagerie', 2000.00, NOW(), TRUE),
(3, 'Centre Hospitalier Mama Yemo', 'Avenue Kasa-Vubu, Kinshasa', '+243812345680', 'contact@mamayemo.cd', 'Dr. Tshimanga', 'CHMYM003', 'Tertiaire', TRUE, 'Maternité, Pédiatrie, Chirurgie', 6000.00, NOW(), TRUE);

-- =====================================================
-- 9. VÉRIFICATION DE L'INSTALLATION
-- =====================================================

-- Vérification des tables créées
SELECT 'Tables créées avec succès!' as Status;

-- Vérification des index
SELECT COUNT(*) as NombreIndex FROM information_schema.statistics 
WHERE table_schema = DATABASE() 
AND table_name IN ('HopitalPartenaires', 'JetonsMedicaux', 'DemandesBonEnvoi');

-- Vérification des contraintes
SELECT COUNT(*) as NombreContraintes FROM information_schema.table_constraints 
WHERE table_schema = DATABASE() 
AND table_name IN ('HopitalPartenaires', 'JetonsMedicaux', 'DemandesBonEnvoi')
AND constraint_type = 'FOREIGN KEY';

COMMIT;

-- =====================================================
-- 10. RAPPORT D'INSTALLATION
-- =====================================================

SELECT 
    'INSTALLATION TERMINEE' as Statut,
    'Système de Jetons Médicaux PROSOC' as Systeme,
    NOW() as DateInstallation,
    DATABASE() as BaseDeDonnees;

-- =====================================================
-- INSTRUCTIONS POST-INSTALLATION
-- =====================================================

/*
1. Vérifier que toutes les tables sont créées:
   SHOW TABLES LIKE '%Hopital%';
   SHOW TABLES LIKE '%Jeton%';
   SHOW TABLES LIKE '%Demande%';

2. Vérifier les index:
   SHOW INDEX FROM JetonsMedicaux;
   SHOW INDEX FROM DemandesBonEnvoi;

3. Tester l'application:
   - Démarrer l'API
   - Créer un hôpital partenaire
   - Créer une demande de bon d'envoi
   - Générer un jeton médical

4. Sauvegarder la base de données après installation

5. Monitorer les performances:
   - Requêtes sur JetonsMedicaux
   - Requêtes sur DemandesBonEnvoi
   - Index utilisés
*/
