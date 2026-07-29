-- =============================================================================
-- Migration : DemandeRechargeWalletVirtuel + seed plafond ParametresMetier
-- =============================================================================
-- Usage :
--   mysql -h <host> -u <user> -p <database> < sql/MigrateDemandeRechargeWalletVirtuel.idempotent.sql
-- =============================================================================

START TRANSACTION;

-- Table DemandesRechargeWalletVirtuel
CREATE TABLE IF NOT EXISTS DemandesRechargeWalletVirtuel (
    IdDemande INT NOT NULL AUTO_INCREMENT,
    AgentId INT NOT NULL,
    MontantCalcule DECIMAL(18,2) NOT NULL,
    SoldeAuMomentDemande DECIMAL(18,2) NOT NULL,
    PlafondAuMomentDemande DECIMAL(18,2) NOT NULL,
    StatutDemande VARCHAR(20) NOT NULL,
    Motif VARCHAR(500) NULL,
    MotifRejet VARCHAR(500) NULL,
    DateDemande DATETIME(6) NOT NULL,
    DateConfirmation DATETIME(6) NULL,
    DateRejet DATETIME(6) NULL,
    DemandeParUtilisateurId INT NOT NULL,
    ConfirmeParUtilisateurId INT NULL,
    RejeteParUtilisateurId INT NULL,
    WalletVirtuelMouvementId INT NULL,
    MontantCredite DECIMAL(18,2) NULL,
    SoldeAvantCredit DECIMAL(18,2) NULL,
    SoldeApresCredit DECIMAL(18,2) NULL,
    DateCreation DATETIME(6) NOT NULL,
    DateModification DATETIME(6) NULL,
    Statut TINYINT(1) NOT NULL DEFAULT 1,
    CONSTRAINT PK_DemandesRechargeWalletVirtuel PRIMARY KEY (IdDemande),
    CONSTRAINT FK_DemandesRechargeWalletVirtuel_Agents_AgentId
        FOREIGN KEY (AgentId) REFERENCES Agents (IdAgent),
    CONSTRAINT FK_DemandesRechargeWalletVirtuel_Utilisateurs_DemandeParUtilisateurId
        FOREIGN KEY (DemandeParUtilisateurId) REFERENCES Utilisateurs (IdUtilisateur),
    CONSTRAINT FK_DemandesRechargeWalletVirtuel_Utilisateurs_ConfirmeParUtilisateurId
        FOREIGN KEY (ConfirmeParUtilisateurId) REFERENCES Utilisateurs (IdUtilisateur),
    CONSTRAINT FK_DemandesRechargeWalletVirtuel_Utilisateurs_RejeteParUtilisateurId
        FOREIGN KEY (RejeteParUtilisateurId) REFERENCES Utilisateurs (IdUtilisateur),
    CONSTRAINT FK_DemandesRechargeWalletVirtuel_WalletVirtuelMouvements_WalletVirtuelMouvementId
        FOREIGN KEY (WalletVirtuelMouvementId) REFERENCES WalletVirtuelMouvements (IdWalletVirtuelMouvement)
) CHARACTER SET utf8mb4;

-- Index AgentId + StatutDemande
SET @hasIdx := (
    SELECT COUNT(*) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'DemandesRechargeWalletVirtuel'
      AND INDEX_NAME = 'IX_DemandesRechargeWalletVirtuel_AgentId_StatutDemande'
);
SET @sql := IF(@hasIdx = 0,
    'CREATE INDEX IX_DemandesRechargeWalletVirtuel_AgentId_StatutDemande ON DemandesRechargeWalletVirtuel (AgentId, StatutDemande)',
    'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Seed paramètre plafond wallet virtuel (défaut 100)
INSERT INTO ParametresMetier (Code, ValeurJson, DateCreation)
SELECT
    'WALLET_VIRTUEL',
    '{"plafondSolde":100}',
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM ParametresMetier WHERE Code = 'WALLET_VIRTUEL');

COMMIT;

SELECT '✅ Table DemandesRechargeWalletVirtuel + seed WALLET_VIRTUEL OK.' AS Resultat;
