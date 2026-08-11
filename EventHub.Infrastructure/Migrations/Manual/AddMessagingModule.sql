/*
    AddMessagingModule.sql
    ---------------------------------------------------------------------------
    Hand-written, idempotent schema update for Vendor<->Customer direct
    messaging ("Contact Vendor") — Conversation / ConversationMessage.

    WHY THIS FILE EXISTS: same reason as AddPaymentModule.sql — no .NET SDK
    available to run `dotnet ef migrations add`. This script creates the
    exact same schema shape that ConversationConfiguration /
    ConversationMessageConfiguration now describe in code.

    HOW TO APPLY:
    Run this script once against your EventHubDb database (SSMS, Azure Data
    Studio, sqlcmd). Safe to re-run: every step is guarded.

    RECOMMENDED ALTERNATIVE: if you have the .NET SDK locally —
        dotnet ef migrations add AddMessagingModule --project EventHub.Infrastructure --startup-project EventHub.API
        dotnet ef database update --project EventHub.Infrastructure --startup-project EventHub.API
    That will also refresh Migrations/ApplicationDbContextModelSnapshot.cs,
    which this script intentionally does not touch.
*/

SET NOCOUNT ON;

-- ─── 1) Conversations table ─────────────────────────────────────────────────
IF OBJECT_ID('dbo.Conversations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Conversations
    (
        Id              int IDENTITY(1,1) NOT NULL,
        CustomerUserId  int NOT NULL,
        VendorUserId    int NOT NULL,
        WorkPostId      int NULL,
        CreatedAt       datetime2 NOT NULL CONSTRAINT DF_Conversations_CreatedAt DEFAULT (GETUTCDATE()),
        UpdatedAt       datetime2 NOT NULL CONSTRAINT DF_Conversations_UpdatedAt DEFAULT (GETUTCDATE()),

        CONSTRAINT PK_Conversations PRIMARY KEY (Id),
        CONSTRAINT FK_Conversations_AspNetUsers_CustomerUserId
            FOREIGN KEY (CustomerUserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Conversations_AspNetUsers_VendorUserId
            FOREIGN KEY (VendorUserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Conversations_WorkPosts_WorkPostId
            FOREIGN KEY (WorkPostId) REFERENCES dbo.WorkPosts (Id) ON DELETE SET NULL
    );

    CREATE INDEX IX_Conversations_CustomerUserId ON dbo.Conversations (CustomerUserId);
    CREATE INDEX IX_Conversations_VendorUserId ON dbo.Conversations (VendorUserId);
    CREATE INDEX IX_Conversations_WorkPostId ON dbo.Conversations (WorkPostId);
END
GO

-- ─── 2) ConversationMessages table ──────────────────────────────────────────
IF OBJECT_ID('dbo.ConversationMessages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConversationMessages
    (
        Id               int IDENTITY(1,1) NOT NULL,
        ConversationId   int NOT NULL,
        SenderUserId     int NOT NULL,
        Body             nvarchar(4000) NOT NULL,
        IsReadByCustomer bit NOT NULL CONSTRAINT DF_ConversationMessages_IsReadByCustomer DEFAULT (0),
        IsReadByVendor   bit NOT NULL CONSTRAINT DF_ConversationMessages_IsReadByVendor DEFAULT (0),
        SentAt           datetime2 NOT NULL CONSTRAINT DF_ConversationMessages_SentAt DEFAULT (GETUTCDATE()),

        CONSTRAINT PK_ConversationMessages PRIMARY KEY (Id),
        CONSTRAINT FK_ConversationMessages_Conversations_ConversationId
            FOREIGN KEY (ConversationId) REFERENCES dbo.Conversations (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_ConversationMessages_ConversationId ON dbo.ConversationMessages (ConversationId);
END
GO

PRINT 'AddMessagingModule.sql applied successfully.';
