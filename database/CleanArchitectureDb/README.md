# CleanArchitectureDb - SQL Server Database Project

Este proyecto contiene la definición completa de la base de datos para la aplicación CleanArchitecture, incluyendo esquema, tablas, índices y datos iniciales.

## 🏗️ Estructura

```
CleanArchitectureDb/
├── Tables/
│   ├── Shared/                    # Tablas de datos compartidos
│   │   ├── AuditLogs.sql
│   │   ├── MailNotificationTemplate.sql
│   │   └── UploadedFile.sql
│   ├── Examples/                  # Tablas de ejemplo
│   │   ├── TestCategories.sql
│   │   └── TestProducts.sql
│   └── Security/                  # Tablas de seguridad e identidad
│       ├── AspNetUsers.sql
│       ├── AspNetRoles.sql
│       ├── AspNetUserRoles.sql
│       ├── AspNetUserClaims.sql
│       ├── AspNetUserLogins.sql
│       ├── AspNetUserTokens.sql
│       ├── AspNetRoleClaims.sql
│       ├── RefreshTokens.sql
│       └── AppUsers.sql
└── Scripts/
    └── PostDeployment/           # Scripts de datos iniciales
        ├── Script.PostDeployment.sql
        ├── SeedSharedData.sql
        ├── SeedExampleData.sql
        └── SeedSecurityData.sql
```

## 📋 Esquemas de Base de Datos

### **Shared**
Contiene tablas de infraestructura y datos compartidos:
- `AuditLogs`: Registro de auditoría de cambios en la base de datos
- `MailNotificationTemplate`: Plantillas de notificaciones por correo
- `UploadedFile`: Registro de archivos subidos al sistema

### **Example**
Contiene tablas de ejemplo para demostración:
- `TestCategories`: Categorías de productos de ejemplo
- `TestProduct`: Productos de ejemplo con relación a categorías

### **Security**
Contiene tablas de autenticación y autorización (ASP.NET Core Identity):
- `AspNetUsers`: Usuarios del sistema
- `AspNetRoles`: Roles de usuario
- `AspNetUserRoles`: Relación usuarios-roles
- `AspNetUserClaims`: Claims de usuarios
- `AspNetUserLogins`: Logins externos (Google, Azure AD, etc.)
- `AspNetUserTokens`: Tokens de autenticación
- `AspNetRoleClaims`: Claims de roles
- `RefreshTokens`: Tokens de refresh JWT personalizados
- `AppUsers`: Información extendida de usuarios (perfil de empleado)

## 🚀 Despliegue

### Opción 1: Desde Visual Studio (Recomendado)

1. **Abrir el proyecto** en Visual Studio
2. **Clic derecho** en el proyecto `CleanArchitectureDb`
3. Seleccionar **"Publish..."**
4. Configurar la conexión a tu servidor SQL Server
5. Click en **"Publish"**

El despliegue incluirá:
- ✅ Creación de esquemas `Shared` y `Example`
- ✅ Creación de todas las tablas con sus índices
- ✅ Ejecución de scripts PostDeployment con datos iniciales
- ✅ Comparación de esquemas (solo aplica cambios necesarios)

### Opción 2: Comparación de Esquemas

Visual Studio permite comparar el esquema del proyecto con una base de datos existente:

1. **Tools** → **SQL Server** → **New Schema Comparison**
2. **Source**: Selecciona el proyecto `CleanArchitectureDb`
3. **Target**: Selecciona tu base de datos SQL Server
4. **Compare** para ver diferencias
5. **Update** para aplicar cambios

### Opción 3: Generar Script SQL

Si prefieres revisar el script antes de ejecutarlo:

1. **Clic derecho** en el proyecto → **"Publish..."**
2. En el diálogo, click en **"Generate Script"**
3. Revisa el script generado
4. Ejecútalo manualmente en SQL Server Management Studio

## 🔧 Configuración

