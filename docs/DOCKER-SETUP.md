# 🐳 Configuración de Docker para Desarrollo

Esta guía explica cómo configurar el ambiente de desarrollo usando Docker con todos los servicios necesarios: SQL Server, Redis y Mailpit SMTP Server.

## 📋 Servicios Incluidos

### 1. SQL Server 2022
- **Puerto**: 11433
- **Usuario**: sa
- **Password**: Mardom01
- **String de conexión**: `Server=localhost,11433;Database=CleanArchitectureDb;User Id=sa;Password=Mardom01;TrustServerCertificate=True;`

### 2. Redis 7.0.8
- **Puerto**: 16379
- **Uso**: Cache distribuido
- **String de conexión**: `localhost:16379`

### 3. Mailpit (SMTP Server)
- **Puerto SMTP**: 1025
- **Web UI**: http://localhost:8025
- **Uso**: Servidor SMTP moderno para desarrollo y testing
- **Persistencia**: SQLite en volumen Docker

---

## 🚀 Inicio Rápido

### Opción 1: Docker Compose (Recomendado)

**Levantar todos los servicios:**
```bash
docker-compose up -d
```

**Ver logs:**
```bash
docker-compose logs -f
```

**Detener servicios:**
```bash
docker-compose down
```

**Detener y eliminar volúmenes (datos):**
```bash
docker-compose down -v
```

**Ver estado de servicios:**
```bash
docker-compose ps
```

---

### Opción 2: Comandos Docker Individuales

Ver comandos individuales en [Docker-Commands.txt](Docker-Commands.txt)

---

## 📧 Mailpit - Servidor SMTP para Desarrollo

### ¿Qué es Mailpit?

Mailpit es un servidor SMTP moderno y ligero diseñado específicamente para desarrollo y testing. Captura todos los emails enviados y los muestra en una interfaz web intuitiva.

**Características principales:**
- ✅ No envía emails reales (seguro para desarrollo)
- ✅ Interfaz web moderna y responsive
- ✅ Búsqueda y filtrado avanzado de emails
- ✅ Soporte completo para HTML y archivos adjuntos
- ✅ API REST para automatización
- ✅ Persistencia con SQLite (mantiene emails entre reinicios)
- ✅ Visualización de headers completos
- ✅ Soporte para múltiples destinatarios y CC
- ✅ Descarga de emails en formato EML
- ✅ Validación de formato de emails
- ✅ Más activo y mantenido que MailHog

### Configuración en appsettings.Development.json

```json
{
  "EMailSettings": {
    "From": "noreply@test.com",
    "Host": "localhost",
    "Port": 1025,
    "UserName": "",
    "Password": ""
  }
}
```

**Nota**: Mailpit no requiere autenticación en desarrollo, deja `UserName` y `Password` vacíos.

### Uso

1. **Levantar Mailpit:**
   ```bash
   docker-compose up -d mailpit
   ```

2. **Enviar un email desde tu aplicación** (se capturará automáticamente)

3. **Ver emails capturados:** 
   - Abre http://localhost:8025 en tu navegador
   - Verás todos los emails con vista previa
   - Haz clic en cualquier email para ver detalles completos
   - Busca por remitente, asunto, destinatario, etc.

4. **Usar la API REST** (opcional):
   ```bash
   # Listar todos los mensajes
   curl http://localhost:8025/api/v1/messages
   
   # Ver un mensaje específico
   curl http://localhost:8025/api/v1/message/{id}
   
   # Eliminar todos los mensajes
   curl -X DELETE http://localhost:8025/api/v1/messages
   ```

---

## 🔧 Configuración de la Aplicación

### 1. appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,11433;Database=CleanArchitectureDb;User Id=sa;Password=Mardom01;TrustServerCertificate=True;"
  },
  "CacheSettings": {
    "UseDistributedCache": true,
    "Redis": {
      "Configuration": "localhost:16379",
      "InstanceName": "CleanArchApp:"
    }
  },
  "EMailSettings": {
    "From": "noreply@test.com",
    "Host": "localhost",
    "Port": 1025,
    "UserName": "",
    "Password": ""
  }
}
```

### 2. Aplicar Migraciones

```bash
# Desde la raíz del proyecto
dotnet ef database update --project src/Infrastructure/Persistence --startup-project src/Presentation/AppApi
```

---

## 📊 Monitoreo y Herramientas

### Ver emails (Mailpit Web UI)
- **URL**: http://localhost:8025
- **Características**:
  - Vista previa de emails en HTML/texto plano
  - Ver headers completos (SMTP, MIME, etc.)
  - Búsqueda avanzada por remitente, destinatario, asunto
  - Filtrado por fecha y tamaño
  - Descarga de emails en formato .eml
  - Visualización de archivos adjuntos
  - Validación de formato HTML
  - Soporte para imágenes incrustadas
  - API REST completa en `/api/v1/`

### Ver datos en Redis (CLI)
```bash
# Conectar a Redis
docker exec -it sossa-redis redis-cli

