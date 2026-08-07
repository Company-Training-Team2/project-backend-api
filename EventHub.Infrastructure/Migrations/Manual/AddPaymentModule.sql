/*
    AddPaymentModule.sql
    ---------------------------------------------------------------------------
    Hand-written, idempotent schema update for the Payment module.

    WHY THIS FILE EXISTS:
    The sandbox this code was written in has no .NET SDK and no network access,
    so `dotnet ef migrations add` could not be run here. This script applies the
    exact same schema shape that PaymentConfiguration / VendorProfileConfiguration
    / PayoutConfiguration now describe in code, so the database and the C# model
    stay in sync without requiring you to run any tooling.

    HOW TO APPLY:
    Run this script once against your EventHubDb database (SSMS, Azure Data
    Studio, sqlcmd, or `dotnet ef` won't complain either way since
    PendingModelChangesWarning is already suppressed in Program.cs).

    Safe to re-run: every step is guarded, so running it twice is a no-op.

    RECOMMENDED ALTERNATIVE:
    If you have the .NET SDK locally, it's cleaner to let EF generate + track
    this properly instead:
        dotnet ef migrations add AddPaymentModule --project EventHub.Infrastructure --startup-project EventHub.API
        dotnet ef database update --project EventHub.Infrastructure --startup-project EventHub.API
    That will also refresh Migrations/ApplicationDbContextModelSnapshot.cs,
    which this script intentionally does not touch.
*/

SET NOCOUNT ON;

-- ─── 1) Payments: commission snapshot columns ──────────────────────────────
IF COL_LENGTH('dbo.Payments', 'GrossAmount') IS NULL
BEGIN
    ALTER TABLE dbo.Payments ADD GrossAmount decimal(18,2) NOT NULL CONSTRAINT DF_Payments_GrossAmount DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Payments', 'CommissionRateSnapshot') IS NULL
BEGIN
    ALTER TABLE dbo.Payments ADD CommissionRateSnapshot decimal(5,4) NOT NULL CONSTRAINT DF_Payments_CommissionRateSnapshot DEFAULT (0.10);
END
GO

IF COL_LENGTH('dbo.Payments', 'PlatformFeeAmount') IS NULL
BEGIN
    ALTER TABLE dbo.Payments ADD PlatformFeeAmount decimal(18,2) NOT NULL CONSTRAINT DF_Payments_PlatformFeeAmount DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Payments', 'VendorPayoutAmount') IS NULL
BEGIN
    ALTER TABLE dbo.Payments ADD VendorPayoutAmount decimal(18,2) NOT NULL CONSTRAINT DF_Payments_VendorPayoutAmount DEFAULT (0);
END
GO

-- Backfill any pre-existing Paid rows so old data isn't left at 0/0.10 defaults.
UPDATE dbo.Payments
SET
    GrossAmount = Amount,
    PlatformFeeAmount = ROUND(Amount * 0.10, 2),
    VendorPayoutAmount = Amount - ROUND(Amount * 0.10, 2)
WHERE GrossAmount = 0 AND Amount <> 0;
GO

-- ─── 2) VendorProfiles: bank account columns (required for Payout) ────────
IF COL_LENGTH('dbo.VendorProfiles', 'BankName') IS NULL
BEGIN
    ALTER TABLE dbo.VendorProfiles ADD BankName nvarchar(200) NULL;
END
GO

IF COL_LENGTH('dbo.VendorProfiles', 'AccountName') IS NULL
BEGIN
    ALTER TABLE dbo.VendorProfiles ADD AccountName nvarchar(200) NULL;
END
GO

IF COL_LENGTH('dbo.VendorProfiles', 'AccountNumber') IS NULL
BEGIN
    ALTER TABLE dbo.VendorProfiles ADD AccountNumber nvarchar(50) NULL;
END
GO

-- ─── 3) Payouts table ───────────────────────────────────────────────────────
IF OBJECT_ID('dbo.Payouts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payouts
    (
        Id               int IDENTITY(1,1) NOT NULL,
        VendorProfileId  int NOT NULL,
        PaymentId        int NOT NULL,
        Amount           decimal(18,2) NOT NULL,
        Status           int NOT NULL,
        ProcessedAt      datetime2 NULL,
        CreatedAt        datetime2 NOT NULL CONSTRAINT DF_Payouts_CreatedAt DEFAULT (GETUTCDATE()),
        UpdatedAt        datetime2 NULL,

        CONSTRAINT PK_Payouts PRIMARY KEY (Id),
        CONSTRAINT FK_Payouts_VendorProfiles_VendorProfileId
            FOREIGN KEY (VendorProfileId) REFERENCES dbo.VendorProfiles (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Payouts_Payments_PaymentId
            FOREIGN KEY (PaymentId) REFERENCES dbo.Payments (Id) ON DELETE NO ACTION
    );

    CREATE INDEX IX_Payouts_VendorProfileId ON dbo.Payouts (VendorProfileId);
    CREATE UNIQUE INDEX IX_Payouts_PaymentId ON dbo.Payouts (PaymentId);
    CREATE INDEX IX_Payouts_Status ON dbo.Payouts (Status);
END
GO

PRINT 'AddPaymentModule.sql applied successfully.';
