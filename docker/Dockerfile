# Keycloak image customized for Fly.io deployment with H2 volume persistence
FROM quay.io/keycloak/keycloak:23.0.7

# Use the embedded H2 database and ensure the data directory exists for the mounted volume
ENV KC_DB=dev-file
RUN mkdir -p /opt/keycloak/data/h2 \
    && chown -R keycloak:keycloak /opt/keycloak/data

# Copy custom themes ensuring they end up directly under /opt/keycloak/themes
COPY --chown=keycloak:keycloak themes/ /opt/keycloak/themes/

# Build at runtime so deployment environment variables (e.g. credentials, host
# configuration) are honored while still producing an optimized distribution
# that includes the custom themes.
ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]
CMD ["start", "--optimized", "--auto-build"]