# Ver todas las keys
KEYS *

# Ver valor de una key
GET "key-name"

# Ver información del servidor
INFO

# Salir
exit
```

### Ver datos en SQL Server (Azure Data Studio o SSMS)
- **Server**: localhost,11433
- **Authentication**: SQL Server Authentication
- **Login**: sa
- **Password**: Mardom01

---

## 🧪 Probar el Envío de Emails

### Desde tu aplicación

```csharp
var mailRequest = new MailRequest
{
    To = new List<string> { "user@example.com" },
    Subject = "Test Email from Clean Architecture",
    Body = @"
        <html>
            <body>
                <h1>Hello from Clean Architecture!</h1>
                <p>This is a test email sent via Mailpit.</p>
                <p>The email service uses Polly for retry logic.</p>
            </body>
        </html>",
    Cc = new List<string> { "admin@example.com" }
};

var result = await _smtpMailService.SendAsync(mailRequest);
```

### Verificar en Mailpit

1. Abre http://localhost:8025
2. Verás el email en la lista principal
3. Haz clic para ver:
   - Vista previa HTML
   - Código fuente HTML
   - Texto plano
   - Headers completos
   - Información de routing
4. Verifica que Polly retry funciona (observa los logs si hay errores de red)

---

## 🐛 Troubleshooting

### SQL Server no inicia

**Error**: "SQL Server is unable to run"

**Solución**:
```bash
# Verificar logs
docker logs sossa-db

# Reintentar con más memoria
docker-compose down
docker-compose up -d

# Verificar healthcheck
docker inspect sossa-db | grep -A 10 Health
```

### Redis no se conecta

**Error**: "Connection refused localhost:16379"

**Solución**:
```bash
# Verificar que Redis está corriendo
docker ps | grep redis

# Ver logs
docker logs sossa-redis

# Probar conexión
docker exec -it sossa-redis redis-cli ping
# Debe responder: PONG
```

### Mailpit no captura emails

**Error**: Emails no aparecen en http://localhost:8025

**Solución**:
```bash
# Verificar que Mailpit está corriendo
docker ps | grep mailpit

# Verificar logs
docker logs sossa-mailpit

# Probar puerto SMTP
telnet localhost 1025
# Debe conectar

# Verificar configuración en appsettings.Development.json
# Host debe ser "localhost"
# Port debe ser 1025

# Verificar que la aplicación está enviando emails
# (revisar logs de la aplicación)
```

### Mailpit Web UI no carga

**Error**: http://localhost:8025 no responde

**Solución**:
```bash
# Verificar que el contenedor está corriendo
docker ps | grep mailpit

# Verificar que el puerto no está en uso
# Windows:
netstat -ano | findstr :8025

# Linux/Mac:
lsof -i :8025

# Reiniciar el contenedor
docker-compose restart mailpit

# Ver logs para errores
docker logs sossa-mailpit
```

### Puerto ya en uso

**Error**: "port is already allocated"

**Solución**:
```bash
# Ver qué proceso usa el puerto (Windows)
netstat -ano | findstr :1025

# Cambiar puerto en docker-compose.yml
# Por ejemplo: "1026:1025" (mapea puerto externo 1026 a interno 1025)

# O detener el proceso que usa el puerto
```

### Perder emails al reiniciar

**Problema**: Los emails desaparecen al reiniciar Mailpit

**Solución**:
Mailpit usa SQLite para persistencia. Los datos se mantienen en el volumen `mailpit-data`.

```bash
# Ver volúmenes
docker volume ls

# Ver información del volumen
docker volume inspect mailpit-data

