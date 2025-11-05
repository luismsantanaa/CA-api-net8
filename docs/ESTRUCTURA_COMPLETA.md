# 🌳 Estructura Completa del Proyecto Clean Architecture .NET 8

Este documento proporciona una vista detallada de la estructura del proyecto, incluyendo todos los archivos y su propósito.

## 📦 Organización del Proyecto

#### Application Layer

### 🛠 Infrastructure Layer

#### Persistence

````````
- src/Infrastructure/Persistence/Constants/ErrorMessage.cs
- src/Infrastructure/Persistence/DbContexts/ApplicationDbContext.cs
- src/Infrastructure/Persistence/Repositories/Base/RepositoryBase.cs
- src/Infrastructure/Persistence/Repositories/Base/UnitOfWork.cs
- src/Infrastructure/Persistence/Caching/Contracts/ICacheService.cs
- src/Infrastructure/Persistence/Caching/Concrete/CacheService.cs
- src/Infrastructure/Persistence/PersistenceServicesRegistration.cs

#### Security
````````

## 🔑 Puntos Clave

### Dependencias
- Las capas externas dependen de las internas
- Domain no tiene dependencias externas
- Infrastructure implementa interfaces de Domain
- Presentation coordina todas las capas

### Patrones Implementados
- Clean Architecture
- CQRS con MediatR
- Repository Pattern
- Unit of Work
- Specification Pattern
- Builder Pattern (configuración)

### Características Principales
- Autenticación JWT
- Caché distribuido
- Logging estructurado
- Manejo de excepciones global
- Validación automática
- Auditoría de cambios
- Paginación y filtrado

## 📚 Guía de Uso

1. **Lógica de Negocio**
   - Agregar entidades en `Domain/Entities/`
   - Definir interfaces en `Domain/Contracts/`

2. **Casos de Uso**
   - Crear Commands/Queries en `Application/Features/`
   - Agregar validadores en `Application/Validators/`

3. **Acceso a Datos**
   - Implementar repositorios en `Infrastructure/Persistence/`
   - Configurar contexto en `Infrastructure/DbContexts/`

4. **API**
   - Agregar controladores en `Presentation/AppApi/Controllers/`
   - Configurar rutas y middleware según necesidad

## 🚀 Mejores Prácticas

1. **Organización**
   - Seguir la estructura de carpetas existente
   - Mantener separación de responsabilidades
   - Usar namespaces consistentes

2. **Código**
   - Seguir convenciones de nombres
   - Documentar clases públicas
   - Implementar validación
   - Usar Result<T> para respuestas

3. **Seguridad**
   - Validar inputs
   - Usar autenticación JWT
   - Implementar autorización
   - Seguir principio de menor privilegio

## 📂 Estructura de Archivos

````````markdown
- src
  - Core
    - Domain
      - Base
        - AuditableEntity.cs
        - BaseEntity.cs
        - TraceableEntity.cs
      - Entities
        - AppUser.cs
        - RefreshToken.cs
        - VwEmployee.cs
    - Application
      - DTOs
        - Result.cs
        - PaginationVm.cs
      - Contracts
        - IApplicationDbContext.cs
      - Behaviours
        - ValidationBehaviour.cs
      - Mappings
        - SecurityMappingProfile.cs
  - Infrastructure
    - Persistence
      - Configurations
        - AppUserConfiguration.cs
        - RefreshTokenConfiguration.cs
        - VwEmployeeConfiguration.cs
      - Migrations
      - Seeding
    - Security
      - DbContext
        - RrHhContext.cs
        - IdentityContext.cs
      - Repositories
        - Contracts
          - IEmployeeRepository.cs
        - Concrete
          - EmployeeRepository.cs
      - Services
        - Concrete
          - EmployeeService.cs
      - SecurityServicesRegistration.cs
    - Shared
      - Extensions
        - ErrorMessageFormatter.cs
      - Exceptions
        - ThrowException.cs
      - Services
        - Contracts
          - IJsonService.cs
        - Concrete
          - JsonService.cs
  - Presentation
    - AppApi
      - appsettings.json
      - Controllers
        - Base
          - ApiBaseController.cs
        - SecurityController.cs
      - Middleware
        - ExceptionMiddleware.cs
      - Program.cs

````````

## 📑 Estructura Detallada

### 🎯 Core Layer

#### Domain Layer

##### Entidades
- `Base/AuditableEntity.cs`: Clase base para entidades auditables. Contiene propiedades para la auditoría como `CreatedDate`, `CreatedBy`, `ModifiedDate`, y `ModifiedBy`.
- `Base/BaseEntity.cs`: Clase base para todas las entidades. Podría contener propiedades como `Id` y `ConcurrencyStamp`.
- `Base/TraceableEntity.cs`: Clase base para entidades que requieren trazabilidad. Usualmente contendría un campo `Deleted` o `IsActive` para el soft delete.
- `Entities/AppUser.cs`: Representa a un usuario de la aplicación. Podría contener propiedades como `UserName`, `Email`, y `PasswordHash`.
- `Entities/RefreshToken.cs`: Representa un token de actualización para la autenticación. Usado para obtener nuevos tokens de acceso.
- `Entities/VwEmployee.cs`: Vista que representa a un empleado. Podría contener propiedades como `EmployeeId`, `FullName`, y `Email`.

