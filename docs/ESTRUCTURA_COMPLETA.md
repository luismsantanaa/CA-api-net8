# 📁 Estructura Completa del Proyecto

Este documento describe la organización de carpetas y archivos del proyecto Clean Architecture .NET 8.

## 🏗️ Estructura General

```
CleanArchitectureNet8/
├── database/                           # ✨ SQL Server Database Project
├── src/                                # Código fuente
├── tests/                              # Tests unitarios
├── docs/                               # Documentación técnica
├── docker-compose.yml                  # Docker para SQL Server
├── CleanArchitectureNet8.sln          # Solución Visual Studio
└── README.md                           # Documentación principal
```

## 🗄️ database/ - SQL Server Database Project

**✨ NUEVO:** Gestión completa de base de datos desde Visual Studio

```
database/
├── CleanArchitectureDb/
│   ├── CleanArchitectureDb.sqlproj    # Proyecto SQL Server
│   ├── README.md                       # Documentación del proyecto DB
│   ├── Development.publish.xml.example # Plantilla de publicación
│   │
│   ├── Tables/
│   │   ├── Shared/                    # Tablas de infraestructura
│   │   │   ├── AuditLogs.sql
│   │   │   ├── MailNotificationTemplate.sql
│   │   │   └── UploadedFile.sql
│   │   │
│   │   ├── Examples/                  # Tablas de ejemplo (CRUD demo)
│   │   │   ├── TestCategories.sql
│   │   │   └── TestProducts.sql
│   │   │
│   │   └── Security/                  # Tablas de ASP.NET Core Identity
│   │       ├── AspNetUsers.sql
│   │       ├── AspNetRoles.sql
│   │       ├── AspNetUserRoles.sql
│   │       ├── AspNetUserClaims.sql
│   │       ├── AspNetUserLogins.sql
│   │       ├── AspNetUserTokens.sql
│   │       ├── AspNetRoleClaims.sql
│   │       ├── RefreshTokens.sql
│   │       └── AppUsers.sql
│   │
│   └── Scripts/PostDeployment/        # Scripts de datos iniciales
│       ├── Script.PostDeployment.sql  # Script principal
│       ├── SeedSharedData.sql         # Datos compartidos
│       ├── SeedExampleData.sql        # Datos de ejemplo
│       └── SeedSecurityData.sql       # Roles por defecto
│
├── QUICK_START.md                      # Guía rápida de uso
└── RESUMEN_IMPLEMENTACION.md           # Detalles técnicos
```

**Ventajas:**

- ✅ Control total del esquema de base de datos
- ✅ Validación en tiempo de diseño
- ✅ Schema Compare automático
- ✅ Scripts idempotentes con MERGE
- ✅ Separación clara entre código y schema

## 📦 src/ - Código Fuente

```
src/
├── AppApi/                  # 🎯 Presentation Layer (API REST)
├── Application/             # 💼 Application Layer (CQRS, DTOs)
├── Domain/                  # 🏛️ Domain Layer (Entidades, Contratos)
├── Persistence/             # 🗄️ Infrastructure - Data Access
├── Security/                # 🔐 Infrastructure - Identity & Auth
└── Shared/                  # 🔧 Infrastructure - Servicios Compartidos
```

### 🎯 src/AppApi/ - Presentation Layer

