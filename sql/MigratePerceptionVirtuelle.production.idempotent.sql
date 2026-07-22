-- =============================================================================
-- Migration PRODUCTION : Perception collectes VIRTUAL_ACCOUNT
-- =============================================================================
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigratePerceptionVirtuelle.production.idempotent.sql
-- =============================================================================

START TRANSACTION;

SET @hasPerceptions := (
    SELECT COUNT(*) FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PerceptionsVirtuelles'
);

SET @sql := IF(@hasPerceptions = 0,
    'CREATE TABLE PerceptionsVirtuelles (
        IdPerceptionVirtuelle INT NOT NULL AUTO_INCREMENT,
        AgentId INT NOT NULL,
        PercepteurUtilisateurId INT NOT NULL,
        MontantTotal DECIMAL(18,2) NOT NULL,
        DeviseId INT NOT NULL,
        NombreCollectes INT NOT NULL,
        DatePerception DATETIME(6) NOT NULL,
        Observation VARCHAR(500) NULL,
        DateCreation DATETIME(6) NOT NULL,
        DateModification DATETIME(6) NULL,
        Statut TINYINT(1) NOT NULL,
        PRIMARY KEY (IdPerceptionVirtuelle),
        CONSTRAINT FK_PerceptionsVirtuelles_Agents_AgentId FOREIGN KEY (AgentId) REFERENCES Agents (IdAgent),
        CONSTRAINT FK_PerceptionsVirtuelles_Utilisateurs_PercepteurUtilisateurId FOREIGN KEY (PercepteurUtilisateurId) REFERENCES Utilisateurs (IdUtilisateur),
        CONSTRAINT FK_PerceptionsVirtuelles_Devises_DeviseId FOREIGN KEY (DeviseId) REFERENCES Devises (IdDevise)
    )',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasLignes := (
    SELECT COUNT(*) FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PerceptionsVirtuellesLignes'
);

SET @sql := IF(@hasLignes = 0,
    'CREATE TABLE PerceptionsVirtuellesLignes (
        IdLigne INT NOT NULL AUTO_INCREMENT,
        PerceptionVirtuelleId INT NOT NULL,
        CollecteId INT NOT NULL,
        AgentId INT NOT NULL,
        Montant DECIMAL(18,2) NOT NULL,
        WalletVirtuelMouvementId INT NULL,
        DateCreation DATETIME(6) NOT NULL,
        Statut TINYINT(1) NOT NULL,
        PRIMARY KEY (IdLigne),
        UNIQUE KEY IX_PerceptionsVirtuellesLignes_CollecteId (CollecteId),
        CONSTRAINT FK_PerceptionsVirtuellesLignes_PerceptionsVirtuelles FOREIGN KEY (PerceptionVirtuelleId) REFERENCES PerceptionsVirtuelles (IdPerceptionVirtuelle) ON DELETE CASCADE,
        CONSTRAINT FK_PerceptionsVirtuellesLignes_Collectes FOREIGN KEY (CollecteId) REFERENCES Collectes (IdCollecte),
        CONSTRAINT FK_PerceptionsVirtuellesLignes_Agents FOREIGN KEY (AgentId) REFERENCES Agents (IdAgent),
        CONSTRAINT FK_PerceptionsVirtuellesLignes_WalletVirtuelMouvements FOREIGN KEY (WalletVirtuelMouvementId) REFERENCES WalletVirtuelMouvements (IdWalletVirtuelMouvement) ON DELETE SET NULL
    )',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Collectes' AND COLUMN_NAME = 'StatutPerception'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE Collectes ADD COLUMN StatutPerception VARCHAR(20) NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Collectes' AND COLUMN_NAME = 'DatePerception'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE Collectes ADD COLUMN DatePerception DATETIME(6) NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Collectes' AND COLUMN_NAME = 'PercepteurUtilisateurId'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE Collectes ADD COLUMN PercepteurUtilisateurId INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasCol := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Collectes' AND COLUMN_NAME = 'PerceptionVirtuelleId'
);
SET @sql := IF(@hasCol = 0,
    'ALTER TABLE Collectes ADD COLUMN PerceptionVirtuelleId INT NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasIdx := (
    SELECT COUNT(*) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Collectes' AND INDEX_NAME = 'IX_Collectes_PercepteurUtilisateurId'
);
SET @sql := IF(@hasIdx = 0,
    'CREATE INDEX IX_Collectes_PercepteurUtilisateurId ON Collectes (PercepteurUtilisateurId)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasIdx := (
    SELECT COUNT(*) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Collectes' AND INDEX_NAME = 'IX_Collectes_PerceptionVirtuelleId'
);
SET @sql := IF(@hasIdx = 0,
    'CREATE INDEX IX_Collectes_PerceptionVirtuelleId ON Collectes (PerceptionVirtuelleId)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasFk := (
    SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Collectes'
      AND CONSTRAINT_NAME = 'FK_Collectes_Utilisateurs_PercepteurUtilisateurId'
);
SET @sql := IF(@hasFk = 0,
    'ALTER TABLE Collectes ADD CONSTRAINT FK_Collectes_Utilisateurs_PercepteurUtilisateurId FOREIGN KEY (PercepteurUtilisateurId) REFERENCES Utilisateurs (IdUtilisateur) ON DELETE SET NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @hasPerceptions := (
    SELECT COUNT(*) FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PerceptionsVirtuelles'
);
SET @hasFk := (
    SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Collectes'
      AND CONSTRAINT_NAME = 'FK_Collectes_PerceptionsVirtuelles_PerceptionVirtuelleId'
);
SET @sql := IF(@hasFk = 0 AND @hasPerceptions > 0,
    'ALTER TABLE Collectes ADD CONSTRAINT FK_Collectes_PerceptionsVirtuelles_PerceptionVirtuelleId FOREIGN KEY (PerceptionVirtuelleId) REFERENCES PerceptionsVirtuelles (IdPerceptionVirtuelle) ON DELETE SET NULL',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Backfill : collectes VA validées historiques → NON_PERCU
UPDATE Collectes
SET StatutPerception = 'NON_PERCU'
WHERE StatutPerception IS NULL
  AND UPPER(REPLACE(IFNULL(ModePaiement, ''), ' ', '_')) IN ('VIRTUAL_ACCOUNT', 'COMPTE_VIRTUEL')
  AND StatutPaiement IN ('VALIDE', 'Validé', 'Valide', 'OK', 'PAYE', 'PAYÉ', 'Payé', 'Paye');

INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
SELECT '20260629121322_PerceptionVirtuelleCollecte', '6.0.25' FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260629121322_PerceptionVirtuelleCollecte'
);

COMMIT;
