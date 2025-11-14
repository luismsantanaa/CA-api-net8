# 🚀 Guía de Deployment - SQL Server Database Project

**Clean Architecture Template - Base de Datos**

Esta guía detalla el proceso completo de deployment del SQL Server Database Project a diferentes entornos.

---

## 📋 Tabla de Contenidos

- [Pre-requisitos](#pre-requisitos)
- [Configuración de Entornos](#configuración-de-entornos)
- [Deployment a DEV](#deployment-a-dev)
- [Deployment a QA](#deployment-a-qa)
- [Deployment a PROD](#deployment-a-prod)
- [Troubleshooting](#troubleshooting)
- [Best Practices](#best-practices)

---

## 🔧 Pre-requisitos

### Herramientas Necesarias

1. **Visual Studio 2022** con SQL Server Data Tools (SSDT)
2. **SqlPackage.exe** (incluido con SSDT o SQL Server)
3. **SQL Server Management Studio (SSMS)** - opcional pero recomendado
4. **Acceso a servidores SQL** de cada entorno

### Verificar Instalación

```bash
# Verificar SqlPackage
sqlpackage /version

# Debería mostrar algo como: 162.x.x.x
```

---

## 🌍 Configuración de Entornos

### Estructura de Entornos

```
DEV (Desarrollo)
├── Server: localhost,11433 (Docker)
├── Database: CleanArchitectureDb_DEV
├── Purpose: Desarrollo local
└── Backup: No requerido

QA (Testing/Staging)
├── Server: sql-qa.company.com
├── Database: CleanArchitectureDb_QA
├── Purpose: Testing y validación
└── Backup: Diario

PROD (Producción)
├── Server: sql-prod.company.com
├── Database: CleanArchitectureDb
├── Purpose: Ambiente productivo
└── Backup: Cada 4 horas + transaccional
```

---

## 💻 Deployment a DEV

### Opción 1: Desde Visual Studio (Recomendado para DEV)

1. **Abrir el proyecto SQL**
   ```
   - Abre CleanArchitectureNet8.sln en Visual Studio
   - En Solution Explorer, ve a database/CleanArchitectureDb
   ```

2. **Configurar Publish Profile**
   - Clic derecho en `CleanArchitectureDb.sqlproj`
   - Seleccionar `Publish...`
   - Configurar conexión:
     ```
     Server: localhost,11433
     Database: CleanArchitectureDb_DEV
     Authentication: SQL Server Authentication
     User: sa
     Password: YourPassword123!
     ```

3. **Opciones de Publicación**
   - ✅ **Always re-create database**: Solo primera vez
   - ✅ **Block incremental deployment if data loss might occur**: Activado
   - ✅ **Include composite objects**: Activado
   - ✅ **Verify deployment**: Activado

4. **Guardar Publish Profile**
   - Click en `Save Profile As...`
   - Nombre: `DEV.publish.xml`
   - Guardar en: `database/CleanArchitectureDb/PublishProfiles/`

5. **Ejecutar Deployment**
   - Click en `Publish`
   - Revisar script de deployment
   - Confirmar ejecución

### Opción 2: Línea de Comandos (Para automatización)

```bash
# 1. Navegar al directorio del proyecto SQL
cd database/CleanArchitectureDb

# 2. Build del proyecto (genera .dacpac)
msbuild CleanArchitectureDb.sqlproj /p:Configuration=Release

# 3. Publicar con SqlPackage
sqlpackage /Action:Publish ^
  /SourceFile:bin/Release/CleanArchitectureDb.dacpac ^
  /TargetConnectionString:"Server=localhost,11433;Database=CleanArchitectureDb_DEV;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;Encrypt=True;" ^
  /p:BlockOnPossibleDataLoss=True ^
  /p:IncludeCompositeObjects=True
```

### Opción 3: Docker Script (Desarrollo rápido)

Crea `database/deploy-dev.ps1`:

```powershell
# Deploy to DEV (Docker SQL Server)
$ErrorActionPreference = "Stop"

Write-Host "🚀 Deploying to DEV..." -ForegroundColor Green

# Build project
Write-Host "📦 Building SQL Project..." -ForegroundColor Yellow
msbuild .\CleanArchitectureDb\CleanArchitectureDb.sqlproj /p:Configuration=Release /v:q

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

# Deploy
Write-Host "📤 Publishing to DEV..." -ForegroundColor Yellow
sqlpackage /Action:Publish `
    /SourceFile:CleanArchitectureDb\bin\Release\CleanArchitectureDb.dacpac `
    /TargetConnectionString:"Server=localhost,11433;Database=CleanArchitectureDb_DEV;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;" `
    /p:BlockOnPossibleDataLoss=False

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Deployment to DEV completed successfully!" -ForegroundColor Green
} else {
    Write-Host "❌ Deployment failed!" -ForegroundColor Red
    exit 1
}
```

Ejecutar:
```powershell
cd database
.\deploy-dev.ps1
```

---

## 🧪 Deployment a QA

### Pre-deployment Checklist

- [ ] Código revisado y aprobado
- [ ] Tests pasando al 100%
- [ ] Backup de QA database realizado
- [ ] Ventana de mantenimiento comunicada al equipo

### Proceso de Deployment

1. **Crear Publish Profile para QA**

   Archivo: `database/CleanArchitectureDb/PublishProfiles/QA.publish.xml`

   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
     <PropertyGroup>
       <TargetDatabaseName>CleanArchitectureDb_QA</TargetDatabaseName>
       <TargetConnectionString>Data Source=sql-qa.company.com;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=True;TrustServerCertificate=False</TargetConnectionString>
       <ProfileVersionNumber>1</ProfileVersionNumber>
       <BlockOnPossibleDataLoss>True</BlockOnPossibleDataLoss>
       <IncludeCompositeObjects>True</IncludeCompositeObjects>
       <ScriptDatabaseOptions>True</ScriptDatabaseOptions>
       <GenerateSmartDefaults>True</GenerateSmartDefaults>
     </PropertyGroup>
   </Project>
   ```

2. **Script de Deployment para QA**

   Archivo: `database/deploy-qa.ps1`

   ```powershell
   # Deploy to QA
   param(
       [string]$Server = "sql-qa.company.com",
       [string]$Database = "CleanArchitectureDb_QA"
   )

   $ErrorActionPreference = "Stop"

   Write-Host "🚀 Deploying to QA Environment..." -ForegroundColor Green
   Write-Host "Server: $Server" -ForegroundColor Cyan
   Write-Host "Database: $Database" -ForegroundColor Cyan

   # Confirm deployment
   $confirmation = Read-Host "⚠️  Are you sure you want to deploy to QA? (yes/no)"
   if ($confirmation -ne "yes") {
       Write-Host "❌ Deployment cancelled" -ForegroundColor Yellow
       exit 0
   }

   # Build project
   Write-Host "`n📦 Building SQL Project..." -ForegroundColor Yellow
   msbuild .\CleanArchitectureDb\CleanArchitectureDb.sqlproj /p:Configuration=Release /v:minimal

   if ($LASTEXITCODE -ne 0) {
       Write-Host "❌ Build failed!" -ForegroundColor Red
       exit 1
   }

   # Generate deployment script (for review)
   Write-Host "`n📝 Generating deployment script..." -ForegroundColor Yellow
   $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
   $scriptPath = ".\deployment_scripts\QA_$timestamp.sql"
   
   New-Item -ItemType Directory -Force -Path ".\deployment_scripts" | Out-Null

   sqlpackage /Action:Script `
       /SourceFile:CleanArchitectureDb\bin\Release\CleanArchitectureDb.dacpac `
       /TargetConnectionString:"Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;" `
       /OutputPath:$scriptPath `
       /p:BlockOnPossibleDataLoss=True

   Write-Host "✅ Script generated: $scriptPath" -ForegroundColor Green
   Write-Host "`n📖 Please review the script before continuing..." -ForegroundColor Yellow
   
   $continue = Read-Host "Continue with deployment? (yes/no)"
   if ($continue -ne "yes") {
       Write-Host "❌ Deployment cancelled. Script saved for manual execution." -ForegroundColor Yellow
       exit 0
   }

   # Deploy
   Write-Host "`n📤 Publishing to QA..." -ForegroundColor Yellow
   sqlpackage /Action:Publish `
       /SourceFile:CleanArchitectureDb\bin\Release\CleanArchitectureDb.dacpac `
       /TargetConnectionString:"Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;" `
       /p:BlockOnPossibleDataLoss=True `
       /p:IncludeCompositeObjects=True

   if ($LASTEXITCODE -eq 0) {
       Write-Host "`n✅ Deployment to QA completed successfully!" -ForegroundColor Green
       Write-Host "📊 Deployment script saved at: $scriptPath" -ForegroundColor Cyan
   } else {
       Write-Host "`n❌ Deployment failed!" -ForegroundColor Red
       exit 1
   }
   ```

3. **Ejecutar Deployment**
   ```powershell
   cd database
   .\deploy-qa.ps1
   ```

---

## 🏭 Deployment a PROD

### ⚠️ Pre-deployment Checklist (CRÍTICO)

- [ ] **Backup completo** de base de datos PROD realizado y verificado
- [ ] **Testing en QA** completado exitosamente
- [ ] **Ventana de mantenimiento** aprobada y comunicada
- [ ] **Rollback plan** documentado y probado
- [ ] **Team on standby** para monitoreo post-deployment
- [ ] **Change Request** aprobado por management

### Proceso de Deployment a PROD

1. **Crear Publish Profile para PROD**

   Archivo: `database/CleanArchitectureDb/PublishProfiles/PROD.publish.xml`

   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
     <PropertyGroup>
       <TargetDatabaseName>CleanArchitectureDb</TargetDatabaseName>
       <TargetConnectionString>Data Source=sql-prod.company.com;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=True;TrustServerCertificate=False</TargetConnectionString>
       <ProfileVersionNumber>1</ProfileVersionNumber>
       <BlockOnPossibleDataLoss>True</BlockOnPossibleDataLoss>
       <IncludeCompositeObjects>True</IncludeCompositeObjects>
       <BackupDatabaseBeforeChanges>True</BackupDatabaseBeforeChanges>
       <ScriptDatabaseOptions>True</ScriptDatabaseOptions>
       <GenerateSmartDefaults>True</GenerateSmartDefaults>
     </PropertyGroup>
   </Project>
   ```

2. **Script de Deployment PROD (con validaciones)**

   Archivo: `database/deploy-prod.ps1`

   ```powershell
   # Deploy to PRODUCTION
   # USE WITH EXTREME CAUTION
   param(
       [string]$Server = "sql-prod.company.com",
       [string]$Database = "CleanArchitectureDb",
       [Parameter(Mandatory=$true)]
       [string]$ChangeRequestNumber
   )

   $ErrorActionPreference = "Stop"

   Write-Host "⚠️  PRODUCTION DEPLOYMENT ⚠️" -ForegroundColor Red -BackgroundColor Yellow
   Write-Host "Server: $Server" -ForegroundColor Cyan
   Write-Host "Database: $Database" -ForegroundColor Cyan
   Write-Host "Change Request: $ChangeRequestNumber" -ForegroundColor Cyan

   # Multiple confirmations
   Write-Host "`n⚠️  This will modify PRODUCTION database!" -ForegroundColor Red
   $confirmation1 = Read-Host "Type 'PRODUCTION' to continue"
   if ($confirmation1 -ne "PRODUCTION") {
       Write-Host "❌ Deployment cancelled" -ForegroundColor Yellow
       exit 0
   }

   $confirmation2 = Read-Host "Type the Change Request Number to confirm"
   if ($confirmation2 -ne $ChangeRequestNumber) {
       Write-Host "❌ Change Request number mismatch. Deployment cancelled" -ForegroundColor Red
       exit 0
   }

   # Check backup
   Write-Host "`n🔍 Checking last backup..." -ForegroundColor Yellow
   # TODO: Add backup verification query

   $backupConfirm = Read-Host "Has a backup been taken in the last hour? (yes/no)"
   if ($backupConfirm -ne "yes") {
       Write-Host "❌ Please ensure a recent backup exists before deploying to PROD" -ForegroundColor Red
       exit 1
   }

   # Build project
   Write-Host "`n📦 Building SQL Project..." -ForegroundColor Yellow
   msbuild .\CleanArchitectureDb\CleanArchitectureDb.sqlproj /p:Configuration=Release /v:minimal

   if ($LASTEXITCODE -ne 0) {
       Write-Host "❌ Build failed!" -ForegroundColor Red
       exit 1
   }

   # Generate deployment script (MANDATORY for PROD)
   Write-Host "`n📝 Generating deployment script (MANDATORY REVIEW)..." -ForegroundColor Yellow
   $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
   $scriptPath = ".\deployment_scripts\PROD_${ChangeRequestNumber}_$timestamp.sql"
   
   New-Item -ItemType Directory -Force -Path ".\deployment_scripts" | Out-Null

   sqlpackage /Action:Script `
       /SourceFile:CleanArchitectureDb\bin\Release\CleanArchitectureDb.dacpac `
       /TargetConnectionString:"Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;" `
       /OutputPath:$scriptPath `
       /p:BlockOnPossibleDataLoss=True

   Write-Host "✅ Script generated: $scriptPath" -ForegroundColor Green
   Write-Host "`n⚠️  MANDATORY: Review the deployment script before continuing!" -ForegroundColor Red
   Write-Host "Press any key to open the script in notepad..." -ForegroundColor Yellow
   $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
   
   notepad $scriptPath
   
   Write-Host "`nHave you reviewed and approved the deployment script?" -ForegroundColor Yellow
   $scriptReview = Read-Host "(yes/no)"
   if ($scriptReview -ne "yes") {
       Write-Host "❌ Deployment cancelled. Script saved for manual execution." -ForegroundColor Yellow
       exit 0
   }

   # Final confirmation
   Write-Host "`n⚠️  FINAL CONFIRMATION ⚠️" -ForegroundColor Red -BackgroundColor Yellow
   $finalConfirm = Read-Host "Type 'DEPLOY NOW' to proceed with PRODUCTION deployment"
   if ($finalConfirm -ne "DEPLOY NOW") {
       Write-Host "❌ Deployment cancelled" -ForegroundColor Yellow
       exit 0
   }

   # Deploy
   Write-Host "`n📤 Publishing to PRODUCTION..." -ForegroundColor Red
   Write-Host "⏰ Started at: $(Get-Date)" -ForegroundColor Cyan

   $deployStart = Get-Date

   sqlpackage /Action:Publish `
       /SourceFile:CleanArchitectureDb\bin\Release\CleanArchitectureDb.dacpac `
       /TargetConnectionString:"Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;" `
       /p:BlockOnPossibleDataLoss=True `
       /p:IncludeCompositeObjects=True `
       /p:BackupDatabaseBeforeChanges=True

   $deployEnd = Get-Date
   $duration = $deployEnd - $deployStart

   if ($LASTEXITCODE -eq 0) {
       Write-Host "`n✅ PRODUCTION DEPLOYMENT COMPLETED SUCCESSFULLY!" -ForegroundColor Green
       Write-Host "⏰ Duration: $($duration.TotalMinutes) minutes" -ForegroundColor Cyan
       Write-Host "📊 Deployment script: $scriptPath" -ForegroundColor Cyan
       Write-Host "`n📋 Post-Deployment Checklist:" -ForegroundColor Yellow
       Write-Host "  1. Verify application connectivity"
       Write-Host "  2. Run smoke tests"
       Write-Host "  3. Monitor application logs"
       Write-Host "  4. Verify seed data executed correctly"
       Write-Host "  5. Update Change Request status"
   } else {
       Write-Host "`n❌ PRODUCTION DEPLOYMENT FAILED!" -ForegroundColor Red
       Write-Host "⚠️  Execute rollback plan immediately!" -ForegroundColor Red
       exit 1
   }
   ```

3. **Ejecutar Deployment PROD**
   ```powershell
   cd database
   .\deploy-prod.ps1 -ChangeRequestNumber "CR-2025-1234"
   ```

---

## 🔧 Troubleshooting

### Error: "Database already exists"

**Causa**: Intentando crear una base de datos que ya existe

**Solución**:
```sql
-- Opción 1: Drop y recrear (solo DEV)
DROP DATABASE IF EXISTS CleanArchitectureDb_DEV;

-- Opción 2: Usar incremental deployment (recomendado)
-- En publish profile: Desmarcar "Always re-create database"
```

### Error: "Possible data loss"

**Causa**: El deployment podría causar pérdida de datos (ej: cambiar tipo de columna)

**Solución**:
```powershell
# Revisar el script generado
sqlpackage /Action:Script /SourceFile:... /OutputPath:review.sql

# Si es aceptable, desactivar bloqueo (solo DEV/QA)
/p:BlockOnPossibleDataLoss=False
```

### Error: "Login failed for user"

**Causa**: Credenciales incorrectas o permisos insuficientes

**Solución**:
```sql
-- Verificar permisos del usuario
USE master;
GO
EXEC sp_helplogins 'tu_usuario';

-- Otorgar permisos necesarios
GRANT CREATE DATABASE TO [tu_usuario];
ALTER SERVER ROLE dbcreator ADD MEMBER [tu_usuario];
```

### Error: "Object already exists"

**Causa**: Conflicto con objetos existentes

**Solución**:
```powershell
# Usar Schema Compare para sincronizar
# En Visual Studio: Tools > SQL Server > New Schema Comparison
```

---

## 📚 Best Practices

### 1. **Siempre usar Publish Profiles**
- Un profile por entorno (DEV, QA, PROD)
- Nunca commitear credenciales (usar ejemplo: `.publish.xml.example`)
- Mantener profiles actualizados

### 2. **Generar Scripts antes de Deployment**
- Revisar manualmente en QA/PROD
- Guardar scripts ejecutados para auditoría
- Nombrar scripts con timestamp y change request

### 3. **Backups**
- DEV: No requerido
- QA: Backup diario
- PROD: Backup antes de cada deployment + continuo

### 4. **Testing**
- Probar primero en DEV
- Validar en QA antes de PROD
- Ejecutar smoke tests post-deployment

### 5. **Rollback Plan**
```sql
-- Mantener script de rollback listo
-- Ejemplo: Restaurar backup
RESTORE DATABASE CleanArchitectureDb 
FROM DISK = 'C:\Backups\CleanArchitectureDb_BeforeDeploy.bak'
WITH REPLACE;
```

### 6. **Variables de Entorno**
```json
// appsettings.{Environment}.json
{
  "ConnectionStrings": {
    "ApplicationConnection": "Server=$(DB_SERVER);Database=$(DB_NAME);..."
  }
}
```

### 7. **Documentación**
- Registrar cada deployment
- Mantener log de cambios
- Documentar decisiones críticas

---

## 📊 Deployment Log Template

Mantener un log de deployments:

```
Date: 2025-11-13
Environment: PROD
Change Request: CR-2025-1234
Deployed By: John Doe
Duration: 5 minutes
Status: Success
Issues: None
Rollback Executed: No
```

---

## 🔗 Referencias

- [SqlPackage Documentation](https://docs.microsoft.com/sql/tools/sqlpackage)
- [SSDT Documentation](https://docs.microsoft.com/sql/ssdt/sql-server-data-tools)
- [Backup Best Practices](https://docs.microsoft.com/sql/relational-databases/backup-restore/backup-overview-sql-server)

---

**Última actualización**: Noviembre 13, 2025  
**Versión**: 2.0  
**Mantenido por**: DevOps Team

