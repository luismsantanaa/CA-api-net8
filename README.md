# Clean Architecture .NET 8 Template

Plantilla base para desarrollo de APIs RESTful usando Clean Architecture, .NET 8, Entity Framework Core y ASP.NET Core.

## 📋 Tabla de Contenidos

- [Características](#características)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Requisitos Previos](#requisitos-previos)
- [Inicio Rápido](#inicio-rápido)
- [Guías de Desarrollo](#guías-de-desarrollo)
- [Arquitectura](#arquitectura)
- [Herramientas y Tecnologías](#herramientas-y-tecnologías)

## 🎯 Características

Este proyecto incluye una arquitectura limpia con las siguientes características:

- ✅ **Clean Architecture** - Separación clara de capas (Domain, Application, Infrastructure, Presentation)
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

## 📁 Estructura del Proyecto

```
CleanArchitectureNet8/
├── src/
│   ├── Core/
│   │   ├── Domain/              # Entidades y lógica de negocio
│   │   └── Application/         # Casos de uso y DTOs
│   ├── Infrastructure/
│   │   ├── Persistence/         # Entity Framework, Repositorios
│   │   ├── Security/           # Identity, JWT
│   │   └── Shared/             # Servicios compartidos
│   └── Presentation/
│       └── AppApi/             # API REST (Controllers, Middleware)
├── tests/
│   └── Tests/                  # Proyecto de pruebas unitarias
├── docs/                       # Documentación técnica
└── tools/                      # Scripts y herramientas
```

> 📖 Para más detalles sobre la estructura, consulta [docs/ESTRUCTURA_COMPLETA.md](docs/ESTRUCTURA_COMPLETA.md)

## 🔧 Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) o configuración para InMemory Database
- [Redis](https://redis.io/download) (opcional, para caché distribuido)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/)

## 🚀 Inicio Rápido

### 1. Clonar y Configurar

```bash
# Clonar el repositorio
git clone <repository-url>
cd CleanArchitectureNet8

# Copiar archivo de configuración
cp src/Presentation/AppApi/appsettings.json.example src/Presentation/AppApi/appsettings.json
```

### 2. Configurar Base de Datos

Edita `src/Presentation/AppApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ApplicationConnection": "Server=localhost;Database=TuBaseDatos;User Id=sa;Password=TuPassword;",
    "IdentityConnection": "Server=localhost;Database=TuBaseDatos;User Id=sa;Password=TuPassword;"
  },
  "UseInMemoryDatabase": false
}
```

**Opciones de Base de Datos:**

- **SQL Server**: Configura `UseInMemoryDatabase: false` y la connection string
- **InMemory (Desarrollo)**: Configura `UseInMemoryDatabase: true` (no requiere SQL Server)

### 3. Configurar JWT

En `appsettings.json`, configura tus credenciales JWT:

```json
{
  "JwtSettings": {
    "Key": "TuClaveSecretaBase64",
    "Issuer": "tu.app.name",
    "Audience": "tu.domain.com",
    "DurationInMinutes": 360,
    "ExpireTime": "06:00:00"
  }
}
```

> ⚠️ **Importante**: Genera una clave segura. Puedes usar: `Convert.ToBase64String(Encoding.UTF8.GetBytes("tu-clave-super-secreta"))`

### 4. Ejecutar el Proyecto

```bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar migraciones (si usas SQL Server)
dotnet ef database update --project src/Infrastructure/Persistence --startup-project src/Presentation/AppApi

# Ejecutar la API
dotnet run --project src/Presentation/AppApi
```

La API estará disponible en: `https://localhost:5001` o `http://localhost:5000`

### 5. Verificar que Funciona

```bash
# Health Check
curl https://localhost:5001/health

# Swagger UI
# Abre en el navegador: https://localhost:5001/swagger
```

## 📚 Guías de Desarrollo

### Crear un Nuevo Feature (CRUD Completo)

El proyecto incluye ejemplos completos de **Productos** y **Categorías**. Sigue estos pasos para crear tu propio feature:

1. **[Crear una Nueva Entidad](docs/GUIA_DESARROLLO.md#1-crear-una-nueva-entidad)**
2. **[Crear Commands (CQRS)](docs/GUIA_DESARROLLO.md#2-crear-commands-cqrs)**
3. **[Crear Queries (CQRS)](docs/GUIA_DESARROLLO.md#3-crear-queries-cqrs)**
4. **[Crear Validators](docs/GUIA_DESARROLLO.md#4-crear-validators)**
5. **[Crear View Models](docs/GUIA_DESARROLLO.md#5-crear-view-models)**
6. **[Crear Controllers](docs/GUIA_DESARROLLO.md#6-crear-controllers)**
7. **[Configurar AutoMapper](docs/GUIA_DESARROLLO.md#7-configurar-automapper)**

> 📖 **Guía Completa**: Consulta [docs/GUIA_DESARROLLO.md](docs/GUIA_DESARROLLO.md) para el paso a paso detallado.

### Ejemplo: Feature de Productos

El proyecto incluye un ejemplo completo de un CRUD de Productos. Puedes usarlo como referencia:

- **Entidad**: `src/Core/Domain/Entities/Examples/TestProduct.cs`
- **Commands**: `src/Core/Application/Features/Examples/Products/Commands/`
- **Queries**: `src/Core/Application/Features/Examples/Products/Queries/`
- **Controller**: `src/Presentation/AppApi/Controllers/Examples/ProductsController.cs`

## 🏗️ Arquitectura

### Clean Architecture - Capas

```
┌─────────────────────────────────────┐
│      Presentation (AppApi)          │  ← Controllers, Middleware
├─────────────────────────────────────┤
│      Application                    │  ← Casos de Uso, DTOs
├─────────────────────────────────────┤
│      Domain                         │  ← Entidades, Interfaces
├─────────────────────────────────────┤
│      Infrastructure                 │  ← EF Core, Repositorios, Servicios
└─────────────────────────────────────┘
```

**Principios:**

- **Dependencias hacia adentro**: Las capas externas dependen de las internas
- **Domain es independiente**: No depende de ninguna otra capa
- **Interfaces en Domain**: Los contratos se definen en Domain, implementaciones en Infrastructure

> 📖 Para más detalles, consulta [docs/ARQUITECTURA.md](docs/ARQUITECTURA.md)

## 🛠️ Herramientas y Tecnologías

| Categoría         | Tecnología                   | Propósito                        |
| ----------------- | ---------------------------- | -------------------------------- |
| **Framework**     | .NET 8                       | Framework principal              |
| **ORM**           | Entity Framework Core 8      | Acceso a datos                   |
| **Patrón**        | CQRS + MediatR               | Separación de comandos/consultas |
| **Validación**    | FluentValidation             | Validación de requests           |
| **Mapeo**         | AutoMapper                   | Mapeo de objetos                 |
| **Autenticación** | JWT + ASP.NET Core Identity  | Seguridad                        |
| **Logging**       | Serilog                      | Logging estructurado             |
| **Caché**         | Memory Cache / Redis         | Optimización                     |
| **Testing**       | xUnit, Moq, FluentAssertions | Pruebas unitarias                |
| **Documentación** | Swagger/OpenAPI              | Documentación de API             |

> 📖 Para más detalles sobre cada herramienta, consulta [docs/HERRAMIENTAS.md](docs/HERRAMIENTAS.md)

## 📖 Documentación Adicional

- [Guía de Desarrollo Completa](docs/GUIA_DESARROLLO.md) - Cómo crear nuevos features
- [Guía de Paginación](docs/PAGINACION.md) - Implementación completa de paginación
- [Arquitectura Detallada](docs/ARQUITECTURA.md) - Explicación de capas y principios
- [Herramientas y Tecnologías](docs/HERRAMIENTAS.md) - Detalles de cada herramienta
- [Estructura del Proyecto](docs/ESTRUCTURA_COMPLETA.md) - Organización de carpetas
- [Ejemplos y Mejores Prácticas](docs/EJEMPLOS.md) - Ejemplos de código y patrones
- [Resumen de Mejoras](docs/RESUMEN_MEJORAS.md) - Resumen ejecutivo de mejoras y helpers disponibles

## 🔐 Autenticación

### Crear Usuario de Prueba

Si usas **InMemory Database**, el sistema crea automáticamente un usuario de prueba:

- **Email**: `testuser@test.com`
- **Password**: `TestPassword123!`

### Generar Token JWT

```bash
POST /api/Auth/login
{
  "email": "testuser@test.com",
  "password": "TestPassword123!"
}
```

Respuesta:

```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "..."
}
```

### Usar el Token

Incluye el token en el header `Authorization`:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

## 🧪 Testing

### Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Tests específicos
dotnet test --filter "FullyQualifiedName~Products"

# Con cobertura
dotnet test /p:CollectCoverage=true
```

### Estructura de Tests

```
tests/Tests/
├── Application/        # Tests de handlers y validators
├── Infrastructure/     # Tests de servicios
├── Presentation/       # Tests de controllers
└── Helpers/            # Utilidades para tests
```

## 📝 Convenciones de Código

- **Nombres de clases**: PascalCase (`ProductController`)
- **Nombres de métodos**: PascalCase (`GetProductById`)
- **Nombres de variables**: camelCase (`productId`)
- **Interfaces**: Prefijo `I` (`IRepository`)
- **DTOs/ViewModels**: Sufijo descriptivo (`ProductVm`, `CreateProductCommand`)
- **Tests**: Sufijo `Tests` (`ProductControllerTests`)

## 🤝 Contribuir

Este es un template base. Para adaptarlo a tu proyecto:

1. Reemplaza los ejemplos (Productos, Categorías) con tus entidades
2. Configura tus connection strings y settings
3. Ajusta los nombres y namespaces según tu dominio
4. Personaliza la autenticación según tus necesidades

## 📄 Licencia

Este proyecto es un template base. Úsalo libremente para tus proyectos.

## 🙋 Soporte

Si tienes dudas sobre cómo implementar algo:

1. Revisa los **ejemplos** en `src/Core/Application/Features/Examples/`
2. Consulta la **documentación** en `docs/`
3. Revisa los **tests** en `tests/Tests/` para ver ejemplos de uso

---

**Nota**: Los ejemplos de Productos y Categorías están incluidos solo como referencia. Elimínalos cuando implementes tus propias entidades de negocio.
