# API de Transacciones Bancarias

API REST para gestión de transacciones bancarias desarrollada con Clean Architecture y .NET 9.0.

## 1. Requisitos Previos

### SDK y Herramientas
- **.NET 9.0 SDK** o superior
  - Descargar desde: https://dotnet.microsoft.com/download/dotnet/9.0
  - Verificar instalación: `dotnet --version`

### Base de Datos
- **SQL Server** (Express, Standard, o Developer Edition)
  - Descargar desde: https://www.microsoft.com/sql-server/sql-server-downloads
  - O usar **SQL Server Express LocalDB** (incluido con Visual Studio)
  - Verificar instalación LocalDB: `sqllocaldb info`

### Herramientas Adicionales
- **Entity Framework Core Tools** (para migraciones):
  ```bash
  dotnet tool install --global dotnet-ef
  ```
  O actualizar si ya está instalado:
  ```bash
  dotnet tool update --global dotnet-ef
  ```

## 2. Instrucciones de Configuración

### Paso 1: Clonar o descargar el proyecto

```bash
git clone <url-del-repositorio>
cd "prueba tecnica"
```

### Paso 2: Configurar la cadena de conexión

Editar `src/Transacciones.API/appsettings.json` o `appsettings.Development.json`:

**Para SQL Server:**
```json
{
  "ConnectionStrings": {
    "TransaccionesConnection": "Server=TU_SERVIDOR;Database=DB_TRANSACCIONES;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Para LocalDB:**
```json
{
  "ConnectionStrings": {
    "TransaccionesConnection": "Server=(localdb)\\mssqllocaldb;Database=DB_TRANSACCIONES;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Para SQL Server con autenticación SQL:**
```json
{
  "ConnectionStrings": {
    "TransaccionesConnection": "Server=TU_SERVIDOR;Database=DB_TRANSACCIONES;User Id=usuario;Password=password;TrustServerCertificate=True;"
  }
}
```

### Paso 3: Configurar JWT (Opcional)

La API incluye autenticación JWT. Ver `README_JWT.md` para más detalles sobre la configuración de tokens.

## 3. Comandos para Ejecutar el Proyecto y Base de Datos

### Crear y aplicar migraciones

```bash
# Crear una nueva migración
dotnet ef migrations add NombreMigracion --project src/Transacciones.Infrastructure --startup-project src/Transacciones.API

# Aplicar migraciones (crea la base de datos si no existe)
dotnet ef database update --project src/Transacciones.Infrastructure --startup-project src/Transacciones.API
```

### Eliminar y recrear la base de datos

```bash
# Eliminar la base de datos
dotnet ef database drop --force --project src/Transacciones.Infrastructure --startup-project src/Transacciones.API

# Recrear desde cero
dotnet ef database update --project src/Transacciones.Infrastructure --startup-project src/Transacciones.API
```

### Ejecutar el proyecto

```bash
# Desde la raíz del proyecto
dotnet run --project src/Transacciones.API/Transacciones.API.csproj

# O desde el directorio de la API
cd src/Transacciones.API
dotnet run
```

La API estará disponible en:
- HTTP: `http://localhost:5034`
- HTTPS: `https://localhost:5034`
- Swagger UI: `http://localhost:5034/swagger/index.html`

### Compilar el proyecto

```bash
dotnet build
```

## 4. Comandos para Ejecutar Tests

### Ejecutar todos los tests

```bash
dotnet test
```

### Ejecutar tests de un proyecto específico

```bash
dotnet test src/Transacciones.Tests/Transacciones.Tests.csproj
```

### Ejecutar tests con salida detallada

```bash
dotnet test --verbosity normal
```

## 5. Decisiones Técnicas Tomadas

### Arquitectura
- **Clean Architecture**: Separación en capas (API, Core, Infrastructure)
  - **Transacciones.API**: Controladores, Middlewares, Configuración
  - **Transacciones.Core**: Entidades, DTOs, Interfaces, Servicios, Excepciones
  - **Transacciones.Infrastructure**: Persistencia, Repositorios, DbContext
  - **Transacciones.Tests**: Tests unitarios

### Framework y Tecnologías
- **.NET 9.0**: Última versión LTS para mejor rendimiento y características
- **Entity Framework Core 9.0**: ORM para acceso a datos
- **SQL Server**: Base de datos relacional robusta y escalable
- **AutoMapper**: Mapeo automático entre entidades y DTOs
- **xUnit**: Framework de testing unitario
- **Moq**: Framework de mocking para tests

### Manejo de Excepciones
- **Excepciones personalizadas del dominio**: 
  - `CustomException` base con códigos HTTP
  - Excepciones específicas por contexto (Cuentas, Transacciones)
  - Mensajes descriptivos y localizados
- **Middleware centralizado**: `ErrorHandlingMiddleware` maneja todas las excepciones y retorna respuestas HTTP apropiadas

### Validaciones
- **Validaciones en el servicio**: Lógica de negocio centralizada
- **Validaciones de saldo**: 
  - Saldo inicial >= 0
  - No permitir saldos negativos en ningún momento
  - Validación de saldo suficiente antes de retiros
- **Validaciones de estado**: Verificación de cuenta activa antes de operaciones

### Transacciones de Base de Datos
- **Transacciones explícitas**: Uso de `BEGIN TRANSACTION` / `COMMIT` / `ROLLBACK`
- **Garantía de consistencia**: Saldo y historial se actualizan atómicamente
- **Rollback automático**: En caso de error, se revierten todos los cambios

### Seguridad
- **JWT Authentication**: Autenticación basada en tokens
- **Endpoints protegidos**: Uso de `[Authorize]` para endpoints sensibles
- **Validación de credenciales**: Endpoint de generación de tokens con validación

## 6. Estrategia de Concurrencia Implementada

### Control de Concurrencia Optimista (Optimistic Concurrency Control)

La aplicación implementa **concurrencia optimista** usando el campo `RowVersion` en la entidad `Cuenta`:

#### Implementación

1. **Campo RowVersion**:
   ```csharp
   public class Cuenta
   {
       public byte[] RowVersion { get; set; } = Array.Empty<byte>();
   }
   ```

2. **Configuración en DbContext**:
   ```csharp
   entity.Property(e => e.RowVersion)
       .IsRowVersion(); // Marca el campo como token de concurrencia
   ```

3. **En la base de datos**:
   - El campo `RowVersion` es de tipo `rowversion` en SQL Server
   - Se actualiza automáticamente en cada modificación de la fila
   - SQL Server garantiza que cada actualización incrementa el valor

#### Funcionamiento

- **Lectura**: Al leer una cuenta, se obtiene el `RowVersion` actual
- **Actualización**: Al actualizar, Entity Framework Core incluye el `RowVersion` en la cláusula WHERE
- **Detección de conflicto**: Si el `RowVersion` cambió entre lectura y escritura, SQL Server detecta que la fila fue modificada
- **Excepción**: Entity Framework Core lanza `DbUpdateConcurrencyException` cuando detecta el conflicto
- **Manejo**: El `ErrorHandlingMiddleware` captura la excepción y retorna HTTP 409 Conflict

#### Ventajas

- ✅ **Mejor rendimiento**: No bloquea registros durante la lectura
- ✅ **Escalabilidad**: Múltiples usuarios pueden leer simultáneamente
- ✅ **Detección automática**: SQL Server y EF Core manejan la detección de conflictos
- ✅ **Mensajes claros**: El middleware retorna mensajes descriptivos al usuario

#### Transacciones Explícitas

Además del control optimista, se usan **transacciones explícitas de base de datos** para garantizar atomicidad:

```csharp
using var transaction = await _context.BeginTransactionAsync();
try
{
    // Operaciones atómicas
    await _transaccionRepository.CreateAsync(transaccion);
    await _cuentaRepository.UpdateAsync(cuenta);
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

Esto garantiza que:
- El saldo y el historial se actualizan juntos
- Si falla alguna operación, se revierten todos los cambios
- No hay estados inconsistentes en la base de datos

#### Casos de Uso Protegidos

- **Múltiples retiros simultáneos**: El `RowVersion` previene que dos retiros se procesen con el mismo saldo base
- **Actualizaciones concurrentes**: Si dos usuarios modifican la misma cuenta, el último en guardar recibe un error 409
- **Consistencia de datos**: Las transacciones explícitas garantizan que saldo e historial siempre estén sincronizados

---

## Estructura del Proyecto

```
src/
├── Transacciones.API/          # Capa de presentación (Controllers, Middlewares)
├── Transacciones.Core/          # Capa de dominio (Entities, Services, DTOs, Exceptions)
├── Transacciones.Infrastructure/# Capa de infraestructura (Persistence, Repositories)
└── Transacciones.Tests/        # Tests unitarios
```

## Endpoints Principales

### Autenticación
- `POST /api/Token/generate` - Generar token JWT

### Cuentas
- `POST /api/Cuentas` - Crear nueva cuenta
- `GET /api/Cuentas/{id}` - Obtener cuenta por ID
- `GET /api/Cuentas/numero/{numeroCuenta}` - Obtener cuenta por numeroCuenta


### Transacciones
- `POST /api/transacciones/abono` - Realizar abono
- `POST /api/transacciones/retiro` - Realizar retiro
- `GET /api/transacciones/cuenta/{id}` - Listar transacciones por cuenta

## Tecnologías Utilizadas

- **ASP.NET Core 9.0**
- **Entity Framework Core 9.0**
- **SQL Server**
- **AutoMapper**
- **xUnit** (Testing)
- **Moq** (Mocking)
- **Swagger/OpenAPI**