```
AppApi/
├── Controllers/                # Controladores REST API
│   ├── ApiBaseController.cs   # Controlador base
│   ├── Auth/                   # Autenticación
│   │   └── AuthController.cs
│   ├── Examples/               # Ejemplos CRUD
│   │   ├── CategoriesController.cs
│   │   └── ProductsController.cs
│   └── Utilities/              # Utilidades
│
├── Middleware/                 # Middlewares personalizados
│   ├── ExceptionMiddleware.cs # Manejo global de excepciones
│   ├── JwtMiddleware.cs        # Validación JWT personalizada
│   └── CorrelationIdMiddleware.cs # IDs de correlación
│
├── Extensions/                 # Extensiones de configuración
│   ├── ServiceCollectionExtensions.cs # DI y Health Checks
│   └── ApplicationBuilderExtensions.cs # Middleware pipeline
│
├── Configuration/              # Validación de configuración
│   └── ConfigurationValidator.cs
│
├── Authorization/              # Autorización personalizada
│   ├── Attributes.cs
│   ├── AuthorizationStrategy.cs
│   └── JwtUtils.cs
│
├── Services/                   # Servicios específicos de API
│   └── GetUserService.cs
│
├── Program.cs                  # ⚙️ Punto de entrada
├── appsettings.json           # ⚙️ Configuración
├── appsettings.Development.json
├── appsettings.Production.json
└── AppApi.csproj              # ⚙️ Proyecto
```

### 💼 src/Application/ - Application Layer

```
Application/
├── Features/                   # 📂 Features organizados por entidad
│   ├── Examples/
│   │   ├── Categories/
│   │   │   ├── Commands/      # CreateCategoryCommand, UpdateCategoryCommand, etc.
│   │   │   └── Queries/       # GetCategoriesQuery, GetCategoryByIdQuery, etc.
│   │   └── Products/
│   │       ├── Commands/
│   │       └── Queries/
│   ├── App/                    # Features de aplicación
│   └── Utilities/              # Utilidades (archivos, correos)
│
├── DTOs/                       # Data Transfer Objects
│   ├── Examples/
│   │   ├── CategoryVm.cs      # View Models
│   │   ├── CategoryDto.cs     # DTOs
│   │   ├── ProductVm.cs
│   │   └── ProductDto.cs
│   ├── Result.cs              # Wrapper de respuestas
│   └── ResultExtensions.cs    # Helpers para Result<T>
│
├── Mappings/                   # Perfiles de AutoMapper
│   ├── Examples/
│   │   ├── CategoryProfile.cs
│   │   └── ProductProfile.cs
│   └── Utilities/
│
├── Behaviours/                 # MediatR Pipeline Behaviors
│   ├── LoggingPipelineBehavior.cs
│   ├── ValidationBehaviour.cs
│   └── UnhandledExceptionBehaviour.cs
│
├── Handlers/Base/              # Handlers base reutilizables
│   └── PaginatedQueryHandler.cs
│
├── Helpers/                    # Helpers y extensiones
│   ├── HandlerExtensions.cs
│   └── PaginationVmHelper.cs
│
├── Commons/                    # Utilidades comunes
│   └── ValidationHelper.cs
│
├── Contracts/                  # ⚠️ DEPRECATED: Interfaces movidas a Domain
│   └── IApplicationDbContext.cs
│
├── ApplicationServicesRegistration.cs # DI de Application
└── Application.csproj
```

**Nota:** `Application/Contracts/` está deprecated. Nuevas interfaces van en `Domain/Contracts/`.

### 🏛️ src/Domain/ - Domain Layer

```
Domain/
├── Entities/                   # Entidades del dominio
│   ├── Examples/               # Ejemplos
│   │   ├── TestCategory.cs
│   │   └── TestProduct.cs
│   ├── Shared/                 # Entidades compartidas
│   │   ├── AuditLog.cs
│   │   ├── MailNotificationTemplate.cs
│   │   └── UploadedFile.cs
│   └── Dbo/                    # Entidades de bases de datos externas
│
├── Base/                       # Clases base para entidades
│   ├── BaseEntity.cs          # Id + Active
│   ├── AuditableEntity.cs     # + Auditoría
│   ├── TraceableEntity.cs     # + Trazabilidad
│   ├── SoftDelete.cs          # + Borrado lógico
│   └── SimpleEntity.cs        # Versión simplificada
│
├── Contracts/                  # Interfaces del dominio
│   └── IApplicationDbContext.cs # Contrato del DbContext
│
├── Common/                     # Clases comunes
│   └── ValueObject.cs         # Base para Value Objects
│
└── Domain.csproj
```

