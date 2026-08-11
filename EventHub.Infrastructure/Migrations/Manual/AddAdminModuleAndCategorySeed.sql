/*
    AddAdminModuleAndCategorySeed.sql
    ---------------------------------------------------------------------------
    Hand-written, idempotent schema update. Found by actually logging into the
    live admin portal and hitting every endpoint: GET /admin/settings,
    GET /admin/conversations, and GET /admin/reports/analytics (which reads
    settings internally) all return 500 "An unexpected error occurred" —
    because the AdminSettings / AdminConversations / AdminConversationMessages
    tables were never created on the live database, even though the C# code
    for all three (AdminController, AdminService, their DTOs) has been
    deployed for a while. GET /categories also comes back as an empty array
    `[]` — the Categories table exists, but the 8-row seed from
    CategoryConfiguration.HasData (migration 20260811021043_AddCategoriesSeedData)
    was likewise never applied.

    WHY A HAND-WRITTEN SCRIPT INSTEAD OF THE EXISTING EF MIGRATION:
    20260811021043_AddCategoriesSeedData also re-declares CreateTable /
    AddColumn calls for things that ARE already live (Payments/VendorProfiles
    columns, Payouts table — see that migration's own comment). Running
    `dotnet ef database update` naively would fail with "already exists" on
    those. This script only does the three things that are actually still
    missing.

    HOW TO APPLY: run once against EventHubDb (SSMS / Azure Data Studio /
    sqlcmd). Safe to re-run — every step is guarded.
*/

SET NOCOUNT ON;

-- ─── 1) AdminSettings table + the one settings row the app expects ─────────
IF OBJECT_ID('dbo.AdminSettings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AdminSettings
    (
        Id                      int IDENTITY(1,1) NOT NULL,
        CommissionPercentage    decimal(5,2) NOT NULL CONSTRAINT DF_AdminSettings_CommissionPercentage DEFAULT (10.00),
        TaxPercentage           decimal(5,2) NOT NULL CONSTRAINT DF_AdminSettings_TaxPercentage DEFAULT (14.00),
        MaxImagesPerWorkPost    int NOT NULL CONSTRAINT DF_AdminSettings_MaxImagesPerWorkPost DEFAULT (10),
        MaxPackagesPerWorkPost  int NOT NULL CONSTRAINT DF_AdminSettings_MaxPackagesPerWorkPost DEFAULT (5),
        PlatformName            nvarchar(100) NOT NULL CONSTRAINT DF_AdminSettings_PlatformName DEFAULT (N'EventHub'),
        PlatformLogoUrl         nvarchar(500) NULL,
        SupportEmail            nvarchar(200) NULL,
        UpdatedAt               datetime2 NOT NULL CONSTRAINT DF_AdminSettings_UpdatedAt DEFAULT (GETUTCDATE()),

        CONSTRAINT PK_AdminSettings PRIMARY KEY (Id)
    );
END
GO

-- AdminService.GetOrCreateSettingsAsync() would create this row lazily on
-- first read anyway, but seeding it up front means the very first
-- GET /admin/settings call (and GET /admin/reports/analytics, which reads
-- the commission rate from it) doesn't depend on that code path succeeding.
IF NOT EXISTS (SELECT 1 FROM dbo.AdminSettings)
BEGIN
    INSERT INTO dbo.AdminSettings (CommissionPercentage, TaxPercentage, MaxImagesPerWorkPost, MaxPackagesPerWorkPost, PlatformName, UpdatedAt)
    VALUES (10.00, 14.00, 10, 5, N'EventHub', GETUTCDATE());
END
GO

-- ─── 2) AdminConversations / AdminConversationMessages (CRM inbox) ─────────
IF OBJECT_ID('dbo.AdminConversations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AdminConversations
    (
        Id         int IDENTITY(1,1) NOT NULL,
        UserId     int NOT NULL,
        Subject    nvarchar(300) NOT NULL,
        Status     nvarchar(20) NOT NULL CONSTRAINT DF_AdminConversations_Status DEFAULT (N'Open'),
        CreatedAt  datetime2 NOT NULL CONSTRAINT DF_AdminConversations_CreatedAt DEFAULT (GETUTCDATE()),
        UpdatedAt  datetime2 NOT NULL CONSTRAINT DF_AdminConversations_UpdatedAt DEFAULT (GETUTCDATE()),

        CONSTRAINT PK_AdminConversations PRIMARY KEY (Id),
        CONSTRAINT FK_AdminConversations_AspNetUsers_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id) ON DELETE NO ACTION
    );

    CREATE INDEX IX_AdminConversations_UserId ON dbo.AdminConversations (UserId);
