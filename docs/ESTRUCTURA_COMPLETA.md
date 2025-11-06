# 🌳 Estructura Completa del Proyecto Clean Architecture .NET 8

Este documento proporciona una vista detallada de la estructura del proyecto, incluyendo todos los archivos y carpetas organizados por capa.

## 📦 Estructura General del Proyecto

```
CleanArchitectureNet8/
├── src/                           # Código fuente
│   ├── Core/                      # Núcleo de la aplicación
│   │   ├── Domain/               # Capa de dominio
│   │   └── Application/          # Capa de aplicación
│   ├── Infrastructure/            # Infraestructura
│   │   ├── Persistence/          # Acceso a datos
│   │   ├── Security/             # Autenticación y seguridad
│   │   └── Shared/               # Servicios compartidos
│   └── Presentation/              # Presentación
│       └── AppApi/               # API REST
├── tests/                         # Pruebas unitarias
│   └── Tests/                    # Proyecto de tests
├── docs/                          # Documentación técnica
└── tools/                         # Scripts y herramientas
```

---

## 🎯 Core Layer

### 📁 Domain (`src/Core/Domain/`)

**Responsabilidad**: Entidades de negocio, reglas de dominio y contratos.

```
Domain/
├── Base/                          # Clases base para entidades
│   ├── AuditableEntity.cs        # Entidad con auditoría (CreatedBy, ModifiedBy, etc.)
│   ├── BaseEntity.cs             # Entidad base (Id, Active, CreatedOn)
│   ├── SoftDelete.cs             # Implementación de eliminación lógica
│   └── TraceableEntity.cs        # Entidad rastreable
├── Contracts/                     # Interfaces del dominio
│   └── IApplicationDbContext.cs  # Contrato para el contexto de base de datos
├── Entities/                      # Entidades de dominio
│   ├── Dbo/                      # Entidades de esquema dbo (vacío actualmente)
│   ├── Examples/                 # Entidades de ejemplo
│   │   ├── TestCategory.cs      # Entidad de categoría (ejemplo)
│   │   └── TestProduct.cs       # Entidad de producto (ejemplo)
│   └── Shared/                   # Entidades compartidas
│       ├── AuditLog.cs          # Registro de auditoría
│       ├── MailNotificationTemplate.cs  # Plantillas de correo
│       └── UploadedFile.cs      # Archivos cargados
└── Domain.csproj                 # Archivo de proyecto
```

**Características clave**:
- ❌ **NO** depende de ninguna otra capa
- ✅ Define contratos (interfaces) que otras capas implementan
- ✅ Contiene solo lógica de negocio pura
- ✅ Entidades heredan de `BaseEntity` o `AuditableEntity`

---

### 📁 Application (`src/Core/Application/`)

**Responsabilidad**: Casos de uso (CQRS), DTOs, validaciones, mapeos y lógica de aplicación.