**Jerarquía de Entidades:**

```
BaseEntity (Id, Active)
  └── AuditableEntity (+ Created, Modified, Version)
      └── TraceableEntity (+ Traceable flag)
          └── SoftDelete (+ IsDeleted, DeletedBy, DeletedAt)
```

### 🗄️ src/Persistence/ - Data Access

```
Persistence/
├── DbContexts/                 # Contextos de EF Core
│   ├── ApplicationDbContext.cs         # DbContext principal
│   ├── ApplicationDbContextFactory.cs  # Factory para design-time
│   └── ApplicationDbContextExtensions.cs
│
├── EntitiesConfigurations/     # Configuraciones de EF Core
│   ├── Shared/
│   │   ├── AuditLogConfiguration.cs
│   │   ├── MailNotificationTemplateConfiguration.cs
│   │   └── UploadedFileConfiguration.cs
│   ├── Examples/
│   │   ├── TestCategoryConfiguration.cs
│   │   └── TestProductConfiguration.cs
│   └── Dbo/                    # Configuraciones de bases externas
│
├── Seeds/                      # ⚠️ DEPRECATED: Seeds ahora en SQL
│   ├── Examples/
│   │   ├── Categories.cs
│   │   ├── Products.cs
│   │   └── MailNotificationData.cs
│   └── ApplicationSeedData.cs  # ❌ ELIMINADO
│
├── Repositories/               # Implementación del patrón Repository
│   ├── Contracts/
│   │   ├── IGenericRepository.cs
│   │   ├── IRepositoryFactory.cs
│   │   └── IUnitOfWork.cs
│   ├── GenericRepository.cs
│   ├── RepositoryFactory.cs
│   ├── UnitOfWork.cs
│   └── Custom/                 # Repositorios personalizados
│
├── Specification/              # Patrón Specification
│   ├── BaseSpecification.cs
│   ├── ISpecification.cs
│   └── SpecificationEvaluator.cs
│
├── Caching/                    # Sistema de caché
│   ├── Contracts/
│   │   ├── ICacheService.cs
│   │   └── ICacheInvalidationService.cs
│   ├── CacheService.cs
│   ├── CacheInvalidationService.cs
│   └── CacheSettings.cs
│
├── Pagination/                 # Sistema de paginación
│   ├── PaginationBase.cs
│   └── PaginationVm.cs
│
├── Constants/                  # Constantes
│   ├── AuditType.cs
│   ├── IScopedService.cs
│   └── SchemasDb.cs
│
├── InternalModels/             # Modelos internos
│   └── AuditEntry.cs
│
├── PersistenceServicesRegistration.cs # DI de Persistence
└── Persistence.csproj
```

**Características:**

- ✅ **Repositories**: Implementación del patrón Repository
- ✅ **Unit of Work**: Gestión de transacciones
- ✅ **Specifications**: Queries reutilizables
- ✅ **Caching**: Invalidación inteligente de caché
- 📝 **Schema**: Gestionado en SQL Server Database Project

### 🔐 src/Security/ - Identity & Authentication

