# Deploy del client Blazor su Fly.io

Questa guida illustra come pubblicare `RenzoAgostini.Client` (Blazor WebAssembly) su Fly.io.
Il client viene servito tramite Nginx usando il Dockerfile dedicato.

## 1. Preparazione

1. Per lo sviluppo locale continua a usare `RenzoAgostini.Client/wwwroot/appsettings.Development.json`.
2. Se preferisci mantenere un file di produzione dedicato per scenari non gestiti da Fly, crea
   `RenzoAgostini.Client/wwwroot/appsettings.Production.json` a partire dal template
   `appsettings.Production.template.json` (non committare i valori reali).
3. Autenticati su Fly.io (`fly auth login`).

## 2. Creazione dell'app

1. Crea l'app (solo la prima volta):
   ```bash
   fly apps create renzoagostini-web
   ```
   Se scegli un nome diverso aggiorna `fly.client.toml`.

## 3. Configurazione dei secrets

Il container di runtime genera `wwwroot/appsettings.json` partendo dal template e dai secrets di Fly.
Imposta almeno le variabili seguenti:

```bash
fly secrets set \
  KEYCLOAK__URL="https://auth.tuodominio.it" \
  KEYCLOAK__REALM="RenzoAgostiniRealm" \
  KEYCLOAK__CLIENT_ID="web-client" \
  KEYCLOAK__REDIRECT_URI="https://app.tuodominio.it/auth/callback" \
  KEYCLOAK__POST_LOGOUT_REDIRECT_URI="https://app.tuodominio.it/" \
  BASE_URL="https://api.tuodominio.it"
```

Puoi aggiornare i secrets in qualsiasi momento con lo stesso comando.

## 4. Deploy

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

## 5. Post deploy

- Verifica che l'applicazione sia raggiungibile all'URL configurato.
- Controlla i log con `fly logs -a renzoagostini-web` in caso di problemi.
- Per aggiornare il client ripeti `fly deploy -c fly.client.toml` dopo aver eseguito il build.

## 6. Note

- Il Dockerfile esegue il `dotnet publish` in modalità Release e copia il contenuto in un container Nginx.
- Il file `docker/nginx.conf` gestisce il fallback SPA (`try_files ... /index.html`).
- Mantieni sincronizzati gli URL di Keycloak e dell'API con i secrets usati dal server.
