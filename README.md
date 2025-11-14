# Clean Architecture .NET 8 Template

Plantilla base para desarrollo de APIs RESTful usando Clean Architecture, .NET 8, Entity Framework Core y ASP.NET Core con SQL Server Database Project.

## 📋 Tabla de Contenidos

- [Características](#-características)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Requisitos Previos](#-requisitos-previos)
- [Inicio Rápido](#-inicio-rápido)
- [Guías de Desarrollo](#-guías-de-desarrollo)
- [Arquitectura](#-arquitectura)
- [Herramientas y Tecnologías](#-herramientas-y-tecnologías)
- [Tests](#-tests)

## 🎯 Características

Este proyecto incluye una arquitectura limpia con las siguientes características:

- ✅ **Clean Architecture** - Separación clara de capas (Domain, Application, Infrastructure, Presentation)
- ✅ **✨ SQL Server Database Project** - Control total del esquema de base de datos desde Visual Studio
- ✅ **CQRS con MediatR** - Separación de comandos y consultas
- ✅ **Repository Pattern** - Abstracción de acceso a datos
- ✅ **Unit of Work** - Gestión de transacciones
- ✅ **Specification Pattern** - Construcción de consultas complejas
- ✅ **JWT Authentication** - Autenticación basada en tokens
- ✅ **ASP.NET Core Identity** - Gestión de usuarios y roles
- ✅ **FluentValidation** - Validación de requests
- ✅ **AutoMapper** - Mapeo de objetos
- ✅ **Serilog** - Logging estructurado
- ✅ **Health Checks** - Monitoreo de salud de la aplicación
- ✅ **Caching** - Caché local y distribuido (Redis)
- ✅ **Pagination** - Sistema completo de paginación con filtros y ordenamiento
- ✅ **Exception Handling** - Manejo centralizado de excepciones
- ✅ **XML Documentation** - Documentación automática de API
- ✅ **Tests Unitarios** - 100+ tests con xUnit y Moq (99% cobertura)

## 📁 Estructura del Proyecto

```
CleanArchitectureNet8/
├── database/                           # ✨ NUEVO: SQL Server Database Project
│   ├── CleanArchitectureDb/
│   │   ├── Tables/                    # Definiciones de tablas
│   │   │   ├── Shared/                # Tablas de infraestructura
│   │   │   ├── Examples/              # Tablas de ejemplo
│   │   │   └── Security/              # Tablas de Identity
│   │   └── Scripts/PostDeployment/   # Seeds SQL
│   ├── QUICK_START.md                 # Guía rápida
│   └── RESUMEN_IMPLEMENTACION.md      # Detalles técnicos
│
├── src/
│   ├── AppApi/                        # Presentation Layer (API REST)
│   ├── Application/                   # Application Layer (CQRS, DTOs)
│   ├── Domain/                        # Domain Layer (Entidades)
│   ├── Persistence/                   # Infrastructure - Data Access
│   ├── Security/                      # Infrastructure - Identity & Auth
│   └── Shared/                        # Infrastructure - Servicios Compartidos
│
├── tests/                             # Tests unitarios (xUnit + Moq)
├── docs/                              # Documentación técnica completa
└── docker-compose.yml                 # Docker para SQL Server
```

> 📖 Para más detalles sobre la estructura, consulta [docs/ESTRUCTURA_COMPLETA.md](docs/ESTRUCTURA_COMPLETA.md)

## 🔧 Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (2019 o superior)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) con SQL Server Data Tools (SSDT)
- [Redis](https://redis.io/download) (opcional, para caché distribuido)

### Instalar SQL Server Data Tools (SSDT)

1. Abre **Visual Studio Installer**
2. Click en **"Modify"** en tu instalación de VS 2022
3. En **"Individual components"**, busca y marca: **"SQL Server Data Tools"**
4. Click **"Modify"** para instalar

## 🚀 Inicio Rápido

### 1. Clonar y Configurar

```bash
# Clonar el repositorio
git clone <repository-url>
cd CleanArchitectureNet8

# Copiar archivo de configuración
cp src/AppApi/appsettings.json.example src/AppApi/appsettings.json
```

### 2. Configurar Base de Datos

Edita `src/AppApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ApplicationConnection": "Server=localhost,11433;Database=CleanArchitectureDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;",
    "IdentityConnection": "Server=localhost,11433;Database=CleanArchitectureDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;"
  },
  "DatabaseOptions": {
    "RunSeedsOnStartup": false
  }
}
```

### 3. Iniciar SQL Server con Docker

```bash
# Iniciar SQL Server
docker-compose up -d mssql

# Verificar que esté corriendo
docker ps
```

### 4. Publicar Base de Datos desde Visual Studio

**✨ IMPORTANTE:** La base de datos se gestiona desde el **SQL Server Database Project**, no con migrations de EF Core.

1. Abre `CleanArchitectureNet8.sln` en Visual Studio 2022
2. En el **Solution Explorer**, navega a: `database → CleanArchitectureDb`
3. **Click derecho** en `CleanArchitectureDb` → **Publish...**
4. Configura la conexión:
   - Server: `localhost,11433`
   - Authentication: SQL Server Authentication
   - Username: `sa`
   - Password: `YourPassword123!`
   - Database: `CleanArchitectureDb`
5. Click **"Publish"**

> 📖 Guía detallada: [database/QUICK_START.md](database/QUICK_START.md)

### 5. Configurar JWT

En `appsettings.json`, configura tus credenciales JWT:

```json
{
  "JwtSettings": {
    "Key": "TWFyZG9tLkRHQS1ET0wuSW50ZWdyYXRpb24oTFMp",
    "Issuer": "mardom.cleanArchitecture",
    "Audience": "mardom.com",
    "DurationInMinutes": 360,
    "ExpireTime": "06:00:00"
  }
}
```

> ⚠️ **Importante**: Cambia la clave en producción. Genera una segura con: `Convert.ToBase64String(Encoding.UTF8.GetBytes("tu-clave-super-secreta"))`

### 6. Ejecutar el Proyecto

```bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar la API
dotnet run --project src/AppApi
```

La API estará disponible en: `https://localhost:5001` o `http://localhost:5000`

### 7. (Opcional) Crear Usuario de Prueba

Si configuraste `RunSeedsOnStartup: true` en `appsettings.json`, la aplicación creará automáticamente:

**Credenciales de prueba:**
- Email: `test@mardom.com`
- Username: `testuser`
- Password: `Test123!@#`

### 8. Verificar que Funciona

```bash
# Health Check
curl https://localhost:5001/health

# Swagger UI (abre en el navegador)
https://localhost:5001/swagger
```

## 📚 Guías de Desarrollo

### Crear un Nuevo Feature (CRUD Completo)

El proyecto incluye ejemplos completos de **Productos** y **Categorías**. Sigue estos pasos para crear tu propio feature:

1. **[Crear una Nueva Entidad](docs/GUIA_DESARROLLO.md#1-crear-una-nueva-entidad)** - `src/Domain/Entities/`
2. **[Crear Tabla en SQL](database/QUICK_START.md)** - `database/CleanArchitectureDb/Tables/`
3. **[Crear Commands (CQRS)](docs/GUIA_DESARROLLO.md#2-crear-commands-cqrs)** - `src/Application/Features/`
4. **[Crear Queries (CQRS)](docs/GUIA_DESARROLLO.md#3-crear-queries-cqrs)** - `src/Application/Features/`
5. **[Crear Validators](docs/GUIA_DESARROLLO.md#4-crear-validators)** - FluentValidation
6. **[Crear View Models](docs/GUIA_DESARROLLO.md#5-crear-view-models)** - DTOs
7. **[Crear Controllers](docs/GUIA_DESARROLLO.md#6-crear-controllers)** - `src/AppApi/Controllers/`
8. **[Configurar AutoMapper](docs/GUIA_DESARROLLO.md#7-configurar-automapper)** - Mappings

### Documentación Completa

- **[📖 Índice](docs/INDICE.md)** - Navegación completa de documentación
- **[🏛️ Arquitectura](docs/ARQUITECTURA.md)** - Principios de Clean Architecture
- **[📁 Estructura Completa](docs/ESTRUCTURA_COMPLETA.md)** - Organización detallada
- **[💡 Ejemplos](docs/EJEMPLOS.md)** - Ejemplos de código y patrones
- **[📄 Paginación](docs/PAGINACION.md)** - Sistema de paginación completo
- **[🛠️ Herramientas](docs/HERRAMIENTAS.md)** - Stack tecnológico
- **[🗄️ Base de Datos](database/QUICK_START.md)** - SQL Server Database Project

## 🏛️ Arquitectura

Este proyecto implementa **Clean Architecture** con las siguientes capas:

```
┌─────────────────────────────────────────────────────┐
│           Presentation Layer (AppApi)               │
│  Controllers, Middleware, Configuration             │
└─────────────────────┬───────────────────────────────┘
                      │ Depends on
                      ▼
┌─────────────────────────────────────────────────────┐
│         Application Layer (Application)             │
│  Use Cases, CQRS (Commands/Queries), DTOs           │
└─────────────────────┬───────────────────────────────┘
                      │ Depends on
                      ▼
┌─────────────────────────────────────────────────────┐
│            Domain Layer (Domain)                    │
│  Entities, Value Objects, Domain Logic              │
│  ⚠️ No dependencies on other layers                 │
└─────────────────────────────────────────────────────┘
           ▲                              ▲
           │                              │
           │ Implements                   │ Implements
           │                              │
┌──────────┴──────────┐      ┌───────────┴──────────┐
│   Persistence        │      │   Security/Shared    │
│   (EF Core, Repos)   │      │   (Identity, Utils)  │
└─────────────────────┘      └──────────────────────┘
```

**Principios Aplicados:**
- **Dependency Inversion**: Las capas externas dependen de las internas
- **Separation of Concerns**: Cada capa tiene responsabilidades claras
- **SOLID Principles**: Diseño orientado a interfaces y extensibilidad

> 📖 Más detalles: [docs/ARQUITECTURA.md](docs/ARQUITECTURA.md)

## 🛠️ Herramientas y Tecnologías

### Core
- **.NET 8.0** - Framework principal
- **ASP.NET Core** - Web API
- **EF Core 8.0** - ORM (sin migrations, solo queries)
- **SQL Server 2019+** - Base de datos

### Patrones y Arquitectura
- **MediatR** - CQRS pattern
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Transaction management
- **Specification Pattern** - Query building

### Seguridad y Auth
- **ASP.NET Core Identity** - User management
- **JWT Bearer** - Token authentication
- **Custom Authorization** - Role-based access

### Utilidades
- **Serilog** - Structured logging
- **Health Checks** - Application monitoring
- **Swagger/OpenAPI** - API documentation
- **Redis** - Distributed caching (opcional)
- **MailKit** - Email sending
- **Docker** - Containerization

### Testing
- **xUnit** - Test framework
- **Moq** - Mocking library
- **FluentAssertions** - Assertion library

> 📖 Detalles completos: [docs/HERRAMIENTAS.md](docs/HERRAMIENTAS.md)

## 🧪 Tests

El proyecto incluye **101 tests unitarios** que pasan al 100%.

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests con cobertura
dotnet test /p:CollectCoverage=true
```

### Tipos de Tests Incluidos

- ✅ **Handler Tests** - Commands y Queries
- ✅ **Controller Tests** - API endpoints
- ✅ **Validator Tests** - Reglas de validación
- ✅ **Repository Tests** - Acceso a datos
- ✅ **Service Tests** - Lógica de negocio

> 📖 Ubicación: `tests/Tests/`

## 📊 Estado del Proyecto

- **Build**: ✅ Compila sin errores (0 errores, 4 warnings menores)
- **Tests**: ✅ 101 de 101 tests pasan (100%)
- **Base de Datos**: ✅ 14 tablas (3 Shared + 2 Examples + 9 Security)
- **Documentación**: ✅ Completa y actualizada
- **Estado**: ✅ Release Candidate para Producción

## 🔄 Workflow de Desarrollo

### Para modificar la Base de Datos:

1. **Editar tabla** en `database/CleanArchitectureDb/Tables/`
2. **Compilar** el proyecto SQL en Visual Studio
3. **Publicar** cambios con "Publish..." o "Schema Compare"
4. **Actualizar entidad** en `src/Domain/Entities/` (si aplica)
5. **Commit** cambios a Git

### Para agregar un nuevo Feature:

1. Sigue la [Guía de Desarrollo](docs/GUIA_DESARROLLO.md)
2. Crea la tabla en el SQL Database Project
3. Crea la entidad en Domain
4. Crea Commands/Queries en Application
5. Crea Controller en AppApi
6. Escribe tests
7. Commit y push

## 📝 Convenciones

- **Nombres**: PascalCase para clases, camelCase para variables
- **Estructura**: Commands/Queries en carpetas separadas por entidad
- **Retornos**: Siempre usar `Result<T>` para respuestas
- **Logging**: Estructurado con Serilog + LogContext
- **Validación**: En Validators de FluentValidation, no en Handlers
- **Excepciones**: Usar excepciones personalizadas en `Shared/Exceptions/`

## 🤝 Contribuir

Este es un template interno. Si encuentras bugs o mejoras:

1. Crea un issue describiendo el problema/mejora
2. Crea un branch: `feature/mi-mejora` o `fix/mi-bugfix`
3. Haz commit con mensajes descriptivos
4. Crea un Pull Request con descripción detallada

## 📄 Licencia

Uso interno. Todos los derechos reservados.

## 🆘 Soporte

- **Documentación**: Consulta [docs/INDICE.md](docs/INDICE.md)
- **Ejemplos**: Revisa `src/Application/Features/Examples/`
- **Tests**: Consulta `tests/Tests/` para ejemplos prácticos
- **Base de Datos**: Lee [database/QUICK_START.md](database/QUICK_START.md)
- **Mejoras Futuras**: Ver [ROADMAP.md](ROADMAP.md)
- **Release Notes**: Ver [RELEASE_NOTES.md](RELEASE_NOTES.md)

---

**Última actualización**: Noviembre 13, 2025  
**Versión**: 2.0 RC (Release Candidate)  
**Framework**: .NET 8.0  
**Estado**: ✅ Listo para Producción
