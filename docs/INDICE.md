# Índice de Documentación Técnica

Bienvenido a la documentación técnica del proyecto Clean Architecture .NET 8. Este índice te ayudará a encontrar rápidamente la información que necesitas.

## 🚀 Para Empezar

- **[README.md](../README.md)** - Inicio rápido, características principales, configuración básica
- **[ESTRUCTURA_COMPLETA.md](ESTRUCTURA_COMPLETA.md)** - Organización de carpetas y estructura del proyecto
- **[database/QUICK_START.md](../database/QUICK_START.md)** - ✨ Guía rápida del SQL Server Database Project
- **[database/RESUMEN_IMPLEMENTACION.md](../database/RESUMEN_IMPLEMENTACION.md)** - ✨ Detalles técnicos de la base de datos

## 📚 Guías de Desarrollo

- **[GUIA_DESARROLLO.md](GUIA_DESARROLLO.md)** - Guía paso a paso para crear un nuevo feature (CRUD completo)
  - Crear entidades
  - Crear Commands y Queries (CQRS)
  - Crear Validators
  - Crear View Models
  - Crear Controllers
  - Configurar AutoMapper
  - Implementar Paginación
  - Helpers y servicios disponibles
- **[PAGINACION.md](PAGINACION.md)** - Guía completa de paginación
  - Componentes de paginación
  - Implementación paso a paso
  - Ejemplos completos
  - Mejores prácticas
- **[RESUMEN_MEJORAS.md](RESUMEN_MEJORAS.md)** - Resumen ejecutivo de mejoras
  - Handler Base para Paginación
  - Servicio de Invalidación de Caché
  - Helpers para Result<T>
  - Extensiones para Handlers

## 🏗️ Arquitectura y Diseño

- **[ARQUITECTURA.md](ARQUITECTURA.md)** - Explicación detallada de Clean Architecture
  - Capas del proyecto (Domain, Application, Infrastructure, Presentation)
  - Principios de diseño (DIP, SRP, OCP)
  - Patrones utilizados (CQRS, Repository, Unit of Work, Specification)
  - Flujo de datos

## 🛠️ Herramientas y Tecnologías

- **[HERRAMIENTAS.md](HERRAMIENTAS.md)** - Detalles de cada herramienta utilizada
  - Frameworks (.NET 8, ASP.NET Core)
  - Patrones (MediatR, CQRS)
  - Acceso a datos (EF Core, Repository Pattern)
  - Validación (FluentValidation)
  - Mapeo (AutoMapper)
  - Autenticación (JWT, Identity)
  - Logging (Serilog)
  - Caching (Memory, Redis)
  - **Manejo de Archivos (IFileStorageService)**
  - **Envío de Correos (SmtpMailService, MailKit)**
  - **Resiliencia (Polly - Retry Logic)**
  - Testing (xUnit, Moq)

## 💡 Ejemplos y Mejores Prácticas

- **[EJEMPLOS.md](EJEMPLOS.md)** - Ejemplos de código y mejores prácticas
  - Ejemplos completos de Handlers
  - Ejemplos de Validators
  - Ejemplos de Controllers
  - **Manejo de Archivos (Upload y Delete con transacciones)**
  - **Envío de Correos Electrónicos (con retry logic)**
  - Mejores prácticas
  - Patrones comunes
  - Casos de uso avanzados

## 📖 Navegación Rápida por Perfil

### 👨‍💼 Para Programadores Junior

**Empieza aquí:**
1. Lee [README.md](../README.md) para entender el proyecto
2. Revisa [GUIA_DESARROLLO.md](GUIA_DESARROLLO.md) para crear tu primer feature
3. Consulta [EJEMPLOS.md](EJEMPLOS.md) cuando tengas dudas sobre cómo hacer algo

**Referencias:**
- Ejemplos completos en: `src/Core/Application/Features/Examples/Products/`
- Tests de ejemplo en: `tests/Tests/Application/Handlers/`

### 👨‍💻 Para Programadores Intermedios

**Enfócate en:**
1. [ARQUITECTURA.md](ARQUITECTURA.md) - Entender la arquitectura
2. [HERRAMIENTAS.md](HERRAMIENTAS.md) - Profundizar en herramientas específicas
3. [EJEMPLOS.md](EJEMPLOS.md) - Patrones avanzados

### 👨‍🔧 Para Arquitectos/Seniors

