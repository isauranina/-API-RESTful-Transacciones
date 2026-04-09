#!/bin/bash
set -e
sleep 30

# Los placeholders __IMAGE_TAG__, __JWT_SECRET__ y __MSSQL_SA_PASSWORD__ los reemplaza el workflow con sed antes de user-data.
sudo mkdir -p /home/deploy
cd /home/deploy

sudo tee compose.yml > /dev/null <<'DEPLOY_COMPOSE_EOF'
services:
  api:
    image: inina14/api-restfull-transacciones:__IMAGE_TAG__
    container_name: transacciones-api
    restart: unless-stopped
    depends_on:
      sqlserver:
        condition: service_healthy
    environment:
      ASPNETCORE_ENVIRONMENT: "production"
      ASPNETCORE_URLS: "http://+:5035"
      JWT_SECRET_KEY: "__JWT_SECRET__"
      LOKI_URL: "http://localhost:3100"
      ConnectionStrings__TransaccionesConnection: "Server=sqlserver,1433;Database=DB_TRANSACCIONES;User Id=sa;Password=__MSSQL_SA_PASSWORD__;TrustServerCertificate=True;Connect Timeout=120;"
    ports:
      - "80:5035"
    healthcheck:
      test:
        [
          "CMD-SHELL",
          "curl -fsS http://127.0.0.1:5035/health/live || exit 1",
        ]
      interval: 30s
      timeout: 10s
      retries: 5
      start_period: 90s

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: transacciones-sqlserver
    restart: unless-stopped
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "__MSSQL_SA_PASSWORD__"
      MSSQL_PID: "Developer"
    ports:
      - "1433:1433"
    # $$ en Compose se convierte en $ para el contenedor: la contraseña sale de MSSQL_SA_PASSWORD (no del host).
    # Prueba tools18 y luego tools (imagen 2022).
    healthcheck:
      test:
        [
          "CMD-SHELL",
          "/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P \"$$MSSQL_SA_PASSWORD\" -Q 'SELECT 1' || /opt/mssql-tools/bin/sqlcmd -C -S localhost -U sa -P \"$$MSSQL_SA_PASSWORD\" -Q 'SELECT 1' || exit 1",
        ]
      interval: 15s
      timeout: 25s
      retries: 15
      start_period: 240s
    volumes:
      - sqlserver_data:/var/opt/mssql

volumes:
  sqlserver_data:
DEPLOY_COMPOSE_EOF

echo "Desplegando contenedores..."
docker compose -f compose.yml up -d || docker-compose -f compose.yml up -d
echo "¡Despliegue finalizado!"
