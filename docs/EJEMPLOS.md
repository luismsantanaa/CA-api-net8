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

## 📚 Recursos

- Revisa los ejemplos completos en `src/Core/Application/Features/Examples/`
- Consulta los tests en `tests/Tests/` para ver patrones de uso
- Ver documentación de cada herramienta en [docs/HERRAMIENTAS.md](HERRAMIENTAS.md)

---

¿Tienes un caso de uso específico? Revisa los ejemplos existentes o consulta la documentación.

