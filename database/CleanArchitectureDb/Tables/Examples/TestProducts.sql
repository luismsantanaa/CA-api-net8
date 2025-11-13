CREATE TABLE [Example].[TestProduct]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [Active] BIT NOT NULL DEFAULT 1,
    [Name] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(1000) NOT NULL,
    [Image] NVARCHAR(500) NULL,
    [Price] FLOAT NOT NULL DEFAULT 0.0,
    [Stock] INT NOT NULL DEFAULT 0,
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [LastModifiedBy] UNIQUEIDENTIFIER NULL,
    [LastModifiedOn] DATETIME2 NULL,
    [Version] INT NOT NULL DEFAULT 1,
    [IsDeleted] BIT NULL DEFAULT 0,
    [DeletedBy] UNIQUEIDENTIFIER NULL,
    [DeletedAt] DATETIME2 NULL,
    CONSTRAINT [FK_TestProduct_TestCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Example].[TestCategories] ([Id]) ON DELETE NO ACTION
)
GO

CREATE NONCLUSTERED INDEX [IX_TestProduct_Active] ON [Example].[TestProduct] ([Active])
GO

CREATE NONCLUSTERED INDEX [IX_TestProduct_CategoryId] ON [Example].[TestProduct] ([CategoryId])
GO

CREATE NONCLUSTERED INDEX [IX_TestProduct_Name] ON [Example].[TestProduct] ([Name])
GO

CREATE NONCLUSTERED INDEX [IX_TestProduct_IsDeleted] ON [Example].[TestProduct] ([IsDeleted])
GO

