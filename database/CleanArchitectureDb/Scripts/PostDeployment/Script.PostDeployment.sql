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

-- Los schemas (Shared, Example, Security) se crean como parte del proyecto SQL
-- No es necesario crearlos aquí

-- Ejecutar scripts de seed
:r .\SeedSharedData.sql
:r .\SeedExampleData.sql
:r .\SeedSecurityData.sql

