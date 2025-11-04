# Estructura de Carpetas del Proyecto - Clean Architecture

## Estructura Actual vs. Recomendada

### Estructura Actual ✅ (Correcta y Recomendada)

```
CleanArchitectureNet8/
├── src/                          # Código fuente de producción
│   ├── Core/
│   │   ├── Application/
│   │   └── Domain/
│   ├── Infrastructure/
│   │   ├── Persistence/
│   │   ├── Security/
│   │   └── Shared/
│   └── Presentation/
│       └── AppApi/
├── tests/                        # Tests al mismo nivel que src/ ✅
│   └── Tests/
│       ├── Application/
│       ├── Persistence/
│       ├── Security/
│       └── ...
├── docs/                         # Documentación
├── tools/                        # Scripts y herramientas
└── CleanArchitectureNet8.sln     # Solution file
```

### ¿Por qué esta estructura es correcta?

1. **Separación clara**: `src/` contiene código de producción, `tests/` contiene código de pruebas
2. **Convención estándar de .NET**: Esta es la estructura más común en proyectos .NET y es utilizada por:
   - Microsoft en sus repositorios oficiales
   - Proyectos open source populares (e.g., eShopOnContainers)
   - Templates oficiales de Visual Studio
3. **Mejores prácticas**:
   - Los tests no se empaquetan con el código de producción
   - Separación física clara entre producción y pruebas
   - Facilita el uso de herramientas de CI/CD

## Estructura Alternativa (Menos Común)

### Opción: Tests dentro de src/

```
CleanArchitectureNet8/
├── src/
│   ├── Core/
│   ├── Infrastructure/
│   ├── Presentation/
│   └── Tests/                    # ⚠️ Menos común pero válido
│       └── Tests.csproj
└── ...
```

**Pros:**

- Todo el código (producción + tests) está dentro de `src/`
- Puede ser útil si todos los tests son parte del "código fuente"

**Contras:**

- Menos común, puede confundir a nuevos desarrolladores
- No sigue las convenciones más extendidas de .NET
- Puede ser más difícil filtrar en CI/CD

## Recomendación Final ✅

**MANTENER la estructura actual** (`tests/` al mismo nivel que `src/`)

Esta es la estructura **estándar y recomendada** para proyectos .NET con Clean Architecture porque:

1. ✅ Sigue las convenciones de Microsoft y la comunidad .NET
2. ✅ Separación clara entre producción y pruebas
3. ✅ Facilita herramientas de CI/CD (pueden excluir `tests/` fácilmente)
4. ✅ Estructura familiar para desarrolladores .NET
5. ✅ Compatible con templates oficiales

## Mejora Sugerida: Organización Interna de Tests

Aunque la ubicación de `tests/` es correcta, podríamos mejorar la organización interna para que refleje mejor la estructura de `src/`:

### Estructura Actual de Tests:

```
tests/
└── Tests/
    ├── Application/          # Tests de Application layer
    ├── Persistence/          # Tests de Infrastructure/Persistence
    ├── Security/             # Tests de Infrastructure/Security
    └── ...
```

### Estructura Mejorada (Opcional):

```
tests/
└── Tests/
    ├── Application/          # Tests de Application layer
    │   ├── Handlers/
    │   ├── Validators/
    │   └── ...
    ├── Infrastructure/        # Tests de Infrastructure layer
    │   ├── Persistence/
    │   │   ├── Caching/
    │   │   ├── Repositories/
    │   │   └── ...
    │   └── Security/
    │       ├── Services/
    │       └── ...
    ├── Presentation/          # Tests de Presentation layer (si es necesario)
    └── Helpers/               # Helpers compartidos
```

**Nota**: La estructura actual de tests también es válida. La sugerencia anterior es solo para mantener mayor coherencia con la estructura de `src/`.

## Referencias

- [Microsoft .NET Solution Structure](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-sln)
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [eShopOnContainers - Example Structure](https://github.com/dotnet-architecture/eShopOnContainers)
