# 🔄 Guía de CI/CD

**Clean Architecture Template - Continuous Integration & Deployment**

Esta guía explica cómo está configurado el CI/CD y cómo usarlo para diferentes plataformas.

---

## 📋 Tabla de Contenidos

- [Resumen](#resumen)
- [GitHub Actions](#github-actions)
- [Azure DevOps](#azure-devops)
- [Configuración de Entornos](#configuración-de-entornos)
- [Secretos y Variables](#secretos-y-variables)
- [Best Practices](#best-practices)

---

## 📝 Resumen

El proyecto incluye configuraciones de CI/CD para:

### Continuous Integration (CI)
- ✅ Build automático del código
- ✅ Ejecución de tests unitarios (101 tests)
- ✅ Análisis de calidad de código
- ✅ Build del SQL Database Project
- ✅ Escaneo de vulnerabilidades
- ✅ Code coverage reporting

### Continuous Deployment (CD)
- ✅ Deployment automático a DEV
- ✅ Deployment manual a QA (con aprobación)
- ✅ Deployment manual a PROD (con múltiples aprobaciones)
- ✅ Smoke tests post-deployment
- ✅ Rollback automático en caso de fallo

---

## 🐙 GitHub Actions

### Archivos de Configuración

```
.github/workflows/
├── ci-build.yml      # CI: Build y Test
└── cd-deploy.yml     # CD: Deployment a entornos
```

### CI - Build Automático

**Trigger**: Push o Pull Request a `main` o `develop`

**Jobs**:
1. **Build and Test**: Compila y ejecuta tests
2. **Code Quality**: Análisis de código
3. **Build Database**: Compila SQL Project
4. **Security Scan**: Escaneo de vulnerabilidades
5. **Build Summary**: Resumen de resultados

**Uso**:
```bash
# Se ejecuta automáticamente en cada push/PR
git push origin develop

# Ver resultados en: https://github.com/{user}/{repo}/actions
```

### CD - Deployment Manual

**Trigger**: Manual desde GitHub Actions

**Environments**: DEV | QA | PROD

**Uso**:

1. Ve a tu repositorio en GitHub
2. Click en **Actions**
3. Selecciona **CD - Deploy to Environments**
4. Click en **Run workflow**
5. Selecciona:
   - Branch (generalmente `main`)
   - Environment (DEV/QA/PROD)
   - Change Request (solo para PROD)
6. Click en **Run workflow**

**Ejemplo para PROD**:
```
Branch: main
Environment: PROD
Change Request: CR-2025-1234
```

### Configuración Inicial

#### 1. Habilitar GitHub Actions

En tu repositorio:
- Settings → Actions → General
- Marcar "Allow all actions and reusable workflows"
- Guardar cambios

#### 2. Configurar Environments

En tu repositorio:
- Settings → Environments
- Crear 3 environments:
  - `development` (auto-deploy desde `develop`)
  - `qa` (requiere aprobación)
  - `production` (requiere múltiples aprobaciones)

Para cada environment:
- Click en "New environment"
- Nombre: `development`, `qa`, o `production`
- Configurar:
  - **Required reviewers**: Para QA y PROD
  - **Wait timer**: 5 minutos para PROD
  - **Deployment branches**: Solo `main` para PROD

#### 3. Configurar Secretos

Settings → Secrets and variables → Actions → New repository secret

**Secretos necesarios**:
```
# Azure (si usas Azure)
AZURE_SUBSCRIPTION_ID
AZURE_CREDENTIALS

# SQL Server
DB_DEV_CONNECTION_STRING
DB_QA_CONNECTION_STRING
DB_PROD_CONNECTION_STRING

# JWT
JWT_SECRET_DEV
JWT_SECRET_QA
JWT_SECRET_PROD

# SMTP
SMTP_PASSWORD_DEV
SMTP_PASSWORD_QA
SMTP_PASSWORD_PROD

# Otros
CODECOV_TOKEN (opcional - para code coverage)
```

**Variables de environment**:
```
# DEV
DB_SERVER: localhost,11433
ASPNETCORE_ENVIRONMENT: Development

# QA
DB_SERVER: sql-qa.company.com
ASPNETCORE_ENVIRONMENT: Staging

# PROD
DB_SERVER: sql-prod.company.com
ASPNETCORE_ENVIRONMENT: Production
```

---

## 🔷 Azure DevOps

### Archivo de Configuración

```
azure-pipelines.yml   # Pipeline de CI/CD
```

### Configuración Inicial

#### 1. Crear Pipeline

1. Ve a **Azure DevOps** → Tu proyecto
2. **Pipelines** → **New Pipeline**
3. Selecciona tu repositorio
4. Selecciona "Existing Azure Pipelines YAML file"
5. Path: `/azure-pipelines.yml`
6. Click en **Continue** → **Run**

#### 2. Configurar Environments

1. **Pipelines** → **Environments**
2. Crear 3 environments:
   - `development`
   - `qa`
   - `production`

Para `production`:
- Click en "More actions" → **Approvals and checks**
- Agregar:
  - Required approvers (mínimo 2)
  - Branch control (solo `main`)
  - Business hours (opcional)

#### 3. Configurar Variable Groups

**Pipelines** → **Library** → **Variable groups**

**Grupo: DEV-Variables**
```
DB_SERVER: localhost,11433
DB_NAME: CleanArchitectureDb_DEV
ASPNETCORE_ENVIRONMENT: Development
```

**Grupo: QA-Variables**
```
DB_SERVER: sql-qa.company.com
DB_NAME: CleanArchitectureDb_QA
ASPNETCORE_ENVIRONMENT: Staging
```

**Grupo: PROD-Variables**
```
DB_SERVER: sql-prod.company.com
DB_NAME: CleanArchitectureDb
ASPNETCORE_ENVIRONMENT: Production
```

Para cada grupo, agregar también variables secretas (marcar el candado):
- `DB_PASSWORD`
- `JWT_SECRET`
- `SMTP_PASSWORD`

#### 4. Configurar Service Connections

**Project Settings** → **Service connections** → **New service connection**

Para Azure:
- Tipo: **Azure Resource Manager**
- Scope: Subscription
- Nombre: `Azure-Subscription`

Para SQL Server:
- Tipo: **SQL Server**
- Server name: `sql-{env}.company.com`
- Database: `CleanArchitectureDb_{ENV}`
- Nombre: `SQL-{ENV}`

---

## 🌍 Configuración de Entornos

### Permisos Recomendados

| Environment | Auto-Deploy | Approvers | Branch |
|-------------|-------------|-----------|---------|
| DEV | ✅ Sí (desde `develop`) | 0 | `develop` |
| QA | ❌ Manual | 1 | `main` |
| PROD | ❌ Manual | 2+ | `main` |

### URLs de Entornos (ejemplo)

```yaml
DEV:  https://dev-api.company.com
QA:   https://qa-api.company.com
PROD: https://api.company.com
```

---

## 🔐 Secretos y Variables

### Jerarquía de Variables

```
1. Pipeline variables (más específico)
2. Environment variables
3. Variable groups
4. Repository secrets
5. Organization secrets (menos específico)
```

### Secretos Requeridos

#### Application
- `JWT_SECRET_{ENV}`: Secret para tokens JWT
- `JWT_ISSUER_{ENV}`: Emisor del token
- `JWT_AUDIENCE_{ENV}`: Audiencia del token

#### Database
- `DB_CONNECTION_STRING_{ENV}`: Connection string completo
- `DB_SERVER_{ENV}`: Servidor SQL
- `DB_NAME_{ENV}`: Nombre de la base de datos
- `DB_USER_{ENV}`: Usuario de SQL
- `DB_PASSWORD_{ENV}`: Contraseña de SQL

#### SMTP
- `SMTP_HOST_{ENV}`: Servidor SMTP
- `SMTP_PORT_{ENV}`: Puerto SMTP
- `SMTP_USER_{ENV}`: Usuario SMTP
- `SMTP_PASSWORD_{ENV}`: Contraseña SMTP

#### Cloud Provider (Azure example)
- `AZURE_SUBSCRIPTION_ID`: ID de suscripción
- `AZURE_TENANT_ID`: ID de tenant
- `AZURE_CLIENT_ID`: ID de cliente
- `AZURE_CLIENT_SECRET`: Secret del cliente

### Rotación de Secretos

**Frecuencia recomendada**:
- DEV: 6 meses
- QA: 3 meses
- PROD: 1 mes (o según política de seguridad)

**Proceso**:
1. Generar nuevo secreto
2. Actualizar en Azure Key Vault / Secrets Manager
3. Actualizar en CI/CD platform
4. Desplegar aplicación
5. Validar funcionalidad
6. Revocar secreto anterior

---

## 📚 Best Practices

### 1. Branching Strategy

Recomendamos **GitFlow**:

```
main (producción)
  ↑
develop (desarrollo)
  ↑
feature/* (features individuales)
```

**Workflow**:
```bash
# Crear feature
git checkout -b feature/nueva-funcionalidad develop

# Desarrollar y hacer commits
git add .
git commit -m "feat: nueva funcionalidad"

# Merge a develop (PR)
git checkout develop
git merge feature/nueva-funcionalidad

# CI se ejecuta automáticamente en develop
# Deployment automático a DEV

# Cuando está listo para QA/PROD
git checkout main
git merge develop

# CI se ejecuta en main
# Deployment manual a QA → PROD
```

### 2. Commits Semánticos

Usar [Conventional Commits](https://www.conventionalcommits.org/):

```bash
feat: nueva funcionalidad
fix: corrección de bug
docs: cambios en documentación
style: formateo de código
refactor: refactorización
test: agregar tests
chore: tareas de mantenimiento
```

### 3. Testing Strategy

```
Unit Tests → Integration Tests → E2E Tests → Smoke Tests
   (CI)           (CI)              (QA)         (PROD)
```

### 4. Deployment Strategy

**DEV**:
- Auto-deploy en cada push a `develop`
- Sin aprobaciones
- Puede fallar sin impacto

**QA**:
- Manual trigger
- 1 aprobación
- Ejecutar tests completos
- Validar con stakeholders

**PROD**:
- Manual trigger
- 2+ aprobaciones
- Change Request obligatorio
- Ventana de mantenimiento
- Backup verificado
- Rollback plan listo

### 5. Monitoring Post-Deployment

Después de cada deployment:

```bash
# 1. Verificar health checks
curl https://api.company.com/health

# 2. Verificar logs
# Azure: Log Analytics
# AWS: CloudWatch
# On-premise: Seq, Grafana

# 3. Verificar métricas
# - Response time
# - Error rate
# - CPU/Memory usage
# - Database connections

# 4. Ejecutar smoke tests
curl https://api.company.com/api/categories
curl https://api.company.com/api/products

# 5. Monitorear por 1 hora
# Alertas configuradas para errores críticos
```

### 6. Rollback Strategy

**Si algo falla**:

```bash
# Opción 1: Rollback de aplicación (rápido)
# - Revertir a versión anterior
# - Re-deploy automático

# Opción 2: Rollback de database (más lento)
# - Restaurar backup
# - Aplicar scripts de rollback

# Opción 3: Hotfix
# - Crear branch hotfix/
# - Fix rápido
# - Deploy emergency
```

### 7. Notificaciones

Configurar notificaciones para:
- ✅ Deployment exitoso a PROD
- ❌ Pipeline fallido
- ⚠️ Tests fallando
- 🔒 Vulnerabilidades detectadas

**Canales recomendados**:
- Slack / Microsoft Teams
- Email a stakeholders
- SMS para PROD (crítico)

---

## 📊 Métricas de CI/CD

### Métricas clave a monitorear:

1. **Build Success Rate**: > 95%
2. **Test Pass Rate**: > 99%
3. **Deployment Frequency**: 
   - DEV: Múltiple por día
   - QA: Diario
   - PROD: Semanal
4. **Mean Time to Recovery (MTTR)**: < 1 hora
5. **Change Failure Rate**: < 5%

### Dashboards Recomendados

**GitHub Actions**:
- Insights → Actions
- Ver success rate, duración, etc.

**Azure DevOps**:
- Pipelines → Analytics
- Ver trends, deployment frequency

---

## 🔗 Referencias

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure DevOps Pipelines](https://docs.microsoft.com/en-us/azure/devops/pipelines/)
- [GitFlow Workflow](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [12-Factor App](https://12factor.net/)

---

## 🆘 Troubleshooting

### Pipeline falla en Build

**Problema**: Error de compilación

**Solución**:
```bash
# Verificar localmente
dotnet build --configuration Release

# Revisar logs del pipeline
# Corregir errores
# Hacer commit y push
```

### Tests fallan en CI pero pasan localmente

**Problema**: Diferencias de entorno

**Solución**:
```bash
# Verificar versión de .NET
dotnet --version

# Limpiar y rebuild
dotnet clean
dotnet build
dotnet test

# Revisar dependencias de tests (time zones, paths, etc.)
```

### Deployment falla en ambiente

**Problema**: Error de conectividad o configuración

**Solución**:
1. Verificar secretos/variables
2. Verificar conectividad a servicios
3. Revisar logs de deployment
4. Ejecutar rollback si es necesario

---

**Última actualización**: Noviembre 13, 2025  
**Versión**: 2.0  
**Mantenido por**: DevOps Team

