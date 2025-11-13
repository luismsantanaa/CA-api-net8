# Actualización a .NET 9 y EF Core 9

## Fecha: 8 de Noviembre de 2025 - 17:52

## ✅ Actualizaciones Realizadas

### 1. SDK y Framework
```json
// global.json
{
  "sdk": {
    "version": "9.0.306",
    "rollForward": "latestFeature"
  }
}
```

### 2. Target Framework
Todos los proyectos actualizados de `net8.0` → `net9.0`:
- ✅ Domain.csproj
- ✅ Application.csproj  
- ✅ Shared.csproj
- ✅ Persistence.csproj
- ✅ Security.csproj
- ✅ AppApi.csproj
- ✅ Tests.csproj

### 3. Paquetes Actualizados

#### Entity Framework Core: 8.0.11 → 9.0.0
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
```

#### Microsoft.AspNetCore: 8.0.x → 9.0.0
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0" />
```

#### Microsoft.Extensions: 8.0.x → 9.0.0
```xml
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
```

#### System: 8.0.x → 9.0.0
```xml
<PackageReference Include="System.DirectoryServices" Version="9.0.0" />
<PackageReference Include="System.DirectoryServices.AccountManagement" Version="9.0.0" />
```

#### Tokens JWT: 8.0.2 → 8.2.1
```xml
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.2.1" />
```

### 4. Cambios en Referencias

**Reemplazamos referencias de paquetes con FrameworkReference:**

```xml
<!-- Aplicado en Application.csproj y Shared.csproj -->
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

Esto elimina la necesidad de referencias explícitas a:
- `Microsoft.AspNetCore.Http.Features` (ahora incluido en el framework)

## ✅ Estado de Compilación

**Compilación: EXITOSA**
```
Build succeeded.
    4 Warning(s)
    0 Error(s)
Time Elapsed 00:00:13.41
```

## ⚠️ RESULTADO CON SQL SERVER

**EL BUG DE EF CORE PERSISTE EN .NET 9.0 / EF CORE 9.0.0**

```
System.NullReferenceException: Object reference not set to an instance of an object.
   at Microsoft.EntityFrameworkCore.Storage.RelationalTypeMappingSource.FindCollectionMapping()
```

### Conclusión Importante

El bug de `FindCollectionMapping` **NO está relacionado con la versión de .NET o EF Core**. El problema es estructural y está relacionado con cómo EF Core maneja las propiedades de navegación de colección en las entidades `TestCategory_DISABLED` y `TestProduct_DISABLED`.

## ✅ FUNCIONA CORRECTAMENTE CON INMEMORY

```
[17:51:25 INF] Datos iniciales cargados exitosamente en la base de datos en memoria.
[17:51:25 INF] Usuario de prueba cargado exitosamente.
[17:51:25 INF] HTTP GET /health responded 200 in 71.6626 ms
```

**Credenciales de prueba:**
- Email: `test@mardom.com`
- Username: `testuser`
- Password: `Test123!@#`

## 🎯 Próximos Pasos

### Opción 1: Usar InMemory Database (Recomendado para desarrollo)
```json
"UseInMemoryDatabase": true
```
✅ Funciona perfectamente
✅ Ideal para desarrollo y pruebas rápidas

### Opción 2: Eliminar Completamente las Entidades de Ejemplo
Para usar SQL Server en producción, es necesario:
1. Eliminar los archivos de entidades:
   - `TestCategory_DISABLED.cs`
   - `TestProduct_DISABLED.cs`
2. Eliminar todas las referencias
3. Eliminar configuraciones y seed data

### Opción 3: Rediseñar las Relaciones
Cambiar las relaciones bidireccionales a unidireccionales:
- Eliminar `ICollection<TestProduct_DISABLED>` de `TestCategory_DISABLED`
- Mantener solo `TestCategory_DISABLED` en `TestProduct_DISABLED`

## 📊 Resumen de Beneficios de .NET 9

Aunque el bug específico persiste, la actualización a .NET 9 trae:

✅ **Mejor rendimiento general**
✅ **Mejoras en el runtime**
✅ **Nuevas características del lenguaje C# 13**
✅ **Soporte a largo plazo (LTS)**
✅ **Última versión de EF Core con mejoras de rendimiento**
✅ **Mejoras en ASP.NET Core**

## 🔧 Comandos Útiles

### Verificar versión actual
```bash
dotnet --version
# Muestra: 9.0.306
```

### Ejecutar aplicación
```bash
dotnet run --project src/Presentation/AppApi/AppApi.csproj
```

### Limpiar y recompilar
```bash
dotnet clean
dotnet restore
dotnet build
```

## ⚠️ Advertencias

1. **El bug de FindCollectionMapping sigue sin resolverse** en EF Core 9.0.0
2. Las entidades de ejemplo siguen deshabilitadas (`*_DISABLED`)
3. SQL Server no funcionará hasta que se resuelva el problema de las entidades

## 📌 Recomendación Final

**Para desarrollo:** Usar `UseInMemoryDatabase: true` ✅

**Para producción:** 
- Opción A: Eliminar las entidades de ejemplo completamente
- Opción B: Esperar a futuras versiones de EF Core
- Opción C: Rediseñar las relaciones para evitar colecciones de navegación

---
**Última actualización:** 8 de Noviembre de 2025, 17:52  
**Versión:** .NET 9.0.306 | EF Core 9.0.0

