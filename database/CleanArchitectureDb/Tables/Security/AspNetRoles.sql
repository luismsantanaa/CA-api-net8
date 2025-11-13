CREATE TABLE [Security].[AspNetRoles]
(
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(256) NULL,
    [NormalizedName] NVARCHAR(256) NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NULL
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [Security].[AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL
GO

