IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] int NOT NULL IDENTITY,
        [Role] int NOT NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [DeletedAt] datetime2 NULL,
        [DeletedBy] int NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [IsEmailVerified] bit NOT NULL DEFAULT CAST(0 AS bit),
        [EmailVerificationCode] nvarchar(6) NULL,
        [EmailVerificationExpiry] datetime2 NULL,
        [PasswordResetCode] nvarchar(6) NULL,
        [PasswordResetCodeExpiry] datetime2 NULL,
        [RefreshToken] nvarchar(500) NULL,
        [RefreshTokenExpiry] datetime2 NULL,
        [MfaSecret] nvarchar(200) NULL,
        [IsMfaEnabled] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] int NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] int NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [CustomerProfiles] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [PhoneNumber] nvarchar(20) NULL,
        [City] nvarchar(100) NULL,
        [AvatarUrl] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_CustomerProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CustomerProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Type] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Body] nvarchar(1000) NOT NULL,
        [IsRead] bit NOT NULL DEFAULT CAST(0 AS bit),
        [RelatedEntityId] int NULL,
        [UserId1] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Notifications_AspNetUsers_UserId1] FOREIGN KEY ([UserId1]) REFERENCES [AspNetUsers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [VendorProfiles] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [BusinessName] nvarchar(200) NOT NULL,
        [BioDescription] nvarchar(2000) NOT NULL,
        [PhoneNumber] nvarchar(20) NULL,
        [City] nvarchar(100) NULL,
        [LogoUrl] nvarchar(500) NULL,
        [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit),
        [ApprovalStatus] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] int NULL,
        CONSTRAINT [PK_VendorProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VendorProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [Events] (
        [Id] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [EventType] int NOT NULL,
        [TargetDate] datetime2 NOT NULL,
        [GuestCount] int NOT NULL,
        [TotalBudget] decimal(18,2) NOT NULL,
        [City] nvarchar(100) NOT NULL,
        [Location] nvarchar(250) NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] int NULL,
        CONSTRAINT [PK_Events] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Events_CustomerProfiles_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [CustomerProfiles] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [WorkPosts] (
        [Id] int NOT NULL IDENTITY,
        [VendorProfileId] int NOT NULL,
        [CategoryId] int NOT NULL,
        [ReviewedByAdminId] int NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(3000) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [City] nvarchar(100) NOT NULL,
        [Address] nvarchar(300) NOT NULL,
        [ApprovalStatus] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] int NULL,
        CONSTRAINT [PK_WorkPosts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkPosts_AspNetUsers_ReviewedByAdminId] FOREIGN KEY ([ReviewedByAdminId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_WorkPosts_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_WorkPosts_VendorProfiles_VendorProfileId] FOREIGN KEY ([VendorProfileId]) REFERENCES [VendorProfiles] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [ChecklistItems] (
        [Id] int NOT NULL IDENTITY,
        [EventId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [DueDate] datetime2 NULL,
        [Priority] int NOT NULL,
        [IsCompleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [Category] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ChecklistItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChecklistItems_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [Documents] (
        [Id] int NOT NULL IDENTITY,
        [EventId] int NOT NULL,
        [Type] int NOT NULL,
        [FileName] nvarchar(255) NOT NULL,
        [FileUrl] nvarchar(1000) NULL,
        [UploadedAt] datetime2 NOT NULL,
        [Amount] decimal(18,2) NULL,
        [Status] nvarchar(50) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Documents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Documents_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [Guests] (
        [Id] int NOT NULL IDENTITY,
        [EventId] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Email] nvarchar(255) NULL,
        [PhoneNumber] nvarchar(20) NULL,
        [RSVPStatus] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Guests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Guests_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [Bookings] (
        [Id] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [EventId] int NOT NULL,
        [WorkPostId] int NOT NULL,
        [BookingDate] date NOT NULL,
        [Status] int NOT NULL,
        [TotalPrice] decimal(18,2) NOT NULL,
        [Quantity] int NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Bookings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Bookings_CustomerProfiles_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [CustomerProfiles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Bookings_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Bookings_WorkPosts_WorkPostId] FOREIGN KEY ([WorkPostId]) REFERENCES [WorkPosts] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [Favorites] (
        [Id] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [WorkPostId] int NOT NULL,
        CONSTRAINT [PK_Favorites] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Favorites_CustomerProfiles_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [CustomerProfiles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Favorites_WorkPosts_WorkPostId] FOREIGN KEY ([WorkPostId]) REFERENCES [WorkPosts] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [ServicePackages] (
        [Id] int NOT NULL IDENTITY,
        [WorkPostId] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Price] decimal(18,2) NOT NULL,
        [Includes] nvarchar(2000) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ServicePackages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ServicePackages_WorkPosts_WorkPostId] FOREIGN KEY ([WorkPostId]) REFERENCES [WorkPosts] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [WorkPostAvailabilities] (
        [Id] int NOT NULL IDENTITY,
        [WorkPostId] int NOT NULL,
        [Date] date NOT NULL,
        [IsAvailable] bit NOT NULL DEFAULT CAST(1 AS bit),
        [Notes] nvarchar(1000) NULL,
        CONSTRAINT [PK_WorkPostAvailabilities] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkPostAvailabilities_WorkPosts_WorkPostId] FOREIGN KEY ([WorkPostId]) REFERENCES [WorkPosts] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [WorkPostImages] (
        [Id] int NOT NULL IDENTITY,
        [WorkPostId] int NOT NULL,
        [ImageUrl] nvarchar(1000) NOT NULL,
        [IsPrimary] bit NOT NULL DEFAULT CAST(0 AS bit),
        [UploadedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WorkPostImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkPostImages_WorkPosts_WorkPostId] FOREIGN KEY ([WorkPostId]) REFERENCES [WorkPosts] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [Expenses] (
        [Id] int NOT NULL IDENTITY,
        [EventId] int NOT NULL,
        [Category] nvarchar(100) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [Date] datetime2 NOT NULL,
        [BookingId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Expenses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Expenses_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Expenses_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Events] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] int NOT NULL IDENTITY,
        [BookingId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentMethod] int NOT NULL,
        [PaymentStatus] int NOT NULL,
        [PaidAt] datetime2 NULL,
        [TransactionId] nvarchar(100) NULL,
        [PaymentGateway] nvarchar(100) NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE TABLE [Reviews] (
        [Id] int NOT NULL IDENTITY,
        [BookingId] int NOT NULL,
        [Rating] int NOT NULL,
        [Comment] nvarchar(1000) NULL,
        [CustomerProfileId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Reviews] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Review_Rating] CHECK ([Rating] >= 1 AND [Rating] <= 5),
        CONSTRAINT [FK_Reviews_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Reviews_CustomerProfiles_CustomerProfileId] FOREIGN KEY ([CustomerProfileId]) REFERENCES [CustomerProfiles] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] ON;
    EXEC(N'INSERT INTO [AspNetRoles] ([Id], [ConcurrencyStamp], [Name], [NormalizedName])
    VALUES (3, N''7e2521e8-3134-4a84-b17a-34977f84515f'', N''Admin'', N''ADMIN'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_EmailVerificationCode] ON [AspNetUsers] ([EmailVerificationCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_IsActive] ON [AspNetUsers] ([IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_IsDeleted] ON [AspNetUsers] ([IsDeleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_IsEmailVerified] ON [AspNetUsers] ([IsEmailVerified]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_PasswordResetCode] ON [AspNetUsers] ([PasswordResetCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_RefreshToken] ON [AspNetUsers] ([RefreshToken]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_Role] ON [AspNetUsers] ([Role]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookings_CustomerId] ON [Bookings] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookings_EventId] ON [Bookings] ([EventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookings_Status] ON [Bookings] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Bookings_WorkPostId] ON [Bookings] ([WorkPostId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Categories_IsDeleted] ON [Categories] ([IsDeleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categories_Name] ON [Categories] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChecklistItems_EventId] ON [ChecklistItems] ([EventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChecklistItems_IsCompleted] ON [ChecklistItems] ([IsCompleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChecklistItems_Priority] ON [ChecklistItems] ([Priority]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomerProfiles_FullName] ON [CustomerProfiles] ([FullName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CustomerProfiles_UserId] ON [CustomerProfiles] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Documents_EventId] ON [Documents] ([EventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Documents_Type] ON [Documents] ([Type]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Documents_UploadedAt] ON [Documents] ([UploadedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Events_CustomerId] ON [Events] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Events_EventType] ON [Events] ([EventType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Events_TargetDate] ON [Events] ([TargetDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Expenses_BookingId] ON [Expenses] ([BookingId]) WHERE [BookingId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Expenses_Date] ON [Expenses] ([Date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Expenses_EventId] ON [Expenses] ([EventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Expenses_Status] ON [Expenses] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Favorites_CustomerId] ON [Favorites] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Favorites_CustomerId_WorkPostId] ON [Favorites] ([CustomerId], [WorkPostId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Favorites_WorkPostId] ON [Favorites] ([WorkPostId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Guests_Email] ON [Guests] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Guests_EventId] ON [Guests] ([EventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Guests_RSVPStatus] ON [Guests] ([RSVPStatus]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_CreatedAt] ON [Notifications] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_IsRead] ON [Notifications] ([IsRead]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_Type] ON [Notifications] ([Type]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId1] ON [Notifications] ([UserId1]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Payments_BookingId] ON [Payments] ([BookingId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_PaymentStatus] ON [Payments] ([PaymentStatus]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Payments_TransactionId] ON [Payments] ([TransactionId]) WHERE [TransactionId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Reviews_BookingId] ON [Reviews] ([BookingId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reviews_CustomerProfileId] ON [Reviews] ([CustomerProfileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reviews_Rating] ON [Reviews] ([Rating]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ServicePackages_IsActive] ON [ServicePackages] ([IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ServicePackages_WorkPostId] ON [ServicePackages] ([WorkPostId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ServicePackages_WorkPostId_Name] ON [ServicePackages] ([WorkPostId], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VendorProfiles_ApprovalStatus] ON [VendorProfiles] ([ApprovalStatus]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VendorProfiles_BusinessName] ON [VendorProfiles] ([BusinessName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VendorProfiles_IsDeleted] ON [VendorProfiles] ([IsDeleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VendorProfiles_IsVerified] ON [VendorProfiles] ([IsVerified]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VendorProfiles_UserId] ON [VendorProfiles] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkPostAvailabilities_Date] ON [WorkPostAvailabilities] ([Date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkPostAvailabilities_IsAvailable] ON [WorkPostAvailabilities] ([IsAvailable]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkPostAvailabilities_WorkPostId] ON [WorkPostAvailabilities] ([WorkPostId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_WorkPostAvailabilities_WorkPostId_Date] ON [WorkPostAvailabilities] ([WorkPostId], [Date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkPostImages_IsPrimary] ON [WorkPostImages] ([IsPrimary]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkPostImages_UploadedAt] ON [WorkPostImages] ([UploadedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkPostImages_WorkPostId] ON [WorkPostImages] ([WorkPostId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkPosts_ApprovalStatus] ON [WorkPosts] ([ApprovalStatus]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkPosts_CategoryId] ON [WorkPosts] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkPosts_CategoryId_City] ON [WorkPosts] ([CategoryId], [City]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkPosts_City] ON [WorkPosts] ([City]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkPosts_IsDeleted] ON [WorkPosts] ([IsDeleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkPosts_ReviewedByAdminId] ON [WorkPosts] ([ReviewedByAdminId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkPosts_VendorProfileId] ON [WorkPosts] ([VendorProfileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkPosts_VendorProfileId_ApprovalStatus] ON [WorkPosts] ([VendorProfileId], [ApprovalStatus]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260805171711_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260805171711_InitialCreate', N'9.0.10');
END;

COMMIT;
GO

