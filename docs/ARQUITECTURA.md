# Arquitectura del Proyecto

Esta documentación explica la arquitectura Clean Architecture utilizada en el proyecto y cómo están organizadas las capas.

## 🏗️ Clean Architecture

El proyecto sigue los principios de **Clean Architecture** (también conocida como Arquitectura Hexagonal o Onion Architecture), que separa el código en capas concéntricas con dependencias hacia el centro.

```
┌─────────────────────────────────────────┐
│         Presentation Layer               │  ← Controllers, Middleware, API
├─────────────────────────────────────────┤
│         Application Layer                │  ← Casos de Uso, DTOs, Validators
├─────────────────────────────────────────┤
│         Domain Layer                     │  ← Entidades, Interfaces, Lógica de Negocio
├─────────────────────────────────────────┤
│         Infrastructure Layer             │  ← EF Core, Repositorios, Servicios Externos
└─────────────────────────────────────────┘
```

## 📦 Capas del Proyecto

### 1. Domain Layer (`src/Core/Domain/`)

**Responsabilidad**: Contiene las entidades de negocio y las interfaces (contratos) que definen las operaciones necesarias.

**Componentes**:
- ✅ **Entidades**: Modelos de dominio (`TestProduct`, `TestCategory`)
- ✅ **Base Entities**: Clases base (`BaseEntity`, `AuditableEntity`)
- ✅ **Interfaces**: Contratos que definen servicios (`IApplicationDbContext`)

**Características**:
- ❌ **NO** depende de ninguna otra capa
- ✅ Contiene solo lógica de negocio pura
- ✅ Define contratos (interfaces) que otras capas implementan

**Ejemplo**:
```csharp
// Domain/Entities/Examples/TestProduct.cs
public class TestProduct : BaseEntity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    // Solo propiedades de negocio, sin dependencias externas
}
```

---

### 2. Application Layer (`src/Core/Application/`)

**Responsabilidad**: Contiene los casos de uso de la aplicación (CQRS), DTOs, validaciones y mapeo de objetos.

**Componentes**:
- ✅ **Features**: Commands y Queries (CQRS)
- ✅ **DTOs**: Objetos de transferencia de datos (`Result<T>`)
- ✅ **Validators**: Validaciones con FluentValidation
- ✅ **Mappings**: Configuración de AutoMapper
- ✅ **Behaviours**: Pipeline behaviors de MediatR (logging, validación, excepciones)

**Características**:
- ✅ Depende solo de `Domain`
- ✅ No conoce detalles de implementación (EF Core, SQL Server, etc.)
- ✅ Define **QUÉ** se hace, no **CÓMO** se hace

**Ejemplo**:
```csharp
// Application/Features/Examples/Products/Commands/CreateProductCommand.cs
public class CreateProductCommand : IRequest<Result<string>>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// El handler define QUÉ hacer, no CÓMO acceder a la BD
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<string>>
{
    // Usa interfaces del Domain, no implementaciones concretas
}
```

---

### 3. Infrastructure Layer (`src/Infrastructure/`)

**Responsabilidad**: Implementa los detalles técnicos: acceso a datos, servicios externos, autenticación, etc.

**Subcarpetas**:

#### 3.1 Persistence (`Infrastructure/Persistence/`)
- ✅ **DbContexts**: `ApplicationDbContext` (Entity Framework)
- ✅ **Repositories**: Implementaciones del patrón Repository
- ✅ **Unit of Work**: Gestión de transacciones
- ✅ **Migrations**: Migraciones de base de datos
- ✅ **Caching**: Servicios de caché (local y distribuido)

#### 3.2 Security (`Infrastructure/Security/`)
- ✅ **Identity**: Configuración de ASP.NET Core Identity
- ✅ **Services**: Servicios de autenticación (`AppAuthService`)
- ✅ **JWT**: Generación y validación de tokens

