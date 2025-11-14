# 🏥 Health Checks - Guía Completa

**Clean Architecture Template - Health Checks Avanzados**

Esta guía explica el sistema de Health Checks implementado en el proyecto.

---

## 📋 Tabla de Contenidos

- [Resumen](#resumen)
- [Endpoints Disponibles](#endpoints-disponibles)
- [Health Checks Implementados](#health-checks-implementados)
- [Configuración](#configuración)
- [Monitoreo](#monitoreo)
- [Troubleshooting](#troubleshooting)

---

## 📝 Resumen

El proyecto incluye un sistema completo de Health Checks que monitorea:

✅ **Aplicación**: Estado general, versión, uptime  
✅ **SQL Server**: Conexión a base de datos (Application e Identity)  
✅ **SMTP**: Conectividad con servidor de correo  
✅ **Redis**: Cache distribuido (opcional)  

### Características

- 🎯 **Múltiples endpoints** para diferentes propósitos
- 📊 **Dashboard visual** (Health Checks UI)
- 🔍 **Información detallada** en formato JSON
- ⚡ **Respuestas rápidas** con timeouts configurables
- 🏷️ **Tags** para filtrar health checks
- 📈 **Historial** de health checks

---

## 🌐 Endpoints Disponibles

### 1. `/health` - Health Check Completo

**Propósito**: Endpoint principal con información detallada de todos los health checks

**Uso**: Monitoreo detallado, debugging, dashboards

**Response Codes**:
- `200 OK`: Todos los servicios están Healthy o Degraded
- `503 Service Unavailable`: Al menos un servicio está Unhealthy

**Ejemplo de Request**:
```bash
curl https://localhost:7001/health
```

**Ejemplo de Response**:
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "application": {
      "data": {
        "version": "1.0.0.0",
        "uptime_seconds": 3600,
        "uptime_formatted": "0d 1h 0m",
        "environment": "Production",
        "dotnet_version": "8.0.11",
        "machine_name": "WEB-SERVER-01",
        "processor_count": 8,
        "working_set_mb": 256
      },
      "description": "Application is running",
      "duration": "00:00:00.0012345",
      "status": "Healthy",
      "tags": ["ready", "application"]
    },
    "sql_application": {
      "data": {},
      "description": null,
      "duration": "00:00:00.0456789",
      "status": "Healthy",
      "tags": ["ready", "database", "sql"]
    },
    "smtp": {
      "data": {
        "server": "smtp.office365.com",
        "port": 587,
        "from": "noreply@company.com",
        "authenticated": true
      },
      "description": "SMTP server is accessible",
      "duration": "00:00:00.0234567",
      "status": "Healthy",
      "tags": ["email", "smtp"]
    },
    "redis": {
      "data": {
        "info": "redis_version:7.0.5"
      },
      "description": null,
      "duration": "00:00:00.0123456",
      "status": "Healthy",
      "tags": ["cache", "redis"]
    }
  }
}
```

---

### 2. `/health/ready` - Readiness Probe

**Propósito**: Verificar si la aplicación está lista para recibir tráfico

**Uso**: Load balancers, Kubernetes readiness probe

**Tags incluidos**: `ready`

**Servicios verificados**:
- ✅ Application
- ✅ SQL Application Database
- ✅ SQL Identity Database (si es diferente)

**Response Codes**:
- `200 OK`: La aplicación está lista
- `503 Service Unavailable`: La aplicación no está lista

**Ejemplo de Request**:
```bash
curl https://localhost:7001/health/ready
```

**Ejemplo de Response** (texto simple):
```
Healthy
```

---

### 3. `/health/live` - Liveness Probe

**Propósito**: Verificar si la aplicación está viva (no colgada)

**Uso**: Kubernetes liveness probe

**Servicios verificados**:
- ✅ Application (solo verifica que el proceso responda)

**Response Codes**:
- `200 OK`: La aplicación está viva
- `503 Service Unavailable`: La aplicación está colgada

**Ejemplo de Request**:
```bash
curl https://localhost:7001/health/live
```

**Ejemplo de Response**:
```
Healthy
```

---

### 4. `/health-ui` - Dashboard Visual

**Propósito**: Interface visual para monitorear health checks

**Uso**: Monitoreo humano, troubleshooting

**Características**:
- 📊 **Gráficos** de estado de servicios
- 📈 **Historial** de health checks
- ⏱️ **Duración** de cada check
- 🔄 **Auto-refresh** cada 30 segundos

**Acceso**:
```
https://localhost:7001/health-ui
```

**Screenshot de ejemplo**:
```
╔════════════════════════════════════════════╗
║  Health Checks UI - Dashboard              ║
╠════════════════════════════════════════════╣
║                                            ║
║  Clean Architecture API                    ║
║  ● Healthy                                 ║
║                                            ║
║  Checks:                                   ║
║  ✓ application       0.001s   Healthy     ║
║  ✓ sql_application   0.045s   Healthy     ║
║  ✓ smtp              0.023s   Healthy     ║
║  ✓ redis             0.012s   Healthy     ║
║                                            ║
║  Last check: 2024-11-13 16:30:00          ║
║  Total duration: 0.081s                    ║
╚════════════════════════════════════════════╝
```

---

## 🔍 Health Checks Implementados

### 1. Application Health Check

**Clase**: `ApplicationHealthCheck`

**Propósito**: Verificar el estado general de la aplicación

**Información incluida**:
- Versión de la aplicación
- Uptime (tiempo de ejecución)
- Ambiente (Development, Staging, Production)
- Versión de .NET
- Sistema operativo
- Nombre de la máquina
- Número de procesadores
- Memoria en uso (Working Set)

**Estado**: Siempre `Healthy` (si responde)

---

### 2. SQL Server Health Check (Application Database)

**Librería**: `AspNetCore.HealthChecks.SqlServer`

**Propósito**: Verificar conectividad con la base de datos de aplicación

**Configuración**:
```csharp
healthChecksBuilder.AddSqlServer(
    connectionString: appConnectionString,
    healthQuery: "SELECT 1;",
    name: "sql_application",
    failureStatus: HealthStatus.Unhealthy,
    tags: new[] { "ready", "database", "sql" },
    timeout: TimeSpan.FromSeconds(5));
```

**Estados**:
- `Healthy`: Conexión exitosa
- `Unhealthy`: No se puede conectar a la base de datos

**Timeout**: 5 segundos

---

### 3. SQL Server Health Check (Identity Database)

**Propósito**: Verificar conectividad con la base de datos de Identity

**Nota**: Solo se configura si la connection string es diferente a la de Application

**Configuración**: Igual que Application DB

---

### 4. SMTP Health Check

**Clase**: `SmtpHealthCheck` (Custom)

**Propósito**: Verificar conectividad con el servidor SMTP

**Verifica**:
- ✅ Configuración de SMTP está presente
- ✅ Puede conectarse al servidor SMTP
- ✅ Puerto SMTP es accesible

**Estados**:
- `Healthy`: Conectado exitosamente
- `Degraded`: No se puede conectar (no crítico para la app)
- `Unhealthy`: Error en la configuración

**Información incluida**:
- Servidor SMTP
- Puerto
- Email FROM configurado
- Si tiene autenticación

**Nota**: Este health check **NO** realiza autenticación ni envía emails, solo verifica conectividad TCP.

---

### 5. Redis Health Check (Opcional)

**Librería**: `AspNetCore.HealthChecks.Redis`

**Propósito**: Verificar conectividad con Redis (cache distribuido)

**Configuración**: Solo se activa si existe connection string para Redis

**Estados**:
- `Healthy`: Conectado exitosamente
- `Degraded`: No se puede conectar (no crítico, fallback a caché local)

**Timeout**: 3 segundos

---

## ⚙️ Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "ApplicationConnection": "Server=localhost;Database=CleanArchitectureDb;...",
    "IdentityConnection": "Server=localhost;Database=CleanArchitectureDb;...",
    "RedisConnection": "localhost:6379" // Opcional
  },
  "EMailSettings": {
    "Host": "smtp.office365.com",
    "Port": 587,
    "From": "noreply@company.com",
    "UserName": "noreply@company.com",
    "Password": "********"
  }
}
```

### Program.cs

```csharp
// Configurar Health Checks
builder.Services.AddAdvancedHealthChecks(builder.Configuration);

// ... después de app.Build()

// Mapear endpoints
app.MapAdvancedHealthChecks();
```

### Personalizar Health Checks

Para agregar un nuevo health check custom:

```csharp
// 1. Crear la clase
public class MyCustomHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Tu lógica de verificación aquí
            bool isHealthy = CheckMyService();

            if (isHealthy)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("Service is working"));
            }

            return Task.FromResult(
                HealthCheckResult.Unhealthy("Service is down"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy($"Error: {ex.Message}", ex));
        }
    }
}

// 2. Registrarlo en HealthCheckExtensions.cs
healthChecksBuilder.AddCheck<MyCustomHealthCheck>(
    name: "my_custom_service",
    failureStatus: HealthStatus.Degraded,
    tags: new[] { "custom", "external" });
```

---

## 📊 Monitoreo

### Con Kubernetes

```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: clean-architecture-api
spec:
  template:
    spec:
      containers:
      - name: api
        image: clean-architecture-api:latest
        ports:
        - containerPort: 8080
        livenessProbe:
          httpGet:
            path: /health/live
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
```

### Con Azure Application Insights

```csharp
// Agregar telemetría de health checks
services.AddApplicationInsightsTelemetry();

services.AddHealthChecks()
    .AddApplicationInsightsPublisher();
```

### Con Prometheus

```csharp
// Agregar métricas de Prometheus
services.AddHealthChecks()
    .ForwardToPrometheus();
```

### Con Alertas

**Ejemplo con Azure Monitor**:
```
IF health_check_status == "Unhealthy" 
AND service == "sql_application"
THEN send_alert_to("devops@company.com")
```

---

## 🔧 Troubleshooting

### Health Check siempre retorna Unhealthy

**Posibles causas**:
1. Connection string incorrecta
2. Firewall bloqueando conexión
3. Servicio externo caído
4. Timeout muy corto

**Solución**:
```bash
# 1. Verificar connection string
curl https://localhost:7001/health

# 2. Ver detalles del error en logs
# Buscar en logs: "health check failed"

# 3. Probar conectividad manualmente
# SQL Server:
sqlcmd -S server -U user -P password

# SMTP:
telnet smtp.server.com 587

# Redis:
redis-cli -h localhost ping
```

### Health Checks UI no carga

**Causa**: Solo disponible en DEV por defecto

**Solución**:
```csharp
// En HealthCheckExtensions.cs
// Modificar para habilitar en todos los ambientes
services.AddHealthChecksUI(/*...*/);

// O usar variable de entorno
if (environment.IsDevelopment() || 
    configuration.GetValue<bool>("HealthChecksUI:Enabled"))
{
    services.AddHealthChecksUI(/*...*/);
}
```

### Health Check muy lento

**Causa**: Timeout configurado muy alto o servicio lento

**Solución**:
```csharp
// Reducir timeout en HealthCheckExtensions.cs
healthChecksBuilder.AddSqlServer(
    /*...*/
    timeout: TimeSpan.FromSeconds(3) // Reducir de 5 a 3
);
```

### SMTP Health Check siempre Degraded

**Causa común**: Puerto o servidor SMTP incorrecto

**Verificación**:
```bash
# Probar conexión TCP
telnet smtp.office365.com 587

# O con PowerShell
Test-NetConnection -ComputerName smtp.office365.com -Port 587
```

---

## 📚 Best Practices

1. **Usar tags apropiados**:
   - `ready`: Para readiness probes
   - `live`: Para liveness probes
   - Customs: `database`, `cache`, `email`, `external`

2. **Configurar timeouts adecuados**:
   - Servicios críticos: 3-5 segundos
   - Servicios no críticos: 1-3 segundos

3. **Usar FailureStatus apropiado**:
   - `Unhealthy`: Servicios críticos (SQL Server)
   - `Degraded`: Servicios no críticos (SMTP, Redis)

4. **Incluir información útil en `data`**:
   ```csharp
   data: new Dictionary<string, object>
   {
       { "server", serverName },
       { "port", port },
       { "error_code", errorCode }
   }
   ```

5. **Loggear health check failures**:
   ```csharp
   _logger.LogWarning(ex, "Health check {Name} failed", context.Registration.Name);
   ```

6. **No hacer operaciones pesadas** en health checks:
   - ❌ No hacer queries complejas
   - ❌ No procesar archivos grandes
   - ✅ Solo verificar conectividad
   - ✅ Usar queries simples (`SELECT 1`)

7. **Proteger el endpoint en producción**:
   ```csharp
   // Opcional: Requerir autenticación para /health
   app.MapHealthChecks("/health")
      .RequireAuthorization("HealthCheckPolicy");
   ```

---

## 🔗 Referencias

- [ASP.NET Core Health Checks](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)
- [Kubernetes Health Checks](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/)
- [Azure Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

---

**Última actualización**: Noviembre 13, 2025  
**Versión**: 2.0  
**Mantenido por**: DevOps Team

