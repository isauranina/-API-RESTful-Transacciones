# Guía: lint-staged con dotnet format para archivos *.cs

Esta guía explica cómo instalar y configurar **lint-staged** para ejecutar **dotnet format** solo sobre los archivos `*.cs` que vas a incluir en el commit, integrado con Husky en el pre-commit.

## Requisitos previos

- **Node.js** y **npm** instalados
- **.NET SDK** (incluye la herramienta `dotnet format`)
- **Husky** ya configurado en el proyecto (pre-commit)
- **Solución .NET** en la raíz del repo (por ejemplo `TransaccionesAPI.sln`)

## 1. Instalar la herramienta dotnet format (si no la tienes)

```bash
dotnet tool install -g dotnet-format
```

Para actualizarla:

```bash
dotnet tool update -g dotnet-format
```

Comprueba que está instalada:

```bash
dotnet format --version
```

## 2. Instalar lint-staged

Desde la raíz del proyecto:

```bash
npm install --save-dev lint-staged
```

## 3. Configurar lint-staged en package.json

Añade la sección `"lint-staged"` en tu `package.json`:

```json
{
  "lint-staged": {
    "*.cs": "dotnet format TransaccionesAPI.sln --include"
  }
}
```

- **`*.cs`**: solo se ejecuta el comando sobre archivos C# que estén en el área de staging.
- **`dotnet format TransaccionesAPI.sln --include`**: lint-staged añade al final la lista de archivos en staging, por lo que se ejecuta algo como:
  `dotnet format TransaccionesAPI.sln --include src/Archivo1.cs src/Archivo2.cs`
- Así solo se formatean los archivos que vas a commitear, no toda la solución.

Ajusta `TransaccionesAPI.sln` si tu archivo de solución tiene otro nombre o está en otra ruta.

## 4. Integrar lint-staged en el pre-commit (Husky)

El hook de Husky debe ejecutar lint-staged en lugar de (o además de) lanzar `dotnet format` sobre toda la solución.

Edita `.husky/pre-commit` y deja algo como:

```sh
npx lint-staged
```

Si quieres seguir comprobando que no haya cambios pendientes de formatear (por ejemplo en CI), puedes mantener un script aparte que ejecute `dotnet format --verify-no-changes` en otro hook o en el pipeline.

## 5. Comportamiento al hacer commit

1. Añades archivos: `git add src/MiClase.cs`
2. Haces commit: `git commit -m "mensaje"`
3. Husky ejecuta el pre-commit.
4. lint-staged ejecuta `dotnet format TransaccionesAPI.sln --include src/MiClase.cs` (y el resto de `*.cs` en staging).
5. Si `dotnet format` modifica algo, esos cambios quedan en el working tree. Puedes volver a añadirlos y commitear:
   - `git add .`
   - `git commit --amend --no-edit`
   o hacer un nuevo commit con los cambios de formato.

## 6. Opciones útiles de dotnet format

Puedes ajustar el comando dentro de `lint-staged` según necesites:

| Opción | Descripción |
|--------|-------------|
| `--include` | Lista de archivos/carpetas a incluir (lint-staged la rellena con los `*.cs` en staging). |
| `--verify-no-changes` | Solo comprobar; no escribe en disco. Útil para CI, no para formatear en pre-commit. |
| `--no-restore` | No ejecutar restore; puede acelerar si la solución ya está restaurada. |

Ejemplo usando `--no-restore`:

```json
"lint-staged": {
  "*.cs": "dotnet format TransaccionesAPI.sln --no-restore --include"
}
```

## 7. Formatear según .editorconfig

`dotnet format` usa por defecto las reglas del **.editorconfig** del proyecto. Asegúrate de tener un `.editorconfig` en la raíz con las reglas que quieras (indentación, espacios, etc.). Así, tanto el IDE como el pre-commit aplican el mismo estilo.

## 8. Solución de problemas

### El comando no encuentra la solución

- Comprueba que ejecutas `npm run` / `npx lint-staged` desde la raíz del repo donde está el `.sln`.
- Si la solución está en una subcarpeta, usa la ruta relativa en el comando, por ejemplo:
  `dotnet format ./ruta/MiSolucion.sln --include`

### dotnet format no está en el PATH

- Si lo instalaste con `dotnet tool install -g dotnet-format`, debe estar en el PATH. Reinicia la terminal si acabas de instalarlo.
- En algunos entornos (CI, containers) puede que tengas que usar la ruta completa o instalar la herramienta en ese mismo paso.

### Quiero que solo verifique y falle el commit si hay que formatear

En `lint-staged` puedes usar `--verify-no-changes` para que no modifique archivos y falle si hubiera cambios:

```json
"lint-staged": {
  "*.cs": "dotnet format TransaccionesAPI.sln --verify-no-changes --include"
}
```

En ese caso el commit se rechaza y el desarrollador debe formatear (en IDE o con `dotnet format` sin `--verify-no-changes`) y volver a intentar.

---

**Resumen:** Con lint-staged + dotnet format en el pre-commit, cada commit formatea automáticamente solo los archivos `*.cs` que vas a subir, usando las reglas de tu `.editorconfig`.
