/*
    AddVendorProfileCategories.sql
    ---------------------------------------------------------------------------
    Hand-written, idempotent schema update for vendor registration's Step 2
    category picker ("Which services do you offer? — choose up to 3").
    Previously that selection was collected in the UI and then silently
    discarded on submit (RegisterRequest.cs had no field for it). This adds
    the join table so it's actually saved, one row per vendor per category
    they selected — VendorProfileCategory / VendorProfileCategoryConfiguration.

    Distinct from WorkPosts.CategoryId, which is the single category a
    specific service listing belongs to — this table is the vendor-level
    "what kind of services do you offer" set captured at signup.

    HOW TO APPLY:
    Run this script once against your EventHubDb database (SSMS, Azure Data
    Studio, sqlcmd). Safe to re-run: every step is guarded.

    RECOMMENDED ALTERNATIVE: if you have the .NET SDK locally —
        dotnet ef migrations add AddVendorProfileCategories --project EventHub.Infrastructure --startup-project EventHub.API
        dotnet ef database update --project EventHub.Infrastructure --startup-project EventHub.API
    That will also refresh Migrations/ApplicationDbContextModelSnapshot.cs,
    which this script intentionally does not touch. Not used here because the
    live model snapshot has drifted from several other manually-applied
    modules (Messaging, Admin, Payments) — regenerating now would try to bundle
    all of that unrelated drift into one migration.
*/

SET NOCOUNT ON;

-- ─── VendorProfileCategories table ──────────────────────────────────────────
IF OBJECT_ID('dbo.VendorProfileCategories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.VendorProfileCategories
    (
        Id              int IDENTITY(1,1) NOT NULL,
        VendorProfileId int NOT NULL,
        CategoryId      int NOT NULL,

        CONSTRAINT PK_VendorProfileCategories PRIMARY KEY (Id),
        CONSTRAINT FK_VendorProfileCategories_VendorProfiles_VendorProfileId
            FOREIGN KEY (VendorProfileId) REFERENCES dbo.VendorProfiles (Id) ON DELETE CASCADE,
        CONSTRAINT FK_VendorProfileCategories_Categories_CategoryId
            FOREIGN KEY (CategoryId) REFERENCES dbo.Categories (Id) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX UX_VendorProfileCategories_VendorProfileId_CategoryId
        ON dbo.VendorProfileCategories (VendorProfileId, CategoryId);
END
GO

PRINT 'AddVendorProfileCategories.sql applied successfully.';
