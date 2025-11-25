# Migraciones de Entity Framework Core

## Crear una migración inicial

```bash
cd src/Transacciones.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Transacciones.API --context TransaccionesDbContext
```

## Aplicar migraciones a la base de datos

```bash
cd src/Transacciones.API
dotnet ef database update --context TransaccionesDbContext
```

## Revertir la última migración

```bash
dotnet ef database update PreviousMigrationName --context TransaccionesDbContext
```

## Eliminar la última migración (sin aplicar cambios a la BD)

```bash
dotnet ef migrations remove --startup-project ../Transacciones.API --context TransaccionesDbContext
```

## Generar script SQL

```bash
dotnet ef migrations script --startup-project ../Transacciones.API --context TransaccionesDbContext --output migration.sql
```



