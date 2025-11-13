CREATE TABLE [Shared].[UploadedFile]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [Active] BIT NOT NULL DEFAULT 1,
    [Name] NVARCHAR(255) NULL,
    [Type] NVARCHAR(100) NULL,
    [Extension] NVARCHAR(50) NULL,
    [Size] DECIMAL(18, 2) NULL,
    [Path] NVARCHAR(1000) NULL,
    [Reference] NVARCHAR(500) NULL,
    [Comment] NVARCHAR(1000) NULL,
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [LastModifiedBy] UNIQUEIDENTIFIER NULL,
    [LastModifiedOn] DATETIME2 NULL,
    [Version] INT NOT NULL DEFAULT 1
)
GO

CREATE NONCLUSTERED INDEX [IX_UploadedFile_Active] ON [Shared].[UploadedFile] ([Active])
GO

CREATE NONCLUSTERED INDEX [IX_UploadedFile_Reference] ON [Shared].[UploadedFile] ([Reference])
GO

