# Keycloak image customized for Fly.io deployment with H2 volume persistence
FROM quay.io/keycloak/keycloak:23.0.7

# Use the embedded H2 database and ensure the data directory exists for the mounted volume
ENV KC_DB=dev-file
RUN mkdir -p /opt/keycloak/data/h2 \
    && chown -R keycloak:keycloak /opt/keycloak/data

# Copy custom themes ensuring they end up directly under /opt/keycloak/themes
COPY themes/ /opt/keycloak/themes/

# Explicitly start Keycloak in optimized mode when the container launches.
# Without passing "start" the base entrypoint prints the CLI help and exits
# immediately, which is what happened nei log mostrati dopo il comando di
# riavvio della macchina.
ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]
CMD ["start", "--optimized"]