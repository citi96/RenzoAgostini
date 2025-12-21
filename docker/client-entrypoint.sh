#!/bin/sh
set -eu

TEMPLATE="/usr/share/nginx/html/appsettings.template.json"
TARGET="/usr/share/nginx/html/appsettings.json"

if [ -f "$TEMPLATE" ]; then
  export BASE_URL="${BASE_URL:-https://api.renzoagostini.it}"
  export GALLERY__DEFAULT_COLUMNS="${GALLERY__DEFAULT_COLUMNS:-3}"
  export PUBLIC_CONTACT_EMAIL="${PUBLIC_CONTACT_EMAIL:-info@renzoagostini.it}"

  envsubst < "$TEMPLATE" > "$TARGET"
fi

if [ "$#" -gt 0 ]; then
  exec "$@"
fi
