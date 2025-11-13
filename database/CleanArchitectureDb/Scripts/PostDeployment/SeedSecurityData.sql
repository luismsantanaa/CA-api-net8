/*
    Script de carga de datos de seguridad e identidad
    
    IMPORTANTE:
    ============
    La creación de usuarios de ASP.NET Core Identity requiere hashing de contraseñas
    que es específico del algoritmo de Identity. Por lo tanto, se RECOMIENDA usar
    el seed de C# (IdentitySeedData.cs) para crear usuarios iniciales.
    
    Este script está disponible para agregar ROLES iniciales u otros datos
    que no requieran hashing de contraseñas.
*/

PRINT 'Cargando datos de seguridad...'

-- Insertar roles por defecto (si los necesitas)
-- Ejemplo:
/*
MERGE [Security].[AspNetRoles] AS target
USING (VALUES
    (NEWID(), N'Admin', N'ADMIN', NULL),
    (NEWID(), N'User', N'USER', NULL),
    (NEWID(), N'Manager', N'MANAGER', NULL)
) AS source ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
ON target.[NormalizedName] = source.[NormalizedName]
WHEN NOT MATCHED THEN
    INSERT ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (CAST(source.[Id] AS NVARCHAR(450)), source.[Name], source.[NormalizedName], source.[ConcurrencyStamp]);

PRINT 'Roles insertados/actualizados correctamente'
*/

-- NOTA: Para crear usuarios de prueba, ejecuta el seed desde C#:
-- await identitySeedData.SeedTestUser();
-- 
-- Esto creará automáticamente:
--   Email: test@mardom.com
--   Username: testuser
--   Password: Test123!@#

PRINT 'Scripts de seguridad completados'
PRINT 'RECORDATORIO: Ejecutar IdentitySeedData.SeedTestUser() desde la aplicación para crear usuarios de prueba'
GO