#### 3.3 Shared (`Infrastructure/Shared/`)
- ✅ **Exceptions**: Excepciones personalizadas
- ✅ **Extensions**: Extensiones de utilidad
- ✅ **Services**: Servicios compartidos (HTTP, Email, etc.)

**Características**:
- ✅ Implementa las interfaces definidas en `Domain`
- ✅ Conoce los detalles técnicos (EF Core, SQL Server, Redis, etc.)
- ✅ Se puede cambiar sin afectar `Domain` o `Application`

---

### 4. Presentation Layer (`src/Presentation/AppApi/`)

**Responsabilidad**: Expone la API REST, maneja requests HTTP, middlewares.

**Componentes**:
- ✅ **Controllers**: Endpoints de la API
- ✅ **Middleware**: Manejo de excepciones, correlation IDs, etc.
- ✅ **Authorization**: Atributos y estrategias de autorización
- ✅ **Configuration**: Configuración de servicios (DI, Serilog, etc.)

**Características**:
- ✅ Depende de `Application` y `Infrastructure`
- ✅ No contiene lógica de negocio
- ✅ Solo coordina y expone funcionalidades

---

## 🔄 Flujo de Datos

### Ejemplo: Crear un Producto

```
1. Cliente HTTP → ProductsController.Create()
2. Controller → MediatR.Send(CreateProductCommand)
3. MediatR → ValidationBehaviour (valida el command)
4. MediatR → CreateProductCommandHandler
5. Handler → UnitOfWork.Repository<Product>() (interfaz)
6. Repository → ApplicationDbContext (implementación)
7. DbContext → SQL Server (persistencia)
8. Handler → CacheService.RemoveAsync() (invalidar caché)
9. Handler → Result<string>.Success()
10. Controller → 201 Created (response)
```

### Puntos Clave

- ✅ El `Controller` solo recibe y retorna, no contiene lógica
- ✅ El `Handler` contiene la lógica de negocio, pero no conoce SQL Server
- ✅ El `Repository` abstrae el acceso a datos
- ✅ Las validaciones ocurren antes de llegar al handler

---

## 🎯 Principios de Diseño

### Dependency Inversion Principle (DIP)

Las capas externas dependen de abstracciones (interfaces) definidas en capas internas:

```csharp
// Domain define el contrato
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
}

// Infrastructure implementa el contrato
public class BaseRepository<T> : IGenericRepository<T>
{
    // Implementación con EF Core
}

// Application usa el contrato, no la implementación
public class GetProductHandler
{
    private readonly IGenericRepository<Product> _repository;
    // Depende de la interfaz, no de BaseRepository
}
```

### Single Responsibility Principle (SRP)

Cada clase tiene una única responsabilidad:

- ✅ `CreateProductCommandHandler`: Solo crea productos
- ✅ `CreateProductValidator`: Solo valida el command
- ✅ `ProductsController`: Solo expone endpoints HTTP
- ✅ `BaseRepository`: Solo accede a datos

### Open/Closed Principle (OCP)

El código está abierto para extensión, cerrado para modificación:

- ✅ Agregar nuevos handlers no requiere modificar código existente
- ✅ Agregar nuevos validators es independiente
- ✅ Nuevas entidades siguen el mismo patrón

---

## 🔌 Patrones Utilizados

### 1. CQRS (Command Query Responsibility Segregation)

Separa operaciones de lectura (Queries) de escritura (Commands):

```csharp
// Command: Modifica datos
public class CreateProductCommand : IRequest<Result<string>>
public class UpdateProductCommand : IRequest<Result<string>>
public class DeleteProductCommand : IRequest<Result<string>>

// Query: Lee datos
public class GetAllProductsQuery : IRequest<Result<IReadOnlyList<ProductVm>>>
public class GetProductByIdQuery : IRequest<Result<ProductVm>>
```

