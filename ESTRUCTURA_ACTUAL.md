# Estructura Actual del Proyecto - Clean Architecture .NET 8

## Estructura de Carpetas Principal

```
CleanArchitectureNet8/
├── database/                           # ✨ NUEVO: SQL Server Database Project
│   ├── CleanArchitectureDb/
│   │   ├── Tables/
│   │   │   ├── Shared/                # Tablas de infraestructura
│   │   │   ├── Examples/              # Tablas de ejemplo
│   │   │   └── Security/              # Tablas de Identity
│   │   └── Scripts/PostDeployment/   # Seeds SQL
│   ├── QUICK_START.md
│   └── RESUMEN_IMPLEMENTACION.md
│
├── src/                               # Código fuente
│   ├── AppApi/                        # Presentation Layer (API)
│   ├── Application/                   # Application Layer (CQRS)
│   ├── Domain/                        # Domain Layer (Entidades)
│   ├── Persistence/                   # Infrastructure - Data Access
│   ├── Security/                      # Infrastructure - Identity
│   └── Shared/                        # Infrastructure - Servicios compartidos
│
├── tests/                             # Tests unitarios
├── docs/                              # Documentación técnica
└── docker-compose.yml                 # Docker para SQL Server
```

## Cambios Importantes desde Versión Anterior

### 1. ✨ Proyecto SQL Server Database (NUEVO)
- **Base de datos gestionada desde Visual Studio**
- No más migrations de EF Core (bug en EF Core 9)
- Control completo del esquema SQL
- Seeds en archivos .sql

### 2. ��� Reestructuración de Carpetas
- `Application` movido de `src/Core/Application` → `src/Application`
- `Domain` movido de `src/Core/Domain` → `src/Domain`
- Eliminadas carpetas obsoletas `src/Core`, `src/Infrastructure`, `src/Presentation`

### 3. ���️ Sin Migrations de EF Core
- Eliminadas carpetas `Migrations/` de Persistence y Security
- Eliminadas herramientas `EntityFrameworkCore.Tools` y `.Design`
- Seeds solo para Identity (usuarios de prueba)

### 4. ��� Configuración Simplificada
- Sin opción `UseInMemoryDatabase`
- Solo SQL Server como base de datos
- `RunMigrationsOnStartup` → `RunSeedsOnStartup`

## Stack Tecnológico Actual

- **.NET 8.0** (Framework)
- **EF Core 8.0.11** (Sin migrations, solo para queries)
- **SQL Server 2019+** (Base de datos)
- **ASP.NET Core Identity** (Autenticación)
- **MediatR** (CQRS)
- **AutoMapper** (Mapeo)
- **FluentValidation** (Validación)
- **Serilog** (Logging)
- **xUnit + Moq** (Testing)

