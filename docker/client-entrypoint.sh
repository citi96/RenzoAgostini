#!/bin/sh
set -eu

TEMPLATE="/usr/share/nginx/html/appsettings.template.json"
TARGET="/usr/share/nginx/html/appsettings.json"

if [ -f "$TEMPLATE" ]; then
  export KEYCLOAK__URL="${KEYCLOAK__URL:-https://auth.renzoagostini.it}"
  export KEYCLOAK__REALM="${KEYCLOAK__REALM:-RenzoAgostiniRealm}"
  export KEYCLOAK__CLIENT_ID="${KEYCLOAK__CLIENT_ID:-web-5e6da1c6-5531-4a9f-a800-397948b8ba5d}"
  export KEYCLOAK__REDIRECT_URI="${KEYCLOAK__REDIRECT_URI:-https://www.renzoagostini.it/auth/callback}"
  export KEYCLOAK__POST_LOGOUT_REDIRECT_URI="${KEYCLOAK__POST_LOGOUT_REDIRECT_URI:-https://www.renzoagostini.it/}"
  export BASE_URL="${BASE_URL:-https://api.renzoagostini.it}"

  envsubst < "$TEMPLATE" > "$TARGET"
fi

if [ "$#" -gt 0 ]; then
  exec "$@"
fi
