IF OBJECT_ID(N'[dbo].[SystemOperationLogs]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SystemOperationLogs]
    (
        [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_SystemOperationLogs] PRIMARY KEY,
        [EntityType] NVARCHAR(100) NOT NULL,
        [EntityId] NVARCHAR(64) NOT NULL,
        [ActionType] NVARCHAR(50) NOT NULL,
        [ChangeJson] NVARCHAR(MAX) NOT NULL,
        [OperatorUserId] NVARCHAR(64) NOT NULL,
        [OperatorName] NVARCHAR(100) NOT NULL,
        [RequestPath] NVARCHAR(300) NOT NULL,
        [IpAddress] NVARCHAR(64) NOT NULL,
        [UserAgent] NVARCHAR(500) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [CreatedBy] NVARCHAR(MAX) NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        [UpdatedBy] NVARCHAR(MAX) NOT NULL,
        [IsDeleted] BIT NOT NULL
    );

    CREATE INDEX [IX_SystemOperationLogs_EntityType_EntityId_CreatedAt]
        ON [dbo].[SystemOperationLogs] ([EntityType], [EntityId], [CreatedAt]);

    CREATE INDEX [IX_SystemOperationLogs_OperatorUserId_CreatedAt]
        ON [dbo].[SystemOperationLogs] ([OperatorUserId], [CreatedAt]);

    CREATE INDEX [IX_SystemOperationLogs_ActionType_CreatedAt]
        ON [dbo].[SystemOperationLogs] ([ActionType], [CreatedAt]);
END;