```
Security/
├── DbContext/                  # Contexto de Identity
│   ├── IdentityContext.cs
│   └── IdentityContextFactory.cs
│
├── Entities/                   # Entidades de seguridad
│   ├── BaseEntity.cs          # Base para RefreshToken y AppUser
│   ├── RefreshToken.cs        # Tokens JWT de refresh
│   ├── AppUser.cs             # Perfil extendido de usuario
│   ├── UserAzureAD.cs         # Usuario de Azure AD
│   ├── VwEmployee.cs          # Vista de empleados (DB externa)
│   │
│   └── DTOs/                   # DTOs de autenticación
│       ├── AuthRequest.cs
│       ├── AuthResponse.cs
│       ├── RegistrationRequest.cs
│       ├── RegistrationResponse.cs
│       ├── TokenRequest.cs
│       ├── ChangePassword.cs
│       ├── JwtSettings.cs
│       ├── CustomClaimTypes.cs
│       ├── AuthMardomApiSettings.cs
│       └── RHEmployeesResult.cs
│
├── Services/                   # Servicios de autenticación
│   ├── AuthService.cs         # Servicio principal
│   ├── AzureADAuthService.cs  # Autenticación Azure AD
│   ├── JwtTokenService.cs     # Generación de tokens JWT
│   ├── LocalAuthService.cs    # Autenticación local
│   ├── MardomApiAuthService.cs # Auth con API externa
│   ├── RRHHService.cs         # Servicio de RRHH
│   ├── RefreshTokenService.cs # Gestión de refresh tokens
│   └── UserManagementService.cs
│
├── Repositories/               # Repositorios de Identity
│   ├── IUserTokenRepository.cs
│   └── UserTokenRepository.cs
│
├── Seeds/                      # Seeds de Identity
│   └── IdentitySeedData.cs    # Usuario de prueba
│
├── IdentityServiceRegistration.cs # DI de Identity
├── SecurityServicesRegistration.cs # DI de Security
└── Security.csproj
```

**Nota:** El seed de usuario de prueba está en C# (`IdentitySeedData.cs`) porque requiere hashing de contraseñas mediante ASP.NET Core Identity. El resto de la estructura de base de datos está en el SQL Database Project.

### 🔧 src/Shared/ - Servicios Compartidos

```
Shared/
├── Services/                   # Servicios de infraestructura
│   ├── Contracts/              # Interfaces
│   │   ├── IFileStorageService.cs
│   │   ├── IJsonService.cs
│   │   ├── ILocalTimeService.cs
│   │   ├── IMailService.cs
│   │   └── IPasswordService.cs
│   │
│   ├── FileStorageService.cs  # Gestión de archivos
│   ├── JsonService.cs         # Serialización JSON
│   ├── LocalTimeService.cs    # Gestión de zonas horarias
│   ├── MailKitService.cs      # Envío de correos (MailKit)
│   ├── SmtpMailService.cs     # Envío de correos (SMTP)
│   ├── PasswordService.cs     # Hashing de contraseñas
│   │
│   └── Configurations/         # Configuraciones
│       ├── EMailSettings.cs
│       ├── FileStorageOptions.cs
│       └── RRHHSettings.cs
│
├── Exceptions/                 # Excepciones personalizadas
│   ├── ApiException.cs
│   ├── BadRequestException.cs
│   ├── ConflictException.cs
│   ├── CustomValidationException.cs
│   ├── ForbiddenException.cs
│   ├── InternalServerError.cs
│   ├── NotFoundException.cs
│   ├── ThrowException.cs
│   └── UnauthorizedException.cs
│
├── Extensions/                 # Extensiones de sistema
│   ├── DateTimeExtensions.cs
│   ├── StringExtensions.cs
│   └── EnumExtensions.cs
│
├── SharedServicesRegistration.cs # DI de Shared
└── Shared.csproj
```

## 🧪 tests/ - Tests Unitarios

```
tests/Tests/
├── Application/
│   └── Handlers/               # Tests de Handlers
│       ├── CreateProductCommandHandlerTests.cs
│       ├── UpdateProductCommandHandlerTests.cs
│       ├── DeleteProductCommandHandlerTests.cs
│       └── GetProductsQueryHandlerTests.cs
│
├── Authorization/              # Tests de autorización
├── Controllers/                # Tests de controladores
├── Infrastructure/             # Tests de infraestructura
├── Persistence/                # Tests de repositorios
├── Security/                   # Tests de seguridad
├── Services/                   # Tests de servicios
│
├── Helpers/                    # Helpers para tests
│   └── TestFixture.cs
│
├── Tests.csproj
└── README.md
```

## 📚 docs/ - Documentación

