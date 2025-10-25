# Keycloak su Fly.io

Questa guida descrive la configurazione aggiornata per l'esecuzione di Keycloak su Fly.io utilizzando il database H2 in modalità file con persistenza su volume. H2 è un database relazionale embedded che, in questa configurazione, Keycloak usa in modalità "file" per salvare i dati di configurazione sul disco montato, garantendo così la persistenza tra i riavvii della macchina.

## Panoramica

* L'immagine Docker è definita in `docker/Dockerfile` e forza l'uso del database H2 in modalità file (`KC_DB=dev-file`).
* Il volume da 1 GB è montato su `/opt/keycloak/data/h2` tramite `fly.toml`.
* Le uniche variabili richieste lato Fly sono `KEYCLOAK_ADMIN` e `KEYCLOAK_ADMIN_PASSWORD`, da impostare come secrets.
* Le variabili d'ambiente specificate in `fly.toml` impostano l'hostname pubblico (`KC_HOSTNAME`), disattivano il controllo
  stretto (`KC_HOSTNAME_STRICT=false`) e attivano la modalità proxy per Fly (`KC_PROXY=edge`).
* Lo scaling delle macchine è bloccato a `min = 1` e `max = 1` per garantire una singola istanza.

## Preparazione dell'applicazione

1. Assicurarsi di avere la CLI Fly (`flyctl`) autenticata.
2. Creare (una sola volta) il volume H2 nella regione scelta, ad esempio:
   ```bash
   fly volumes create keycloak_h2 --size 1 --region ams --app renzoagostini-keycloak
   ```

## Configurazione delle credenziali amministrative

Le credenziali admin vanno salvate nello store dei secrets di Fly, **non** nei file di progetto. Esegui il comando `fly secrets set` dal tuo terminale (dopo esserti autenticato con `fly auth login`):

```bash
fly secrets set KEYCLOAK_ADMIN=<utente> KEYCLOAK_ADMIN_PASSWORD=<password> --app renzoagostini-keycloak
```

Fly memorizza i secrets in modo sicuro lato server e li espone all'istanza Keycloak come variabili d'ambiente; non è necessario (né consigliato) salvarli in `fly.toml` o in altri file versionati. Dopo il comando `fly secrets set` le credenziali **non compariranno** da nessuna parte nel repository o nel file `fly.toml`: rimangono visibili solo lato Fly. Puoi verificare che siano presenti (nome senza valore) con:

```bash
fly secrets list --app renzoagostini-keycloak
```

Se devi controllarne il contenuto, usa il comando seguente (restituisce il valore in chiaro):

```bash
fly secrets get KEYCLOAK_ADMIN --app renzoagostini-keycloak
```

Ricorda che l'output resta sul tuo terminale locale: la piattaforma non mostrerà mai i valori in dashboard o file di configurazione.

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

### Nota su macchine sospese e controlli DNS

Se durante il comando `fly deploy` o `fly status` compare l'avviso

```
Checking DNS configuration for <app>.fly.dev
WARN DNS checks failed: expected 1 A records for <app>.fly.dev., got 0
```

significa quasi sempre che l'unica macchina dell'app è nello stato `suspended` e non sta rispondendo alle probe DNS. Puoi
verificarlo con:

```bash
fly machines list --app renzoagostini-keycloak
```

Se l'istanza risulta `suspended`, riattivala con:

```bash
fly machine restart <ID-della-macchina> --app renzoagostini-keycloak
```

Il comando `restart` fa spegnere e avviare nuovamente la macchina, rimontando il volume `keycloak_h2` (vedrai nei log messaggi come "Setting up volume
'keycloak_h2'") e facendo tornare verdi i controlli DNS. In alternativa puoi fare un semplice deploy (`fly deploy`)
che crea o riavvia automaticamente la macchina.

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

Nel file `fly.toml` sono invece definite esplicitamente le variabili:

* `KC_HOSTNAME=renzoagostini-keycloak.fly.dev`: necessario per evitare l'errore "Strict hostname resolution configured but no hostname setting provided" e far sì che Keycloak riconosca l'URL pubblico generato da Fly.
* `KC_HOSTNAME_STRICT=false`: disattiva il controllo rigido sull'hostname per gestire eventuali accessi tramite domini alternativi (es. `fly.dev` e `flycast`), evitando riavvii in loop della macchina.
* `KC_PROXY=edge`: informa Keycloak che si trova dietro un reverse proxy/edge provider (Fly) e che deve fidarsi degli header `X-Forwarded-*` forniti dalla piattaforma.

Se cambi dominio personalizzato, aggiorna `KC_HOSTNAME` (ed eventuali record DNS) e ridistribuisci l'applicazione.