# Para mantener los emails, NO uses: docker-compose down -v
# Usa solo: docker-compose down
```

---

## 🔄 Scripts Útiles

### Reiniciar todo el ambiente

**Windows (PowerShell):**
```powershell
docker-compose down -v
docker-compose up -d
Start-Sleep -Seconds 10
dotnet ef database update --project src/Infrastructure/Persistence --startup-project src/Presentation/AppApi
```

**Linux/Mac (Bash):**
```bash
docker-compose down -v && \
docker-compose up -d && \
sleep 10 && \
dotnet ef database update --project src/Infrastructure/Persistence --startup-project src/Presentation/AppApi
```

### Limpiar todo (reset completo)

```bash
# Detener y eliminar contenedores, volúmenes e imágenes
docker-compose down -v --rmi local

# Eliminar volúmenes huérfanos
docker volume prune -f

# Reiniciar desde cero
docker-compose up -d
```

### Ver todos los emails capturados (API)

```bash
# Listar mensajes
curl http://localhost:8025/api/v1/messages | jq

# Ver un mensaje específico
curl http://localhost:8025/api/v1/message/{id} | jq

# Eliminar todos los mensajes
curl -X DELETE http://localhost:8025/api/v1/messages
```

---

## 🎯 Funcionalidades Avanzadas de Mailpit

### 1. Búsqueda y Filtrado

En la interfaz web:
- **Búsqueda por texto**: En cualquier campo (asunto, cuerpo, headers)
- **Filtrado por remitente**: `from:user@example.com`
- **Filtrado por destinatario**: `to:admin@example.com`
- **Filtrado por asunto**: `subject:invoice`
- **Combinación**: `from:noreply subject:welcome`

### 2. API REST

**Endpoints disponibles**:
```bash
# GET /api/v1/messages - Listar mensajes
# GET /api/v1/message/{id} - Ver mensaje específico
# DELETE /api/v1/message/{id} - Eliminar mensaje
# DELETE /api/v1/messages - Eliminar todos
# GET /api/v1/message/{id}/raw - Ver email crudo
# GET /api/v1/message/{id}/headers - Ver solo headers
# GET /api/v1/message/{id}/html - Ver solo HTML
# GET /api/v1/message/{id}/text - Ver solo texto
# GET /api/v1/message/{id}/attachments/{partID} - Descargar adjunto
```

### 3. Variables de Entorno

Configurables en `docker-compose.yml`:

```yaml
environment:
  MP_MAX_MESSAGES: 5000              # Máximo de mensajes a almacenar
  MP_DATA_FILE: /data/mailpit.db     # Ubicación de la base de datos
  MP_SMTP_AUTH_ACCEPT_ANY: 1         # Aceptar cualquier autenticación
  MP_SMTP_AUTH_ALLOW_INSECURE: 1     # Permitir autenticación insegura
  MP_SMTP_BIND_ADDR: 0.0.0.0:1025    # Dirección de bind SMTP
  MP_UI_BIND_ADDR: 0.0.0.0:8025      # Dirección de bind Web UI
  MP_WEBROOT: /                      # Ruta raíz de la web UI
```

---

## 📚 Referencias

- **Mailpit GitHub**: https://github.com/axllent/mailpit
- **Mailpit Documentation**: https://mailpit.axllent.org/docs/
- **SQL Server Docker**: https://hub.docker.com/_/microsoft-mssql-server
- **Redis Docker**: https://hub.docker.com/_/redis

---

## 🎯 Próximos Pasos

1. ✅ Levanta los servicios: `docker-compose up -d`
2. ✅ Verifica que estén corriendo: `docker-compose ps`
3. ✅ Aplica migraciones de BD
4. ✅ Configura `appsettings.Development.json`
5. ✅ Ejecuta la aplicación
6. ✅ Prueba enviar un email y verlo en http://localhost:8025
7. ✅ Verifica el cache en Redis
8. ✅ Explora la API REST de Mailpit

---

## 💡 Consejos de Uso

### Para Desarrollo
- Mantén Mailpit corriendo todo el tiempo
- Usa la búsqueda para encontrar emails específicos
- Descarga emails en .eml para debugging avanzado
- Usa la API para automatización de tests

### Para Testing
- Elimina todos los emails antes de cada test: `DELETE /api/v1/messages`
- Verifica la recepción de emails vía API
- Valida headers y contenido programáticamente
- Usa la interfaz web para inspección visual

### Para Demos
- Prepara emails de ejemplo antes de la demo
- Usa la interfaz web para mostrar resultados
- Resalta las características de Polly retry si hay errores
- Muestra la búsqueda y filtrado en acción

---

**Nota**: Este ambiente es solo para desarrollo/testing. **NO usar en producción**.

Para producción, configura un servidor SMTP real (SendGrid, AWS SES, etc.) en `appsettings.Production.json`.
