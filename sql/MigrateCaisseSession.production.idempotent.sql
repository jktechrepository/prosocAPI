-- =============================================================================
-- Migration PRODUCTION : Session caisse + paiement retrait agent
-- =============================================================================
-- Prérequis : CollecteAgentIdNullable déjà appliquée
--
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateCaisseSession.production.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @hasSessions := (
    SELECT COUNT(*) FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SessionsCaisses'
);

SET @sql := IF(@hasSessions = 0,
    'CREATE TABLE SessionsCaisses (
        IdSessionCaisse INT NOT NULL AUTO_INCREMENT,
        UtilisateurId INT NOT NULL,
        SoldeOuverture DECIMAL(18,2) NOT NULL,
        DeviseId INT NOT NULL,
        Statut VARCHAR(20) NOT NULL,
        DateOuverture DATETIME(6) NOT NULL,
        DateCloture DATETIME(6) NULL,
        ObservationCloture VARCHAR(500) NULL,
        SoldeTheoriqueCloture DECIMAL(18,2) NULL,
        SoldeReelCloture DECIMAL(18,2) NULL,
        DateCreation DATETIME(6) NOT NULL,
        DateModification DATETIME(6) NULL,
        StatutActif TINYINT(1) NOT NULL,
        PRIMARY KEY (IdSessionCaisse),
        CONSTRAINT FK_SessionsCaisses_Utilisateurs_UtilisateurId FOREIGN KEY (UtilisateurId) REFERENCES Utilisateurs (IdUtilisateur),
        CONSTRAINT FK_SessionsCaisses_Devises_DeviseId FOREIGN KEY (DeviseId) REFERENCES Devises (IdDevise)
    )',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasMouvements := (
    SELECT COUNT(*) FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'MouvementsCaisses'
);

SET @sql := IF(@hasMouvements = 0,
    'CREATE TABLE MouvementsCaisses (
        IdMouvementCaisse INT NOT NULL AUTO_INCREMENT,
        SessionCaisseId INT NOT NULL,
        UtilisateurId INT NOT NULL,
        TypeOperation VARCHAR(10) NOT NULL,
        Source VARCHAR(30) NOT NULL,
        Montant DECIMAL(18,2) NOT NULL,
        DeviseId INT NOT NULL,
        DateOperation DATETIME(6) NOT NULL,
        CollecteId INT NULL,
        DemandeRetraitId INT NULL,
        JetonRetraitId INT NULL,
        WalletMouvementId INT NULL,
        Description VARCHAR(500) NULL,
        DateCreation DATETIME(6) NOT NULL,
        Statut TINYINT(1) NOT NULL,
        PRIMARY KEY (IdMouvementCaisse),
        CONSTRAINT FK_MouvementsCaisses_SessionsCaisses_SessionCaisseId FOREIGN KEY (SessionCaisseId) REFERENCES SessionsCaisses (IdSessionCaisse) ON DELETE CASCADE,
        CONSTRAINT FK_MouvementsCaisses_Utilisateurs_UtilisateurId FOREIGN KEY (UtilisateurId) REFERENCES Utilisateurs (IdUtilisateur),
        CONSTRAINT FK_MouvementsCaisses_Devises_DeviseId FOREIGN KEY (DeviseId) REFERENCES Devises (IdDevise)
    )',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'JetonsRetraits' AND COLUMN_NAME = 'OperateurUtilisateurId'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE JetonsRetraits ADD COLUMN OperateurUtilisateurId INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DemandesRetraitAgents' AND COLUMN_NAME = 'OperateurPaiementUtilisateurId'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE DemandesRetraitAgents ADD COLUMN OperateurPaiementUtilisateurId INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DemandesRetraitAgents' AND COLUMN_NAME = 'WalletMouvementId'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE DemandesRetraitAgents ADD COLUMN WalletMouvementId INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
SELECT '20260626093053_CaisseSessionRetraitAgent', '6.0.25' FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260626093053_CaisseSessionRetraitAgent'
);

COMMIT;