**Ventajas**:
- ✅ Separación clara de responsabilidades
- ✅ Optimización independiente (caché en queries, transacciones en commands)
- ✅ Escalabilidad (pueden ir a bases diferentes)

### 2. Repository Pattern

Abstrae el acceso a datos:

```csharp
// Interfaz (Domain)
public interface IGenericRepository<T>
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
}

// Implementación (Infrastructure)
public class BaseRepository<T> : IGenericRepository<T>
{
    // Implementación con EF Core
}
```

**Ventajas**:
- ✅ Fácil de testear (mock del repository)
- ✅ Cambiar de EF Core a Dapper sin afectar Application
- ✅ Centraliza lógica de acceso a datos

### 3. Unit of Work Pattern

Gestiona transacciones y coordinación de repositorios:

```csharp
public interface IUnitOfWork
{
    IGenericRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
```

**Ventajas**:
- ✅ Una transacción por operación
- ✅ Rollback automático en caso de error
- ✅ Coordinación de múltiples repositorios

### 4. Specification Pattern

Construye consultas complejas de forma declarativa:

```csharp
public class ProductPaginationSpecification : BaseSpecification<Product>
{
    public ProductPaginationSpecification(SpecificationParams specParams)
    {
        ApplyPaging(specParams);
        ApplySorting();
        // Filtros, includes, etc.
    }
}
```

**Ventajas**:
- ✅ Consultas reutilizables
- ✅ Separación de lógica de consulta
- ✅ Fácil de testear

---

## 🗄️ Acceso a Datos

### Entity Framework Core

El proyecto usa **EF Core** con Code First approach:

1. **Entidades** definidas en `Domain`
2. **Configuraciones** en `Infrastructure/Persistence/EntitiesConfigurations/`
3. **DbContext** en `Infrastructure/Persistence/DbContexts/`
4. **Migrations** generadas con `dotnet ef migrations add`

### Opciones de Base de Datos

#### SQL Server (Producción)
```json
{
  "UseInMemoryDatabase": false,
  "ConnectionStrings": {
    "ApplicationConnection": "Server=...;Database=..."
  }
}
```

#### InMemory (Desarrollo/Testing)
```json
{
  "UseInMemoryDatabase": true
}
```

---

## 🔐 Seguridad

### Autenticación JWT

1. **Login**: `POST /api/Auth/login` → Retorna JWT token
2. **Refresh**: `POST /api/Auth/refresh` → Renueva token
3. **Autorización**: Atributo `[CustomAuthorize]` en controllers

### Flujo de Autenticación

```
1. Cliente → POST /api/Auth/login
2. AppAuthService → Valida credenciales (Identity o AD)
3. AppAuthService → Genera JWT token
4. AppAuthService → Guarda refresh token en BD
5. Retorna token al cliente
6. Cliente incluye token en header: Authorization: Bearer <token>
7. Middleware valida token en cada request
```

---

## 📝 Logging

El proyecto usa **Serilog** para logging estructurado:

- ✅ Logs en consola (desarrollo)
- ✅ Logs en archivo (producción) con rotación diaria
- ✅ Correlation IDs para rastrear requests
- ✅ Niveles configurables por namespace

---

## 🎯 Mejores Prácticas

1. ✅ **Siempre usa interfaces** en lugar de implementaciones concretas
2. ✅ **Una responsabilidad por clase**
3. ✅ **Dependencias hacia adentro** (externa → interna)
4. ✅ **Tests unitarios** para lógica de negocio
5. ✅ **Validación** antes de llegar al handler
6. ✅ **Logging estructurado** en operaciones importantes
7. ✅ **Manejo centralizado de excepciones**
8. ✅ **Result<T>** para respuestas consistentes

---

## 📚 Recursos

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microsoft - Architecture e-books](https://docs.microsoft.com/en-us/dotnet/architecture/)

---

¿Necesitas más detalles? Consulta los ejemplos de **Productos** y **Categorías** en el código.