```
docs/
├── INDICE.md                   # 📖 Índice principal
├── ARQUITECTURA.md             # 🏛️ Arquitectura Clean
├── ESTRUCTURA_COMPLETA.md      # 📁 Este archivo
├── GUIA_DESARROLLO.md          # 👨‍💻 Crear features
├── EJEMPLOS.md                 # 💡 Ejemplos de código
├── PAGINACION.md               # 📄 Sistema de paginación
├── HERRAMIENTAS.md             # 🛠️ Stack tecnológico
├── DOCKER-SETUP.md             # 🐳 Configuración Docker
└── Docker-Commands.txt         # 🐳 Comandos útiles
```

## 🔑 Archivos de Configuración Raíz

```
CleanArchitectureNet8/
├── CleanArchitectureNet8.sln  # Solución Visual Studio
├── global.json                 # Versión de .NET SDK
├── docker-compose.yml          # Docker Compose para SQL Server
├── .gitignore                  # Archivos ignorados por Git
├── .editorconfig               # Configuración de editor
├── .dockerignore               # Archivos ignorados por Docker
│
├── README.md                   # 📖 Documentación principal
├── ESTRUCTURA_ACTUAL.md        # 📊 Análisis de estructura
│
└── Archivos de estado/progreso:
    ├── ESTADO_ACTUAL.md
    ├── ESTADO_ACTUAL_MIGRACIONES.md
    ├── ESTADO_NET9_EFCORE9.md
    ├── RESUMEN_FINAL_BUG_EF_CORE.md
    └── RESUMEN_RESTAURACION.md
```

## 🎯 Convenciones de Organización

### Nombres de Carpetas

- **PascalCase** para carpetas de código: `Features/`, `Handlers/`, `Services/`
- **Plural** para colecciones: `Entities/`, `Commands/`, `Queries/`

### Nombres de Archivos

- **PascalCase** para clases: `ProductController.cs`, `CreateProductCommand.cs`
- **UPPERCASE** para documentación: `README.md`, `GUIA_DESARROLLO.md`

### Organización de Features

```
Features/
└── [EntityName]/              # Ejemplo: Products/
    ├── Commands/              # Comandos (Create, Update, Delete)
    │   ├── CreateProductCommand.cs
    │   ├── UpdateProductCommand.cs
    │   └── DeleteProductCommand.cs
    └── Queries/               # Consultas (Get, GetById, GetPaginated)
        ├── GetProductsQuery.cs
        ├── GetProductByIdQuery.cs
        └── GetPaginatedProductsQuery.cs
```

## 📊 Estadísticas del Proyecto

- **Total de proyectos**: 7 (.NET) + 1 (SQL)
- **Total de tests**: 101 (100 exitosos)
- **Total de tablas**: 14 (3 Shared + 2 Examples + 9 Security)
- **Líneas de código**: ~25,000+ (estimado)
- **Cobertura de tests**: 99%

## 🔄 Dependencias Entre Proyectos

```
┌─────────────┐
│   AppApi    │ (Presentation)
└──────┬──────┘
       │ references
       ▼
┌──────────────┐
│ Application  │ (CQRS, Use Cases)
└──────┬───────┘
       │ references
       ▼
┌─────────────┐     ┌────────────┐
│   Domain    │────▶│ Shared     │
└──────┬──────┘     └──────┬─────┘
       │                   │
       ▼                   ▼
┌─────────────┐     ┌────────────┐
│ Persistence │     │  Security  │
└─────────────┘     └────────────┘
```

## 🚀 Próximos Pasos

1. **Revisa** [ARQUITECTURA.md](ARQUITECTURA.md) para entender el diseño
2. **Consulta** [GUIA_DESARROLLO.md](GUIA_DESARROLLO.md) para crear features
3. **Explora** el código de ejemplo en `src/Application/Features/Examples/`
4. **Publica** la base de datos desde `database/CleanArchitectureDb/`

---

**Nota**: Los ejemplos de Products y Categories son solo para demostración. Elimínalos cuando implementes tus propias entidades de negocio.
