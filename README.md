# VinCipher

VinCipher est une application web .NET (laboratoire) pour le décodage de VIN et un petit playground d’API.

## Prérequis
- .NET 10 SDK
- SQL Server (pour `VinCipherContext`)
- SQLite (fichier `playground.db` créé automatiquement)

## Configuration
Les principaux paramètres se trouvent dans `appsettings.json` / variables d’environnement :
- `ConnectionStrings:DefaultConnection` – connexion SQL Server
- `Playground:HmacSecret` – secret HMAC pour les défis playground
- `Admin:RootUsername` / `Admin:RootPassword` – compte admin racine créé au démarrage

## Démarrage en local
```bash
# Restaurer, compiler et lancer en HTTPS
 dotnet restore
 dotnet run --project VinCipher.com/VinCipher.com.csproj
```

L’application applique automatiquement les migrations et crée :
- un compte admin racine si inexistant
- une demande d’accès playground de démonstration

## Déploiement sur le VPS
Deux scripts sont fournis dans le dossier `deploy` :
- `deploy-local.ps1` – à lancer depuis votre machine Windows
- `deploy.sh` – exécuté sur le VPS (copie les fichiers publiés dans `/var/www/vincipher` et redémarre le service `vincipher`)

Flux typique :
1. `dotnet publish` via `deploy-local.ps1`
2. Copie des fichiers publiés + `deploy.sh` vers `/tmp/vincipher-deploy` (SSH/SCP)
3. Exécution de `deploy.sh` qui met à jour `/var/www/vincipher` et redémarre `systemd`

## Sécurité
- Ne pas commiter de secrets de production dans le dépôt.
- Utiliser des variables d’environnement / Azure Key Vault pour les clés API et chaînes de connexion.
- S’assurer que l’accès SSH au VPS est restreint (clé SSH ou mot de passe fort).

## Tests
(Si un projet de tests existe) :
```bash
 dotnet test
```

Ajoutez des tests unitaires pour tout nouveau service public ou logique métier importante.