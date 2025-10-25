# Keycloak image customized for Fly.io deployment with H2 volume persistence
FROM quay.io/keycloak/keycloak:23.0.7

# Use the embedded H2 database and ensure the data directory exists for the mounted volume
ENV KC_DB=dev-file
RUN mkdir -p /opt/keycloak/data/h2 \
    && chown -R keycloak:keycloak /opt/keycloak/data

# Copy any additional configuration or themes if needed in the future
# COPY themes /opt/keycloak/themes

# Entrypoint inherited from Keycloak image

