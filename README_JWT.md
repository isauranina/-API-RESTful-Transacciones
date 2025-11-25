# Configuración de JWT para Transacciones API

## Autenticación

La API utiliza autenticación JWT mediante un endpoint de generación de tokens que valida credenciales específicas.

## Generar Token

Para obtener un token JWT, utiliza el endpoint `/api/Token/generate`:

```bash
POST /api/Token/generate
Content-Type: application/json

{
  "User": "admin",
  "Password": "Clave*"
}
```

### Credenciales

- **Usuario:** `admin`
- **Contraseña:** `Clave*`

### Respuesta Exitosa

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 86400,
  "tokenType": "Bearer"
}
```

**Nota:** El token tiene una duración de 1 dia desde su generación solo por que es una prueba tecnica.

### Respuesta de Error

Si las credenciales son incorrectas:

```json
{
  "error": "Credenciales inválidas"
}
```

## Usar Token en Requests

Una vez obtenido el token, inclúyelo en el header `Authorization` de todas las peticiones:

```
Authorization: Bearer {tu-token-jwt}
```

### Ejemplo con cURL

```bash
curl -X GET "https://tu-api.com/api/endpoint" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

## Validar Token

Puedes validar un token usando el endpoint `/api/Token/validate` (requiere autenticación):

```bash
GET /api/Token/validate
Authorization: Bearer {tu-token-jwt}
```

### Respuesta

```json
{
  "isValid": true,
  "claims": [
    {
      "type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
      "value": "admin"
    }
  ],
  "username": "admin"
}
```

## Proteger Endpoints

Para proteger un endpoint, usa el atributo `[Authorize]`:

```csharp
[Authorize]
[HttpGet]
public IActionResult GetProtectedData()
{
    // Solo usuarios autenticados pueden acceder
    return Ok();
}
```



## Swagger UI

Swagger está configurado para aceptar tokens JWT. En la interfaz de Swagger:

1. Haz clic en el botón **"Authorize"** (🔒)
2. Ingresa: `Bearer {tu-token-jwt}`
3. Haz clic en **"Authorize"**
4. Ahora puedes probar los endpoints protegidos directamente desde Swagger

**Consejo:** Primero genera un token usando el endpoint `/api/Token/generate` y luego úsalo en el botón Authorize.

## Implementación Técnica

### Clave Secreta

La autenticación JWT está configurada en el `Program.cs` y utiliza la configuración de `appsettings.json` para los parámetros del Bearer token (Issuer, Audience, etc.).  y utiliza el algoritmo `HS256`.

## Seguridad
para la puesta en produccion se implementara Variables de Entorno  para token
