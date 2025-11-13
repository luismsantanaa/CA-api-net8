/*
Post-Deployment Script Template                            
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.        
 Use SQLCMD syntax to include a file in the post-deployment script.            
 Example:      :r .\myfile.sql                                
 Use SQLCMD syntax to reference a variable in the post-deployment script.        
 Example:      :setvar TableName MyTable                            
               SELECT * FROM [$(TableName)]                    
--------------------------------------------------------------------------------------
*/

-- Crear esquemas si no existen
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Shared')
BEGIN
    EXEC('CREATE SCHEMA [Shared]')
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Example')
BEGIN
    EXEC('CREATE SCHEMA [Example]')
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Security')
BEGIN
    EXEC('CREATE SCHEMA [Security]')
END
GO

-- Ejecutar scripts de seed
:r .\SeedSharedData.sql
:r .\SeedExampleData.sql
:r .\SeedSecurityData.sql

