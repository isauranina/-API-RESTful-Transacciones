# Configuración de Base de Datos - SQL Server Express LocalDB

## ¿Por qué LocalDB?

**SQL Server Express LocalDB** es la opción más liviana y recomendada para desarrollo y pruebas técnicas:

- ✅ **Muy liviana**: No requiere servicio de Windows
- ✅ **Gratuita**: Sin costos de licencia
- ✅ **Fácil de instalar**: Solo ~50MB
- ✅ **Perfecta para desarrollo local**: Se ejecuta como proceso de usuario
- ✅ **No requiere configuración compleja**: Funciona out-of-the-box

## Instalación

### Opción 1: Instalar LocalDB directamente

1. **Descargar:**
   - Visita: https://go.microsoft.com/fwlink/?LinkID=866658
   - Descarga "SqlLocalDB.msi" (aprox. 50MB)

2. **Instalar:**
   - Ejecuta el instalador
   - Acepta los términos y completa la instalación

3. **Verificar instalación:**
   ```powershell
   sqllocaldb info
   ```

### Opción 2: Instalar con Visual Studio

Si tienes Visual Studio instalado, LocalDB ya viene incluido. Solo verifica:

```powershell
sqllocaldb info MSSQLLocalDB
```

Si no existe, créala:

```powershell
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

## Configuración del Proyecto

La cadena de conexión ya está configurada en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "TransaccionesConnection": "Server=(localdb)\\mssqllocaldb;Database=DB_TRANSACCIONES;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## Crear la Base de Datos

### Paso 1: Instalar herramientas de EF Core

```bash
dotnet tool install --global dotnet-ef
```

Si ya está instalado, actualízalo:

```bash
dotnet tool update --global dotnet-ef
```

### Paso 2: Crear la migración inicial

```bash
cd src/Transacciones.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Transacciones.API --context TransaccionesDbContext
```

### Paso 3: Aplicar migraciones (crea la base de datos automáticamente)

```bash
cd src/Transacciones.API
dotnet ef database update --context TransaccionesDbContext
```

¡Listo! La base de datos `DB_TRANSACCIONES` se creará automáticamente en LocalDB.

## Verificar la Base de Datos

### Usando Visual Studio

1. Abre **SQL Server Object Explorer** (View → SQL Server Object Explorer)
2. Expande `(localdb)\MSSQLLocalDB`
3. Verifica que existe `DB_TRANSACCIONES`
4. Expande las tablas: `Cuentas` y `Transacciones`

### Usando SQL Server Management Studio (SSMS)

1. Descarga SSMS: https://aka.ms/ssmsfullsetup
2. Conecta a: `(localdb)\MSSQLLocalDB`
3. Navega a `Databases → DB_TRANSACCIONES`

### Usando línea de comandos

```bash
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT name FROM sys.databases WHERE name = 'DB_TRANSACCIONES'"
```

## Comandos Útiles

### Ver migraciones aplicadas
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

## Alternativas (si prefieres otras opciones)

### SQL Server Express (servicio completo)

**Cadena de conexión:**
```json
"TransaccionesConnection": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=DB_TRANSACCIONES;Integrated Security=True;TrustServerCertificate=True;"
```

### Docker con SQL Server

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

**Cadena de conexión:**
```json
"TransaccionesConnection": "Data Source=localhost,1433;Initial Catalog=DB_TRANSACCIONES;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
```

## Solución de Problemas

### Error: "Cannot open database"

**Solución:** Asegúrate de que LocalDB esté iniciado:
```powershell
sqllocaldb start MSSQLLocalDB
```

### Error: "Instance name is invalid"

**Solución:** Verifica el nombre de la instancia:
```powershell
sqllocaldb info
```

Si la instancia se llama diferente, actualiza la cadena de conexión en `appsettings.json`.

### Error: "dotnet-ef not found"

**Solución:** Instala las herramientas:
```bash
dotnet tool install --global dotnet-ef
```