**Revisa:**
1. [ARQUITECTURA.md](ARQUITECTURA.md) - Decisiones de diseño
2. [ESTRUCTURA_COMPLETA.md](ESTRUCTURA_COMPLETA.md) - Organización del proyecto
3. Todo el código fuente en `src/` para patrones implementados

## 🔍 Búsqueda Rápida

### ¿Cómo crear...?

- **Un nuevo feature (CRUD)**: [GUIA_DESARROLLO.md](GUIA_DESARROLLO.md)
- **Un Command/Query**: [GUIA_DESARROLLO.md#2-crear-commands-cqrs](GUIA_DESARROLLO.md#2-crear-commands-cqrs)
- **Un Query con Paginación**: [PAGINACION.md](PAGINACION.md)
- **Un Validator**: [GUIA_DESARROLLO.md#4-crear-validators](GUIA_DESARROLLO.md#4-crear-validators)
- **Un Controller**: [GUIA_DESARROLLO.md#6-crear-controllers](GUIA_DESARROLLO.md#6-crear-controllers)
- **Upload de Archivos**: [EJEMPLOS.md#4-manejo-de-archivos---upload](EJEMPLOS.md#4-manejo-de-archivos---upload)
- **Envío de Correos**: [EJEMPLOS.md#6-envío-de-correos-electrónicos](EJEMPLOS.md#6-envío-de-correos-electrónicos)

### ¿Qué helpers y servicios están disponibles?

- **Helpers para Result<T>**: [RESUMEN_MEJORAS.md#3-helpers-para-resultt](RESUMEN_MEJORAS.md#3-helpers-para-resultt)
- **Servicio de Invalidación de Caché**: [RESUMEN_MEJORAS.md#2-servicio-de-invalidación-de-caché](RESUMEN_MEJORAS.md#2-servicio-de-invalidación-de-caché)
- **Handler Base para Paginación**: [RESUMEN_MEJORAS.md#1-handler-base-para-paginación](RESUMEN_MEJORAS.md#1-handler-base-para-paginación)
- **Extensiones para Handlers**: [RESUMEN_MEJORAS.md#4-extensiones-para-handlers](RESUMEN_MEJORAS.md#4-extensiones-para-handlers)

### ¿Cómo funciona...?

- **CQRS**: [HERRAMIENTAS.md#cqrs](HERRAMIENTAS.md#cqrs)
- **Repository Pattern**: [HERRAMIENTAS.md#repository-pattern](HERRAMIENTAS.md#repository-pattern)
- **Unit of Work**: [HERRAMIENTAS.md#unit-of-work](HERRAMIENTAS.md#unit-of-work)
- **Specification Pattern**: [HERRAMIENTAS.md#specification-pattern](HERRAMIENTAS.md#specification-pattern)

### ¿Cómo configurar...?

- **Base de datos**: [README.md#2-configurar-base-de-datos](../README.md#2-configurar-base-de-datos)
- **JWT**: [README.md#3-configurar-jwt](../README.md#3-configurar-jwt)
- **AutoMapper**: [GUIA_DESARROLLO.md#7-configurar-automapper](GUIA_DESARROLLO.md#7-configurar-automapper)

## 📝 Convenciones del Proyecto

- **Nombres**: PascalCase para clases, camelCase para variables
- **Estructura**: Commands/Queries en carpetas separadas
- **Retornos**: Siempre usar `Result<T>`
- **Logging**: Estructurado con Serilog
- **Validación**: En Validators, no en Handlers

## 🎯 Próximos Pasos

1. **Configura el proyecto**: Sigue [README.md](../README.md#-inicio-rápido)
2. **Revisa los ejemplos**: Examina `src/Core/Application/Features/Examples/`
3. **Crea tu primer feature**: Usa [GUIA_DESARROLLO.md](GUIA_DESARROLLO.md)
4. **Escribe tests**: Revisa `tests/Tests/` para ejemplos

## ❓ ¿Necesitas Ayuda?

1. **Revisa los ejemplos**: Productos y Categorías en el código
2. **Consulta la documentación**: Usa este índice para encontrar información
3. **Revisa los tests**: Ejemplos prácticos en `tests/Tests/`
4. **Consulta la documentación oficial**: Links en [HERRAMIENTAS.md](HERRAMIENTAS.md)

---

**Nota**: Los ejemplos de Productos y Categorías están incluidos solo como referencia. Elimínalos cuando implementes tus propias entidades de negocio.

