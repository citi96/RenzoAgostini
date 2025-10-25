# Keycloak su Fly.io

Questa guida descrive la configurazione aggiornata per l'esecuzione di Keycloak su Fly.io utilizzando il database H2 in modalità file con persistenza su volume. H2 è un database relazionale embedded che, in questa configurazione, Keycloak usa in modalità "file" per salvare i dati di configurazione sul disco montato, garantendo così la persistenza tra i riavvii della macchina.

## Panoramica

* L'immagine Docker è definita in `docker/Dockerfile` e forza l'uso del database H2 in modalità file (`KC_DB=dev-file`).
* Il volume da 1 GB è montato su `/opt/keycloak/data/h2` tramite `fly.toml`.
* Le uniche variabili richieste lato Fly sono `KEYCLOAK_ADMIN` e `KEYCLOAK_ADMIN_PASSWORD`, da impostare come secrets.
* Lo scaling delle macchine è bloccato a `min = 1` e `max = 1` per garantire una singola istanza.

## Preparazione dell'applicazione

1. Assicurarsi di avere la CLI Fly (`flyctl`) autenticata.
2. Creare (una sola volta) il volume H2 nella regione scelta, ad esempio:
   ```bash
   fly volumes create keycloak_h2 --size 1 --region ams --app renzoagostini-keycloak
   ```

## Configurazione delle credenziali amministrative

Impostare le credenziali admin tramite secrets (non inserirle nel `fly.toml`):

```bash
fly secrets set KEYCLOAK_ADMIN=<utente> KEYCLOAK_ADMIN_PASSWORD=<password> --app renzoagostini-keycloak
```

## Deploy

1. Generare il file `fly.toml` (se non esiste) senza effettuare il deploy immediato:
   ```bash
   fly launch --no-deploy
   ```
2. Eseguire il deploy:
   ```bash
   fly deploy
   ```
3. Verificare i log dell'applicazione per assicurarsi che Keycloak sia avviato correttamente:
   ```bash
   fly logs
   ```

## Snapshot periodici del volume H2

Per garantire la sicurezza dei dati è consigliato creare snapshot periodici del volume H2. È disponibile lo script PowerShell `scripts/create-h2-snapshot.ps1` che automatizza la creazione dello snapshot identificando il volume per nome.

Esempio di utilizzo manuale (PowerShell):

```powershell
pwsh ./scripts/create-h2-snapshot.ps1 -AppName renzoagostini-keycloak -VolumeName keycloak_h2
```

Su Windows è possibile pianificare l'esecuzione periodica tramite l'Utilità di pianificazione, ad esempio con un'attività giornaliera che esegue:

```powershell
pwsh -File "C:\path\to\repo\scripts\create-h2-snapshot.ps1" -AppName renzoagostini-keycloak -VolumeName keycloak_h2
```

Gli snapshot sono consultabili con:

```bash
fly volumes snapshots list --app renzoagostini-keycloak
```

Per ripristinare un volume da snapshot consultare la documentazione Fly:

```bash
fly volumes snapshots restore <snapshot-id>
```

## Variabili d'ambiente

Le variabili `KC_DB_*` non sono più necessarie né richieste nel deploy. L'immagine imposta già `KC_DB=dev-file`, mentre le credenziali amministrative devono essere configurate unicamente con `KEYCLOAK_ADMIN` e `KEYCLOAK_ADMIN_PASSWORD` tramite secrets.
