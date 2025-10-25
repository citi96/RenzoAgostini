# Deploy dell'API su Fly.io

Questa guida spiega come pubblicare `RenzoAgostini.Server` su Fly.io usando il Dockerfile
ottimizzato per l'ambiente di produzione. Le istruzioni assumono che tu abbia già configurato
Keycloak seguendo la guida `keycloak-deployment.md` e che `flyctl` sia installato e autenticato.

## 1. Preparazione

1. Assicurati di avere una copia aggiornata del repository in locale.
2. Verifica di avere il CLI di Fly configurato: `fly auth login`.
3. Aggiorna `RenzoAgostini.Server/appsettings.Production.json` con i valori del tuo ambiente (il file viene
   incluso automaticamente nei build `Release` come `appsettings.json`). In alternativa puoi impostare tutto
   tramite secrets di Fly. In particolare verifica:
   - `ConnectionStrings:DefaultConnection`
   - `Keycloak:Authority`, `Keycloak:ClientId`, eventuali `Keycloak:Audiences`
   - `Cors:AllowedOrigins`
   - `Stripe:SecretKey`
   - `Storage:UploadsPath` e `Storage:CustomOrdersPath` se vuoi percorsi diversi da quelli predefiniti.

## 2. Creazione delle app Fly

1. Crea l'applicazione (la prima volta):
   ```bash
   fly apps create renzoagostini-api
   ```
   Puoi usare un nome diverso, ma ricordati di aggiornare `fly.server.toml`.

2. Crea i volumi necessari. Uno ospita il database SQLite, l'altro le immagini e gli allegati:
   ```bash
   fly volumes create renzoagostini_api_data --region ams --size 1
   fly volumes create renzoagostini_api_assets --region ams --size 1
   ```

## 3. Configurazione dei secrets

Imposta le variabili sensibili tramite `fly secrets`. Esempio:

```bash
fly secrets set \
  ConnectionStrings__DefaultConnection="Data Source=/data/RenzoAgostini.db" \
  Keycloak__Authority="https://auth.tuodominio.it/realms/RenzoAgostiniRealm" \
  Keycloak__ClientId="web-client" \
  Keycloak__Audiences__0="web-client" \
  Stripe__SecretKey="sk_live_xxx" \
  Cors__AllowedOrigins__0="https://app.tuodominio.it"
```

Puoi aggiungere ulteriori origini CORS usando indici incrementali (`Cors__AllowedOrigins__1`, ecc.).
Se hai percorsi custom per gli upload:

```bash
fly secrets set \
  Storage__UploadsPath="/app/wwwroot/uploads" \
  Storage__CustomOrdersPath="/app/wwwroot/custom-orders"
```

## 4. Deploy

1. Posizionati nella radice del progetto e lancia:
   ```bash
   fly deploy -c fly.server.toml
   ```
2. Alla fine del deploy, registra il certificato TLS e collega il dominio (se necessario):
   ```bash
   fly certs create api.tuodominio.it
   fly ips allocate-v4
   ```
   Segui le istruzioni di Fly per configurare i record DNS.

## 5. Verifiche post deploy

1. Controlla i log:
   ```bash
   fly logs -a renzoagostini-api
   ```
2. Esegui un health-check manuale visitando `https://api.tuodominio.it/health` (se hai esposto l'endpoint) o un endpoint dell'API.
3. Verifica che gli upload vengano salvati correttamente sul volume montato (`fly ssh console` > controlla `/app/wwwroot`).

## 6. Aggiornamenti futuri

- Per aggiornare l'app ripeti `fly deploy -c fly.server.toml`.
- Se devi migrare il database, effettua un backup del volume prima (`fly volumes snapshot create`).
- Tieni sincronizzati i secrets con i valori configurati in Keycloak e Stripe.
