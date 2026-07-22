-- SCRIPT DE MIGRATION DES DONNÉES EXISTANTES
-- À exécuter après l'application de la migration EF Core

-- =====================================================
-- ÉTAPE 1 : INSÉRER LES FRAIS PAR DÉFAUT SI NÉCESSAIRE
-- =====================================================

-- Vérifier si les frais par défaut existent déjà
INSERT IGNORE INTO Frais (Libelle, Montant, DeviseId, CreeParId, DateCreation, Statut, EstSupprime) 
VALUES 
('Frais Adhesion', 5000.00, 1, 1, NOW(), 1, 0),
('Frais Carte Membre', 2000.00, 1, 1, NOW(), 1, 0),
('Frais Dossier', 1000.00, 1, 1, NOW(), 1, 0),
('Frais Traitement', 1500.00, 1, 1, NOW(), 1, 0),
('Frais Inscription', 3000.00, 1, 1, NOW(), 1, 0),
('Frais Certificat', 2500.00, 1, 1, NOW(), 1, 0),
('Frais Administratifs', 500.00, 1, 1, NOW(), 1, 0),
('Frais Timbre', 200.00, 1, 1, NOW(), 1, 0);

-- =====================================================
-- ÉTAPE 2 : CLASSIFIER LES COLLECTES EXISTANTES
-- =====================================================

-- Marquer comme SOUSCRIPTION les collectes avec SouscriptionPrestationId
UPDATE Collectes 
SET TypeCollecte = 2  -- TypeCollecte.Souscription
WHERE SouscriptionPrestationId IS NOT NULL;

-- Marquer comme FRAIS les collectes sans SouscriptionPrestationId
UPDATE Collectes 
SET TypeCollecte = 1  -- TypeCollecte.Frais
WHERE SouscriptionPrestationId IS NULL AND TypeCollecte = 1;

-- =====================================================
-- ÉTAPE 3 : LIER LES COLLECTES DE FRAIS AUX FRAIS APPROPRIÉS
-- =====================================================

-- Stratégie 1 : Basé sur les montants courants des frais
-- Lier les collectes de 5000 CDF au frais d'adhésion
UPDATE Collectes 
SET FraisId = (SELECT IdFrais FROM Frais WHERE Libelle = 'Frais Adhesion' AND EstSupprime = 0 LIMIT 1)
WHERE TypeCollecte = 1 
AND Montant = 5000.00 
AND SouscriptionPrestationId IS NULL
AND DeviseId = 1  -- CDF
AND FraisId IS NULL;

-- Lier les collectes de 2000 CDF au frais de carte membre
UPDATE Collectes 
SET FraisId = (SELECT IdFrais FROM Frais WHERE Libelle = 'Frais Carte Membre' AND EstSupprime = 0 LIMIT 1)
WHERE TypeCollecte = 1 
AND Montant = 2000.00 
AND SouscriptionPrestationId IS NULL
AND DeviseId = 1  -- CDF
AND FraisId IS NULL;

-- Lier les collectes de 1000 CDF au frais de dossier
UPDATE Collectes 
SET FraisId = (SELECT IdFrais FROM Frais WHERE Libelle = 'Frais Dossier' AND EstSupprime = 0 LIMIT 1)
WHERE TypeCollecte = 1 
AND Montant = 1000.00 
AND SouscriptionPrestationId IS NULL
AND DeviseId = 1  -- CDF
AND FraisId IS NULL;

-- Lier les collectes de 1500 CDF au frais de traitement
UPDATE Collectes 
SET FraisId = (SELECT IdFrais FROM Frais WHERE Libelle = 'Frais Traitement' AND EstSupprime = 0 LIMIT 1)
WHERE TypeCollecte = 1 
AND Montant = 1500.00 
AND SouscriptionPrestationId IS NULL
AND DeviseId = 1  -- CDF
AND FraisId IS NULL;

-- Lier les collectes de 3000 CDF au frais d'inscription
UPDATE Collectes 
SET FraisId = (SELECT IdFrais FROM Frais WHERE Libelle = 'Frais Inscription' AND EstSupprime = 0 LIMIT 1)
WHERE TypeCollecte = 1 
AND Montant = 3000.00 
AND SouscriptionPrestationId IS NULL
AND DeviseId = 1  -- CDF
AND FraisId IS NULL;

