# Tests Project

Este proyecto contiene las pruebas unitarias e integración para la API CleanArchitectureNet8.

## Estructura del Proyecto

```
Tests/
├── Authorization/
│   └── JwtUtilsTests.cs          # Tests para validación y procesamiento de JWT tokens
├── Services/
│   └── GetUserServiceTests.cs    # Tests para servicio de obtención de usuario
├── Application/
│   └── Handlers/
│       └── CreateProductCommandHandlerTests.cs  # Tests para handlers de MediatR
└── Helpers/
    └── TestFixture.cs            # Helpers y fixtures para facilitar testing
```

## Componentes Testeados

### 1. **JwtUtils (Autenticación/Login)**
- ✅ Validación de tokens válidos
- ✅ Manejo de tokens nulos/vacíos
- ✅ Validación de tokens expirados
- ✅ Extracción de claims (uid, email, etc.)
- ✅ Manejo de tokens con firma inválida
- ✅ Validación de tokens sin claim uid

### 2. **GetUserService (Usuario)**
- ✅ Extracción de UserId desde JWT
- ✅ Verificación de autenticación
- ✅ Manejo de HttpContext nulo
- ✅ Manejo de headers de autorización faltantes
- ✅ Validación de formato de UserId
- ✅ Evaluación lazy (solo se inicializa una vez)
- ✅ Manejo de excepciones

### 3. **Handlers de MediatR**
- ✅ CreateProductCommandHandler
  - Creación exitosa de productos
  - Validación de nombres duplicados
  - Invalidación de caché

## Ejecutar Tests

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests con cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Ejecutar tests específicos
dotnet test --filter "FullyQualifiedName~JwtUtilsTests"
```

## Dependencias

- **xUnit**: Framework de testing
- **Moq**: Mocking framework
- **FluentAssertions**: Assertions más legibles
- **AutoFixture**: Generación automática de datos de prueba
- **Microsoft.EntityFrameworkCore.InMemory**: Base de datos en memoria para tests

## Próximos Tests a Implementar

1. **AppAuthService** - Tests para login completo
2. **Validators** - Tests para FluentValidation
3. **CacheKeyService** - Tests para generación de claves de caché
4. **Repository Tests** - Tests para repositorios con InMemory database
5. **Integration Tests** - Tests de endpoints completos

## Notas

- Todos los tests están enfocados en la funcionalidad crítica de login y usuario como se solicitó
- Los tests usan mocks para aislar las dependencias
- Se incluyen casos de éxito y error para asegurar robustez