```
Application/
├── Behaviours/                    # Comportamientos de MediatR (pipeline)
│   ├── LoggingPipelineBehavior.cs     # Logging de requests/responses
│   ├── UnhandledExceptionBehaviour.cs # Manejo de excepciones no controladas
│   └── ValidationBehaviour.cs         # Validación automática con FluentValidation
├── Commons/                       # Utilidades comunes
│   └── ValidationHelper.cs       # Helpers para validación
├── Contracts/                     # Interfaces de aplicación
│   ├── IApplicationDbContext.cs  # Contrato del contexto de BD
│   └── IResult.cs               # Interfaz para Result<T>
├── DTOs/                          # Objetos de transferencia de datos
│   ├── Result.cs                 # DTO genérico para respuestas (Result<T>)
│   └── ResultExtensions.cs       # Extensiones para crear Result<T> fácilmente
├── Features/                      # Features organizados por dominio (CQRS)
│   ├── App/                      # Features de la aplicación
│   ├── Examples/                 # Ejemplos de implementación
│   │   ├── Categories/          # Feature de Categorías
│   │   │   ├── Commands/        # Comandos (Create, Update, Delete)
│   │   │   │   ├── CreateCategoryCommand.cs
│   │   │   │   ├── UpdateCategoryCommand.cs
│   │   │   │   ├── DeleteCategoryCommand.cs
│   │   │   │   └── Validators/  # Validadores de comandos
│   │   │   │       ├── CreateCategoryValidator.cs
│   │   │   │       ├── UpdateCategoryValidator.cs
│   │   │   │       └── DeleteCategoryValidator.cs
│   │   │   ├── Queries/         # Consultas (GetAll, GetById, Paginated)
│   │   │   │   ├── GetAllCategoriesQuery.cs
│   │   │   │   ├── GetCategoryByIdQuery.cs
│   │   │   │   ├── GetPaginatedCategoriesQuery.cs
│   │   │   │   ├── Specs/      # Specifications para consultas complejas
│   │   │   │   │   └── CategoryPaginationSpecification.cs
│   │   │   │   └── Validators/ # Validadores de queries
│   │   │   │       └── GetCategoryByIdValidator.cs
│   │   │   └── VMs/            # View Models (DTOs de salida)
│   │   │       └── CategoryVm.cs
│   │   └── Products/           # Feature de Productos
│   │       ├── Commands/
│   │       │   ├── CreateProductCommand.cs
│   │       │   ├── UpdateProductCommand.cs
│   │       │   ├── DeleteProductCommand.cs
│   │       │   └── Validators/
│   │       │       ├── CreateProductValidator.cs
│   │       │       ├── UpdateProductValidator.cs
│   │       │       └── DeleteProductValidator.cs
│   │       ├── Queries/
│   │       │   ├── GetAllProductsQuery.cs
│   │       │   ├── GetProductByIdQuery.cs
│   │       │   ├── GetPaginatedProductsQuery.cs
│   │       │   ├── GetProductsByCategoryQuery.cs
│   │       │   ├── GetProductWithCagegoryQuery.cs
│   │       │   ├── Specs/
│   │       │   │   ├── ProductPaginationSpecification.cs
│   │       │   │   └── ProductQueriesSpecifications.cs
│   │       │   └── Validators/
│   │       │       ├── GetProductByIdValidator.cs
│   │       │       ├── GetProductsByCategoryValidator.cs
│   │       │       └── GetProductWithCategoryValidator.cs
│   │       └── VMs/
│   │           ├── ProductVm.cs
│   │           └── ProductWithCategoryVm.cs
│   └── Utilities/              # Utilidades de aplicación
│       ├── SendMails/          # Envío de correos
│       │   ├── SendMailCommand.cs
│       │   └── SendNotificationCommand.cs
│       └── UploadFiles/        # Carga de archivos
│           ├── Comands/
│           │   ├── Create/
│           │   │   └── UploadFileCommand.cs
│           │   └── VoidFile/
│           │       └── VoidUploadedFileCommand.cs
│           └── Queries/
│               ├── GetUploadedFileByIdQuery.cs
│               ├── GetUploadFileByReferenceQuery.cs
│               ├── UploadFilesPaginateQuery.cs
│               ├── Specs/
│               │   └── UploadFilesPaginatedSpecification.cs
│               └── Vm/
│                   └── UploadedFileVm.cs
├── Handlers/                      # Handlers base reutilizables
│   └── Base/
│       └── PaginatedQueryHandlerBase.cs  # Handler base para paginación
├── Helpers/                       # Helpers de aplicación
│   ├── HandlerExtensions.cs      # Extensiones para handlers
│   └── PaginationVmHelper.cs     # Helper para crear PaginationVm
├── Mappings/                      # Perfiles de AutoMapper
│   ├── Examples/
│   │   └── ExamplesMappingProfile.cs  # Mapeos de Products y Categories
│   └── Utilities/
│       └── UtilitiesMappingProfile.cs # Mapeos de utilidades
├── Application.csproj             # Archivo de proyecto
└── ApplicationServicesRegistration.cs  # Registro de servicios de Application
```

**Características clave**:
- ✅ Depende solo de `Domain`
- ✅ Implementa CQRS con MediatR
- ✅ Validación con FluentValidation
- ✅ Mapeo con AutoMapper
- ✅ Handlers base para reducir código repetitivo

---

## 🏗️ Infrastructure Layer

### 📁 Persistence (`src/Infrastructure/Persistence/`)

**Responsabilidad**: Acceso a datos, repositorios, caché y especificaciones.