### Perfil de Publicación

Puedes crear perfiles de publicación (`.publish.xml`) para diferentes ambientes:

- `Development.publish.xml` → Base de datos local
- `Testing.publish.xml` → Base de datos de pruebas
- `Production.publish.xml` → Base de datos de producción

**Ejemplo de perfil Development:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <TargetDatabaseName>CleanArchitectureDb</TargetDatabaseName>
    <TargetConnectionString>Server=localhost,11433;Database=CleanArchitectureDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;</TargetConnectionString>
    <ProfileVersionNumber>1</ProfileVersionNumber>
  </PropertyGroup>
</Project>
```

## 📊 Scripts PostDeployment

Los scripts de PostDeployment se ejecutan **cada vez** que publicas el proyecto:

### `SeedSharedData.sql`
Carga datos maestros compartidos:
- Template de notificación de correo de prueba

### `SeedExampleData.sql`
Carga datos de ejemplo para demostración:
- 7 categorías de productos
- 23 productos de ejemplo con relaciones a categorías

### `SeedSecurityData.sql`
Script de referencia para datos de seguridad:
- Contiene ejemplo comentado para crear roles por defecto
- **IMPORTANTE**: Para crear usuarios, se recomienda usar `IdentitySeedData.cs` desde C# debido al hashing de contraseñas de Identity

**Nota:** Los scripts usan `MERGE` o validaciones `IF NOT EXISTS` para evitar duplicados.

## 🎯 Ventajas de usar SQL Server Database Project

✅ **Control de versiones**: Todo el esquema de base de datos está en Git  
✅ **Validación en diseño**: Detecta errores de sintaxis antes de desplegar  
✅ **Comparación de esquemas**: Identifica diferencias automáticamente  
✅ **Deployment seguro**: Solo aplica cambios necesarios  
✅ **Rollback fácil**: Puedes volver a versiones anteriores del esquema  
✅ **CI/CD friendly**: Integrable con pipelines de Azure DevOps  
✅ **Refactoring**: Puedes renombrar objetos y actualiza referencias automáticamente  

## 🔄 Workflow Recomendado

1. **Desarrollo**: Modifica las tablas en el proyecto SQL
2. **Build**: Compila el proyecto para validar sintaxis
3. **Publish**: Publica en base de datos de desarrollo
4. **Test**: Verifica que todo funciona correctamente
5. **Commit**: Sube cambios a Git
6. **CI/CD**: El pipeline despliega automáticamente en Testing/Production

## ⚠️ Consideraciones

- **Datos existentes**: El despliegue respeta datos existentes, solo modifica esquema
- **PostDeployment**: Los scripts de seed se ejecutan en cada publicación
- **Backups**: Siempre haz backup antes de desplegar en producción
- **Passwords**: Nunca subas archivos `.publish.xml` con contraseñas a Git

## 🛠️ Troubleshooting

### Error: "Unable to connect to server"
- Verifica que SQL Server esté ejecutándose
- Revisa la cadena de conexión en el perfil de publicación
- Asegúrate de tener permisos en la base de datos

### Error: "Database already exists"
- El proyecto puede actualizar bases de datos existentes
- Usa "Compare Schema" para ver qué cambiará

### Scripts PostDeployment no se ejecutan
- Verifica que estén marcados como "PostDeploy" en el `.sqlproj`
- Revisa el archivo `Script.PostDeployment.sql` que los incluye con `:r`

## 📚 Recursos

- [SQL Server Data Tools Documentation](https://docs.microsoft.com/sql/ssdt/)
- [Database Projects in Visual Studio](https://docs.microsoft.com/visualstudio/data-tools/)
- [SQLCMD Scripting](https://docs.microsoft.com/sql/tools/sqlcmd-utility)

---

**Última actualización**: Noviembre 2024  
**Versión SDK**: .NET 9.0  
**SQL Server Version**: 2019+ (compatible con Azure SQL Database)

