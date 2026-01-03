# 📝 Changelog

Tous les changements notables de ce projet seront documentés dans ce fichier.

Le format est basé sur [Keep a Changelog](https://keepachangelog.com/fr/1.0.0/),
et ce projet adhère au [Semantic Versioning](https://semver.org/lang/fr/).

## [Non publié]

### 🎯 À venir
- Système de messagerie interne
- Export des bulletins en PDF
- Notifications par email
- Gestion des absences
- Module de paiement des frais de scolarité
- Application mobile (Xamarin/MAUI)
- Support multilingue (FR/EN/AR)
- Thème sombre/clair

## [1.0.0] - 2024-12-XX

### ✨ Ajouté
- Interface d'administration complète
  - Tableau de bord avec statistiques en temps réel
  - Gestion CRUD des utilisateurs
  - Validation des demandes d'inscription
  - Gestion des cours et matières
  - Visualisation des moyennes par groupe avec Chart.js

- Espace Enseignant
  - Consultation de l'emploi du temps personnel
  - Saisie et modification des notes par groupe
  - Visualisation des listes d'étudiants
  - Suivi des performances par matière

- Espace Étudiant
  - Consultation de l'emploi du temps personnel
  - Visualisation des notes publiées
  - Affichage du groupe d'appartenance
  - Notifications des nouvelles notes

- Espace Surveillant
  - Création et gestion des groupes d'étudiants
  - Attribution des étudiants aux groupes
  - Gestion des emplois du temps par groupe
  - Publication et validation des notes

- Design Moderne
  - Interface utilisateur élégante avec gradients et animations CSS3
  - Page de connexion stylée avec background immersif
  - Cartes interactives avec effets hover 3D
  - Sidebar dynamique avec navigation intuitive
  - Responsive Design adapté à tous les écrans
  - Palette de couleurs professionnelle (violet, bleu, orange, rose)

### 🔒 Sécurité
- Authentification par cookies sécurisés
- Hachage des mots de passe avec SHA-256
- Protection CSRF avec antiforgery tokens
- Autorisation basée sur les rôles
- Validation des entrées utilisateur
- Retry policy pour la résilience MySQL

### 🛠️ Technique
- ASP.NET Core 9.0
- C# 13.0
- Entity Framework Core
- MySQL 8.0 avec Pomelo.EntityFrameworkCore.MySql
- Bootstrap 5.3
- Font Awesome 6.5
- Chart.js
- Architecture MVC
- Cookie Authentication
- Role-Based Authorization
- Repository Pattern
- Dependency Injection

### 📚 Documentation
- README complet en français
- Guide d'installation détaillé
- Documentation de la structure du projet
- Comptes par défaut documentés
- Licence MIT

## [0.1.0] - 2024-XX-XX

### ✨ Ajouté
- Version initiale du projet
- Configuration de base ASP.NET Core
- Modèles de données Entity Framework
- Configuration MySQL

---

## Types de Changements

- `✨ Ajouté` - Nouvelles fonctionnalités
- `🔄 Modifié` - Changements dans les fonctionnalités existantes
- `🗑️ Déprécié` - Fonctionnalités bientôt supprimées
- `❌ Supprimé` - Fonctionnalités supprimées
- `🐛 Corrigé` - Corrections de bugs
- `🔒 Sécurité` - Corrections de vulnérabilités

## Format de Version

Le projet suit le [Semantic Versioning](https://semver.org/lang/fr/) :

- **MAJOR** : Changements incompatibles avec les versions précédentes
- **MINOR** : Ajout de fonctionnalités rétro-compatibles
- **PATCH** : Corrections de bugs rétro-compatibles

Exemple : `1.2.3` = Version Majeure.Mineure.Patch
