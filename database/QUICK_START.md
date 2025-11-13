# 🚀 Quick Start - SQL Server Database Project

## ✅ Verificar Requisitos

Antes de empezar, asegúrate de tener:
- ✅ Visual Studio 2022 (Community, Professional o Enterprise)
- ✅ SQL Server Data Tools (SSDT) instalado
- ✅ SQL Server ejecutándose (local o Docker)

### Instalar SSDT si no lo tienes

1. Abre **Visual Studio Installer**
2. Click en **"Modify"** en tu instalación de VS
3. En la pestaña **"Individual components"**
4. Busca y marca: **"SQL Server Data Tools"**
5. Click **"Modify"** para instalar

## 📂 Paso 1: Abrir la Solución

1. Abre `CleanArchitectureNet8.sln` en Visual Studio
2. En el **Solution Explorer**, verás la carpeta `database`
3. Expande para ver el proyecto `CleanArchitectureDb`

## 🔧 Paso 2: Configurar Perfil de Publicación

### Opción A: Crear perfil desde Visual Studio

1. **Click derecho** en `CleanArchitectureDb` → **Publish...**
2. En el diálogo, click **"Edit..."** junto a Target database connection
3. Configura tu conexión:
   ```
   Server: localhost,11433
   Authentication: SQL Server Authentication
   Username: sa
   Password: [tu contraseña]
   Database: CleanArchitectureDb
   ```
4. Click **"Test Connection"** para verificar
5. Click **"OK"**
6. **Opcional**: Click **"Save Profile As..."** para guardar como `Development.publish.xml`

### Opción B: Usar el archivo de ejemplo

1. Navega a `database/CleanArchitectureDb/`
2. Copia `Development.publish.xml.example` como `Development.publish.xml`
3. Edita `Development.publish.xml` y actualiza la contraseña:
   ```xml
   <TargetConnectionString>Server=localhost,11433;Database=CleanArchitectureDb;User Id=sa;Password=TU_PASSWORD_AQUI;TrustServerCertificate=True;</TargetConnectionString>
   ```

## 🚀 Paso 3: Publicar Base de Datos

### Desde Visual Studio

1. **Click derecho** en `CleanArchitectureDb` → **Publish...**
2. Si guardaste un perfil, selecciónalo en el dropdown
3. **Opcional**: Click **"Generate Script"** para revisar el SQL antes de aplicar
4. Click **"Publish"**
5. Espera a que termine (verás el progreso en "Data Tools Operations")

### Desde línea de comandos (SqlPackage)

```bash
# Compilar el proyecto
dotnet build database/CleanArchitectureDb/CleanArchitectureDb.sqlproj

# Publicar con SqlPackage
SqlPackage.exe /Action:Publish \
  /SourceFile:database/CleanArchitectureDb/bin/Debug/CleanArchitectureDb.dacpac \
  /TargetServerName:localhost,11433 \
  /TargetDatabaseName:CleanArchitectureDb \
  /TargetUser:sa \
  /TargetPassword:YourPassword123!
```

## ✨ Paso 4: Verificar Instalación

### Desde SQL Server Management Studio (SSMS)

```sql
-- Ver esquemas creados
SELECT * FROM sys.schemas WHERE name IN ('Shared', 'Example')

-- Ver tablas creadas
SELECT TABLE_SCHEMA, TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA IN ('Shared', 'Example')

-- Verificar datos de ejemplo
SELECT COUNT(*) AS TotalCategorias FROM [Example].[TestCategories]
SELECT COUNT(*) AS TotalProductos FROM [Example].[TestProduct]
SELECT COUNT(*) AS TotalTemplates FROM [Shared].[MailNotificationTemplate]
```

**Resultado esperado:**
- ✅ 7 categorías
- ✅ 23 productos
- ✅ 1 template de notificación

### Desde Azure Data Studio

1. Conecta a tu servidor SQL
2. Expande `Databases` → `CleanArchitectureDb`
3. Verifica que existan los esquemas `Shared` y `Example`
4. Verifica las tablas en cada esquema

## 🔄 Actualizar Base de Datos Existente

Si ya tienes una base de datos y quieres aplicar cambios:

1. **Click derecho** en `CleanArchitectureDb` → **Schema Compare...**
2. **Source**: Selecciona el proyecto `CleanArchitectureDb`
3. **Target**: Selecciona tu base de datos
4. Click **"Compare"**
5. Revisa las diferencias
6. Click **"Update"** para aplicar solo los cambios necesarios

## 🆘 Problemas Comunes

### Error: "Unable to connect to target server"

**Solución:**
```bash
# Verificar que SQL Server esté corriendo
docker ps

# Si no está corriendo, iniciarlo
docker-compose up -d mssql

# Probar conexión
sqlcmd -S localhost,11433 -U sa -P YourPassword123!
```

### Error: "Project build failed"

**Solución:**
1. Limpia la solución: **Build** → **Clean Solution**
2. Reconstruye: **Build** → **Rebuild Solution**
3. Verifica que no haya errores de sintaxis SQL en los archivos `.sql`

### Scripts PostDeployment no se ejecutan

**Solución:**
1. Verifica que `Script.PostDeployment.sql` esté marcado como **"PostDeploy"** en las propiedades
2. Verifica que los otros scripts estén marcados como **"None"**
3. En el `.sqlproj`, busca:
   ```xml
   <PostDeploy Include="Scripts\PostDeployment\Script.PostDeployment.sql" />
   <None Include="Scripts\PostDeployment\SeedSharedData.sql" />
   ```

### Datos duplicados en cada deployment

Esto es normal. Los scripts usan `MERGE` o `IF NOT EXISTS` para evitar duplicados.

## 📝 Notas Importantes

- ⚠️ **NUNCA** subas archivos `.publish.xml` a Git (contienen contraseñas)
- ✅ Los archivos `.publish.xml.example` sí se pueden subir
- 🔄 Cada "Publish" ejecuta los scripts PostDeployment
- 💾 Se recomienda hacer backup antes de publicar en producción
- 🔒 Los scripts respetan datos existentes, solo modifican el esquema

## 📚 Próximos Pasos

1. ✅ Publica la base de datos
2. ✅ Actualiza `appsettings.json` en AppApi con la cadena de conexión correcta
3. ✅ Ejecuta la aplicación: `dotnet run --project src/AppApi/AppApi.csproj`
4. ✅ Prueba los endpoints de ejemplo en Swagger

---

¿Necesitas más ayuda? Consulta el [README completo](./CleanArchitectureDb/README.md).

