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
      ConnectionStrings__TransaccionesConnection: "Server=sqlserver,1433;Database=DB_TRANSACCIONES;User Id=sa;Password=__MSSQL_SA_PASSWORD__;TrustServerCertificate=True;"
    ports:
      - "80:5035"

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
    healthcheck:
      test: ["CMD", "/opt/mssql-tools18/bin/sqlcmd", "-C", "-S", "localhost", "-U", "sa", "-P", "__MSSQL_SA_PASSWORD__", "-Q", "SELECT 1"]
      interval: 30s
      timeout: 20s
      retries: 10
      start_period: 150s
    volumes:
      - sqlserver_data:/var/opt/mssql

volumes:
  sqlserver_data:
DEPLOY_COMPOSE_EOF

echo "Desplegando contenedores..."
docker compose -f compose.yml up -d || docker-compose -f compose.yml up -d
echo "¡Despliegue finalizado!"
