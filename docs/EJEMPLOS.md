# Ejemplos y Mejores Prácticas

Esta documentación contiene ejemplos prácticos de código y mejores prácticas basadas en los patrones del proyecto.

## 📋 Índice

- [Ejemplos de Código](#ejemplos-de-código)
- [Mejores Prácticas](#mejores-prácticas)
- [Patrones Comunes](#patrones-comunes)
- [Casos de Uso Avanzados](#casos-de-uso-avanzados)

---

## Ejemplos de Código

### 1. Handler con Validación y Caché

```csharp
public class GetAllProductsQueryHandler(
    IMapper _mapper,
    ILogger<GetAllProductsQueryHandler> _logger,
    IRepositoryFactory _repositoryFactory,
    ICacheKeyService _cacheKeyService,
    ICacheService _cacheService) 
    : IRequestHandler<GetAllProductsQuery, Result<IReadOnlyList<ProductVm>>>
{
    public async Task<Result<IReadOnlyList<ProductVm>>> Handle(
        GetAllProductsQuery request, 
        CancellationToken cancellationToken)
    {
        // 1. Logging estructurado
        using (LogContext.PushProperty("Query", "GetAllProducts"))
        {
            _logger.LogDebug("Fetching all products");

            // 2. Obtener repositorio
            var repo = _repositoryFactory.GetRepository<Product>();
            
            // 3. Construir clave de caché
            var cacheKey = _cacheKeyService.GetListKey(typeof(ProductVm).Name);

            // 4. Get or Set pattern (caché)
            var result = await _cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    // Si no está en caché, obtener de BD
                    var products = await repo.GetAllAsync(cancellationToken);
                    return _mapper.Map<IReadOnlyList<ProductVm>>(products);
                },
                cancellationToken: cancellationToken,
                logger: _logger);

            _logger.LogDebug("Retrieved {Count} products", result?.Count ?? 0);
            
            // 5. Retornar Result estándar
            return Result<IReadOnlyList<ProductVm>>.Success(
                result ?? new List<ProductVm>(), 
                result?.Count ?? 0);
        }
    }
}
```

**Puntos clave**:
- ✅ Logging estructurado con contexto
- ✅ Uso de caché con GetOrSetAsync
- ✅ Retorno estándar con `Result<T>`
- ✅ Manejo de nulls

---

### 2. Command con Validación y Transacción

```csharp
public class CreateProductCommandHandler(
    ICacheKeyService _cacheKeyService,
    ICacheService _cacheService,
    IMapper _mapper,
    ILogger<CreateProductCommandHandler> _logger,
    IUnitOfWork _unitOfWork) 
    : IRequestHandler<CreateProductCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateProductCommand request, 
        CancellationToken cancellationToken)
    {
        using (LogContext.PushProperty("ProductName", request.Name))
        {
            try
            {
                _logger.LogInformation("Starting product creation. Name: {ProductName}", 
                    request.Name);

                // 1. Obtener repositorio desde UnitOfWork
                var repo = _unitOfWork.Repository<Product>();

                // 2. Validar negocio (no duplicados)
                var exists = await repo.GetFirstAsync(
                    x => x.Name == request.Name, 
                    cancellationToken);
                ThrowException.Exception.IfNotNull(exists, ErrorMessage.RecordExist);

                // 3. Mapear y agregar
                var entityToAdd = _mapper.Map<Product>(request);
                await repo.AddAsync(entityToAdd, cancellationToken);
                
                // 4. Guardar cambios (transacción)
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 5. Invalidar caché
                var cacheKey = _cacheKeyService.GetListKey(typeof(ProductVm).Name);
                await _cacheService.RemoveAsync(cacheKey);

                _logger.LogInformation("Product created successfully. ProductId: {ProductId}", 
                    entityToAdd.Id);

                return Result<string>.Success(
                    entityToAdd.Id.ToString(), 
                    1, 
                    ErrorMessage.AddedSuccessfully("Product", request.Name));
            }
            catch (Exception ex)
            {
                // 6. Manejo de excepciones
                var message = ErrorMessageFormatter.Format(ex, ErrorMessage.ErrorCreating);
                _logger.LogError(ex, "Error creating product. Error: {ErrorMessage}", message);
                throw new InternalServerError(message, ex);
            }
        }
    }
}
```

**Puntos clave**:
- ✅ Uso de UnitOfWork para transacciones
- ✅ Validación de negocio antes de persistir
- ✅ Invalidación de caché después de crear
- ✅ Manejo centralizado de excepciones
- ✅ Logging estructurado

---

### 3. Validator con Validación Asíncrona

```csharp
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    private readonly IRepositoryFactory _repositoryFactory;

    public CreateProductValidator(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;

        // Validaciones síncronas
        RuleFor(x => x.Name)
            .NotEmpty()
            .NotNull()
            .MinimumLength(10)
            .MaximumLength(50);

        RuleFor(x => x.Price)
            .NotEmpty()
            .NotNull()
            .GreaterThan(0);

        // Validación asíncrona (consulta BD)
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .NotNull()
            .MustAsync(CategoryIdExistInDb!)
            .WithMessage("La categoría [{PropertyValue}] No Existe en la BD!");
    }

    private async Task<bool> CategoryIdExistInDb(
        Guid categoryId, 
        CancellationToken cancellationToken)
    {
        try
        {
            var repo = _repositoryFactory.GetRepository<Category>();
            return await repo.ExistsAsync(categoryId, cancellationToken);
        }
        catch (Exception)
        {
            // Si hay error en BD, falla la validación
            return false;
        }
    }
}
```

**Puntos clave**:
- ✅ Validaciones síncronas simples
- ✅ Validaciones asíncronas para consultas BD
- ✅ Manejo de excepciones en validaciones async
- ✅ Mensajes de error personalizados

---

### 4. Controller RESTful

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ApiBaseController
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene todos los productos
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Result<IReadOnlyList<ProductVm>>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllProductsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un producto por ID
    /// </summary>
    [HttpGet("getById")]
    public async Task<ActionResult<Result<ProductVm>>> GetById([FromQuery] string? pkId)
    {
        var result = await _mediator.Send(new GetProductByIdQuery() { PkId = pkId });
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Result<string>>> Create([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { pkId = result.Items }, result);
    }

    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<Result<string>>> Update([FromBody] UpdateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Elimina un producto
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<Result<string>>> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteProductCommand() { Id = id });
        return Ok(result);
    }
}
```

**Puntos clave**:
- ✅ Hereda de `ApiBaseController`
- ✅ Solo usa MediatR, sin lógica de negocio
- ✅ Retorna `Result<T>` estándar
- ✅ XML documentation para Swagger
- ✅ Códigos HTTP apropiados (200, 201, etc.)

---

## Mejores Prácticas

### 1. Manejo de Excepciones

✅ **Sí hacer**:
```csharp
try
{
    // Lógica
}
catch (Exception ex)
{
    var message = ErrorMessageFormatter.Format(ex, ErrorMessage.ErrorCreating);
    _logger.LogError(ex, "Error creating product. Error: {ErrorMessage}", message);
    throw new InternalServerError(message, ex);
}
```

❌ **No hacer**:
```csharp
try
{
    // Lógica
}
catch (Exception ex)
{
    throw; // Perdemos contexto
}
```

---

### 2. Logging Estructurado

✅ **Sí hacer**:
```csharp
using (LogContext.PushProperty("ProductId", productId))
{
    _logger.LogInformation("Product created. ProductId: {ProductId}, Name: {Name}", 
        productId, name);
}
```

❌ **No hacer**:
```csharp
_logger.LogInformation($"Product created. ProductId: {productId}"); // String interpolation
```

---

### 3. Uso de Result<T>

✅ **Sí hacer**:
```csharp
return Result<string>.Success(id.ToString(), 1, "Product created successfully");
return Result<ProductVm>.Success(productVm, 1);
return Result<string>.Fail("Error message");
```

❌ **No hacer**:
```csharp
return productVm; // Sin Result<T>
throw new Exception("Error"); // Para errores de negocio, usa Result.Fail
```

---

### 4. Validación

✅ **Sí hacer**:
```csharp
// En Validator
RuleFor(x => x.Name).NotEmpty().MinimumLength(10);

// En Handler (validación de negocio)
var exists = await repo.GetFirstAsync(x => x.Name == request.Name);
ThrowException.Exception.IfNotNull(exists, ErrorMessage.RecordExist);
```

❌ **No hacer**:
```csharp
// Validación en Controller o Handler
if (string.IsNullOrEmpty(request.Name))
    return BadRequest(); // ❌ Debe estar en Validator
```

---

## Patrones Comunes

### Patrón Get or Set (Caché)

```csharp
var result = await _cacheService.GetOrSetAsync(
    cacheKey,
    async () => await GetAllFromDb(), // Callback si no está en caché
    cancellationToken: cancellationToken,
    logger: _logger);
```

**Ventajas**:
- ✅ Código más limpio
- ✅ Manejo automático de errores
- ✅ Logging integrado

---

### Patrón UnitOfWork

```csharp
var repo = _unitOfWork.Repository<Product>();
await repo.AddAsync(product, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken); // Una transacción
```

**Ventajas**:
- ✅ Transacciones automáticas
- ✅ Rollback en caso de error
- ✅ Coordinación de múltiples repositorios

---

### Patrón Specification

```csharp
var spec = new ProductPaginationSpecification(specParams);
var products = await repo.GetAllWithSpec(spec, cancellationToken);
```

**Ventajas**:
- ✅ Consultas reutilizables
- ✅ Separación de lógica de consulta
- ✅ Fácil de testear

---

## Casos de Uso Avanzados

### 1. Búsqueda con Filtros y Paginación

```csharp
public class SearchProductsQuery : IRequest<Result<PaginationVm<ProductVm>>>
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, ...>
{
    public async Task<Result<PaginationVm<ProductVm>>> Handle(...)
    {
        var spec = new ProductSearchSpecification(request);
        var repo = _repositoryFactory.GetRepository<Product>();
        
        var count = await repo.CountAsync(spec, cancellationToken);
        var products = await repo.GetAllWithSpec(spec, cancellationToken);
        
        var pagination = new PaginationVm<ProductVm>(
            request.PageIndex,
            request.PageSize,
            count,
            _mapper.Map<IReadOnlyList<ProductVm>>(products));
        
        return Result<PaginationVm<ProductVm>>.Success(pagination, 1);
    }
}
```

---

### 2. Operación en Lote

```csharp
public class BulkCreateProductsCommand : IRequest<Result<int>>
{
    public List<CreateProductCommand> Products { get; set; } = new();
}

public class BulkCreateProductsCommandHandler : IRequestHandler<...>
{
    public async Task<Result<int>> Handle(...)
    {
        var repo = _unitOfWork.Repository<Product>();
        var created = 0;

        foreach (var productCmd in request.Products)
        {
            var product = _mapper.Map<Product>(productCmd);
            await repo.AddAsync(product, cancellationToken);
            created++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(created, created);
    }
}
```

---

### 3. Soft Delete

```csharp
public class DeleteProductCommandHandler : ...
{
    public async Task<Result<string>> Handle(...)
    {
        var product = await repo.GetByIdAsync(id, cancellationToken);
        
        // Soft delete (marcar como eliminado)
        product.IsDeleted = true;
        product.DeletedDate = DateTime.UtcNow;
        
        await repo.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<string>.Success(id.ToString(), 1);
    }
}
```

---

## 🔧 Uso de Helpers y Servicios

### Helpers para Result<T>

```csharp
// Antes
return Result<string>.Success(entityId.ToString(), 1, ErrorMessage.AddedSuccessfully("Product", name!));

// Después (más legible)
return ResultExtensions.CreatedSuccessfully(entityId, "Product", name);
```

### Servicio de Invalidación de Caché

```csharp
// Antes
var genericCacheKey = _cacheKeyService.GetListKey(typeof(ProductVm).Name);
await _cacheService.RemoveAsync(genericCacheKey);
var categoryCacheKey = _cacheKeyService.GetKey($"{typeof(ProductVm).Name}:Category", categoryId);
await _cacheService.RemoveAsync(categoryCacheKey);

// Después (1 línea)
await _cacheInvalidationService.InvalidateEntityCacheAsync<ProductVm>(categoryId, cancellationToken);
```

### Handler Base para Paginación

Ver ejemplos completos en `GetPaginatedCategoriesQueryHandler`.

---

### 4. Manejo de Archivos - Upload

```csharp
public class UploadFileCommand : IRequest<Result<bool>>
{
    public List<IFormFile>? Files { get; set; }
    public string? Type { get; set; }
    public string? Reference { get; set; }
    public string? Comment { get; set; }
}

public class UploadFileCommandHandler(
    IUnitOfWork _unitOfWork,
    IConfiguration _configuration,
    IFileStorageService _fileStorageService,
    ICacheInvalidationService _cacheInvalidationService,
    ILogger<UploadFileCommandHandler> _logger) 
    : IRequestHandler<UploadFileCommand, Result<bool>>
{
    private readonly long _maxFileSize = 3000000; // 3MB

    public async Task<Result<bool>> Handle(
        UploadFileCommand request, 
        CancellationToken cancellationToken)
    {
        var uploadedFiles = new List<UploadedFile>();
        var savedFiles = new List<string>();

        using (LogContext.PushProperty("FileCount", request.Files?.Count ?? 0))
        {
            try
            {
                // 1. Validaciones tempranas (fail fast)
                ThrowException.Exception.IfNull(request.Files, "Files list cannot be null");
                
                if (request.Files!.Count == 0)
                {
                    throw new BadRequestException("At least one file must be provided");
                }

                var path = _configuration["FilesPaths:TestPath"];
                if (string.IsNullOrEmpty(path))
                {
                    throw new BadRequestException("FilesPaths:TestPath configuration is missing");
                }

                // 2. Validar extensiones y tamaños
                var extensions = FileValidExtensions.ValidFiles;
                foreach (var file in request.Files)
                {
                    var extFile = Path.GetExtension(file.FileName);
                    
                    if (!extensions.Contains(extFile))
                    {
                        throw new BadRequestException(
                            $"El Archivo [{file.FileName}] no tiene una extensión válida");
                    }

                    if (file.Length > _maxFileSize)
                    {
                        throw new BadRequestException(
                            $"El Archivo [{file.FileName}] excede el tamaño máximo");
                    }
                }

                // 3. Iniciar transacción
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                try
                {
                    var fileRepository = _unitOfWork.Repository<UploadedFile>();

                    // 4. Procesar cada archivo
                    foreach (var formFile in request.Files)
                    {
                        // Guardar archivo físico
                        var filePath = await _fileStorageService.SaveFileAsync(
                            formFile,
                            path!,
                            cancellationToken: cancellationToken);

                        savedFiles.Add(filePath);

                        // Crear registro en BD
                        var uploadedFile = new UploadedFile
                        {
                            Name = formFile.FileName,
                            Type = request.Type,
                            Reference = request.Reference,
                            Size = Convert.ToDecimal(formFile.Length / 1024f / 1024f),
                            Comment = request.Comment,
                            Extension = Path.GetExtension(formFile.FileName),
                            Path = filePath
                        };

                        await fileRepository.AddAsync(uploadedFile, cancellationToken);
                        uploadedFiles.Add(uploadedFile);
                    }

                    // 5. Confirmar transacción
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    // 6. Invalidar caché
                    await _cacheInvalidationService
                        .InvalidateEntityListCacheAsync<UploadedFile>(cancellationToken);

                    _logger.LogInformation("Files uploaded successfully. Total: {Count}", 
                        uploadedFiles.Count);

                    return Result<bool>.Success(true, uploadedFiles.Count, 
                        $"{uploadedFiles.Count} archivo(s) subido(s) exitosamente");
                }
                catch (Exception)
                {
                    // Rollback de transacción
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                    // Cleanup de archivos físicos guardados
                    await CleanupSavedFiles(savedFiles, cancellationToken);

                    throw;
                }
            }
            catch (BadRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var message = ErrorMessageFormatter.Format(ex, ErrorMessage.ErrorCreating);
                _logger.LogError(ex, "Error uploading files");
                throw new InternalServerError(message, ex);
            }
        }
    }

    private async Task CleanupSavedFiles(
        List<string> filePaths, 
        CancellationToken cancellationToken)
    {
        foreach (var filePath in filePaths)
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup file: {FilePath}", filePath);
            }
        }
    }
}
```

**Puntos clave**:
- ✅ Validación fail-fast antes de procesamiento
- ✅ Transacción con rollback automático
- ✅ Cleanup de archivos físicos en caso de error
- ✅ Validación de extensiones y tamaño
- ✅ Logging estructurado con contexto
- ✅ Invalidación de caché

**Uso desde Controller**:
```csharp
[HttpPost("upload")]
public async Task<ActionResult<Result<bool>>> UploadFiles([FromForm] UploadFileCommand command)
{
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

**Uso desde cliente**:
```javascript
const formData = new FormData();
formData.append('Files', file1);
formData.append('Files', file2);
formData.append('Type', 'Invoice');
formData.append('Reference', 'INV-2024-001');

const response = await fetch('/api/files/upload', {
    method: 'POST',
    body: formData
});
```

---

### 5. Manejo de Archivos - Eliminación

```csharp
public class VoidUploadedFileCommand : IRequest<Result<string>>
{
    public string? Id { get; set; }
    public bool PhysicalDelete { get; set; } = false; // Opción para eliminar archivo físico
}

public class VoidUploadedFileCommandHandler(
    IUnitOfWork _unitOfWork,
    IFileStorageService _fileStorageService,
    ICacheInvalidationService _cacheInvalidationService,
    ILogger<VoidUploadedFileCommandHandler> _logger) 
    : IRequestHandler<VoidUploadedFileCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        VoidUploadedFileCommand request, 
        CancellationToken cancellationToken)
    {
        using (LogContext.PushProperty("FileId", request.Id))
        {
            try
            {
                // 1. Validar GUID
                Guid fileId;
                try
                {
                    fileId = Guid.Parse(request.Id!);
                }
                catch (FormatException ex)
                {
                    _logger.LogWarning(ex, "Invalid GUID format");
                    throw;
                }

                var repoUploadFile = _unitOfWork.Repository<UploadedFile>();

                // 2. Buscar archivo
                var existUploadFile = await repoUploadFile.GetFirstAsync(
                    x => x.Id == fileId && (bool)x.Active!, 
                    cancellationToken)!;

                ThrowException.Exception.IfObjectClassNull(existUploadFile, fileId);

                // 3. Soft delete (marcar como inactivo)
                existUploadFile!.Active = false;
                await repoUploadFile.UpdateAsync(existUploadFile, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("File marked as inactive. Name: {FileName}", 
                    existUploadFile.Name);

                // 4. Eliminación física (opcional)
                if (request.PhysicalDelete && !string.IsNullOrEmpty(existUploadFile.Path))
                {
                    try
                    {
                        var deleted = await _fileStorageService
                            .DeleteFileAsync(existUploadFile.Path, cancellationToken);
                        
                        if (deleted)
                        {
                            _logger.LogInformation("Physical file deleted. Path: {FilePath}", 
                                existUploadFile.Path);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting physical file");
                        // No lanzamos excepción - el registro en BD ya fue actualizado
                    }
                }

                // 5. Invalidar caché
                await _cacheInvalidationService
                    .InvalidateEntityListCacheAsync<UploadedFile>(cancellationToken);

                var message = $"El registro [{request.Id}] ({existUploadFile.Name}), " +
                             $"ha sido eliminado de la entidad *UploadedFile* correctamente!";
                return Result<string>.Success(request.Id!, 1, message);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var message = ErrorMessageFormatter.Format(ex, ErrorMessage.ErrorDeleting);
                _logger.LogError(ex, "Error voiding file");
                throw new InternalServerError(message, ex);
            }
        }
    }
}
```

**Puntos clave**:
- ✅ Soft delete por defecto (mantiene registro en BD)
- ✅ Opción de eliminación física del archivo
- ✅ Validación de GUID con manejo específico
- ✅ No falla si el archivo físico no existe
- ✅ Logging detallado
- ✅ Invalidación de caché

**Uso desde Controller**:
```csharp
[HttpDelete("{id}")]
public async Task<ActionResult<Result<string>>> VoidFile(
    string id, 
    [FromQuery] bool physicalDelete = false)
{
    var result = await _mediator.Send(new VoidUploadedFileCommand 
    { 
        Id = id, 
        PhysicalDelete = physicalDelete 
    });
    return Ok(result);
}
```

---

### 6. Envío de Correos Electrónicos

```csharp
public interface ISmtpMailService
{
    Task<bool> SendAsync(
        MailRequest request, 
        string? pathImages = null, 
        CancellationToken cancellationToken = default);
}

public class SmtpMailService : ISmtpMailService
{
    private readonly EMailSettings? _emailSettings;
    private readonly ILogger<SmtpMailService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly int _timeoutSeconds = 30;

    public SmtpMailService(
        IOptions<EMailSettings> mailSettings, 
        ILogger<SmtpMailService> logger)
    {
        _emailSettings = mailSettings.Value;
        _logger = logger;

        // Configurar Polly para retry logic
        _retryPolicy = Policy
            .Handle<SocketException>()
            .Or<TimeoutException>()
            .Or<IOException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception, 
                        "Retry {RetryCount} after {Delay}s due to {ExceptionType}",
                        retryCount, timeSpan.TotalSeconds, exception.GetType().Name);
                });
    }

    public async Task<bool> SendAsync(
        MailRequest request, 
        string? pathImages = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Validar request
            if (!ValidateRequest(request))
            {
                return false;
            }

            _logger.LogInformation("Sending email to: {Recipients}, Subject: {Subject}",
                string.Join(", ", request.To), request.Subject);

            // 2. Ejecutar con retry policy
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await SendEmailAsync(request, pathImages, cancellationToken);
            });

            _logger.LogInformation("Email sent successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email after retries");
            return false;
        }
    }

    private async Task SendEmailAsync(
        MailRequest request, 
        string? pathImages, 
        CancellationToken cancellationToken)
    {
        using var smtp = new SmtpClient
        {
            Timeout = _timeoutSeconds * 1000
        };

        try
        {
            var email = BuildEmailMessage(request, pathImages);

            using var cts = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

            await smtp.ConnectAsync(
                _emailSettings!.Host, 
                _emailSettings.Port, 
                SecureSocketOptions.StartTlsWhenAvailable, 
                cts.Token);

            await smtp.AuthenticateAsync(
                _emailSettings.UserName, 
                _emailSettings.Password, 
                cts.Token);

            await smtp.SendAsync(email, cts.Token);
            await smtp.DisconnectAsync(true, cts.Token);
        }
        catch (Exception)
        {
            // Asegurar desconexión en caso de error
            if (smtp.IsConnected)
            {
                try
                {
                    await smtp.DisconnectAsync(true, CancellationToken.None);
                }
                catch (Exception disconnectEx)
                {
                    _logger.LogWarning(disconnectEx, "Error disconnecting SMTP");
                }
            }
            throw;
        }
    }

    private bool ValidateRequest(MailRequest request)
    {
        if (request?.To == null || request.To.Count == 0)
        {
            _logger.LogWarning("Email request has no recipients");
            return false;
        }

        foreach (var email in request.To)
        {
            if (!IsValidEmail(email))
            {
                _logger.LogWarning("Invalid email format: {Email}", email);
                return false;
            }
        }

        return true;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
```

**Puntos clave**:
- ✅ **Polly** para retry logic con exponential backoff
- ✅ 3 reintentos automáticos para errores transitorios
- ✅ Timeout configurable (30 segundos)
- ✅ Validación completa de request
- ✅ Desconexión garantizada incluso en errores
- ✅ Logging detallado de reintentos y errores

**Configuración en appsettings.json**:

**Para Desarrollo (con Mailpit):**
```json
{
  "EMailSettings": {
    "From": "noreply@test.com",
    "Host": "localhost",
    "Port": 1025,
    "UserName": "",
    "Password": ""
  }
}
```

**Para Producción:**
```json
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

**Uso desde Handler**:
```csharp
public class SendWelcomeEmailCommandHandler(
    ISmtpMailService _mailService) 
    : IRequestHandler<SendWelcomeEmailCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(...)
    {
        var mailRequest = new MailRequest
        {
            To = new List<string> { user.Email },
            Subject = "Bienvenido a la plataforma",
            Body = "<h1>Bienvenido!</h1><p>Gracias por registrarte.</p>",
            Cc = new List<string> { "admin@company.com" },
            Attach = new List<string> { "/path/to/manual.pdf" }
        };

        var success = await _mailService.SendAsync(
            mailRequest, 
            pathImages: "/path/to/images",
            cancellationToken);

        if (success)
        {
            return Result<bool>.Success(true, 1, "Email sent successfully");
        }

        return Result<bool>.Fail("Failed to send email");
    }
}
```

**Extensiones válidas de archivo**:
```csharp
// src/Infrastructure/Shared/Services/Enums/FileValidExtensions.cs
public struct FileValidExtensions
{
    public static List<string> ValidFiles => new List<string>
    {
        ".doc", ".docx",   // Word
        ".pdf",            // PDF
        ".xls", ".xlsx",   // Excel
        ".ppt", ".pptx",   // PowerPoint
        ".txt", ".xml",    // Texto
        ".jpg", ".jpeg",   // Imágenes
        ".png"
    };
}
```

---

## 📚 Recursos

- Revisa los ejemplos completos en `src/Core/Application/Features/Examples/`
- Ver implementaciones de archivos en `src/Core/Application/Features/Utilities/UploadFiles/`
- Ver servicio de email en `src/Infrastructure/Shared/Services/SmtpMailService.cs`
- Consulta los tests en `tests/Tests/` para ver patrones de uso
- Ver documentación de cada herramienta en [docs/HERRAMIENTAS.md](HERRAMIENTAS.md)
- Ver mejoras implementadas en [docs/RESUMEN_MEJORAS.md](RESUMEN_MEJORAS.md)

---

¿Tienes un caso de uso específico? Revisa los ejemplos existentes o consulta la documentación.

