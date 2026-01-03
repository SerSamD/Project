# 🚀 Quick Start Guide

Guide rapide pour démarrer avec le Système de Gestion Scolaire.

## 📋 Table des Matières

- [Prérequis](#-prérequis)
- [Installation Rapide](#-installation-rapide)
- [Premier Démarrage](#-premier-démarrage)
- [Structure du Projet](#-structure-du-projet)
- [Commandes Utiles](#-commandes-utiles)
- [Problèmes Courants](#-problèmes-courants)

## 🔧 Prérequis

Avant de commencer, assurez-vous d'avoir installé :

| Logiciel | Version Minimale | Lien de Téléchargement |
|----------|------------------|------------------------|
| .NET SDK | 9.0 | [Télécharger](https://dotnet.microsoft.com/download/dotnet/9.0) |
| MySQL Server | 8.0+ | [Télécharger](https://dev.mysql.com/downloads/mysql/) |
| Git | Dernière | [Télécharger](https://git-scm.com/downloads) |

**Éditeurs Recommandés :**
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (Windows/Mac)
- [Visual Studio Code](https://code.visualstudio.com/) + Extension C#
- [JetBrains Rider](https://www.jetbrains.com/rider/)

## ⚡ Installation Rapide

### 1️⃣ Cloner le Repository

```bash
git clone https://github.com/SerSamD/Project.git
cd Project
```

### 2️⃣ Configurer MySQL

Créez une base de données :

```sql
CREATE DATABASE schooldb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### 3️⃣ Configurer la Connexion

Éditez `Project/appsettings.json` :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=schooldb;User=root;Password=VOTRE_MOT_DE_PASSE;"
  }
}
```

> 💡 **Astuce** : Pour la sécurité, utilisez `appsettings.Development.json` pour vos configurations locales (déjà dans .gitignore)

### 4️⃣ Appliquer les Migrations

```bash
cd Project
dotnet restore
dotnet ef database update
```

### 5️⃣ Lancer l'Application

```bash
dotnet run
```

L'application sera accessible à : **https://localhost:7000**

## 🎬 Premier Démarrage

### Connexion Administrateur

Utilisez les identifiants par défaut :

```
Username: admin
Password: admin123
```

> ⚠️ **IMPORTANT** : Changez ce mot de passe immédiatement après la première connexion !

### Exploration Rapide

1. **Dashboard Administrateur** - Vue d'ensemble des statistiques
2. **Gestion des Utilisateurs** - Créer et gérer les comptes
3. **Gestion des Cours** - Ajouter des matières
4. **Groupes** - Créer des groupes d'étudiants
5. **Emplois du Temps** - Planifier les cours

## 📁 Structure du Projet

```
Project/
│
├── Controllers/           # 🎮 Logique de contrôle MVC
│   ├── AccountController.cs      # Authentification
│   ├── AdminController.cs        # Espace admin
│   ├── EnseignantController.cs   # Espace enseignant
│   ├── EtudiantController.cs     # Espace étudiant
│   └── SurveillantController.cs  # Espace surveillant
│
├── Models/               # 📦 Modèles de données
│   ├── Utilisateur.cs            # Utilisateur de base
│   ├── Etudiant.cs               # Étudiant
│   ├── Enseignant.cs             # Enseignant
│   ├── Surveillant.cs            # Surveillant
│   ├── Cours.cs                  # Cours/Matière
│   ├── Note.cs                   # Notes
│   ├── Groupe.cs                 # Groupe d'étudiants
│   └── EmploiDuTemps.cs          # Emploi du temps
│
├── Views/                # 🎨 Vues Razor
│   ├── Account/                  # Pages de connexion
│   ├── Admin/                    # Pages admin
│   ├── Enseignant/               # Pages enseignant
│   ├── Etudiant/                 # Pages étudiant
│   ├── Surveillant/              # Pages surveillant
│   └── Shared/                   # Composants partagés
│
├── Data/                 # 💾 Accès aux données
│   └── SchoolContext.cs          # Contexte EF Core
│
├── Migrations/           # 🔄 Migrations de base de données
│
├── wwwroot/              # 🌐 Fichiers statiques
│   ├── css/                      # Feuilles de style
│   ├── js/                       # Scripts JavaScript
│   └── lib/                      # Bibliothèques (Bootstrap, jQuery)
│
├── Program.cs            # 🚀 Point d'entrée
├── appsettings.json      # ⚙️ Configuration
└── Project.csproj        # 📋 Fichier projet
```

## 🛠️ Commandes Utiles

### Développement

```bash
# Lancer en mode développement (avec hot reload)
dotnet watch run

# Restaurer les dépendances
dotnet restore

# Build du projet
dotnet build

# Build en mode Release
dotnet build --configuration Release

# Nettoyer les builds
dotnet clean
```

### Base de Données

```bash
# Créer une nouvelle migration
dotnet ef migrations add NomDeLaMigration

# Appliquer les migrations
dotnet ef database update

# Revenir à une migration spécifique
dotnet ef database update NomDeLaMigration

# Supprimer la dernière migration
dotnet ef migrations remove

# Lister les migrations
dotnet ef migrations list

# Générer un script SQL des migrations
dotnet ef migrations script
```

### Tests (si disponibles)

```bash
# Exécuter tous les tests
dotnet test

# Exécuter avec couverture de code
dotnet test --collect:"XPlat Code Coverage"
```

### Code Quality

```bash
# Formatter le code selon .editorconfig
dotnet format

# Vérifier le formatage sans modifier
dotnet format --verify-no-changes
```

## 🐛 Problèmes Courants

### Erreur de Connexion MySQL

**Problème** : `Unable to connect to any of the specified MySQL hosts`

**Solutions** :
1. Vérifiez que MySQL est démarré
2. Vérifiez le nom d'utilisateur et le mot de passe dans `appsettings.json`
3. Vérifiez que la base de données existe

```bash
# Vérifier le statut de MySQL (Linux/Mac)
sudo systemctl status mysql

# Windows
net start MySQL80
```

### Port Déjà Utilisé

**Problème** : `Address already in use`

**Solutions** :
1. Changez le port dans `Properties/launchSettings.json`
2. Ou tuez le processus qui utilise le port :

```bash
# Windows
netstat -ano | findstr :7000
taskkill /PID <PID> /F

# Linux/Mac
lsof -ti:7000 | xargs kill -9
```

### Migrations Ne S'appliquent Pas

**Problème** : Les migrations ne mettent pas à jour la base de données

**Solutions** :

```bash
# Supprimer la base de données et recréer
dotnet ef database drop
dotnet ef database update

# Ou forcer la recréation
dotnet ef database update --force
```

### Erreur de Build

**Problème** : `The type or namespace name could not be found`

**Solutions** :

```bash
# Nettoyer et reconstruire
dotnet clean
dotnet restore
dotnet build
```

## 📚 Ressources Supplémentaires

### Documentation

- [README Principal](README.md) - Documentation complète
- [Guide de Contribution](CONTRIBUTING.md) - Comment contribuer
- [Politique de Sécurité](SECURITY.md) - Signaler des vulnérabilités

### Liens Utiles

- [Documentation ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.3)
- [MySQL Documentation](https://dev.mysql.com/doc/)

### Support

- 💬 [GitHub Discussions](https://github.com/SerSamD/Project/discussions)
- 🐛 [Signaler un Bug](https://github.com/SerSamD/Project/issues/new?template=bug_report.md)
- ✨ [Demander une Fonctionnalité](https://github.com/SerSamD/Project/issues/new?template=feature_request.md)

## 🎯 Prochaines Étapes

Maintenant que vous avez l'application en cours d'exécution :

1. 📖 Lisez le [README.md](README.md) pour comprendre toutes les fonctionnalités
2. 🤝 Consultez [CONTRIBUTING.md](CONTRIBUTING.md) si vous voulez contribuer
3. 🔐 Lisez [SECURITY.md](SECURITY.md) pour les bonnes pratiques de sécurité
4. 💻 Explorez le code et amusez-vous !

## 💡 Conseils pour les Débutants

- Commencez par explorer l'interface utilisateur avant de plonger dans le code
- Utilisez le débogueur de Visual Studio pour comprendre le flux de l'application
- Consultez les commentaires dans le code
- N'hésitez pas à poser des questions dans les Discussions GitHub

---

**Bon développement ! 🚀✨**

Si vous avez des questions, n'hésitez pas à ouvrir une discussion sur GitHub.
