# Deploy del client Blazor su Fly.io

Questa guida illustra come pubblicare `RenzoAgostini.Client` (Blazor WebAssembly) su Fly.io.
Il client viene servito tramite Nginx usando il Dockerfile dedicato.

## 1. Preparazione

1. Configura gli endpoint nel file di sviluppo `RenzoAgostini.Client/wwwroot/appsettings.Development.json`
   (usato automaticamente durante i build `Debug`).
2. Per la produzione crea `RenzoAgostini.Client/wwwroot/appsettings.Production.json` a partire dal template
   `appsettings.Production.template.json` e inserisci i valori reali di:
   - `Keycloak:Url`, `Keycloak:Realm`, `Keycloak:ClientId`
   - `Keycloak:RedirectUri` e `Keycloak:PostLogoutRedirectUri`
   - `BaseUrl` (URL pubblico dell'API)
   Il file è già ignorato da Git: viene copiato automaticamente in `appsettings.json` quando esegui
   `dotnet build`/`dotnet publish` in modalità `Release`, quindi non devi effettuare copie manuali.
3. Autenticati su Fly.io (`fly auth login`).

## 2. Creazione dell'app

1. Crea l'app (solo la prima volta):
   ```bash
   fly apps create renzoagostini-web
   ```
   Se scegli un nome diverso aggiorna `fly.client.toml`.

## 3. Deploy

1. Posizionati nella radice del repository.
2. Avvia il deploy:
   ```bash
   fly deploy -c fly.client.toml
   ```
3. Al termine collega il dominio personalizzato (se necessario):
   ```bash
   fly certs create app.tuodominio.it
   fly ips allocate-v4
   ```
   Configura i record DNS seguendo le indicazioni restituite da Fly.

## 4. Post deploy

- Verifica che l'applicazione sia raggiungibile all'URL configurato.
- Controlla i log con `fly logs -a renzoagostini-web` in caso di problemi.
- Per aggiornare il client ripeti `fly deploy -c fly.client.toml` dopo aver eseguito il build.

## 5. Note

- Il Dockerfile esegue il `dotnet publish` in modalità Release e copia il contenuto in un container Nginx.
- Il file `docker/nginx.conf` gestisce il fallback SPA (`try_files ... /index.html`).
- Mantieni sincronizzati gli URL di Keycloak e dell'API nei file di configurazione del client con i secrets usati dal server.