```
Persistence/
├── Caching/                       # Servicios de caché
│   ├── Contracts/                # Interfaces de caché
│   │   ├── ICacheInvalidationService.cs  # Invalidación de caché
│   │   ├── ICacheKeyService.cs          # Generación de claves de caché
│   │   └── ICacheService.cs             # Servicio de caché genérico
│   ├── Extensions/               # Extensiones de caché
│   │   └── CacheServiceExtensions.cs    # GetOrSetAsync, etc.
│   ├── CacheInvalidationService.cs      # Implementación de invalidación
│   ├── CacheKeyService.cs               # Implementación de claves
│   ├── CacheSettings.cs                 # Configuración de caché
│   ├── DistributedCacheService.cs       # Caché distribuido (Redis)
│   └── LocalCacheService.cs             # Caché local (Memory)
├── Constants/                     # Constantes de persistencia
│   ├── AuditType.cs              # Tipos de auditoría
│   ├── IScopedService.cs         # Marcador de servicios scoped
│   └── SchemasDb.cs              # Esquemas de base de datos
├── DbContexts/                    # Contextos de Entity Framework
│   ├── Contracts/
│   │   └── IGetUserServices.cs   # Servicio para obtener usuario actual
│   └── ApplicationDbContext.cs   # Contexto principal de la aplicación
├── EntitiesConfigurations/        # Configuraciones de entidades (Fluent API)
│   ├── Dbo/                      # Configuraciones de esquema dbo
│   └── Shared/                   # Configuraciones compartidas
│       ├── MailNotificationTemplateConfigurations.cs
│       └── UploadedFileConfigurations.cs
├── InternalModels/                # Modelos internos
│   └── AuditEntry.cs             # Entrada de auditoría
├── Migrations/                    # Migraciones de EF Core
├── Pagination/                    # Clases base para paginación
│   ├── PaginationBase.cs         # Clase base para queries paginados
│   └── PaginationVm.cs           # View Model para respuestas paginadas
├── Repositories/                  # Implementaciones de repositorios
│   ├── Application/
│   │   └── ApplicationRepository.cs    # Repositorio de aplicación
│   ├── Base/
│   │   └── BaseRepository.cs          # Repositorio genérico base
│   ├── Contracts/                # Interfaces de repositorios
│   │   ├── IGenericRepository.cs      # Repositorio genérico
│   │   ├── IRepositoryFactory.cs      # Fábrica de repositorios
│   │   └── IUnitOfWork.cs            # Unit of Work
│   ├── Custom/                   # Repositorios personalizados
│   ├── RepositoryFactory.cs      # Implementación de fábrica
│   └── UnitOfWork.cs            # Implementación de Unit of Work
├── Seeds/                         # Datos de semilla (seeding)
│   ├── Examples/
│   │   ├── Categories.cs         # Seed de categorías
│   │   ├── MailNotificationData.cs  # Seed de plantillas de correo
│   │   └── Products.cs          # Seed de productos
│   └── ApplicationSeedData.cs   # Configuración de seeding
├── Specification/                 # Patrón Specification
│   ├── Contracts/
│   │   └── ISpecification.cs     # Interfaz de specification
│   ├── BaseSpecification.cs      # Specification base con helpers
│   ├── SpecificationEvaluator.cs # Evaluador de specifications
│   └── SpecificationParams.cs    # Parámetros base para specifications
├── Persistence.csproj             # Archivo de proyecto
└── PersistenceServicesRegistration.cs  # Registro de servicios
```

**Características clave**:
- ✅ Entity Framework Core para acceso a datos
- ✅ Patrón Repository y Unit of Work
- ✅ Patrón Specification para consultas complejas
- ✅ Caché local (Memory) y distribuido (Redis)
- ✅ Sistema completo de paginación
- ✅ Auditoría automática de cambios

---

### 📁 Security (`src/Infrastructure/Security/`)

**Responsabilidad**: Autenticación, autorización, JWT e Identity.

