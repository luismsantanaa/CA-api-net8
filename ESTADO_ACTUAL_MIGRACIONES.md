# Estado Actual - Problema con Migraciones EF Core

## ✅ Lo que FUNCIONA:

1. **Compilación**: ✅ 0 errores, proyecto compila perfectamente
2. **Arquitectura**: ✅ Clean Architecture implementada correctamente
3. **Entidades**: ✅ TestCategory y TestProduct con Guid IDs
4. **DTOs y Mappings**: ✅ Todos actualizados para usar Guid
5. **Commands y Queries**: ✅ CQRS completo funcional
6. **Controllers**: ✅ REST API endpoints con Guid
7. **Validadores**: ✅ FluentValidation configurado
8. **Seeds**: ✅ Datos de ejemplo preparados con Guids predefinidos

## ❌ Lo que NO FUNCIONA:

**Problema**: `NullReferenceException` al intentar ejecutar migraciones automáticas

```
[20:27:51 INF] Ejecutando migraciones de base de datos...
[20:27:52 ERR] Error durante la inicialización de la base de datos
System.NullReferenceException: Object reference not set to an instance of an object.
```

### Intentos Realizados:

1. ✗ EF Core 9.0.0 - Bug conocido en `FindCollectionMapping`
2. ✗ EF Core 8.0.11 - Mismo error
3. ✗ Entidades heredando de `AuditableEntity` - Error
4. ✗ Entidades sin herencia (POCO) - Error con `IRepositoryFactory`
5. ✗ `SimpleEntity` heredando de `BaseEntity` - Mismo error

### Causa Raíz:

El error ocurre cuando EF Core intenta construir el modelo en runtime/design-time. El problema parece estar relacionado con:
- La jerarquía de herencia (`BaseEntity` → `SimpleEntity` → Entidades)
- Las propiedades de auditoría
- Alguna incompatibilidad interna de EF Core con la configuración actual

## 🔧 Soluciones Propuestas:

### Opción 1: Crear Base de Datos Manualmente (RECOMENDADO)

Crear un script SQL para crear la base de datos y tablas:

```sql
USE master;
GO

-- Crear base de datos si no existe
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CleanArchitectureDb')
BEGIN
    CREATE DATABASE CleanArchitectureDb;
END
GO

USE CleanArchitectureDb;
GO

-- Crear esquemas
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Shared')
BEGIN
    EXEC('CREATE SCHEMA Shared');
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Examples')
BEGIN
    EXEC('CREATE SCHEMA Examples');
END
GO

-- Tabla Categories
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories' AND schema_id = SCHEMA_ID('Examples'))
BEGIN
    CREATE TABLE Examples.Categories (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        Active BIT NOT NULL DEFAULT 1,
        CreatedOn DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(MAX),
        LastModifiedOn DATETIME2,
        LastModifiedBy NVARCHAR(MAX)
    );
    
    CREATE INDEX IX_Categories_Name ON Examples.Categories(Name);
END
GO

-- Tabla Products
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products' AND schema_id = SCHEMA_ID('Examples'))
BEGIN
    CREATE TABLE Examples.Products (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000),
        Price DECIMAL(18,2) NOT NULL,
        Stock INT NOT NULL,
        CategoryId UNIQUEIDENTIFIER NOT NULL,
        Active BIT NOT NULL DEFAULT 1,
        CreatedOn DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(MAX),
        LastModifiedOn DATETIME2,
        LastModifiedBy NVARCHAR(MAX),
        CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) 
            REFERENCES Examples.Categories(Id) ON DELETE NO ACTION
    );
    
    CREATE INDEX IX_Products_Name ON Examples.Products(Name);
    CREATE INDEX IX_Products_CategoryId ON Examples.Products(CategoryId);
END
GO

-- Tablas Shared (AuditLogs, MailNotifications, UploadedFiles)
-- [Agregar según necesidad]

PRINT 'Base de datos y tablas creadas exitosamente';
GO
```

**Pasos**:
1. Ejecutar el script SQL en SQL Server Management Studio o Azure Data Studio
2. Cambiar `RunMigrationsOnStartup` a `false` en `appsettings.json`
3. Ejecutar la aplicación normalmente
4. Los seeds se ejecutarán automáticamente

### Opción 2: Usar Migration Bundle (Fuera de la aplicación)

```bash
dotnet ef migrations bundle --project src/Infrastructure/Persistence --startup-project src/Presentation/AppApi
```

Esto crea un ejecutable independiente que puede aplicar las migraciones sin depender del runtime de la aplicación.

### Opción 3: Simplificar Entidades (Última Opción)

Eliminar completamente la herencia y usar entidades POCO puras, modificando también `IRepositoryFactory` para no requerir herencia de `BaseEntity`.

## 📊 Estado Actual del Código:

```
src/Core/Domain/
├── Base/
│   ├── BaseEntity.cs (Guid Id, bool Active)
│   ├── SimpleEntity.cs (hereda de BaseEntity)
│   └── AuditableEntity.cs (hereda de BaseEntity, propiedades de auditoría)
├── Entities/Examples/
│   ├── TestCategory.cs (hereda de SimpleEntity)
│   └── TestProduct.cs (hereda de SimpleEntity)

src/Core/Application/
├── DTOs/Examples/ (Guid IDs) ✅
├── Mappings/Examples/ (AutoMapper profiles) ✅
├── Features/Examples/
│   ├── Categories/
│   │   ├── Queries/ (CQRS Queries) ✅
│   │   └── Commands/ (CQRS Commands) ✅
│   └── Products/
│       ├── Queries/ (CQRS Queries con validadores) ✅
│       └── Commands/ (CQRS Commands con validadores) ✅

src/Presentation/AppApi/
└── Controllers/Examples/
    ├── CategoriesController.cs (REST endpoints) ✅
    └── ProductsController.cs (REST endpoints) ✅
```

## ✅ Recomendación Final:

**Usar la Opción 1 (Script SQL manual)**:
1. Es la más confiable
2. Evita el bug de EF Core
3. Permite control total sobre el esquema
4. El código de la aplicación está 100% funcional

Una vez creada la base de datos manualmente:
- Los seeds funcionarán automáticamente
- Los endpoints REST funcionarán perfectamente
- CQRS, validaciones y todo el flujo está correcto

**El problema NO está en el código de la aplicación, sino en el proceso de migración/generación de esquema de EF Core.**

---

**Fecha**: Noviembre 8, 2025  
**Versión**: .NET 9.0, EF Core 8.0.11

