#!/bin/bash
sleep 30
hostname=$(curl -s http://169.254.169)

sudo mkdir -p /home/deploy
cd /home/deploy
# Crear el archivo compose.yml
# Usamos 'EOF' con comillas para que no intente expandir las variables de Docker ahora mismo
sudo bash -c "cat << 'EOF' > compose.yml
services:
  api:
    image: inina14/api-restfull-transacciones:$hostname
    container_name: transacciones-api
    restart: unless-stopped
    depends_on:
      sqlserver:
        condition: service_healthy
    environment:
      ASPNETCORE_ENVIRONMENT: \"production\"
      ASPNETCORE_URLS: \"http://+:5035\"
      JWT_SECRET_KEY: \"\${JWT_SECRET_KEY}\"
      ConnectionStrings__TransaccionesConnection: \"Server=sqlserver,1433;Database=DB_TRANSACCIONES;User Id=sa;Password=\${MSSQL_SA_PASSWORD};TrustServerCertificate=True;\"
    ports:
      - \"80:5035\"

  sqlserver:
    image: ://microsoft.com
    container_name: transacciones-sqlserver
    restart: unless-stopped
    environment:
      ACCEPT_EULA: \"Y\"
      MSSQL_SA_PASSWORD: \"\${MSSQL_SA_PASSWORD}\"
      MSSQL_PID: \"Developer\"
    ports:
      - \"1433:1433\"
    healthcheck:
      test: [\"CMD\", \"/opt/mssql-tools/bin/sqlcmd\", \"-U\", \"sa\", \"-P\", \"\${MSSQL_SA_PASSWORD}\", \"-Q\", \"SELECT 1\"]
      interval: 30s
      timeout: 20s
      retries: 10
      start_period: 150s
    volumes:
      - sqlserver_data:/var/opt/mssql

volumes:
  sqlserver_data:
EOF"

# 4. Levantar los contenedores en segundo plano
echo "Desplegando contenedores..."
# Intentar ejecutar con el plugin nuevo, si falla, intentar con el viejo
docker compose up -d || docker-compose up -d
echo "¡Despliegue finalizado!"
