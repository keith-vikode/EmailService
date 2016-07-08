CREATE TABLE [dbo].[Translation]
(
	[TemplateId] UNIQUEIDENTIFIER NOT NULL, 
    [CultureId] VARCHAR(5) NOT NULL,
	[Subject] NVARCHAR(100) NOT NULL, 
    [Body] NVARCHAR(MAX) NOT NULL, 
    [ConcurrencyToken] ROWVERSION NOT NULL, 
    CONSTRAINT PK_Translation PRIMARY KEY (TemplateId, CultureId),
	CONSTRAINT FK_TemplateHasTranslations FOREIGN KEY (TemplateId) REFERENCES [Template] (Id)
)
