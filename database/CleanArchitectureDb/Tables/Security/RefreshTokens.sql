CREATE TABLE [Security].[RefreshTokens]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
    [UserId] NVARCHAR(450) NOT NULL,
    [Token] NVARCHAR(MAX) NOT NULL,
    [JwtId] NVARCHAR(MAX) NOT NULL,
    [IsUsed] BIT NOT NULL DEFAULT 0,
    [IsRevoked] BIT NOT NULL DEFAULT 0,
    [ExpireDate] DATETIME2 NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] NVARCHAR(450) NULL,
    [LastModifiedDate] DATETIME2 NULL,
    [LastModifiedBy] NVARCHAR(450) NULL,
    CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [Security].[AspNetUsers] ([Id]) ON DELETE CASCADE
)
GO

CREATE NONCLUSTERED INDEX [IX_RefreshTokens_UserId] ON [Security].[RefreshTokens] ([UserId])
GO

CREATE NONCLUSTERED INDEX [IX_RefreshTokens_ExpireDate] ON [Security].[RefreshTokens] ([ExpireDate])
GO

CREATE NONCLUSTERED INDEX [IX_RefreshTokens_IsUsed_IsRevoked] ON [Security].[RefreshTokens] ([IsUsed], [IsRevoked])
GO