-- Lier les collectes de 2500 CDF au frais de certificat
UPDATE Collectes 
SET FraisId = (SELECT IdFrais FROM Frais WHERE Libelle = 'Frais Certificat' AND EstSupprime = 0 LIMIT 1)
WHERE TypeCollecte = 1 
AND Montant = 2500.00 
AND SouscriptionPrestationId IS NULL
AND DeviseId = 1  -- CDF
AND FraisId IS NULL;

-- Lier les collectes de 500 CDF au frais administratifs
UPDATE Collectes 
SET FraisId = (SELECT IdFrais FROM Frais WHERE Libelle = 'Frais Administratifs' AND EstSupprime = 0 LIMIT 1)
WHERE TypeCollecte = 1 
AND Montant = 500.00 
AND SouscriptionPrestationId IS NULL
AND DeviseId = 1  -- CDF
AND FraisId IS NULL;

-- Lier les collectes de 200 CDF au frais de timbre
UPDATE Collectes 
SET FraisId = (SELECT IdFrais FROM Frais WHERE Libelle = 'Frais Timbre' AND EstSupprime = 0 LIMIT 1)
WHERE TypeCollecte = 1 
AND Montant = 200.00 
AND SouscriptionPrestationId IS NULL
AND DeviseId = 1  -- CDF
AND FraisId IS NULL;

-- =====================================================
-- ÉTAPE 4 : GÉRER LES COLLECTES DE FRAIS NON CLASSIFIÉES
-- =====================================================

-- Pour les collectes de frais restantes (montants non standards), 
-- les lier au frais "Frais Dossier" par défaut
UPDATE Collectes 
SET FraisId = (SELECT IdFrais FROM Frais WHERE Libelle = 'Frais Dossier' AND EstSupprime = 0 LIMIT 1)
WHERE TypeCollecte = 1 
AND FraisId IS NULL
AND SouscriptionPrestationId IS NULL;

-- =====================================================
-- ÉTAPE 5 : VALIDATION DE LA MIGRATION
-- =====================================================

-- Rapport de migration
SELECT 
    'RAPPORT DE MIGRATION' as Rapport,
    NOW() as DateMigration;

-- Total des collectes par type
SELECT 
    'TOTAL PAR TYPE' as Type,
    TypeCollecte,
    COUNT(*) as Nombre,
    SUM(Montant) as TotalMontant
FROM Collectes 
GROUP BY TypeCollecte;

-- Collectes de frais avec FraisId assigné
SELECT 
    'FRAIS AVEC ID' as Type,
    COUNT(*) as Nombre,
    COUNT(CASE WHEN FraisId IS NOT NULL THEN 1 END) as AvecFraisId,
    COUNT(CASE WHEN FraisId IS NULL THEN 1 END) as SansFraisId
FROM Collectes 
WHERE TypeCollecte = 1;

-- Collectes de souscription (devraient avoir SouscriptionPrestationId)
SELECT 
    'SOUSCRIPTIONS' as Type,
    COUNT(*) as Nombre,
    COUNT(CASE WHEN SouscriptionPrestationId IS NOT NULL THEN 1 END) as AvecSouscriptionId,
    COUNT(CASE WHEN SouscriptionPrestationId IS NULL THEN 1 END) as SansSouscriptionId
FROM Collectes 
WHERE TypeCollecte = 2;

-- Échantillon de collectes migrées pour vérification manuelle
SELECT 
    'ECHANTILLON' as Type,
    c.IdCollecte,
    c.TypeCollecte,
    c.FraisId,
    f.Libelle as FraisLibelle,
    c.Montant,
    c.SouscriptionPrestationId,
    c.ReferencePaiement,
    c.DateCreation
FROM Collectes c
LEFT JOIN Frais f ON c.FraisId = f.IdFrais
ORDER BY c.DateCreation DESC
LIMIT 10;

-- =====================================================
-- ÉTAPE 6 : NETTOYAGE ET VALIDATION FINALE
-- =====================================================

-- S'assurer qu'il n'y a pas de collectes invalides
SELECT 
    'VALIDATION' as Type,
    COUNT(*) as NombreInvalides
FROM Collectes 
WHERE (TypeCollecte = 1 AND FraisId IS NULL) 
   OR (TypeCollecte = 2 AND SouscriptionPrestationId IS NULL);

-- Message de fin
SELECT 
    'MIGRATION TERMINÉE' as Statut,
    'Vérifiez les rapports ci-dessus pour valider la migration' as Message;
