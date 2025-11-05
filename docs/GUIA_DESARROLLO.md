# Guía de Desarrollo - Crear un Nuevo Feature

Esta guía te enseñará paso a paso cómo crear un nuevo feature (CRUD completo) siguiendo las convenciones y patrones del proyecto.

> 📌 **Nota**: El proyecto incluye ejemplos completos de **Productos** y **Categorías** que puedes usar como referencia.

## 📋 Índice

1. [Crear una Nueva Entidad](#1-crear-una-nueva-entidad)
2. [Crear Commands (CQRS)](#2-crear-commands-cqrs)
3. [Crear Queries (CQRS)](#3-crear-queries-cqrs)
4. [Crear Validators](#4-crear-validators)
5. [Crear View Models](#5-crear-view-models)
6. [Crear Controllers](#6-crear-controllers)
7. [Configurar AutoMapper](#7-configurar-automapper)
8. [Implementar Paginación](#8-implementar-paginación)

---

## 1. Crear una Nueva Entidad

### Ubicación
`src/Core/Domain/Entities/[TuDominio]/`

### Ejemplo: Crear entidad `Employee`

```csharp
using Domain.Base;

namespace Domain.Entities.HumanResources
{
    public class Employee : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public Guid DepartmentId { get; set; }
        
        // Navegación (opcional)
        // public virtual Department? Department { get; set; }
    }
}
```

### Puntos Importantes

- ✅ Hereda de `BaseEntity` (incluye `Id`, `CreatedDate`, etc.)
- ✅ Usa tipos de .NET primitivos
- ✅ Propiedades públicas con get/set
- ✅ Namespace: `Domain.Entities.[TuDominio]`

### Referencia
Ver ejemplo completo: `src/Core/Domain/Entities/Examples/TestProduct.cs`

---

## 2. Crear Commands (CQRS)

Los **Commands** representan operaciones que **modifican** datos (Create, Update, Delete).

### Ubicación
`src/Core/Application/Features/[TuDominio]/[Entidad]/Commands/`

### 2.1 Command de Creación

Crea el archivo: `CreateEmployeeCommand.cs`

```csharp
using Application.DTOs;
using MediatR;

namespace Application.Features.HumanResources.Employees.Commands
{
    public class CreateEmployeeCommand : IRequest<Result<string>>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public Guid DepartmentId { get; set; }
    }
}
```

### 2.2 Handler de Creación

En el mismo archivo, agrega el Handler:

```csharp
using Application.DTOs;
using Domain.Entities.HumanResources;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.Caching.Contracts;
using Persistence.Repositories.Contracts;
using Serilog.Context;
using Shared.Exceptions;
using Shared.Extensions.Contracts;

namespace Application.Features.HumanResources.Employees.Commands
{
    public class CreateEmployeeCommandHandler(
        ICacheKeyService _cacheKeyService,
        ICacheService _cacheService,
        IMapper _mapper,
        ILogger<CreateEmployeeCommandHandler> _logger,
        IUnitOfWork _unitOfWork) : IRequestHandler<CreateEmployeeCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("EmployeeName", $"{request.FirstName} {request.LastName}"))
            {
                try
                {
                    _logger.LogInformation("Starting employee creation. Name: {FirstName} {LastName}", 
                        request.FirstName, request.LastName);

                    var repo = _unitOfWork.Repository<Employee>();

                    // Validar que no existe (ejemplo)
                    var exists = await repo.GetFirstAsync(
                        x => x.Email == request.Email, 
                        cancellationToken);
                    ThrowException.Exception.IfNotNull(exists, ErrorMessage.RecordExist);

                    // Mapear y agregar
                    var entityToAdd = _mapper.Map<Employee>(request);
                    await repo.AddAsync(entityToAdd, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Invalidar caché
                    var cacheKey = _cacheKeyService.GetListKey(typeof(EmployeeVm).Name);
                    await _cacheService.RemoveAsync(cacheKey);

                    _logger.LogInformation("Employee created successfully. EmployeeId: {EmployeeId}", 
                        entityToAdd.Id);

                    return Result<string>.Success(
                        entityToAdd.Id.ToString(), 
                        1, 
                        ErrorMessage.AddedSuccessfully("Employee", $"{request.FirstName} {request.LastName}"));
                }
                catch (Exception ex)
                {
                    var message = ErrorMessageFormatter.Format(ex, ErrorMessage.ErrorCreating);
                    _logger.LogError(ex, "Error creating employee. Error: {ErrorMessage}", message);
                    throw new InternalServerError(message, ex);
                }
            }
        }
    }
}
```

### 2.3 Commands de Update y Delete

Sigue el mismo patrón. Ver ejemplos:
- `UpdateProductCommand.cs`
- `DeleteProductCommand.cs`

---

## 3. Crear Queries (CQRS)

Las **Queries** representan operaciones que **leen** datos (GetAll, GetById, etc.).

### Ubicación
`src/Core/Application/Features/[TuDominio]/[Entidad]/Queries/`

### 3.1 Query para Obtener Todos

Crea el archivo: `GetAllEmployeesQuery.cs`

```csharp
using Application.DTOs;
using Application.Features.HumanResources.Employees.VMs;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.Caching.Contracts;
using Persistence.Caching.Extensions;
using Persistence.Repositories.Contracts;
using Serilog.Context;

namespace Application.Features.HumanResources.Employees.Queries
{
    public class GetAllEmployeesQuery : IRequest<Result<IReadOnlyList<EmployeeVm>>>
    {
    }

    public class GetAllEmployeesQueryHandler(
        IMapper _mapper,
        ILogger<GetAllEmployeesQueryHandler> _logger,
        IRepositoryFactory _repositoryFactory,
        ICacheKeyService _cacheKeyService,
        ICacheService _cacheService) : IRequestHandler<GetAllEmployeesQuery, Result<IReadOnlyList<EmployeeVm>>>
    {
        public async Task<Result<IReadOnlyList<EmployeeVm>>> Handle(GetAllEmployeesQuery request, CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("Query", "GetAllEmployees"))
            {
                _logger.LogDebug("Fetching all employees");

                var repo = _repositoryFactory.GetRepository<Employee>();
                var cacheKey = _cacheKeyService.GetListKey(typeof(EmployeeVm).Name);

                var result = await _cacheService.GetOrSetAsync(
                    cacheKey,
                    async () =>
                    {
                        var employees = await repo.GetAllAsync(cancellationToken);
                        return _mapper.Map<IReadOnlyList<EmployeeVm>>(employees);
                    },
                    cancellationToken: cancellationToken,
                    logger: _logger);

                _logger.LogDebug("Retrieved {Count} employees", result?.Count ?? 0);
                return Result<IReadOnlyList<EmployeeVm>>.Success(result ?? new List<EmployeeVm>(), result?.Count ?? 0);
            }
        }
    }
}
```

### 3.2 Query para Obtener por ID

Crea el archivo: `GetEmployeeByIdQuery.cs`

```csharp
using Application.DTOs;
using Application.Features.HumanResources.Employees.VMs;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.Repositories.Contracts;
using Serilog.Context;
using Shared.Extensions;
using Shared.Extensions.Contracts;

namespace Application.Features.HumanResources.Employees.Queries
{
    public class GetEmployeeByIdQuery : IRequest<Result<EmployeeVm>>
    {
        public string? PkId { get; set; }
    }

    public class GetEmployeeByIdQueryHandler(
        IMapper _mapper,
        ILogger<GetEmployeeByIdQueryHandler> _logger,
        IRepositoryFactory _repositoryFactory) : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeVm>>
    {
        public async Task<Result<EmployeeVm>> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
        {
            var employeeId = request.PkId!.StringToGuid();
            
            using (LogContext.PushProperty("EmployeeId", employeeId))
            {
                _logger.LogDebug("Fetching employee by ID. EmployeeId: {EmployeeId}", employeeId);

                var repo = _repositoryFactory.GetRepository<Employee>();
                var employee = await repo.GetByIdAsync(employeeId, cancellationToken);
                
                ThrowException.Exception.IfObjectClassNull(employee, request.PkId!);

                var result = _mapper.Map<EmployeeVm>(employee);

                _logger.LogDebug("Employee retrieved successfully. EmployeeId: {EmployeeId}", employeeId);
                return Result<EmployeeVm>.Success(result, 1);
            }
        }
    }
}
```

### Referencia
Ver ejemplos completos:
- `GetAllProductsQuery.cs`
- `GetProductByIdQuery.cs`

### 3.3 Query con Paginación

Para implementar paginación, sigue estos pasos:

1. **Crear Query que hereda de PaginationBase**:
```csharp
public class GetPaginatedProductsQuery : PaginationBase, IRequest<PaginationVm<ProductVm>>
{
    // Propiedades adicionales para filtros
    public string? CategoryName { get; set; }
}
```

2. **Crear SpecificationParams y Specification**:
   - Ver sección [Implementar Paginación](#8-implementar-paginación) para detalles completos

3. **Crear Handler**:
```csharp
public class GetPaginatedProductsQueryHandler : IRequestHandler<GetPaginatedProductsQuery, PaginationVm<ProductVm>>
{
    // Ver documentación completa en docs/PAGINACION.md
}
```

> 📖 **Guía Completa de Paginación**: Consulta [docs/PAGINACION.md](PAGINACION.md) para el paso a paso detallado.

---

## 4. Crear Validators

Los **Validators** usan FluentValidation para validar los requests.

### Ubicación
`src/Core/Application/Features/[TuDominio]/[Entidad]/Commands/Validators/`

### Ejemplo: Validator de Creación

Crea el archivo: `CreateEmployeeValidator.cs`

```csharp
using Domain.Entities.HumanResources;
using FluentValidation;
using Persistence.Repositories.Contracts;

namespace Application.Features.HumanResources.Employees.Commands.Validators
{
    public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public CreateEmployeeValidator(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .NotNull()
                .MinimumLength(2)
                .MaximumLength(50);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .NotNull()
                .MinimumLength(2)
                .MaximumLength(50);

            RuleFor(x => x.Email)
                .NotEmpty()
                .NotNull()
                .EmailAddress()
                .MustAsync(EmailNotExistsInDb!)
                .WithMessage("El email [{PropertyValue}] ya está registrado!");

            RuleFor(x => x.Salary)
                .NotEmpty()
                .NotNull()
                .GreaterThan(0);

            RuleFor(x => x.DepartmentId)
                .NotEmpty()
                .NotNull()
                .MustAsync(DepartmentIdExistsInDb!)
                .WithMessage("El departamento [{PropertyValue}] No Existe en la BD!");
        }

        private async Task<bool> EmailNotExistsInDb(string email, CancellationToken cancellationToken)
        {
            try
            {
                var repo = _repositoryFactory.GetRepository<Employee>();
                var exists = await repo.ExistsAsync(x => x.Email == email, cancellationToken);
                return !exists; // Retorna true si NO existe (válido)
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> DepartmentIdExistsInDb(Guid departmentId, CancellationToken cancellationToken)
        {
            try
            {
                var repo = _repositoryFactory.GetRepository<Department>();
                return await repo.ExistsAsync(departmentId, cancellationToken);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
```

### Puntos Importantes

- ✅ Hereda de `AbstractValidator<T>`
- ✅ Usa `RuleFor` para definir reglas
- ✅ Validaciones asíncronas con `MustAsync`
- ✅ Manejo de excepciones en validaciones async

### Referencia
Ver ejemplo: `CreateProductValidator.cs`

---

## 5. Crear View Models

Los **View Models** (VMs) son los DTOs que se retornan en las respuestas de la API.

### Ubicación
`src/Core/Application/Features/[TuDominio]/[Entidad]/VMs/`

### Ejemplo: View Model de Employee

Crea el archivo: `EmployeeVm.cs`

```csharp
namespace Application.Features.HumanResources.Employees.VMs
{
    /// <summary>
    /// View Model for Employee entities.
    /// Represents employee data for API responses.
    /// </summary>
    public record EmployeeVm(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        DateTime HireDate,
        decimal Salary,
        Guid DepartmentId,
        string? DepartmentName
    );
}
```

### Puntos Importantes

- ✅ Usa `record` para inmutabilidad
- ✅ Incluye XML documentation
- ✅ Solo propiedades necesarias para la respuesta
- ✅ Puede incluir datos relacionados (ej: `DepartmentName`)

### Referencia
Ver ejemplo: `ProductVm.cs`

---

## 6. Crear Controllers

Los **Controllers** exponen los endpoints de la API.

### Ubicación
`src/Presentation/AppApi/Controllers/[TuDominio]/`

### Ejemplo: Controller de Employees

Crea el archivo: `EmployeesController.cs`

```csharp
using Application.Features.HumanResources.Employees.Commands;
using Application.Features.HumanResources.Employees.Queries;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppApi.Controllers.HumanResources
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ApiBaseController
    {
        private readonly IMediator _mediator;

        public EmployeesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtiene todos los empleados
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Result<IReadOnlyList<EmployeeVm>>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllEmployeesQuery());
            return Ok(result);
        }

        /// <summary>
        /// Obtiene un empleado por ID
        /// </summary>
        [HttpGet("getById")]
        public async Task<ActionResult<Result<EmployeeVm>>> GetById([FromQuery] string? pkId)
        {
            var result = await _mediator.Send(new GetEmployeeByIdQuery() { PkId = pkId });
            return Ok(result);
        }

        /// <summary>
        /// Crea un nuevo empleado
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Result<string>>> Create([FromBody] CreateEmployeeCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { pkId = result.Items }, result);
        }

        /// <summary>
        /// Actualiza un empleado existente
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<Result<string>>> Update([FromBody] UpdateEmployeeCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Elimina un empleado
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Result<string>>> Delete(string id)
        {
            var result = await _mediator.Send(new DeleteEmployeeCommand() { Id = id });
            return Ok(result);
        }
    }
}
```

### Puntos Importantes

- ✅ Hereda de `ApiBaseController`
- ✅ Usa `IMediator` para enviar commands/queries
- ✅ Retorna `Result<T>` (formato estándar de respuestas)
- ✅ Incluye XML documentation
- ✅ Usa atributos HTTP apropiados (`[HttpGet]`, `[HttpPost]`, etc.)

### Referencia
Ver ejemplo: `ProductsController.cs`

---

## 7. Configurar AutoMapper

El **AutoMapper** mapea entre Entidades ↔ View Models ↔ Commands.

### Ubicación
`src/Core/Application/Mappings/[TuDominio]/`

### Ejemplo: Mapping Profile

Crea el archivo: `HumanResourcesMappingProfile.cs`

```csharp
using Application.Features.HumanResources.Employees.Commands;
using Application.Features.HumanResources.Employees.VMs;
using AutoMapper;
using Domain.Entities.HumanResources;

namespace Application.Mappings.HumanResources
{
    public class HumanResourcesMappingProfile : Profile
    {
        public HumanResourcesMappingProfile()
        {
            // Create Command → Entity
            CreateMap<CreateEmployeeCommand, Employee>();

            // Update Command → Entity
            CreateMap<UpdateEmployeeCommand, Employee>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // Entity → View Model
            CreateMap<Employee, EmployeeVm>()
                .ForMember(dest => dest.DepartmentName, 
                    opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null));
        }
    }
}
```

### Registrar el Profile

Edita `ApplicationServicesRegistration.cs`:

```csharp
services.AddAutoMapper(Assembly.GetExecutingAssembly());
```

Esto registra automáticamente todos los `Profile` en el assembly.

### Referencia
Ver ejemplo: `ExamplesMappingProfile.cs`

---

## 8. Implementar Paginación

La paginación permite obtener datos en páginas con filtros, ordenamiento y búsqueda.

### Componentes Necesarios

1. **Query**: Hereda de `PaginationBase`
2. **SpecificationParams**: Hereda de `SpecificationParams`
3. **Specification**: Con paginación usando `ApplyPaging()`
4. **SpecificationForCounting**: Sin paginación para contar total
5. **Handler**: Implementa la lógica de paginación

### Ejemplo Rápido

```csharp
// 1. Query
public class GetPaginatedProductsQuery : PaginationBase, IRequest<PaginationVm<ProductVm>>
{
    public string? CategoryName { get; set; }
}

// 2. SpecificationParams
internal class ProductSpecificationParams : SpecificationParams
{
    public string? CategoryName { get; set; }
}

// 3. Specification (con paginación)
internal class ProductSpecification : BaseSpecification<TestProduct>
{
    public ProductSpecification(ProductSpecificationParams @params) : base(/* filtros */)
    {
        ApplySorting(@params.Sort, sortMappings, defaultOrderBy);
        ApplyPaging(@params); // ✅ Aplica paginación
    }
}

// 4. SpecificationForCounting (sin paginación)
internal class ProductForCountingSpecification : BaseSpecification<TestProduct>
{
    public ProductForCountingSpecification(ProductSpecificationParams @params) : base(/* mismos filtros */)
    {
        // NO incluir ApplyPaging()
    }
}

// 5. Handler
public class GetPaginatedProductsQueryHandler : IRequestHandler<...>
{
    public async Task<PaginationVm<ProductVm>> Handle(...)
    {
        var spec = new ProductSpecification(@params);
        var data = await repo.GetAllWithSpec(spec);
        
        var specCount = new ProductForCountingSpecification(@params);
        var total = await repo.CountAsync(specCount);
        
        var pageCount = Math.Ceiling(total / (decimal)@params.PageSize);
        
        return new PaginationVm<ProductVm> {
            Count = total,
            PageCount = (int)pageCount,
            PageIndex = @params.PageIndex,
            PageSize = @params.PageSize,
            Data = _mapper.Map<IReadOnlyList<ProductVm>>(data)
        };
    }
}
```

> 📖 **Documentación Completa**: Consulta [docs/PAGINACION.md](PAGINACION.md) para la guía completa paso a paso con ejemplos detallados.

### Referencia
Ver ejemplo completo: `GetPaginatedCategoriesQuery.cs`

---

## ✅ Checklist Completo

Al crear un nuevo feature, asegúrate de tener:

- [ ] Entidad en `Domain/Entities/`
- [ ] Commands (Create, Update, Delete) en `Application/Features/.../Commands/`
- [ ] Queries (GetAll, GetById) en `Application/Features/.../Queries/`
- [ ] Query con Paginación (opcional) en `Application/Features/.../Queries/`
- [ ] Validators en `Application/Features/.../Commands/Validators/`
- [ ] View Models en `Application/Features/.../VMs/`
- [ ] Controller en `Presentation/AppApi/Controllers/`
- [ ] AutoMapper Profile configurado
- [ ] Tests unitarios (opcional pero recomendado)

---

## 📚 Recursos Adicionales

- **Ejemplos Completos**: Revisa `src/Core/Application/Features/Examples/Products/`
- **Tests**: Revisa `tests/Tests/Application/Handlers/` para ver ejemplos de testing
- **Arquitectura**: Consulta [docs/ARQUITECTURA.md](ARQUITECTURA.md)
- **Mejoras Implementadas**: Consulta [docs/MEJORAS_IMPLEMENTADAS.md](MEJORAS_IMPLEMENTADAS.md) para ver helpers y servicios disponibles

## 🚀 Helpers y Servicios Disponibles

El proyecto incluye varios helpers y servicios que simplifican el desarrollo:

### Helpers para Result<T>

```csharp
// Crear resultado de éxito para entidad creada
return ResultExtensions.CreatedSuccessfully(entityId, "Product", entityName);

// Crear resultado de éxito para entidad actualizada
return ResultExtensions.UpdatedSuccessfully(entityId, "Product", entityName);

// Crear resultado de éxito para entidad eliminada
return ResultExtensions.DeletedSuccessfully(entityId, "Product");
```

### Servicio de Invalidación de Caché

```csharp
// Invalidar solo lista
await _cacheInvalidationService.InvalidateEntityListCacheAsync<CategoryVm>(cancellationToken);

// Invalidar lista + caché relacionado
await _cacheInvalidationService.InvalidateEntityCacheAsync<ProductVm>(categoryId, cancellationToken);
```

### Handler Base para Paginación

Para crear handlers de paginación, hereda de `PaginatedQueryHandlerBase`:

```csharp
internal class GetPaginatedProductsQueryHandler 
    : PaginatedQueryHandlerBase<TestProduct, ProductVm, ProductSpecificationParams, GetPaginatedProductsQuery>
{
    // Solo implementa 3 métodos abstractos
}
```

Ver más detalles en [docs/MEJORAS_IMPLEMENTADAS.md](MEJORAS_IMPLEMENTADAS.md).

---

¿Tienes dudas? Revisa los ejemplos de **Productos** y **Categorías** que están incluidos en el proyecto como referencia completa.

