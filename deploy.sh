#!/bin/bash

# Configuración para depuración y robustez:
# -e: Salir inmediatamente si un comando falla.
# -u: Tratar las variables no definidas como un error y salir.
# -x: Imprimir los comandos y sus argumentos a medida que se ejecutan.
# -o pipefail: Asegurar que el estado de salida de una tubería sea el del último comando que falló.
set -euxo pipefail

echo "--- Iniciando script deploy.sh ---"

# Obtener el hostname para el tag de la imagen
hostname=$(curl -s http://169.254.169.254/metadata/v1/hostname)
echo "Hostname detectado: $hostname"

mkdir -p /home/deploy
cd /home/deploy

# Usar un "heredoc" para escribir el archivo compose.yml de forma más robusta
# Las variables de entorno se expandirán aquí
cat << EOF > compose.yml
services:
  api:
    image: inina14/api-restfull-transacciones:$hostname
    container_name: transacciones-api
    restart: unless-stopped
    depends_on:
      - sqlserver
       # condition: service_healthy
    environment:
      ASPNETCORE_ENVIRONMENT: "production"
      ASPNETCORE_URLS: "http://+:5035"
      JWT_SECRET_KEY: "${JWT_SECRET_KEY}"
      ConnectionStrings__TransaccionesConnection: "Server=sqlserver,1433;Database=DB_TRANSACCIONES;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True;"
    ports:
      - "80:5035"

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: transacciones-sqlserver
    restart: unless-stopped
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "${MSSQL_SA_PASSWORD}"
      MSSQL_PID: "Developer"
    ports:
      - "1433:1433"
    healthcheck:
      test: ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-U", "sa", "-P", "${MSSQL_SA_PASSWORD}", "-Q", "SELECT 1"]
      interval: 30s
      timeout: 20s # Espera 10 seg por la respuesta
      retries: 10 # Intenta hasta 15 veces (3-4 minutos de margen total)
      start_period: 150s  # <--- No lo marques como "unhealthy" durante los primeros 150s
    volumes:
      - sqlserver_data:/var/opt/mssql ##

volumes:
  sqlserver_data:
EOF

echo "Archivo compose.yml creado. Contenido:"
cat compose.yml
echo "--- Fin del contenido de compose.yml ---"

docker compose up -d

echo "--- Script deploy.sh finalizado ---"
