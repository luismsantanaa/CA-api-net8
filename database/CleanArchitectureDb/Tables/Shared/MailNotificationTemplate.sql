CREATE TABLE [Shared].[MailNotificationTemplate]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [Active] BIT NOT NULL DEFAULT 1,
    [Description] NVARCHAR(500) NULL,
    [Suject] NVARCHAR(255) NULL,
    [BodyHtml] NVARCHAR(MAX) NULL,
    [PathImages] NVARCHAR(500) NULL,
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [LastModifiedBy] UNIQUEIDENTIFIER NULL,
    [LastModifiedOn] DATETIME2 NULL,
    [Version] INT NOT NULL DEFAULT 1
)
GO

CREATE NONCLUSTERED INDEX [IX_MailNotificationTemplate_Active] ON [Shared].[MailNotificationTemplate] ([Active])
GO

