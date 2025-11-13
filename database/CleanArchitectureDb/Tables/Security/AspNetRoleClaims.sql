CREATE TABLE [Security].[AspNetRoleClaims]
(
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [RoleId] NVARCHAR(450) NOT NULL,
    [ClaimType] NVARCHAR(MAX) NULL,
    [ClaimValue] NVARCHAR(MAX) NULL,
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Security].[AspNetRoles] ([Id]) ON DELETE CASCADE
)
GO

CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [Security].[AspNetRoleClaims] ([RoleId])
GO

