/*
    AddVendorUploadColumns.sql
    ---------------------------------------------------------------------------
    Adds the columns VendorProfileConfiguration.cs now describes for real
    file uploads collected during vendor registration:

      CoverImageUrl               - public marketing image (wwwroot, has a URL)
      CommercialRegistrationPath  - verification document (private, opaque path)
      NationalIdPath              - verification document (private, opaque path)
      BusinessLicensePath         - verification document (private, opaque path)

    The three "Path" columns are NOT public URLs - they only resolve through
    the admin-only GET /api/admin/vendors/{id}/documents/{type} endpoint.
    LogoUrl already existed (unrelated to this script) and is reused as-is
    for the registration wizard's "Business Logo" upload.

    HOW TO APPLY: SSMS / Azure Data Studio / sqlcmd / MonsterASP Run T-SQL
    against EventHubDb. Safe to re-run - every column add is guarded.
*/

SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.VendorProfiles') AND name = 'CoverImageUrl')
    ALTER TABLE dbo.VendorProfiles ADD CoverImageUrl nvarchar(500) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.VendorProfiles') AND name = 'CommercialRegistrationPath')
    ALTER TABLE dbo.VendorProfiles ADD CommercialRegistrationPath nvarchar(500) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.VendorProfiles') AND name = 'NationalIdPath')
    ALTER TABLE dbo.VendorProfiles ADD NationalIdPath nvarchar(500) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.VendorProfiles') AND name = 'BusinessLicensePath')
    ALTER TABLE dbo.VendorProfiles ADD BusinessLicensePath nvarchar(500) NULL;

PRINT 'AddVendorUploadColumns.sql applied successfully.';
