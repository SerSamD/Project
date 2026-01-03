# 🤝 Guide de Contribution

Merci de votre intérêt pour contribuer au Système de Gestion Scolaire ! Nous apprécions toutes les contributions, qu'il s'agisse de corrections de bugs, de nouvelles fonctionnalités ou d'améliorations de la documentation.

## 📋 Table des Matières

- [Code de Conduite](#code-de-conduite)
- [Comment Contribuer](#comment-contribuer)
- [Processus de Développement](#processus-de-développement)
- [Standards de Code](#standards-de-code)
- [Commit Messages](#commit-messages)
- [Pull Requests](#pull-requests)

## 📜 Code de Conduite

Ce projet adhère à un Code de Conduite. En participant, vous êtes tenu de respecter ce code. Veuillez lire [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) pour plus de détails.

## 🚀 Comment Contribuer

### Signaler des Bugs

Si vous trouvez un bug, veuillez créer une issue en incluant :

- **Description claire** du problème
- **Étapes pour reproduire** le bug
- **Comportement attendu** vs **comportement actuel**
- **Captures d'écran** si applicable
- **Environnement** (OS, version .NET, version MySQL, navigateur)

### Proposer des Fonctionnalités

Pour proposer une nouvelle fonctionnalité :

1. Vérifiez d'abord les issues existantes pour éviter les doublons
2. Créez une issue décrivant :
   - Le problème que cette fonctionnalité résout
   - La solution proposée
   - Les alternatives envisagées
3. Attendez l'approbation avant de commencer le développement

### Améliorer la Documentation

Les améliorations de la documentation sont toujours bienvenues :

- Corriger les fautes de frappe
- Clarifier les instructions
- Ajouter des exemples
- Traduire en d'autres langues

## 💻 Processus de Développement

### 1. Fork et Clone

```bash
# Fork le repository sur GitHub, puis :
git clone https://github.com/votre-username/Project.git
cd Project
git remote add upstream https://github.com/SerSamD/Project.git
```

### 2. Créer une Branche

```bash
git checkout -b feature/ma-nouvelle-fonctionnalite
# ou
git checkout -b fix/correction-bug
```

### 3. Configuration de l'Environnement

```bash
# Installer les dépendances
cd Project
dotnet restore

# Configurer la base de données (voir README.md)
dotnet ef database update

# Lancer l'application
dotnet run
```

### 4. Développer et Tester

- Écrivez du code propre et maintenable
- Ajoutez des tests si applicable
- Testez localement avant de commit
- Assurez-vous que l'application compile sans erreurs

```bash
# Build
dotnet build

# Run tests (si disponibles)
dotnet test
```

### 5. Commit et Push

```bash
git add .
git commit -m "✨ Add: Description de votre changement"
git push origin feature/ma-nouvelle-fonctionnalite
```

### 6. Créer une Pull Request

1. Allez sur GitHub et créez une Pull Request
2. Remplissez le template fourni
3. Liez les issues concernées
4. Attendez la revue de code

## 📝 Standards de Code

### Style de Code C#

- Suivez les [conventions C# de Microsoft](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Utilisez des noms significatifs pour les variables et méthodes
- Commentez le code complexe
- Respectez le principe SOLID

### Exemple

```csharp
// ✅ Bon
public async Task<IActionResult> GetStudentById(int id)
{
    var student = await _context.Students.FindAsync(id);
    if (student == null)
    {
        return NotFound();
    }
    return View(student);
}

// ❌ Mauvais
public async Task<IActionResult> Get(int i)
{
    var s = await _context.Students.FindAsync(i);
    if(s==null) return NotFound();
    return View(s);
}
```

### Structure des Fichiers

- **Controllers** : Logique de contrôle MVC
- **Models** : Entités et modèles de données
- **Views** : Vues Razor
- **Data** : Contexte de base de données
- **wwwroot** : Fichiers statiques (CSS, JS, images)

## 💬 Commit Messages

Utilisez des messages de commit clairs et descriptifs :

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- ✨ `feat`: Nouvelle fonctionnalité
- 🐛 `fix`: Correction de bug
- 📚 `docs`: Documentation uniquement
- 💅 `style`: Formatage, point-virgules manquants, etc.
- ♻️ `refactor`: Refactorisation du code
- ⚡ `perf`: Amélioration des performances
- ✅ `test`: Ajout ou modification de tests
- 🔧 `chore`: Maintenance, configuration

### Exemples

```bash
git commit -m "✨ feat(admin): add student bulk import feature"
git commit -m "🐛 fix(auth): correct password hashing algorithm"
git commit -m "📚 docs(readme): update installation instructions"
```

## 🔍 Pull Requests

### Checklist

Avant de soumettre une Pull Request, assurez-vous que :

- [ ] Le code compile sans erreurs ni avertissements
- [ ] Les tests passent (si applicable)
- [ ] La documentation est à jour
- [ ] Le code respecte les standards du projet
- [ ] Les commits sont propres et descriptifs
- [ ] La PR est liée aux issues concernées

### Template de Pull Request

Lors de la création d'une PR, veuillez remplir le template fourni avec :

- **Description** : Que fait cette PR ?
- **Type de changement** : Bug fix, feature, documentation, etc.
- **Issues liées** : #123, #456
- **Tests** : Comment avez-vous testé ?
- **Captures d'écran** : Si changements UI

### Revue de Code

- Soyez ouvert aux commentaires et suggestions
- Répondez aux questions de manière constructive
- Effectuez les modifications demandées
- Soyez patient pendant le processus de revue

## 🎯 Zones Prioritaires

Nous recherchons particulièrement des contributions dans les domaines suivants :

- **Tests unitaires et d'intégration**
- **Amélioration de la sécurité**
- **Optimisation des performances**
- **Accessibilité (WCAG 2.1)**
- **Internationalisation (i18n)**
- **Documentation et tutoriels**

## 🔐 Sécurité

Pour signaler une vulnérabilité de sécurité, **NE créez PAS d'issue publique**. Consultez [SECURITY.md](SECURITY.md) pour la procédure à suivre.

## ❓ Questions

Si vous avez des questions :

- 💬 Utilisez les [Discussions GitHub](https://github.com/SerSamD/Project/discussions)
- 📧 Contactez [@SerSamD](https://github.com/SerSamD)

## 🙏 Reconnaissance

Tous les contributeurs seront mentionnés dans la section des remerciements du README.

---

Merci encore de contribuer au Système de Gestion Scolaire ! 🎓✨
