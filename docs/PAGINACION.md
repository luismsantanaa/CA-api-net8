# Guía de Paginación

Esta guía explica cómo implementar paginación en el proyecto usando el patrón Specification y las clases base proporcionadas.

## 📋 Índice

- [Componentes de Paginación](#componentes-de-paginación)
- [Flujo de Paginación](#flujo-de-paginación)
- [Implementación Paso a Paso](#implementación-paso-a-paso)
- [Ejemplo Completo](#ejemplo-completo)
- [Uso en Controllers](#uso-en-controllers)
- [Parámetros de Paginación](#parámetros-de-paginación)

---

## Componentes de Paginación

El proyecto incluye varios componentes para facilitar la implementación de paginación:

### 1. PaginationBase

**Ubicación**: `src/Infrastructure/Persistence/Pagination/PaginationBase.cs`

**Para qué se usa**: Clase base para queries que requieren paginación.

**Propiedades**:
- `PageIndex` (int): Página actual (por defecto: 1)
- `PageSize` (int): Tamaño de página (por defecto: 10, máximo: 50)
- `Search` (string?): Término de búsqueda
- `Sort` (string?): Campo y dirección de ordenamiento

**Ejemplo**:
```csharp
public class GetPaginatedProductsQuery : PaginationBase, IRequest<PaginationVm<ProductVm>>
{
    // Propiedades adicionales específicas del feature
    public string? CategoryName { get; set; }
}
```

### 2. SpecificationParams

**Ubicación**: `src/Infrastructure/Persistence/Specification/SpecificationParams.cs`

**Para qué se usa**: Clase base abstracta para parámetros de especificación.

**Propiedades**:
- `PageIndex` (int): Página actual
- `PageSize` (int): Tamaño de página (máximo: 50)
- `Search` (string?): Término de búsqueda
- `Sort` (string?): Campo y dirección de ordenamiento

**Uso**: Se hereda para crear parámetros específicos de cada feature.

### 3. PaginationVm<T>

**Ubicación**: `src/Infrastructure/Persistence/Pagination/PaginationVm.cs`

**Para qué se usa**: View Model para respuestas paginadas.

**Propiedades**:
- `Count` (int): Total de registros
- `PageIndex` (int): Página actual
- `PageSize` (int): Tamaño de página
- `PageCount` (int): Total de páginas
- `Data` (IReadOnlyList<T>?): Lista de registros de la página actual

### 4. BaseSpecification.ApplyPaging()

**Ubicación**: `src/Infrastructure/Persistence/Specification/BaseSpecification.cs`

**Para qué se usa**: Método helper para aplicar paginación a una especificación.

**Métodos**:
```csharp
// Método 1: Con parámetros específicos
protected void ApplyPaging(int skip, int take)

// Método 2: Con SpecificationParams (recomendado)
protected void ApplyPaging(SpecificationParams @params)
```

---

## Flujo de Paginación

```
1. Cliente → GET /api/products/pagination?pageIndex=1&pageSize=10&sort=nameAsc
2. Controller → GetPaginatedProductsQuery (hereda de PaginationBase)
3. Handler → Crea SpecificationParams con los parámetros
4. Handler → Crea Specification (aplica filtros, ordenamiento, paginación)
5. Handler → Crea SpecificationForCounting (solo filtros, sin paginación)
6. Repository → GetAllWithSpec(spec) → Obtiene datos paginados
7. Repository → CountAsync(specCount) → Obtiene total de registros
8. Handler → Calcula PageCount y crea PaginationVm
9. Controller → Retorna PaginationVm<T>
```

---

## Implementación Paso a Paso

### Paso 1: Crear el Query

**Ubicación**: `src/Core/Application/Features/[TuDominio]/[Entidad]/Queries/GetPaginated[Entidad]Query.cs`

```csharp
using Persistence.Pagination;
using MediatR;

namespace Application.Features.Examples.Products.Queries
{
    public class GetPaginatedProductsQuery : PaginationBase, IRequest<PaginationVm<ProductVm>>
    {
        // Propiedades adicionales para filtros específicos
        public string? CategoryName { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
```

**Puntos clave**:
- ✅ Hereda de `PaginationBase`
- ✅ Implementa `IRequest<PaginationVm<T>>`
- ✅ Puede agregar propiedades adicionales para filtros

---

### Paso 2: Crear SpecificationParams

**Ubicación**: `src/Core/Application/Features/[TuDominio]/[Entidad]/Queries/Specs/[Entidad]SpecificationParams.cs`

```csharp
using Persistence.Specification;

namespace Application.Features.Examples.Products.Queries.Specs
{
    internal class ProductSpecificationParams : SpecificationParams
    {
        // Propiedades adicionales para filtros
        public string? CategoryName { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
```

**Puntos clave**:
- ✅ Hereda de `SpecificationParams`
- ✅ Incluye propiedades de paginación automáticamente
- ✅ Agrega propiedades específicas para filtros

---

### Paso 3: Crear Specification con Paginación

**Ubicación**: `src/Core/Application/Features/[TuDominio]/[Entidad]/Queries/Specs/[Entidad]PaginationSpecification.cs`

```csharp
using System.Linq.Expressions;
using Domain.Entities.Examples;
using Persistence.Specification;

namespace Application.Features.Examples.Products.Queries.Specs
{
    internal class ProductSpecification : BaseSpecification<TestProduct>
    {
        public ProductSpecification(ProductSpecificationParams @params) : base(
            // Filtros (Criteria)
            x =>
                (string.IsNullOrWhiteSpace(@params.Search) || x.Name!.Contains(@params.Search!)) &&
                (string.IsNullOrWhiteSpace(@params.CategoryName) || x.Category!.Name!.Contains(@params.CategoryName!)) &&
                (!@params.MinPrice.HasValue || x.Price >= @params.MinPrice) &&
                (!@params.MaxPrice.HasValue || x.Price <= @params.MaxPrice)
        )
        {
            // Includes (relaciones)
            AddInclude(x => x.Category!);

            // Ordenamiento
            var sortMappings = new Dictionary<string, Expression<Func<TestProduct, object>>>
            {
                { "nameAsc", p => p.Name! },
                { "nameDesc", p => p.Name! },
                { "priceAsc", p => p.Price! },
                { "priceDesc", p => p.Price! },
                { "categoryAsc", p => p.Category!.Name! },
                { "categoryDesc", p => p.Category!.Name! }
            };

            ApplySorting(@params.Sort, sortMappings, p => p.CreatedOn!, defaultOrderDescending: true);

            // PAGINACIÓN - Usa el helper method
            ApplyPaging(@params);
        }
    }
}
```

**Puntos clave**:
- ✅ Usa `ApplyPaging(@params)` para aplicar paginación automáticamente
- ✅ Calcula `Skip` y `Take` automáticamente
- ✅ Incluye filtros, ordenamiento e includes

---

### Paso 4: Crear Specification para Conteo

**Para qué**: Obtener el total de registros (sin paginación) para calcular el número de páginas.

```csharp
internal class ProductForCountingSpecification : BaseSpecification<TestProduct>
{
    public ProductForCountingSpecification(ProductSpecificationParams @params) : base(
        // Mismos filtros que ProductSpecification (sin paginación)
        x =>
            (string.IsNullOrWhiteSpace(@params.Search) || x.Name!.Contains(@params.Search!)) &&
            (string.IsNullOrWhiteSpace(@params.CategoryName) || x.Category!.Name!.Contains(@params.CategoryName!)) &&
            (!@params.MinPrice.HasValue || x.Price >= @params.MinPrice) &&
            (!@params.MaxPrice.HasValue || x.Price <= @params.MaxPrice)
    )
    {
        // Includes necesarios para los filtros
        AddInclude(x => x.Category!);
        // NO incluir ApplyPaging() aquí
    }
}
```

**Puntos clave**:
- ✅ Mismos filtros que la specification principal
- ✅ **NO** incluye `ApplyPaging()`
- ✅ Se usa solo para contar registros

---

### Paso 5: Crear el Handler (Usando Handler Base)

**Recomendado**: Usar `PaginatedQueryHandlerBase` para simplificar el código.

```csharp
using Application.Features.Examples.Products.Queries.Specs;
using Application.Features.Examples.Products.VMs;
using Application.Handlers.Base;
using AutoMapper;
using Domain.Entities.Examples;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.Pagination;
using Persistence.Repositories.Contracts;
using Persistence.Specification;
using Persistence.Specification.Contracts;

namespace Application.Features.Examples.Products.Queries
{
    internal class GetPaginatedProductsQueryHandler(
        IMapper mapper,
        ILogger<GetPaginatedProductsQueryHandler> logger,
        IRepositoryFactory repositoryFactory) 
        : PaginatedQueryHandlerBase<TestProduct, ProductVm, ProductSpecificationParams, GetPaginatedProductsQuery>(
            mapper, logger, repositoryFactory)
    {
        protected override ISpecification<TestProduct> CreateSpecification(ProductSpecificationParams @params)
        {
            return new ProductSpecification(@params);
        }

        protected override ISpecification<TestProduct> CreateCountingSpecification(ProductSpecificationParams @params)
        {
            return new ProductForCountingSpecification(@params);
        }

        protected override ProductSpecificationParams CreateParamsFromRequest(GetPaginatedProductsQuery request)
        {
            return new ProductSpecificationParams
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Search = request.Search,
                Sort = request.Sort,
                CategoryName = request.CategoryName,
                MinPrice = request.MinPrice,
                MaxPrice = request.MaxPrice
            };
        }
    }
}
```

**Puntos clave**:
- ✅ Hereda de `PaginatedQueryHandlerBase` para reducir código
- ✅ Solo implementa 3 métodos abstractos
- ✅ El handler base maneja automáticamente: obtención de datos, conteo, mapeo, creación de PaginationVm y manejo de excepciones
- ✅ Reducción de ~75 líneas a ~25 líneas (67% menos código)

**Alternativa sin Handler Base** (método tradicional):

Si prefieres no usar el handler base, puedes implementar el handler completo manualmente. Ver ejemplo en `GetPaginatedCategoriesQueryHandler` antes de la refactorización.

---

## Ejemplo Completo

Ver el ejemplo completo en:
- **Query**: `src/Core/Application/Features/Examples/Categories/Queries/GetPaginatedCategoriesQuery.cs`
- **Specification**: `src/Core/Application/Features/Examples/Categories/Queries/Specs/CategoryPaginationSpecification.cs`
- **Controller**: `src/Presentation/AppApi/Controllers/Examples/CategoriesController.cs`

---

## Uso en Controllers

```csharp
[HttpGet("pagination", Name = "productPaginate")]
[ProducesResponseType(typeof(PaginationVm<ProductVm>), (int)HttpStatusCode.OK)]
public async Task<ActionResult<PaginationVm<ProductVm>>> GetProductsPaginated(
    [FromQuery] GetPaginatedProductsQuery parameters)
{
    var dataResult = await _mediator.Send(parameters);
    return Ok(dataResult);
}
```

**Ejemplo de request**:
```
GET /api/products/pagination?pageIndex=1&pageSize=10&sort=nameAsc&search=laptop&categoryName=Electronics
```

**Ejemplo de response**:
```json
{
  "count": 45,
  "pageIndex": 1,
  "pageSize": 10,
  "pageCount": 5,
  "data": [
    {
      "id": "...",
      "name": "Laptop Dell",
      "price": 999.99
    },
    // ... más productos
  ]
}
```

---

## Parámetros de Paginación

### Parámetros Estándar (PaginationBase)

| Parámetro | Tipo | Descripción | Ejemplo |
|-----------|------|-------------|---------|
| `pageIndex` | int | Página actual (inicia en 1) | `?pageIndex=1` |
| `pageSize` | int | Registros por página (máx: 50) | `?pageSize=10` |
| `search` | string? | Búsqueda general | `?search=laptop` |
| `sort` | string? | Ordenamiento (ver abajo) | `?sort=nameAsc` |

### Formato de Sort

El parámetro `sort` sigue el formato: `campoDirección`

- `nameAsc`: Ordenar por nombre ascendente
- `nameDesc`: Ordenar por nombre descendente
- `priceAsc`: Ordenar por precio ascendente
- `priceDesc`: Ordenar por precio descendente

**Configuración en Specification**:
```csharp
var sortMappings = new Dictionary<string, Expression<Func<TestProduct, object>>>
{
    { "nameAsc", p => p.Name! },
    { "nameDesc", p => p.Name! },
    { "priceAsc", p => p.Price! },
    { "priceDesc", p => p.Price! }
};
```

### Parámetros Personalizados

Puedes agregar parámetros adicionales en tu Query:

```csharp
public class GetPaginatedProductsQuery : PaginationBase, IRequest<PaginationVm<ProductVm>>
{
    public string? CategoryName { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
```

**Uso**:
```
GET /api/products/pagination?pageIndex=1&pageSize=10&categoryName=Electronics&minPrice=100&maxPrice=500
```

---

## Mejores Prácticas

### ✅ Sí hacer:

1. **Usar ApplyPaging() helper**: Simplifica el código
```csharp
ApplyPaging(@params); // ✅ Correcto
```

2. **Crear specification para conteo**: Sin paginación para contar correctamente
```csharp
var specCount = new ProductForCountingSpecification(@params); // ✅ Correcto
```

3. **Validar PageSize**: El `PaginationBase` ya limita a 50, pero puedes validar más
```csharp
// PaginationBase ya valida máximo 50
```

4. **Usar Math.Ceiling para PageCount**: Redondea hacia arriba
```csharp
var totalPages = Convert.ToInt32(Math.Ceiling(totalRecords / (decimal)pageSize));
```

### ❌ No hacer:

1. **No aplicar paginación en specification de conteo**
```csharp
// ❌ Incorrecto
public ProductForCountingSpecification(...) {
    ApplyPaging(@params); // ❌ NO aplicar paginación aquí
}
```

2. **No olvidar calcular PageCount**
```csharp
// ❌ Incorrecto
var pagination = new PaginationVm<ProductVm> {
    // Falta PageCount
};
```

3. **No usar Skip/Take directamente**
```csharp
// ❌ Incorrecto
var skip = (pageIndex - 1) * pageSize;
var take = pageSize;
// Usar ApplyPaging() en su lugar
```

---

## Resumen Rápido

1. **Query**: Hereda de `PaginationBase`, implementa `IRequest<PaginationVm<T>>`
2. **SpecificationParams**: Hereda de `SpecificationParams`, incluye propiedades de filtro
3. **Specification**: Usa `ApplyPaging(@params)` para aplicar paginación
4. **SpecificationForCounting**: Mismos filtros, sin paginación
5. **Handler**: Crea ambas specifications, obtiene datos y conteo, calcula `PageCount`
6. **Controller**: Expone endpoint con `[FromQuery] GetPaginatedProductsQuery`

---

## 📚 Referencias

- **Ejemplo Completo**: `src/Core/Application/Features/Examples/Categories/Queries/GetPaginatedCategoriesQuery.cs`
- **Specification Pattern**: [docs/HERRAMIENTAS.md](HERRAMIENTAS.md#specification-pattern)
- **BaseSpecification**: `src/Infrastructure/Persistence/Specification/BaseSpecification.cs`

---

¿Tienes dudas? Revisa el ejemplo de **Categorías** que está completamente implementado.

