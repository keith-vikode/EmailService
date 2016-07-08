CREATE TABLE [dbo].[Transport]
(
	[Id] UNIQUEIDENTIFIER NOT NULL,
	[TransportType] TINYINT NOT NULL, 
    [Host] NVARCHAR(255) NOT NULL, 
    [UserId] NVARCHAR(255) NOT NULL, 
    [Password] NVARCHAR(255) NOT NULL, 
    [Port] INT NULL, 
    [UseSSL] BIT NOT NULL, 
    [ConcurrencyToken] ROWVERSION NOT NULL, 
    CONSTRAINT PK_Transport PRIMARY KEY (Id)
)