```
Security/
├── DbContext/                     # Contextos de seguridad
│   ├── IdentityContext.cs        # Contexto de ASP.NET Core Identity
│   └── RrHhContext.cs           # Contexto de recursos humanos
├── Entities/                      # Entidades de seguridad
│   ├── DTOs/                     # DTOs de autenticación
│   │   ├── AuthMardomApiSettings.cs   # Configuración de API externa
│   │   ├── AuthRequest.cs            # Request de autenticación
│   │   ├── AuthResponse.cs           # Response de autenticación
│   │   ├── ChangePassword.cs         # DTO para cambio de contraseña
│   │   ├── CustomClaimTypes.cs       # Claims personalizados
│   │   ├── JwtSettings.cs            # Configuración de JWT
│   │   ├── RegistrationRequest.cs    # Request de registro
│   │   └── RHEmployeesResult.cs      # Resultado de empleados de RH
│   ├── AppUser.cs                # Usuario de la aplicación (Identity)
│   ├── BaseEntity.cs             # Entidad base para Security
│   ├── RefreshToken.cs           # Token de actualización
│   ├── RegistrationResponse.cs   # Response de registro
│   ├── TokenRequest.cs           # Request de token
│   ├── UserAzureAD.cs           # Usuario de Azure AD
│   └── VwEmployee.cs            # Vista de empleado
├── Migrations/                    # Migraciones de Identity
│   ├── 20231228190357_InitialCreate.cs
│   ├── 20231228190357_InitialCreate.Designer.cs
│   └── IdentityContextModelSnapshot.cs
├── Repositories/                  # Repositorios de seguridad
│   ├── Contracts/
│   │   └── IEmployeeRepository.cs    # Interfaz de empleados
│   └── Concrete/
│       └── EmployeeRepository.cs     # Implementación de empleados
├── Seeds/                         # Datos de semilla
│   └── IdentitySeedData.cs       # Seed de usuarios y roles
├── Services/                      # Servicios de seguridad
│   ├── Contracts/
│   │   ├── IActiveDirectoryService.cs   # Servicio de Active Directory
│   │   ├── IAppAuthService.cs          # Servicio de autenticación
│   │   ├── IEmployeeService.cs         # Servicio de empleados
│   │   └── IJwtUtils.cs                # Utilidades JWT
│   └── Concrete/
│       ├── ActiveDirectoryService.cs    # Implementación de AD
│       ├── AppAuthService.cs           # Implementación de auth
│       ├── EmployeeService.cs          # Implementación de empleados
│       └── JwtUtils.cs                 # Implementación de JWT
├── Security.csproj                # Archivo de proyecto
├── IdentityServiceRegistration.cs # Registro de Identity
└── SecurityServicesRegistration.cs # Registro de servicios de Security
```

**Características clave**:
- ✅ ASP.NET Core Identity para gestión de usuarios
- ✅ JWT para autenticación stateless
- ✅ Refresh tokens para renovación
- ✅ Integración con Active Directory (Windows)
- ✅ Roles y permisos

---

### 📁 Shared (`src/Infrastructure/Shared/`)

**Responsabilidad**: Servicios y extensiones compartidas.

```
Shared/
├── Exceptions/                    # Excepciones personalizadas
│   ├── ActiveDirectoryCOMExceptions.cs  # Excepciones de AD
│   ├── ApiException.cs                 # Excepción de API
│   ├── AuthorizationValidationException.cs  # Autorización
│   ├── BadRequestException.cs          # BadRequest (400)
│   ├── DataImportException.cs          # Importación de datos
│   ├── ErrorMessage.cs                 # Mensajes de error
│   ├── ErrorMessageFormatter.cs        # Formateador de mensajes
│   ├── InternalServerError.cs          # InternalServerError (500)
│   ├── NotFoundException.cs            # NotFound (404)
│   ├── SecurityCustomException.cs      # Excepciones de seguridad
│   └── ValidationException.cs          # Validación (400)
├── Extensions/                    # Extensiones de utilidad
│   ├── Contracts/
│   │   └── [Interfaces de extensiones]
│   ├── AdoExtesion.cs            # Extensiones de ADO.NET
│   ├── ExeptionsExtensions.cs    # Extensiones de excepciones
│   ├── IEnumerableExtensions.cs  # Extensiones de IEnumerable
│   └── StringExtensions.cs       # Extensiones de string
├── Services/                      # Servicios compartidos
│   ├── Configurations/           # Configuraciones de servicios
│   │   └── [Archivos de configuración]
│   ├── Contracts/                # Interfaces de servicios
│   │   └── [Interfaces]
│   ├── Enums/                    # Enumeraciones
│   │   └── [Enums]
│   ├── GenericHttpClientService.cs    # Cliente HTTP genérico
│   ├── JsonService.cs                 # Servicio JSON
│   ├── LocalTimeService.cs            # Servicio de tiempo local
│   └── SmtpMailService.cs             # Servicio de correo SMTP
├── Shared.csproj                  # Archivo de proyecto
└── SharedServicesRegistration.cs  # Registro de servicios compartidos
```

**Características clave**:
- ✅ Excepciones personalizadas con mensajes consistentes
- ✅ Extensiones de utilidad (string, IEnumerable, etc.)
- ✅ Servicios de email (SMTP)
- ✅ Servicio HTTP genérico
- ✅ Servicio JSON

---

## 🖥️ Presentation Layer

### 📁 AppApi (`src/Presentation/AppApi/`)

**Responsabilidad**: API REST, controllers, middleware.

