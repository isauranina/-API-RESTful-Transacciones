# Configuración de Base de Datos para Transacciones API

## Opción Recomendada: SQL Server Express LocalDB

### Instalación

1. **Descargar SQL Server Express LocalDB:**
   - Visita: https://www.microsoft.com/sql-server/sql-server-downloads
   - Descarga "SQL Server Express" (incluye LocalDB)
   - O instala solo LocalDB desde: https://go.microsoft.com/fwlink/?LinkID=866658

2. **Verificar instalación:**
   ```powershell
   sqllocaldb info
   ```

3. **Crear instancia LocalDB (si no existe):**
   ```powershell
   sqllocaldb create "MSSQLLocalDB"
   sqllocaldb start "MSSQLLocalDB"
   ```

### Cadena de Conexión para LocalDB

```json
{
  "ConnectionStrings": {
    "TransaccionesConnection": "Server=(localdb)\\mssqllocaldb;Database=DB_TRANSACCIONES;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## Crear la Base de Datos

### Paso 1: Instalar herramientas de Entity Framework Core

```bash
dotnet tool install --global dotnet-ef
```

### Paso 2: Crear la migración inicial

```bash
cd src/Transacciones.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Transacciones.API --context TransaccionesDbContext
```

### Paso 3: Aplicar migraciones a la base de datos

```bash
cd src/Transacciones.API
dotnet ef database update --context TransaccionesDbContext
```

Esto creará automáticamente la base de datos `DB_TRANSACCIONES` en LocalDB.

## Alternativas

### SQL Server Express (si prefieres un servicio completo)

**Cadena de conexión:**
```json
{
  "ConnectionStrings": {
    "TransaccionesConnection": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=DB_TRANSACCIONES;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

### Docker con SQL Server (muy liviano y portable)

**1. Ejecutar SQL Server en Docker:**
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

**2. Cadena de conexión:**
```json
{
  "ConnectionStrings": {
    "TransaccionesConnection": "Data Source=localhost,1433;Initial Catalog=DB_TRANSACCIONES;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
  }
}
```

## Verificar la Base de Datos

### Usando SQL Server Management Studio (SSMS)

1. Descarga SSMS: https://aka.ms/ssmsfullsetup
2. Conecta a: `(localdb)\mssqllocaldb`
3. Verifica que existe la base de datos `DB_TRANSACCIONES`

### Usando Visual Studio

1. Abre "SQL Server Object Explorer"
2. Agrega conexión: `(localdb)\mssqllocaldb`
3. Navega a `DB_TRANSACCIONES`

### Usando línea de comandos

```bash
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT name FROM sys.databases"
```

## Comandos Útiles

### Ver todas las migraciones aplicadas
```bash
dotnet ef migrations list --startup-project ../Transacciones.API --context TransaccionesDbContext
```

### Revertir última migración
```bash
dotnet ef database update PreviousMigrationName --startup-project ../Transacciones.API --context TransaccionesDbContext
```

### Eliminar última migración (sin aplicar cambios)
```bash
dotnet ef migrations remove --startup-project ../Transacciones.API --context TransaccionesDbContext
```

### Generar script SQL
```bash
dotnet ef migrations script --startup-project ../Transacciones.API --context TransaccionesDbContext --output migration.sql
```

