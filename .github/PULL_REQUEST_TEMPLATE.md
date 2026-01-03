# 🔀 Pull Request

## 📝 Description

Décrivez clairement les changements apportés par cette PR.

## 🔗 Issues Liées

Closes #(issue)

## 🎯 Type de Changement

Cochez les cases appropriées :

- [ ] 🐛 Bug fix (changement non-breaking qui corrige un problème)
- [ ] ✨ Nouvelle fonctionnalité (changement non-breaking qui ajoute une fonctionnalité)
- [ ] 💥 Breaking change (correction ou fonctionnalité qui causerait un dysfonctionnement des fonctionnalités existantes)
- [ ] 📚 Mise à jour de la documentation
- [ ] ♻️ Refactoring (pas de changement de fonctionnalité)
- [ ] ⚡ Amélioration de performance
- [ ] ✅ Ajout ou mise à jour de tests
- [ ] 🔧 Configuration ou fichiers de build

## 🧪 Comment Tester

Décrivez les étapes pour tester vos changements :

1. Étape 1
2. Étape 2
3. Étape 3

## 📸 Captures d'écran (si applicable)

Ajoutez des captures d'écran pour montrer les changements visuels.

| Avant | Après |
|-------|-------|
| Image | Image |

## ✅ Checklist

Veuillez vérifier que votre PR remplit les conditions suivantes :

### Code Quality
- [ ] Mon code suit les conventions de style de ce projet (voir `.editorconfig`)
- [ ] J'ai effectué une auto-revue de mon code
- [ ] J'ai commenté mon code, particulièrement dans les zones difficiles à comprendre
- [ ] Mes changements ne génèrent pas de nouveaux warnings

### Documentation
- [ ] J'ai mis à jour la documentation en conséquence
- [ ] J'ai ajouté/mis à jour les commentaires XML/JSDoc si nécessaire
- [ ] J'ai mis à jour le CHANGELOG.md

### Tests
- [ ] J'ai ajouté des tests qui prouvent que ma correction est efficace ou que ma fonctionnalité fonctionne
- [ ] Les tests unitaires nouveaux et existants passent localement
- [ ] Toutes les vérifications CI/CD passent

### Sécurité
- [ ] Mes changements n'introduisent pas de nouvelles vulnérabilités
- [ ] J'ai validé toutes les entrées utilisateur
- [ ] J'ai utilisé des requêtes paramétrées (pas de concaténation SQL)
- [ ] Les données sensibles ne sont pas loggées ou exposées

### Base de Données (si applicable)
- [ ] J'ai créé/mis à jour les migrations Entity Framework
- [ ] Les migrations sont testées (up et down)
- [ ] Les données existantes ne seront pas perdues

### Performance
- [ ] Mes changements n'impactent pas négativement les performances
- [ ] J'ai optimisé les requêtes de base de données si nécessaire

## 🔍 Revue de Code

### Points à Vérifier

Liste des points spécifiques que vous aimeriez que les reviewers vérifient :

- [ ] Point 1
- [ ] Point 2

### Questions pour les Reviewers

Avez-vous des questions ou préoccupations spécifiques ?

## 📋 Informations Additionnelles

### Environnement de Test

- **OS** : 
- **Navigateur** : 
- **Version .NET** : 
- **Version MySQL** : 

### Dépendances Ajoutées

Liste des nouvelles dépendances NuGet ou npm ajoutées :

- Package 1 (version)
- Package 2 (version)

### Breaking Changes

Si cette PR introduit des breaking changes, décrivez-les et comment migrer :

```
Décrivez les breaking changes et le guide de migration
```

## 🎉 Notes pour les Reviewers

Informations supplémentaires qui pourraient aider les reviewers :

---

**Merci pour votre contribution ! 🙏**

Pour plus d'informations, consultez [CONTRIBUTING.md](../CONTRIBUTING.md)
