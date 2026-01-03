# ?? Système de Gestion Scolaire - ASP.NET Core 9

[![.NET Version](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![C# Version](https://img.shields.io/badge/C%23-13.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

## ?? Description

Système de gestion scolaire moderne et complet développé avec **ASP.NET Core 9**, **Entity Framework Core** et **MySQL**. Cette application web offre une interface intuitive et élégante pour gérer efficacement les étudiants, les enseignants, les cours, les notes et les emplois du temps.

## ? Fonctionnalités Principales

### ????? **Espace Administrateur**
- ?? Tableau de bord avec statistiques en temps réel
- ?? Gestion complète des utilisateurs (CRUD)
- ? Validation des demandes d'inscription
- ?? Gestion des cours et matières
- ?? Visualisation des moyennes par groupe avec Chart.js

### ????? **Espace Enseignant**
- ?? Consultation de l'emploi du temps personnel
- ?? Saisie et modification des notes par groupe
- ????? Visualisation des listes d'étudiants
- ?? Suivi des performances par matière

### ????? **Espace Étudiant**
- ?? Consultation de l'emploi du temps personnel
- ?? Visualisation des notes publiées
- ?? Affichage du groupe d'appartenance
- ?? Notifications des nouvelles notes

### ?? **Espace Surveillant**
- ?? Création et gestion des groupes d'étudiants
- ? Attribution des étudiants aux groupes
- ?? Gestion des emplois du temps par groupe
- ? Publication et validation des notes

## ?? Design Moderne

- **Interface utilisateur élégante** avec gradients et animations CSS3
- **Page de connexion stylée** avec background immersif
- **Cartes interactives** avec effets hover 3D
- **Sidebar dynamique** avec navigation intuitive
- **Responsive Design** adapté à tous les écrans
- **Palette de couleurs professionnelle** (violet, bleu, orange, rose)

## ??? Technologies Utilisées

### Backend
- **ASP.NET Core 9.0** - Framework web moderne et performant
- **C# 13.0** - Langage de programmation orienté objet
- **Entity Framework Core** - ORM pour l'accès aux données
- **MySQL 8.0** - Base de données relationnelle
- **Pomelo.EntityFrameworkCore.MySql** - Provider MySQL pour EF Core

### Frontend
- **Bootstrap 5.3** - Framework CSS responsive
- **Font Awesome 6.5** - Bibliothèque d'icônes
- **Chart.js** - Bibliothèque de graphiques interactifs
- **CSS3** - Animations et effets modernes
- **JavaScript/jQuery** - Interactivité côté client

### Architecture
- **ASP.NET Core MVC** - Pattern Model-View-Controller
- **Cookie Authentication** - Authentification sécurisée
- **Role-Based Authorization** - Contrôle d'accès par rôles
- **Repository Pattern** - Abstraction de l'accès aux données
- **Dependency Injection** - Inversion de contrôle

## ?? Installation et Configuration

### Prérequis
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [MySQL Server 8.0+](https://dev.mysql.com/downloads/mysql/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [Visual Studio Code](https://code.visualstudio.com/)

### Étapes d'installation

1. **Cloner le repository**
```bash
git clone https://github.com/SerSamD/Project.git
cd Project
```

2. **Configurer la base de données**

Ouvrez `appsettings.json` et modifiez la chaîne de connexion MySQL :
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=schooldb;User=root;Password=votre_mot_de_passe;"
  }
}
```

3. **Appliquer les migrations**
```bash
cd Project
dotnet ef database update
```

4. **Lancer l'application**
```bash
dotnet run
```

5. **Accéder à l'application**
```
https://localhost:7000
```

## ?? Comptes par Défaut

### Administrateur
- **Username:** `admin`
- **Password:** `admin123`

> ?? **Important:** Changez le mot de passe administrateur après la première connexion !

## ?? Structure du Projet

```
Project/
??? Controllers/        # Contrôleurs MVC
?   ??? AccountController.cs
?   ??? AdminController.cs
?   ??? EnseignantController.cs
?   ??? EtudiantController.cs
?   ??? SurveillantController.cs
??? Models/         # Modèles de données
? ??? Utilisateur.cs
?   ??? Etudiant.cs
?   ??? Enseignant.cs
?   ??? Surveillant.cs
?   ??? Cours.cs
?   ??? Note.cs
? ??? Groupe.cs
?   ??? EmploiDuTemps.cs
??? Views/               # Vues Razor
?   ??? Account/
?   ??? Admin/
?   ??? Enseignant/
?   ??? Etudiant/
?   ??? Surveillant/
?   ??? Shared/
??? Data/                # Contexte de base de données
?   ??? SchoolContext.cs
??? Migrations/          # Migrations EF Core
??? wwwroot/       # Fichiers statiques
? ??? css/
?   ?   ??? site.css
?   ?   ??? modern-pages.css
?   ?   ??? login-page.css
?   ??? js/
??? Program.cs           # Point d'entrée de l'application
```

## ?? Sécurité

- ? Authentification par cookies sécurisés
- ? Hachage des mots de passe avec SHA-256
- ? Protection CSRF avec antiforgery tokens
- ? Autorisation basée sur les rôles
- ? Validation des entrées utilisateur
- ? Retry policy pour la résilience MySQL

## ?? Fonctionnalités Avancées

- **Approbation des comptes** - Les nouveaux utilisateurs doivent être validés par un administrateur
- **Gestion des rôles dynamique** - Attribution flexible des rôles (Admin, Enseignant, Étudiant, Surveillant)
- **Publication conditionnelle des notes** - Les notes ne sont visibles qu'après validation
- **Emplois du temps personnalisés** - Chaque groupe a son propre planning
- **Statistiques en temps réel** - Dashboard administrateur avec graphiques interactifs

## ?? Contribution

Les contributions sont les bienvenues ! Pour contribuer :

1. **Fork** le projet
2. Créez votre **branche de fonctionnalité** (`git checkout -b feature/AmazingFeature`)
3. **Committez** vos changements (`git commit -m 'Add some AmazingFeature'`)
4. **Poussez** vers la branche (`git push origin feature/AmazingFeature`)
5. Ouvrez une **Pull Request**

## ?? Roadmap

- [ ] Ajout d'un système de messagerie interne
- [ ] Export des bulletins en PDF
- [ ] Notifications par email
- [ ] Gestion des absences
- [ ] Module de paiement des frais de scolarité
- [ ] Application mobile (Xamarin/MAUI)
- [ ] Support multilingue (FR/EN/AR)
- [ ] Thème sombre/clair

## ?? License

Ce projet est sous licence MIT. Voir le fichier [LICENSE](LICENSE) pour plus de détails.

## ????? Auteur

**Walid** - [@SerSamD](https://github.com/SerSamD)

## ?? Contact & Support

- ?? **Signaler un bug** : [Ouvrir une issue](https://github.com/SerSamD/Project/issues)
- ?? **Questions** : [Discussions GitHub](https://github.com/SerSamD/Project/discussions)

## ? Remerciements

- [ASP.NET Core](https://docs.microsoft.com/aspnet/core) - Documentation officielle
- [Bootstrap](https://getbootstrap.com/) - Framework CSS
- [Font Awesome](https://fontawesome.com/) - Icônes
- [Chart.js](https://www.chartjs.org/) - Graphiques

---

<p align="center">
  Fait avec ?? en utilisant ASP.NET Core 9
</p>

<p align="center">
  <a href="#-système-de-gestion-scolaire---aspnet-core-9">? Retour en haut</a>
</p>