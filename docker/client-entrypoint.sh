#!/bin/sh
set -eu

TEMPLATE="/usr/share/nginx/html/appsettings.template.json"
TARGET="/usr/share/nginx/html/appsettings.json"

if [ -f "$TEMPLATE" ]; then
  export KEYCLOAK__URL="${KEYCLOAK__URL:-https://auth.your-domain.tld}"
  export KEYCLOAK__REALM="${KEYCLOAK__REALM:-RenzoAgostiniRealm}"
  export KEYCLOAK__CLIENT_ID="${KEYCLOAK__CLIENT_ID:-web-client-id}"
  export KEYCLOAK__REDIRECT_URI="${KEYCLOAK__REDIRECT_URI:-https://app.your-domain.tld/auth/callback}"
  export KEYCLOAK__POST_LOGOUT_REDIRECT_URI="${KEYCLOAK__POST_LOGOUT_REDIRECT_URI:-https://app.your-domain.tld/}"
  export BASE_URL="${BASE_URL:-https://api.your-domain.tld}"

  envsubst < "$TEMPLATE" > "$TARGET"
fi

exec "$@"