##### DTOs
- `DTOs/Result.cs`: DTO genérico para estandarizar respuestas. Contiene propiedades como `Success`, `Message`, y `Data`.
- `DTOs/PaginationVm.cs`: DTO para la paginación. Contiene propiedades como `PageNumber` y `PageSize`. Utiliza `IList<>` para la colección de datos paginados.

##### Contratos
- `Contracts/IApplicationDbContext.cs`: Contrato para el contexto de la base de datos. Define las operaciones básicas como `SaveChanges()` y `Dispose()`.

##### Comportamientos
- `Behaviours/ValidationBehaviour.cs`: Se encarga de la validación de los comandos y consultas. Usa FluentValidation para validar las propiedades de los objetos.

##### Mapeos
- `Mappings/SecurityMappingProfile.cs`: Perfil de mapeo para las entidades y DTOs relacionadas con la seguridad. Utiliza AutoMapper para configurar los mapeos entre tipos.

### 📂 Infraestructura Layer

#### Persistence
##### Configuraciones
- `Configurations/AppUserConfiguration.cs`: Configuración fluida para la entidad `AppUser`. Define el esquema de la tabla y las restricciones.
- `Configurations/RefreshTokenConfiguration.cs`: Configuración fluida para la entidad `RefreshToken`.
- `Configurations/VwEmployeeConfiguration.cs`: Configuración para la vista `VwEmployee`.

##### Migrations
- Contiene las migraciones de Entity Framework Core para crear y actualizar la base de datos según el modelo de dominio.

##### Seeding
- Scripts o clases que insertan datos por defecto en la base de datos al inicio.

#### Security
##### DbContext
- `DbContext/RrHhContext.cs`: Contexto de base de datos para la seguridad. Podría contener DbSet para `Roles`, `Permisos`, etc.
- `DbContext/IdentityContext.cs`: Contexto de identidad, generalmente para manejar la autenticación y autorización de usuarios.

##### Repositories
###### Contracts
- `Repositories/Contracts/IEmployeeRepository.cs`: Interfaz para el repositorio de empleados. Define métodos como `GetEmployeeById()`, `GetAllEmployees()`, etc.
###### Concrete
- `Repositories/Concrete/EmployeeRepository.cs`: Implementación concreta del repositorio de empleados.

##### Services
###### Concrete
- `Services/Concrete/EmployeeService.cs`: Servicio para la lógica de negocio relacionada con empleados. Usa el repositorio de empleados para acceder a los datos.

##### SecurityServicesRegistration.cs
- Archivo para registrar los servicios de seguridad en el contenedor de inyección de dependencias.

#### Shared
##### Extensions
- `Extensions/ErrorMessageFormatter.cs`: Clase para formatear mensajes de error de una manera consistente.
##### Exceptions
- `Exceptions/ThrowException.cs`: Clase para lanzar excepciones personalizadas.
##### Services
###### Contracts
- `Services/Contracts/IJsonService.cs`: Interfaz para un servicio que maneja operaciones JSON.
###### Concrete
- `Services/Concrete/JsonService.cs`: Implementación concreta del servicio JSON.

### 🖥 Presentation Layer

#### AppApi
- Controladores para la API RESTful.
- Middlewares para manejo de excepciones y otras preocupaciones transversales.
- Program.cs para configuración y arranque de la aplicación.

### 🔒 Seguridad
- Implementa autenticación y autorización usando JWT y [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-5.0).
- Protégete contra ataques comunes como XSS, CSRF y

 SQL Injection.
- Asegúrate de que las conexiones a la base de datos están seguras y usan credenciales fuertes.

### ⚙️ Ejecución y Despliegue
- Usa `dotnet run` para ejecutar la aplicación en desarrollo.
- Configura variables de entorno para la cadena de conexión y secretos.
- Para despliegue en producción, usa entornos como Azure App Service, AWS, o servidores dedicados.

### 🧪 Pruebas
- Las pruebas unitarias se encuentran en la carpeta `/tests`.
- Usa `dotnet test` para ejecutar las pruebas.
- Asegúrate de que todas las pruebas pasan antes de hacer un despliegue.

### 📈 Monitoreo y Rendimiento
- Implementa monitoreo usando herramientas como Application Insights o ELK Stack.
- Revisa los registros de error y acceso para detectar comportamientos anómalos.
- Optimiza las consultas a la base de datos y el uso de caché para mejorar el rendimiento.

### 📅 Roadmap
- **Versión 1.0**
  - Autenticación y autorización básicas.
  - CRUD completo para empleados.
  - Paginación y filtrado de datos.
- **Futuras versiones**
  - Integración con servicios externos.
  - Mejoras en el rendimiento y escalabilidad.
  - Más opciones de configuración y personalización.

## 📞 Soporte
- Para preguntas o problemas, por favor abre un issue en el repositorio.
- Consulta la sección de [FAQ](link-a-faq) antes de abrir un nuevo issue.

## 📝 Notas Adicionales
- Este proyecto sigue las recomendaciones de la [Guía de Estilo de C#](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-primaries)
- La documentación de la API se genera automáticamente y se puede encontrar en `/docs/api-specs`.

------

Esto concluye la documentación de la estructura completa del proyecto. Asegúrate de seguir todas las pautas y convenciones establecidas para mantener la calidad y la coherencia en todo el proyecto.