END
GO

IF OBJECT_ID('dbo.AdminConversationMessages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AdminConversationMessages
    (
        Id             int IDENTITY(1,1) NOT NULL,
        ConversationId int NOT NULL,
        SenderUserId   int NULL,
        Body           nvarchar(4000) NOT NULL,
        IsReadByUser   bit NOT NULL CONSTRAINT DF_AdminConversationMessages_IsReadByUser DEFAULT (0),
        IsReadByAdmin  bit NOT NULL CONSTRAINT DF_AdminConversationMessages_IsReadByAdmin DEFAULT (0),
        SentAt         datetime2 NOT NULL CONSTRAINT DF_AdminConversationMessages_SentAt DEFAULT (GETUTCDATE()),

        CONSTRAINT PK_AdminConversationMessages PRIMARY KEY (Id),
        CONSTRAINT FK_AdminConversationMessages_AdminConversations_ConversationId
            FOREIGN KEY (ConversationId) REFERENCES dbo.AdminConversations (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_AdminConversationMessages_ConversationId ON dbo.AdminConversationMessages (ConversationId);
END
GO

-- ─── 3) Categories seed (idempotent per-row — safe even if some already exist) ─
SET IDENTITY_INSERT dbo.Categories ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id = 1)
    INSERT INTO dbo.Categories (Id, Name, Description, CreatedAt, IsDeleted) VALUES (1, N'Catering', N'Fine dining, buffets, tastings', '2026-01-01T00:00:00', 0);
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id = 2)
    INSERT INTO dbo.Categories (Id, Name, Description, CreatedAt, IsDeleted) VALUES (2, N'Florals', N'Bouquets, centerpieces, installations', '2026-01-01T00:00:00', 0);
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id = 3)
    INSERT INTO dbo.Categories (Id, Name, Description, CreatedAt, IsDeleted) VALUES (3, N'Venue', N'Halls, estates, rooftops, gardens', '2026-01-01T00:00:00', 0);
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id = 4)
    INSERT INTO dbo.Categories (Id, Name, Description, CreatedAt, IsDeleted) VALUES (4, N'Photography', N'Photo & video coverage', '2026-01-01T00:00:00', 0);
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id = 5)
    INSERT INTO dbo.Categories (Id, Name, Description, CreatedAt, IsDeleted) VALUES (5, N'Makeup Artist', N'Bridal & event makeup', '2026-01-01T00:00:00', 0);
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id = 6)
    INSERT INTO dbo.Categories (Id, Name, Description, CreatedAt, IsDeleted) VALUES (6, N'Decoration', N'Styling, tablescapes, lighting', '2026-01-01T00:00:00', 0);
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id = 7)
    INSERT INTO dbo.Categories (Id, Name, Description, CreatedAt, IsDeleted) VALUES (7, N'Music', N'Live bands, DJs, ensembles', '2026-01-01T00:00:00', 0);
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id = 8)
    INSERT INTO dbo.Categories (Id, Name, Description, CreatedAt, IsDeleted) VALUES (8, N'Planning', N'Full-service event coordination', '2026-01-01T00:00:00', 0);

SET IDENTITY_INSERT dbo.Categories OFF;
GO

PRINT 'AddAdminModuleAndCategorySeed.sql applied successfully.';
