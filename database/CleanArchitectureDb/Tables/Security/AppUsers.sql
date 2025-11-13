CREATE TABLE [Security].[AppUsers]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [UserId] NVARCHAR(450) NOT NULL,
    [Codigo] VARCHAR(25) NULL,
    [FullName] VARCHAR(75) NOT NULL,
    [Email] VARCHAR(50) NOT NULL,
    [Department] VARCHAR(50) NOT NULL,
    [Position] VARCHAR(75) NOT NULL,
    [Company] VARCHAR(50) NULL,
    [Office] VARCHAR(50) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] NVARCHAR(450) NULL,
    [LastModifiedDate] DATETIME2 NULL,
    [LastModifiedBy] NVARCHAR(450) NULL,
    CONSTRAINT [FK_AppUsers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [Security].[AspNetUsers] ([Id]) ON DELETE CASCADE
)
GO

CREATE NONCLUSTERED INDEX [IX_AppUsers_UserId] ON [Security].[AppUsers] ([UserId])
GO

CREATE NONCLUSTERED INDEX [IX_AppUsers_Email] ON [Security].[AppUsers] ([Email])
GO

CREATE NONCLUSTERED INDEX [IX_AppUsers_Codigo] ON [Security].[AppUsers] ([Codigo]) WHERE [Codigo] IS NOT NULL
GO

