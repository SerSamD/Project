# Système de Gestion Scolaire  
### ASP.NET Core 9 · Entity Framework Core · MySQL

![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![C#](https://img.shields.io/badge/C%23-13-blue)
![License](https://img.shields.io/badge/License-MIT-green)

---

## Description

Ce projet est une application web de gestion scolaire développée avec **ASP.NET Core 9**, **Entity Framework Core** et **MySQL**.  
Il a pour objectif de digitaliser la gestion administrative et pédagogique d’un établissement scolaire à travers une interface moderne, sécurisée et intuitive.

L’application permet de gérer :
- les utilisateurs (administrateurs, enseignants, étudiants, surveillants),
- les cours et matières,
- les notes,
- les groupes,
- les emplois du temps,
- les statistiques pédagogiques.

---

## Fonctionnalités

### Espace Administrateur
- Tableau de bord avec statistiques en temps réel  
- Gestion complète des utilisateurs (CRUD)  
- Validation des demandes d’inscription  
- Gestion des cours et matières  
- Visualisation des moyennes par groupe (Chart.js)

### Espace Enseignant
- Consultation de l’emploi du temps personnel  
- Saisie et modification des notes par groupe  
- Consultation des listes d’étudiants  
- Suivi des performances par matière  

### Espace Étudiant
- Consultation de l’emploi du temps  
- Visualisation des notes publiées  
- Affichage du groupe d’appartenance  
- Notifications des nouvelles notes  

### Espace Surveillant
- Création et gestion des groupes  
- Attribution des étudiants aux groupes  
- Gestion des emplois du temps  
- Validation et publication des notes  

---

## Interface & Design

- Interface moderne et responsive  
- Animations CSS3 et effets interactifs  
- Page de connexion personnalisée  
- Navigation via sidebar dynamique  
- Compatible desktop, tablette et mobile  

---

## Technologies utilisées

### Backend
- ASP.NET Core 9.0  
- C# 13  
- Entity Framework Core  
- MySQL 8  
- Pomelo.EntityFrameworkCore.MySql  

### Frontend
- Bootstrap 5.3  
- Font Awesome 6  
- Chart.js  
- CSS3  
- JavaScript / jQuery  

### Architecture & Sécurité
- Architecture MVC  
- Authentification par cookies  
- Autorisation basée sur les rôles  
- Dependency Injection  
- Repository Pattern  

---

## Installation & Configuration

### Prérequis
- .NET SDK 9  
- MySQL Server 8+  
- Visual Studio 2022 ou Visual Studio Code  

### Étapes

1. Cloner le projet
```bash
git clone https://github.com/SerSamD/Project.git
cd Project
