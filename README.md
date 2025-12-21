# Projet .NET — Serveur Web + Application Windows

Ce dépôt contient notre projet .NET réalisé dans le cadre du cours.  
Nous avons choisi d’implémenter **la partie serveur (API)** ainsi que **l’application (client)**.

L’objectif du projet est de développer :
- un **serveur** exposant une **API** (gestion des données, règles métier, authentification si demandée),
- une **application** permettant d’interagir avec le serveur (UI, appels API, affichage/édition des données).
- une **application** en dur pour démarrer un jeu, acheter un jeu

## Pré-requis
- Docker ou Docker Desktop
- un IDE comme Visual Studio Code ou Visual Studio
- .NET SDK 10
- Git

## Pour récupérer le projet 
sur le terminal de l'IDE
``git clone https://github.com/PaulineStn/Dotnet-project.git``


## Lancement du projet 

1) Avec Docker sur l'IDE : lancer tous les services sur docker-compose-db.yml
ou sur le terminal : ``docker compose -f docker-compose-db.yml up -d``

2) http://localhost:8888/browser/ : permet de se connecter à la base de données, parcourir les tables, ...
 
  Identifiants :

  **email** : admin@admin.com
  
  **password** : password

3) Lancer le WebServer sur le terminal :
  - sur le fichier Gauniv.WebServer/Program.cs : Cliquer sur ▶ Play

4) Lancer le client (application Windows)
  - sur le fichier Gauniv.Client/MauiProgram.cs : Cliquer sur ▶ Play
  - ou sur le terminal :
  ``cd .\Gauniv.Client\``
  (si besoin : ``dotnet restore``)
  ``dotnet run --framework net10.0-windows10.0.19041.0``


## Comptes Admin et User pour tester les applications
Tous les comptes pourront accéder à la liste de jeux disponibles ainsi qu'aux catégories de jeux et pourront faire des recherches / filtres sur les jeux

**Compte admin** : l'Admin pourra, sur le WebServer : ajouter des jeux, en modifier et en supprimer et il pourra ajouter des catégories, en modifier et en supprimer

  **email** : admin@test.com
  
  **password** : AdminPassword123!

**Compte User :**
- Le User pourra, sur le WebServer et le Client : accéder à leur listes de jeux, acheter de nouveaux jeux
- sur le Client : le User pourra parcourir les jeux, accéder à ses jeux puis acheter un nouveau jeu, en télécharger et en lancer.

    **email** : test@test.com
  
    **password** : Password123!

## Technologies

- **.NET** : .NET 10
- **Serveur** : ASP.NET Core Web API + Razor
- **Accès aux données** : Entity Framework Core
- **Base de données** : PostgreSQL
- **Docs API** : Swagger
- **Client** : MAUI

## Auteurs
- Steichen Pauline
- Mahussi Jeff Datongnon
- Khadija Bendib
