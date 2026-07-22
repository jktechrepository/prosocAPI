#!/bin/bash

# Script pour appliquer les permissions via MySQL
# Base de données: dev-prosoc_db

echo "🔧 Application des permissions..."

# Connexion MySQL et exécution du script
cat fix-all-permissions.sql | mysql -h localhost -P 3306 -u kansa -pkansa@2025 dev-prosoc_db

if [ $? -eq 0 ]; then
    echo "✅ Permissions appliquées avec succès !"
    echo ""
    echo "📊 Vérification..."
    
    # Vérifier les permissions
    mysql -h localhost -P 3306 -u kansa -pkansa@2025 dev-prosoc_db -e "
    SELECT 'Super-Admin' as Role, COUNT(*) as NbPermissions FROM RolePermissions WHERE RoleId = 1
    UNION ALL
    SELECT 'Admin' as Role, COUNT(*) as NbPermissions FROM RolePermissions WHERE RoleId = 2
    UNION ALL
    SELECT 'Superviseur' as Role, COUNT(*) as NbPermissions FROM RolePermissions WHERE RoleId = 3;
    "
else
    echo "❌ Erreur lors de l'application des permissions"
    exit 1
fi
