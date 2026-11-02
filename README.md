# Magic Johnson

Boutique en ligne de cartes à collectionner **Magic: The Gathering**, réalisée en équipe de 4 dans le cadre du projet intégrateur en techniques de l'informatique au Cégep de Granby (automne 2025).

> **Note sur l'historique** : le projet a été développé à l'origine sur **Azure DevOps** (dépôt Git, pull requests, pipelines). Ce dépôt GitHub en est une copie de la version finale. C'est pourquoi on n'y trouve pas l'historique des commits. Les identifiants et les clés ont été retirés de la configuration (voir [Configuration](#configuration)).

## Fonctionnalités

- **Catalogue** de plus de 100 000 cartes en plusieurs langues (incluant les versions numériques et les variantes), avec recherche avancée, filtres multiples et pagination
- **Panier et commandes** pour les clients, avec le suivi du statut des commandes
- **Paiement en ligne** avec Stripe, et paiement en magasin enregistré par un employé
- **Promotions** et **demandes de remboursement** (remboursements traités par l'API Stripe)
- **Comptes et rôles** : client, employé et gérant (ASP.NET Core Identity), réinitialisation du mot de passe par courriel
- **Gestion du magasin** pour les employés : inventaire, commandes, cartes les plus vendues et tendances

## Points techniques

- **Importation des cartes** : téléchargement de l'archive `AllPrintings.json` de [MTGJSON](https://mtgjson.com/) et lecture en flux avec Newtonsoft.Json (`JsonTextReader`) pour traiter le fichier sans le charger entièrement en mémoire. L'insertion en lot se fait avec EF Core Bulk Extensions, et les prix proviennent de `AllPricesToday.json`.
- **Tâches planifiées (Quartz.NET)** : mise à jour du catalogue chaque nuit et remise à zéro hebdomadaire des compteurs de consultation utilisés pour les tendances.
- **Paiements (Stripe.net / Stripe.js)** : sessions de paiement, paiements en attente et remboursements.
- **API REST** ASP.NET Core avec validation des requêtes (FluentValidation), documentée avec Swagger.
- **CORS** configuré entre le front-end Vue.js et l'API.
- **CI/CD** : pipelines Azure DevOps (compilation, tests, mise à jour de la base de données et publication).

## Démarche

1. Conception : diagrammes de base de données, de fonctionnalités et de classes
2. Maquettes des interfaces
3. Développement en **Scrum** (sprints, branches par fonctionnalité, pull requests et revues de code)

## Technologies

| Couche | Technologies |
|---|---|
| Back-end | .NET 8, ASP.NET Core, Entity Framework Core, ASP.NET Core Identity, Quartz.NET, Stripe.net, FluentValidation, MailKit |
| Base de données | SQL Server |
| Front-end | Vue 3, Vite, Pinia, Vue Router, Quasar, Bootstrap, Axios, Stripe.js |
| Outils | Azure DevOps, Git, Swagger |

## Structure

```
Back-end/Magik-Johnson/        # Solution .NET (API, services, entités, migrations EF Core)
Front-end/Magik-Johnson/       # Application Vue.js (Vite)
Azure_pipeline/                # Pipelines CI/CD Azure DevOps
```

## Configuration

Les secrets ne sont pas inclus dans le dépôt. Avant de lancer l'API, remplir `Back-end/Magik-Johnson/Magik-Johnson/appsettings.json` (ou utiliser `dotnet user-secrets`) :

| Clé | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | Chaîne de connexion SQL Server |
| `EmailConfiguration:Username` / `Password` | Identifiants du serveur SMTP |
| `Stripe:SecretKey` | Clé secrète Stripe (mode test : `sk_test_…`) |
| `FrontUrl` | URL du front-end (liens dans les courriels) |

Côté front-end, l'URL de l'API se définit avec `VITE_API_URL` dans un fichier `.env`.

## Lancer le projet

**API**

```bash
cd Back-end/Magik-Johnson/Magik-Johnson
dotnet ef database update
dotnet run
```

**Front-end** (Node 20.19+ ou 22.12+)

```bash
cd Front-end/Magik-Johnson
npm install
npm run dev
```

## Équipe

Projet réalisé en équipe de 4. Dépôt publié par Yacine Bourezak ([GitHub](https://github.com/Yacine3111) · [LinkedIn](https://www.linkedin.com/in/yacine-bourezak)), qui s'est notamment occupé de l'importation des cartes, des tâches Quartz.NET, de l'intégration Stripe (paiements et remboursements), des promotions et de la configuration CORS.
