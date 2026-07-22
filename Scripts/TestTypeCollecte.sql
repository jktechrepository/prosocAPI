-- SCRIPT DE TEST POUR VALIDER L'IMPLÉMENTATION TYPECOLLECTE

-- =====================================================
-- ÉTAPE 1 : VÉRIFIER LA STRUCTURE DE LA BASE DE DONNÉES
-- =====================================================

-- Vérifier que les nouvelles colonnes existent
DESCRIBE Collectes;

-- Vérifier les types de données et contraintes
SHOW COLUMNS FROM Collectes WHERE Field IN ('TypeCollecte', 'FraisId');

-- =====================================================
-- ÉTAPE 2 : VÉRIFIER LES DONNÉES EXISTANTES
-- =====================================================

-- Compter les collectes par type
SELECT 
    TypeCollecte,
    COUNT(*) as Nombre,
    SUM(Montant) as TotalMontant
FROM Collectes
GROUP BY TypeCollecte
ORDER BY TypeCollecte;

-- Vérifier les collectes avec FraisId
SELECT 
    COUNT(*) as TotalCollectes,
    COUNT(CASE WHEN FraisId IS NOT NULL THEN 1 END) as AvecFraisId,
    COUNT(CASE WHEN FraisId IS NULL THEN 1 END) as SansFraisId
FROM Collectes;

-- =====================================================
-- ÉTAPE 3 : CRÉER UNE COLLECTE DE TEST MANUELLEMENT
-- =====================================================

-- Insérer une collecte de frais de test
INSERT INTO Collectes (
    TypeCollecte,
    FraisId,
    AffilieId,
    AgentId,
    Montant,
    ModePaiement,
    DeviseId,
    DateCollecte,
    Observation,
    DateCreation,
    Statut
) VALUES (
    1, -- TypeCollecte.Frais
    1, -- FraisId (Frais Adhesion)
    1, -- AffilieId
    1, -- AgentId
    5000.00,
    'Mobile Money',
    2, -- DeviseId (USD)
    NOW(),
    'Test collecte frais manuelle',
    NOW(),
    1
);

-- Insérer une collecte de souscription de test
INSERT INTO Collectes (
    TypeCollecte,
    SouscriptionPrestationId,
    AffilieId,
    AgentId,
    Montant,
    ModePaiement,
    DeviseId,
    DateCollecte,
    Observation,
    DateCreation,
    Statut
) VALUES (
    2, -- TypeCollecte.Souscription
    1, -- SouscriptionPrestationId
    1, -- AffilieId
    1, -- AgentId
    10000.00,
    'Compte Virtuel',
    2, -- DeviseId (USD)
    NOW(),
    'Test collecte souscription manuelle',
    NOW(),
    1
);

-- =====================================================
-- ÉTAPE 4 : VALIDATION DES DONNÉES CRÉÉES
-- =====================================================

-- Vérifier les collectes créées
SELECT 
    c.IdCollecte,
    c.TypeCollecte,
    c.FraisId,
    f.Libelle as FraisLibelle,
    c.SouscriptionPrestationId,
    c.Montant,
    c.ModePaiement,
    c.DateCreation,
    c.Observation
FROM Collectes c
LEFT JOIN Frais f ON c.FraisId = f.IdFrais
WHERE c.Observation LIKE 'Test collecte %'
ORDER BY c.DateCreation DESC;

-- =====================================================
-- ÉTAPE 5 : VALIDATION DES RELATIONS
-- =====================================================

-- Vérifier la relation Frais-Collectes
SELECT 
    f.IdFrais,
    f.Libelle,
    COUNT(c.IdCollecte) as NombreCollectes,
    SUM(c.Montant) as TotalMontant
FROM Frais f
LEFT JOIN Collectes c ON f.IdFrais = c.FraisId AND c.TypeCollecte = 1
GROUP BY f.IdFrais, f.Libelle
ORDER BY f.IdFrais;

-- =====================================================
-- ÉTAPE 6 : NETTOYAGE
-- =====================================================

-- Supprimer les collectes de test (décommenter si nécessaire)
-- DELETE FROM Collectes WHERE Observation LIKE 'Test collecte %';

-- Message de fin
SELECT 
    'TEST TYPECOLLECTE TERMINÉ' as Statut,
    'Vérifiez les résultats ci-dessus pour valider l\'implémentation' as Message;
