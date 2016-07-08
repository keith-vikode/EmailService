CREATE TABLE [dbo].[ApplicationTransport]
(
	[ApplicationId] UNIQUEIDENTIFIER NOT NULL, 
    [TransportId] UNIQUEIDENTIFIER NOT NULL, 
    [Ordinal] TINYINT NOT NULL,
	CONSTRAINT PK_ApplicationTransport PRIMARY KEY (ApplicationId, TransportId),
	CONSTRAINT FK_ApplicationHasTransports FOREIGN KEY (ApplicationId) REFERENCES [Application] (Id),
	CONSTRAINT FK_TransportHasApplications FOREIGN KEY (TransportId) REFERENCES [Transport] (Id),
)
