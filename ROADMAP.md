# 🗺️ Roadmap del Proyecto

**Clean Architecture Template - .NET 8**

Este documento lista funcionalidades pendientes o mejoras futuras para el template.

---

## 🎯 Funcionalidades Pendientes

### Alta Prioridad

- [x] **Publicación de Base de Datos**: Documentar proceso completo de deployment a diferentes entornos (DEV, QA, PROD) ✅
- [x] **CI/CD Pipeline**: Configurar pipeline de integración continua ✅
- [x] **Health Checks avanzados**: Implementar health checks para Redis, SMTP, SQL Server ✅

### Media Prioridad

- [ ] **Redis Caching**: Implementar Redis como caché distribuida (actualmente opcional)
- [ ] **Rate Limiting**: Agregar limitación de requests por IP/usuario
- [ ] **API Versioning**: Implementar versionado de API (v1, v2)
- [ ] **Audit Trail**: Sistema completo de auditoría de cambios en entidades
- [ ] **Soft Delete**: Implementar borrado lógico en entidades core

### Baja Prioridad

- [ ] **Background Jobs**: Integrar Hangfire o similar para jobs en background
- [ ] **Notificaciones Push**: Sistema de notificaciones en tiempo real
- [ ] **Multi-tenancy**: Soporte para múltiples tenants
- [ ] **GraphQL API**: Endpoint GraphQL alternativo a REST
- [ ] **Elasticsearch**: Búsqueda avanzada con Elasticsearch

---

## 🔧 Mejoras Técnicas

### Código

- [ ] **Eliminar warnings**: Resolver 4 warnings menores de compilación (parámetros no usados en loggers)
- [ ] **Performance profiling**: Hacer profiling de endpoints más usados
- [ ] **Documentación XML**: Agregar XML comments en todos los public methods

### Infraestructura

- [ ] **Docker Compose completo**: Incluir Redis, Seq, MailHog en docker-compose
- [ ] **Kubernetes**: Templates de deployment para K8s
- [ ] **Azure DevOps**: Templates de pipelines
- [ ] **Terraform**: Scripts de infraestructura como código

---

## 📚 Documentación Pendiente

- [ ] **API Postman Collection**: Crear collection completa de Postman
- [ ] **Architecture Decision Records (ADR)**: Documentar decisiones arquitectónicas clave
- [ ] **Onboarding Guide**: Guía de onboarding para nuevos desarrolladores
- [ ] **Security Best Practices**: Documento de mejores prácticas de seguridad

---

## 🐛 Issues Conocidos

### Resueltos

- ✅ **EF Core Migration Bug**: Se eliminaron las migrations de EF Core y se implementó SQL Server Database Project como solución definitiva
- ✅ **Namespace inconsistencies**: Se corrigieron todos los namespaces tras la reestructuración de carpetas
- ✅ **Test de CreateProduct**: Resuelto - todos los tests (101) ahora pasan correctamente

---

## 💡 Ideas Futuras

- **Multi-database support**: PostgreSQL, MySQL
- **Event Sourcing**: Para dominios con histórico crítico
- **CQRS con Event Bus**: Separación física de lecturas/escrituras
- **Microservices split**: Guía para separar en microservicios

---

## 📋 Notas

- Este es un template base, no todas las funcionalidades listadas son necesarias para todos los proyectos
- Priorizar según necesidades del negocio
- Mantener balance entre funcionalidades y simplicidad

---

**Última actualización**: Noviembre 2025  
**Versión actual**: 2.0

