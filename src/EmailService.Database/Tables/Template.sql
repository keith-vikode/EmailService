CREATE TABLE [dbo].[Template]
(
	[Id] UNIQUEIDENTIFIER NOT NULL,
	[ApplicationId] UNIQUEIDENTIFIER NOT NULL, 
    [Name] NVARCHAR(50) NOT NULL, 
    [BodyType] CHAR(4) NOT NULL CONSTRAINT CK_Template_BodyType CHECK ( BodyType IN ('html', 'text') ), 
    [Subject] NVARCHAR(255) NOT NULL, 
    [Body] NVARCHAR(MAX) NOT NULL, 
    [Params] NVARCHAR(MAX) NOT NULL, 
    [ConcurrencyToken] ROWVERSION NOT NULL, 
    CONSTRAINT PK_Template PRIMARY KEY (Id),
	CONSTRAINT FK_ApplicationHasTemplates FOREIGN KEY (ApplicationId) REFERENCES [Application] (Id)
)
