# Herramientas y Tecnologías

Esta documentación explica las herramientas y tecnologías utilizadas en el proyecto, cómo funcionan y cuándo usarlas.

## 📋 Tabla de Contenidos

- [Frameworks y Librerías Core](#frameworks-y-librerías-core)
- [Patrones y Arquitectura](#patrones-y-arquitectura)
- [Acceso a Datos](#acceso-a-datos)
- [Validación](#validación)
- [Mapeo de Objetos](#mapeo-de-objetos)
- [Autenticación y Seguridad](#autenticación-y-seguridad)
- [Logging](#logging)
- [Caching](#caching)
- [Testing](#testing)

---

## Frameworks y Librerías Core

### .NET 8

**Qué es**: Framework de desarrollo multiplataforma de Microsoft.

**Para qué se usa**:
- Base del proyecto
- Runtime de ejecución
- Librerías estándar (Collections, LINQ, etc.)

**Ejemplo de uso**:
```csharp
// C# 12 features disponibles
public class Example(string name) // Primary constructor
{
    public string Name { get; } = name;
}
```

---

### ASP.NET Core

**Qué es**: Framework para construir APIs y aplicaciones web.

**Para qué se usa**:
- Controllers y routing
- Middleware pipeline
- Dependency Injection
- Configuration management

**Configuración en**: `Program.cs`

---

## Patrones y Arquitectura

### MediatR

**Qué es**: Librería para implementar el patrón Mediator (mediador).

**Para qué se usa**:
- Desacoplar controllers de handlers
- Implementar CQRS (Commands/Queries)
- Pipeline behaviors (validación, logging)

**Cómo funciona**:
```csharp
// Controller envía command
await _mediator.Send(new CreateProductCommand { Name = "..." });

// MediatR encuentra y ejecuta el handler correspondiente
// Puede ejecutar behaviors antes/después (validación, logging)
```

**Ventajas**:
- ✅ Controllers más limpios (solo reciben y retornan)
- ✅ Lógica de negocio en handlers (testeable)
- ✅ Pipeline behaviors centralizados

**Documentación**: Ver `Application/Behaviours/` para ejemplos

---

### CQRS (Command Query Responsibility Segregation)

**Qué es**: Patrón que separa operaciones de lectura (Query) de escritura (Command).

**Cuándo usar**:
- ✅ **Command**: Cuando modificas datos (Create, Update, Delete)
- ✅ **Query**: Cuando lees datos (GetAll, GetById)

**Ejemplo**:
```csharp
// COMMAND: Modifica datos
public class CreateProductCommand : IRequest<Result<string>>
public class CreateProductCommandHandler : IRequestHandler<...>

// QUERY: Lee datos
public class GetAllProductsQuery : IRequest<Result<IReadOnlyList<ProductVm>>>
public class GetAllProductsQueryHandler : IRequestHandler<...>
```

**Ventajas**:
- ✅ Separación clara de responsabilidades
- ✅ Optimización independiente (caché en queries)
- ✅ Escalabilidad

---

## Acceso a Datos

### Entity Framework Core

**Qué es**: ORM (Object-Relational Mapping) de Microsoft.

**Para qué se usa**:
- Acceso a base de datos
- Migraciones
- Code First approach

**Cómo funciona**:
```csharp
// 1. Definir entidad
public class Product : BaseEntity
{
    public string Name { get; set; }
}

// 2. Configurar en DbContext
public DbSet<Product> Products { get; set; }

// 3. Usar en repositorio
var products = await _context.Products.ToListAsync();
```

**Migraciones**:
```bash
# Crear migración
dotnet ef migrations add NombreMigracion --project src/Infrastructure/Persistence --startup-project src/Presentation/AppApi

# Aplicar migración
dotnet ef database update --project src/Infrastructure/Persistence --startup-project src/Presentation/AppApi
```

---

### Repository Pattern

**Qué es**: Patrón que abstrae el acceso a datos.

**Para qué se usa**:
- Centralizar lógica de acceso a datos
- Facilita testing (mock del repository)
- Permite cambiar de ORM sin afectar Application

**Estructura**:
```
Domain/
  └── Contracts/
      └── IGenericRepository<T>  ← Interfaz (contrato)

Infrastructure/
  └── Repositories/
      └── BaseRepository<T>      ← Implementación
```

**Uso**:
```csharp
// En Handler
var repo = _unitOfWork.Repository<Product>();
var product = await repo.GetByIdAsync(id, cancellationToken);
```

---

### Unit of Work

**Qué es**: Patrón que gestiona transacciones y coordinación de repositorios.

**Para qué se usa**:
- Una transacción por operación
- Rollback automático en caso de error
- Coordinación de múltiples repositorios

**Uso**:
```csharp
var repo = _unitOfWork.Repository<Product>();
await repo.AddAsync(product, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken); // Commit
```

---

### Specification Pattern

**Qué es**: Patrón para construir consultas complejas de forma declarativa.

**Para qué se usa**:
- Consultas reutilizables
- Separación de lógica de consulta
- Filtros, paginación, ordenamiento

**Ejemplo**:
```csharp
public class ProductPaginationSpecification : BaseSpecification<Product>
{
    public ProductPaginationSpecification(SpecificationParams specParams)
    {
        ApplyPaging(specParams); // Paginación
        ApplySorting();          // Ordenamiento
        // Filtros, includes, etc.
    }
}

// Uso
var spec = new ProductPaginationSpecification(params);
var products = await repo.GetAllWithSpec(spec);
```

---

### Paginación

**Qué es**: Sistema para obtener datos en páginas con filtros, ordenamiento y búsqueda.

**Componentes**:
- `PaginationBase`: Clase base para queries con paginación
- `SpecificationParams`: Parámetros de paginación para specifications
- `PaginationVm<T>`: View Model para respuestas paginadas
- `ApplyPaging()`: Método helper en `BaseSpecification`

**Para qué se usa**:
- Obtener datos en páginas
- Filtrar y ordenar resultados
- Mejorar rendimiento (no cargar todos los registros)

**Ejemplo**:
```csharp
// Query
public class GetPaginatedProductsQuery : PaginationBase, IRequest<PaginationVm<ProductVm>>
{
    public string? CategoryName { get; set; }
}

// Specification
public class ProductSpecification : BaseSpecification<Product>
{
    public ProductSpecification(ProductSpecificationParams @params) : base(/* filtros */)
    {
        ApplySorting(@params.Sort, sortMappings, defaultOrderBy);
        ApplyPaging(@params); // Aplica paginación automáticamente
    }
}

// Handler
var spec = new ProductSpecification(@params);
var data = await repo.GetAllWithSpec(spec);
var total = await repo.CountAsync(new ProductForCountingSpecification(@params));
```

**Uso en API**:
```
GET /api/products/pagination?pageIndex=1&pageSize=10&sort=nameAsc&search=laptop
```

> 📖 **Guía Completa**: Consulta [docs/PAGINACION.md](PAGINACION.md) para implementación detallada.

---

## Validación

### FluentValidation

**Qué es**: Librería para validación de objetos usando fluent syntax.

**Para qué se usa**:
- Validar commands/queries antes de procesarlos
- Mensajes de error personalizados
- Validaciones asíncronas (ej: verificar en BD)

**Ejemplo**:
```csharp
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(10)
            .MaximumLength(50);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.CategoryId)
            .MustAsync(CategoryExists) // Validación async
            .WithMessage("La categoría no existe");
    }
}
```

**Cómo se ejecuta**: Automáticamente mediante `ValidationBehaviour` (pipeline de MediatR)

---

## Mapeo de Objetos

### AutoMapper

**Qué es**: Librería para mapear objetos de un tipo a otro.

**Para qué se usa**:
- Mapear Commands → Entidades
- Mapear Entidades → View Models
- Evitar código repetitivo de asignación

**Configuración**:
```csharp
// Mapping Profile
public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<CreateProductCommand, Product>();
        CreateMap<Product, ProductVm>();
    }
}
```

**Uso**:
```csharp
// Command → Entity
var product = _mapper.Map<Product>(command);

// Entity → ViewModel
var vm = _mapper.Map<ProductVm>(product);
```

---

## Autenticación y Seguridad

### JWT (JSON Web Tokens)

**Qué es**: Estándar para tokens de autenticación.

**Para qué se usa**:
- Autenticación stateless
- Tokens seguros y firmados
- Refresh tokens para renovación

**Flujo**:
1. Cliente → Login → Recibe JWT token
2. Cliente incluye token en header: `Authorization: Bearer <token>`
3. Middleware valida token en cada request

**Configuración**: `appsettings.json` → `JwtSettings`

---

### ASP.NET Core Identity

**Qué es**: Sistema de autenticación y autorización de ASP.NET Core.

**Para qué se usa**:
- Gestión de usuarios
- Roles y permisos
- Password hashing
- Token providers

**Configuración**: `Security/IdentityServiceRegistration.cs`

---

## Logging

### Serilog

**Qué es**: Librería de logging estructurado.

**Para qué se usa**:
- Logs estructurados (JSON)
- Múltiples sinks (consola, archivo, etc.)
- Correlation IDs
- Niveles configurables

**Configuración**: `appsettings.json` → `Serilog`

**Uso**:
```csharp
_logger.LogInformation("Product created. ProductId: {ProductId}, Name: {Name}", 
    productId, name);
```

**Salida**:
```json
{
  "Timestamp": "2024-01-01T10:00:00",
  "Level": "Information",
  "Message": "Product created",
  "ProductId": "123e4567-e89b-12d3-a456-426614174000",
  "Name": "Product Name"
}
```

---

## Caching

### Memory Cache (Local)

**Qué es**: Caché en memoria local de la aplicación.

**Para qué se usa**:
- Caché rápido para datos frecuentes
- Reducir carga en base de datos
- Sin dependencias externas

**Uso**:
```csharp
// Obtener
var cached = await _cacheService.GetAsync<List<ProductVm>>(key);

// Guardar
await _cacheService.SetAsync(key, products, TimeSpan.FromMinutes(10));
```

---

### Redis (Distribuido)

**Qué es**: Caché distribuido en memoria.

**Para qué se usa**:
- Caché compartido entre múltiples instancias
- Escalabilidad horizontal
- Persistencia opcional

**Configuración**: `appsettings.json` → `CacheSettings`

---

### CacheServiceExtensions

**Qué es**: Extensiones para patrón "Get or Set" en caché.

**Para qué se usa**:
- Obtener de caché o calcular y guardar
- Simplifica código de caché

**Uso**:
```csharp
var products = await _cacheService.GetOrSetAsync(
    cacheKey,
    async () => await GetAllFromDb(), // Callback si no está en caché
    cancellationToken: cancellationToken
);
```

---

## Testing

### xUnit

**Qué es**: Framework de testing para .NET.

**Para qué se usa**:
- Tests unitarios
- Tests de integración

**Ejemplo**:
```csharp
[Fact]
public async Task CreateProduct_ShouldReturnSuccess()
{
    // Arrange
    var command = new CreateProductCommand { Name = "Test" };
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.True(result.Succeeded);
}
```

---

### Moq

**Qué es**: Librería para crear mocks en tests.

**Para qué se usa**:
- Mockear dependencias (repositorios, servicios)
- Aislar el código bajo prueba

**Ejemplo**:
```csharp
var repositoryMock = new Mock<IRepository<Product>>();
repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
    .ReturnsAsync(new Product());
```

---

### FluentAssertions

**Qué es**: Librería para assertions más legibles.

**Para qué se usa**:
- Assertions más expresivas
- Mejores mensajes de error

**Ejemplo**:
```csharp
result.Should().NotBeNull();
result.Succeeded.Should().BeTrue();
result.Items.Should().HaveCount(5);
```

---

## Manejo de Archivos

### IFileStorageService

**Qué es**: Servicio para gestionar almacenamiento de archivos.

**Para qué se usa**:
- Guardar archivos en disco
- Eliminar archivos físicos
- Crear directorios
- Abstraer el sistema de archivos

**Métodos principales**:
```csharp
public interface IFileStorageService
{
    // Guardar archivo
    Task<string> SaveFileAsync(
        IFormFile file, 
        string directory, 
        string? customFileName = null,
        CancellationToken cancellationToken = default);

    // Eliminar archivo
    Task<bool> DeleteFileAsync(
        string filePath, 
        CancellationToken cancellationToken = default);

    // Crear directorio
    Task<bool> EnsureDirectoryExistsAsync(string path);
}
```

**Ventajas**:
- ✅ Abstracción del sistema de archivos
- ✅ Fácil de testear (mock)
- ✅ Centraliza lógica de almacenamiento
- ✅ Facilita cambiar a cloud storage (S3, Azure Blob, etc.)

**Uso**:
```csharp
// Guardar archivo
var path = await _fileStorageService.SaveFileAsync(
    formFile, 
    "C:\\uploads", 
    customFileName: "document.pdf",
    cancellationToken);

// Eliminar archivo
var deleted = await _fileStorageService.DeleteFileAsync(path, cancellationToken);
```

---

### FileValidExtensions

**Qué es**: Enumeración de extensiones de archivo válidas.

**Para qué se usa**:
- Validar archivos antes de subirlos
- Prevenir subida de archivos maliciosos
- Mantener lista centralizada de extensiones permitidas

**Extensiones válidas**:
```csharp
.doc, .docx    // Microsoft Word
.pdf           // PDF
.xls, .xlsx    // Microsoft Excel
.ppt, .pptx    // Microsoft PowerPoint
.txt, .xml     // Texto
.jpg, .jpeg    // Imágenes JPEG
.png           // Imágenes PNG
```

**Uso**:
```csharp
var extensions = FileValidExtensions.ValidFiles;
var fileExtension = Path.GetExtension(fileName);

if (!extensions.Contains(fileExtension))
{
    throw new BadRequestException("Invalid file extension");
}
```

---

### UploadedFile (Entidad)

**Qué es**: Entidad de dominio para archivos subidos.

**Propiedades**:
```csharp
public class UploadedFile : BaseEntity
{
    public string Name { get; set; }        // Nombre del archivo
    public string? Type { get; set; }       // Tipo/categoría (Invoice, Contract, etc.)
    public string? Reference { get; set; }  // Referencia externa
    public decimal? Size { get; set; }      // Tamaño en MB
    public string? Comment { get; set; }    // Comentario opcional
    public string? Extension { get; set; }  // Extensión (.pdf, .docx, etc.)
    public string Path { get; set; }        // Ruta física del archivo
}
```

**Uso**:
- Tracking de archivos subidos
- Metadatos y auditoría
- Soft delete (Active property from BaseEntity)

---

## Envío de Correos

### SmtpMailService

**Qué es**: Servicio para envío de correos electrónicos con SMTP.

**Para qué se usa**:
- Enviar emails (bienvenida, notificaciones, reportes)
- Emails con HTML
- Adjuntos
- CC y múltiples destinatarios

**Configuración**:
```json
// appsettings.json
{
  "EMailSettings": {
    "From": "noreply@miempresa.com",
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UserName": "user@gmail.com",
    "Password": "app-password"
  }
}
```

**Características**:
- ✅ Retry logic con Polly (3 reintentos)
- ✅ Timeout configurable (30 segundos)
- ✅ Validación de emails
- ✅ Soporte para HTML
- ✅ Attachments y CC
- ✅ Logging detallado

**Uso**:
```csharp
var mailRequest = new MailRequest
{
    To = new List<string> { "user@example.com" },
    Subject = "Welcome!",
    Body = "<h1>Welcome to our platform</h1>",
    Cc = new List<string> { "admin@example.com" },
    Attach = new List<string> { "/path/to/file.pdf" },
    IsNotification = true
};

var success = await _smtpMailService.SendAsync(
    mailRequest, 
    pathImages: "/images",
    cancellationToken);
```

---

### MailKit

**Qué es**: Librería open source para SMTP, POP3 e IMAP.

**Para qué se usa**:
- Envío de correos (SMTP)
- Cliente de email robusto
- Soporte completo de estándares

**Ventajas sobre System.Net.Mail**:
- ✅ Más moderno y mantenido
- ✅ Mejor soporte de MIME
- ✅ Autenticación OAuth2
- ✅ Certificados SSL/TLS
- ✅ Async/await nativo

**Documentación**: [MailKit GitHub](https://github.com/jstedfast/MailKit)

---

### Mailpit (SMTP para desarrollo)

**Qué es**: Servidor SMTP moderno para desarrollo y testing.

**Para qué se usa**:
- Capturar emails en desarrollo sin enviarlos realmente
- Testing de funcionalidad de correos
- Debugging de templates de email
- Visualización de headers y contenido

**Características**:
- ✅ Interfaz web moderna (http://localhost:8025)
- ✅ API REST completa
- ✅ Persistencia con SQLite
- ✅ Búsqueda y filtrado avanzado
- ✅ No envía emails reales (seguro)

**Configuración en Docker**: Ver [docs/DOCKER-SETUP.md](DOCKER-SETUP.md)

---

## Resiliencia y Reintentos

### Polly

**Qué es**: Librería para resiliencia y manejo de fallas transitorias.

**Para qué se usa**:
- Retry policies (reintentos automáticos)
- Circuit breaker
- Timeout policies
- Fallback strategies

**Políticas implementadas**:

**1. Retry con Exponential Backoff**:
```csharp
_retryPolicy = Policy
    .Handle<SocketException>()
    .Or<TimeoutException>()
    .Or<IOException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (exception, timeSpan, retryCount, context) =>
        {
            _logger.LogWarning(exception, 
                "Retry {RetryCount} after {Delay}s", 
                retryCount, timeSpan.TotalSeconds);
        });
```

**Delays**:
- Intento 1: Inmediato
- Intento 2: Espera 2 segundos (2^1)
- Intento 3: Espera 4 segundos (2^2)
- Intento 4: Espera 8 segundos (2^3)

**Uso en SmtpMailService**:
```csharp
// Se ejecuta con retry automático
await _retryPolicy.ExecuteAsync(async () =>
{
    await SendEmailAsync(request, pathImages, cancellationToken);
});
```

**Excepciones manejadas**:
- `SocketException`: Problemas de red
- `TimeoutException`: Timeout de conexión
- `IOException`: Errores de I/O

**Ventajas**:
- ✅ Manejo automático de fallas transitorias
- ✅ Exponential backoff para no saturar el servidor
- ✅ Logging de reintentos
- ✅ Código más limpio (sin try-catch anidados)

**Documentación**: [Polly GitHub](https://github.com/App-vNext/Polly)

---

## Mejoras Implementadas

### 1. UploadFileCommand (Mejorado)

**Mejoras**:
- ✅ Validación fail-fast (valida todo antes de procesar)
- ✅ Transacciones con rollback automático
- ✅ Cleanup de archivos físicos en caso de error
- ✅ Logging estructurado con Serilog.Context
- ✅ Invalidación de caché automática
- ✅ Mejor manejo de excepciones

**Flujo**:
1. Validar archivos (extensión, tamaño)
2. Iniciar transacción
3. Guardar archivos físicos
4. Crear registros en BD
5. Confirmar transacción
6. Si hay error: Rollback + Cleanup

**Código**: `src/Core/Application/Features/Utilities/UploadFiles/Commands/Create/`

---

### 2. VoidUploadedFileCommand (Mejorado)

**Mejoras**:
- ✅ Validación de GUID con manejo específico
- ✅ Opción de eliminación física del archivo
- ✅ Logging detallado con información contextual
- ✅ No falla si el archivo físico no existe
- ✅ Invalidación de caché
- ✅ Mensaje de éxito que incluye el nombre del archivo

**Opciones**:
- Soft delete (default): Solo marca como inactivo en BD
- Physical delete: También elimina el archivo físico

**Código**: `src/Core/Application/Features/Utilities/UploadFiles/Commands/VoidFile/`

---

### 3. SmtpMailService (Mejorado)

**Mejoras**:
- ✅ Polly para retry logic (3 reintentos con exponential backoff)
- ✅ Validación completa de request (formato de email, campos requeridos)
- ✅ Timeout de 30 segundos configurable
- ✅ Desconexión garantizada incluso en errores
- ✅ Logging de reintentos y errores
- ✅ Manejo de excepciones transitórias

**Validaciones**:
- Request no nulo
- Al menos un destinatario
- Formato de email válido
- Subject y Body no vacíos

**Código**: `src/Infrastructure/Shared/Services/SmtpMailService.cs`

---

## 📚 Recursos

- [Microsoft .NET Documentation](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core Docs](https://docs.microsoft.com/ef/core/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation Docs](https://docs.fluentvalidation.net/)
- [AutoMapper Docs](https://docs.automapper.org/)
- [Serilog Documentation](https://serilog.net/)
- [Polly Documentation](https://github.com/App-vNext/Polly)
- [MailKit Documentation](https://github.com/jstedfast/MailKit)

**Ejemplos completos**:
- Ver ejemplos de archivos y correos en [docs/EJEMPLOS.md](EJEMPLOS.md)
- Ver tests en `tests/Tests/Application/Utilities/` y `tests/Tests/Infrastructure/Services/`

---

¿Necesitas más información sobre alguna herramienta específica? Consulta la documentación oficial o revisa los ejemplos en el código.