```
AppApi/
├── Controllers/                   # Controllers de la API
│   ├── Auth/                     # Controllers de autenticación
│   │   ├── SecurityController.cs    # Login, refresh, etc.
│   │   └── UsersController.cs      # Gestión de usuarios
│   ├── Examples/                 # Controllers de ejemplo
│   │   ├── CategoriesController.cs  # CRUD de categorías
│   │   └── ProductsController.cs    # CRUD de productos
│   ├── Utilities/                # Controllers de utilidades
│   │   ├── EmailsController.cs      # Envío de correos
│   │   └── FileUploadController.cs  # Carga de archivos
│   └── ApiBaseController.cs      # Controller base
├── Authorization/                 # Autorización personalizada
│   └── [Atributos y políticas]
├── Middleware/                    # Middlewares personalizados
│   ├── CorrelationIdMiddleware.cs   # Correlation ID para tracing
│   ├── ExceptionMiddleware.cs       # Manejo de excepciones global
│   └── [Otros middlewares]
├── Properties/                    # Propiedades del proyecto
│   └── launchSettings.json       # Configuración de ejecución
├── appsettings.json              # Configuración principal
├── appsettings.Development.json  # Configuración de desarrollo
├── AppApi.csproj                 # Archivo de proyecto
└── Program.cs                    # Punto de entrada de la aplicación
```

**Características clave**:
- ✅ Controllers RESTful con Swagger
- ✅ Middleware de manejo de excepciones
- ✅ Correlation IDs para tracing
- ✅ Health checks
- ✅ CORS configurado
- ✅ Authentication y Authorization

---

## 🧪 Tests Layer

### 📁 Tests (`tests/Tests/`)

**Responsabilidad**: Pruebas unitarias del proyecto.

```
Tests/
├── Application/                   # Tests de Application layer
│   ├── Handlers/                 # Tests de handlers
│   │   ├── CreateProductCommandHandlerTests.cs
│   │   ├── UpdateProductCommandHandlerTests.cs
│   │   ├── DeleteProductCommandHandlerTests.cs
│   │   ├── GetAllProductsQueryHandlerTests.cs
│   │   └── GetProductByIdQueryHandlerTests.cs
│   └── Validators/               # Tests de validators
│       └── CreateProductValidatorTests.cs
├── Authorization/                 # Tests de autorización
│   └── JwtUtilsTests.cs
├── Infrastructure/                # Tests de Infrastructure
│   └── [Tests de servicios]
├── Persistence/                   # Tests de Persistence
│   └── Specification/
│       └── [Tests de specifications]
├── Security/                      # Tests de Security
│   └── AppAuthServiceTests.cs
└── Tests.csproj                  # Archivo de proyecto de tests
```

**Frameworks utilizados**:
- ✅ xUnit para pruebas
- ✅ Moq para mocking
- ✅ FluentAssertions para assertions

---

## 📄 Documentación (`docs/`)

```
docs/
├── INDICE.md                     # Índice de toda la documentación
├── ARQUITECTURA.md               # Explicación de Clean Architecture
├── ESTRUCTURA_COMPLETA.md        # Este archivo
├── GUIA_DESARROLLO.md            # Guía paso a paso para crear features
├── PAGINACION.md                 # Guía completa de paginación
├── HERRAMIENTAS.md               # Detalles de herramientas y tecnologías
├── EJEMPLOS.md                   # Ejemplos de código y mejores prácticas
└── RESUMEN_MEJORAS.md            # Resumen de mejoras y helpers
```

---

## 🔑 Puntos Clave

### Dependencias entre Capas
```
Presentation → Application → Domain
    ↓              ↓
Infrastructure ←───┘
```

- Las capas externas dependen de las internas
- Domain no tiene dependencias externas
- Infrastructure implementa interfaces de Domain

### Patrones Implementados
- ✅ Clean Architecture
- ✅ CQRS con MediatR
- ✅ Repository Pattern
- ✅ Unit of Work
- ✅ Specification Pattern
- ✅ Result Pattern

### Características Principales
- ✅ Autenticación JWT
- ✅ Caché distribuido (Redis) y local (Memory)
- ✅ Logging estructurado (Serilog)
- ✅ Manejo de excepciones global
- ✅ Validación automática (FluentValidation)
- ✅ Auditoría de cambios
- ✅ Paginación y filtrado
- ✅ Soft Delete
- ✅ Health Checks

---

## 📚 Referencias

- **Ejemplos de Código**: `src/Core/Application/Features/Examples/`
- **Tests de Ejemplo**: `tests/Tests/`
- **Configuración**: `src/Presentation/AppApi/appsettings.json`

---

**Nota**: Los ejemplos de Productos y Categorías están incluidos solo como referencia. Elimínalos cuando implementes tus propias entidades de negocio.
