CREATE TABLE [Example].[TestCategories]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [Active] BIT NOT NULL DEFAULT 1,
    [Name] NVARCHAR(255) NULL,
    [Description] NVARCHAR(1000) NULL,
    [Image] NVARCHAR(500) NULL,
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [LastModifiedBy] UNIQUEIDENTIFIER NULL,
    [LastModifiedOn] DATETIME2 NULL,
    [Version] INT NOT NULL DEFAULT 1
)
GO

CREATE NONCLUSTERED INDEX [IX_TestCategories_Active] ON [Example].[TestCategories] ([Active])
GO

CREATE NONCLUSTERED INDEX [IX_TestCategories_Name] ON [Example].[TestCategories] ([Name])
GO

