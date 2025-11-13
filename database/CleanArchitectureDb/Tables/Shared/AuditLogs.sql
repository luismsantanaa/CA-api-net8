CREATE TABLE [Shared].[AuditLogs]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Type] VARCHAR(10) NOT NULL,
    [TableName] VARCHAR(50) NOT NULL,
    [DateTime] DATETIME2 NOT NULL,
    [OldValues] VARCHAR(MAX) NULL,
    [NewValues] VARCHAR(MAX) NULL,
    [AffectedColumns] VARCHAR(MAX) NULL,
    [PrimaryKey] VARCHAR(50) NOT NULL
)
GO

CREATE NONCLUSTERED INDEX [IX_AuditLogs_UserId] ON [Shared].[AuditLogs] ([UserId])
GO

CREATE NONCLUSTERED INDEX [IX_AuditLogs_TableName] ON [Shared].[AuditLogs] ([TableName])
GO

CREATE NONCLUSTERED INDEX [IX_AuditLogs_DateTime] ON [Shared].[AuditLogs] ([DateTime] DESC)
GO

