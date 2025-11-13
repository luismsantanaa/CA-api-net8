# Estado Actual del Proyecto CleanArchitectureNet8

## Fecha: 8 de Noviembre de 2025

## Problema Identificado

El proyecto está enfrentando un **bug crítico de Entity Framework Core** que causa un `NullReferenceException` en:

```
Microsoft.EntityFrameworkCore.Storage.RelationalTypeMappingSource.FindCollectionMapping()
```

### Causa Raíz

Este bug está relacionado con:
- Propiedades de navegación de colección (ICollection<T>) en entidades
- Interacción entre .NET 8 SDK y versiones específicas de EF Core 8.0.x
- El descubrimiento automático de entidades por parte de EF Core

## Acciones Tomadas

### 1. Configuración de SDK (.NET 8)
✅ Creado `global.json` para forzar .NET 8.0.121:
```json
{
  "sdk": {
    "version": "8.0.121",
    "rollForward": "latestPatch"
  }
}
```

### 2. Actualización de Paquetes EF Core
✅ Todos los paquetes de EF Core actualizados a versión 8.0.11

### 3. Entidades de Ejemplo Temporalmente Deshabilitadas
⚠️ Las siguientes entidades han sido **renombradas** para evitar el bug:
- `TestCategory` → `TestCategory_DISABLED`
- `TestProduct` → `TestProduct_DISABLED`

**Archivos afectados:**
- `src/Core/Domain/Entities/Examples/TestCategory.cs`
- `src/Core/Domain/Entities/Examples/TestProduct.cs`
- Todas las referencias en Application, Persistence, y Tests

### 4. Configuración de Base de Datos
✅ **Actualmente configurado para usar InMemory Database**

En `appsettings.json`:
```json
"UseInMemoryDatabase": true
```

## Configuración Actual

### Funcional ✅
- ✅ Compilación exitosa
- ✅ Base de datos InMemory
- ✅ Entidades Shared (AuditLog, MailNotificationTemplate, UploadedFile)
- ✅ Identity y autenticación JWT
- ✅ Todos los patrones de arquitectura limpia
- ✅ Tests compilando correctamente

### Temporalmente Deshabilitado ⚠️
- ⚠️ Entidades de ejemplo (TestCategory, TestProduct)
- ⚠️ Controladores de ejemplo (eliminados)
- ⚠️ Migraciones de SQL Server (pendientes de recrear)
- ⚠️ Seed data de ejemplos (comentado)

## Próximos Pasos

### Opción 1: Mantener InMemory (Desarrollo Rápido)
```json
"UseInMemoryDatabase": true
```
- Desarrollo y pruebas rápidas
- No requiere SQL Server
- Datos se pierden al reiniciar

### Opción 2: Usar SQL Server (Producción)
```json
"UseInMemoryDatabase": false
```
⚠️ **REQUIERE** resolver el bug de EF Core primero

**Posibles soluciones:**
1. Actualizar a EF Core 9.0 (cuando sea estable)
2. Esperar patch de Microsoft para EF Core 8.0.x
3. Mantener entidades de ejemplo deshabilitadas
4. Rediseñar relaciones para evitar colecciones de navegación

### Opción 3: Reactivar Entidades de Ejemplo

**Para reactivar las entidades de ejemplo:**

1. Renombrar clases:
   ```
   TestCategory_DISABLED → TestCategory
   TestProduct_DISABLED → TestProduct
   ```

2. Actualizar todas las referencias en el código

3. Descomentar configuraciones y seed data

4. **IMPORTANTE**: El bug de EF Core puede volver a aparecer

## Comandos Útiles

### Ejecutar Aplicación (InMemory)
```bash
dotnet run --project src/Presentation/AppApi/AppApi.csproj
```

### Verificar Versión de SDK
```bash
dotnet --version
# Debe mostrar: 8.0.121
```

### Limpiar y Recompilar
```bash
dotnet clean
dotnet restore
dotnet build
```

### Crear Migraciones (cuando se resuelva el bug)
```bash
dotnet ef migrations add InitialMigration --project src/Infrastructure/Persistence/Persistence.csproj --startup-project src/Presentation/AppApi/AppApi.csproj --context ApplicationDbContext
```

## Conclusión

El proyecto está **funcional** con base de datos InMemory. Para usar SQL Server en producción, se necesita:
- Resolver el bug de EF Core, O
- Mantener las entidades de ejemplo deshabilitadas

## Referencias

- [EF Core GitHub Issues - FindCollectionMapping](https://github.com/dotnet/efcore/issues)
- [.NET 8 LTS Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [EF Core 8 Release Notes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew)

---
**Última actualización:** 8 de Noviembre de 2025, 17:30

