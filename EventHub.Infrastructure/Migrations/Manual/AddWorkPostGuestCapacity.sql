/*
    AddWorkPostGuestCapacity.sql
    ---------------------------------------------------------------------------
    Adds the two columns WorkPost.cs now describes for the vendor "Add
    Service" form's Min/Max Guests fields (Pricing & Logistics card):

      MinGuests   - nullable int, e.g. 50
      MaxGuests   - nullable int, e.g. 200

    Both null means no capacity range was set. These were UI-only before
    (kept in local component state, never sent to the API) — the frontend
    now sends them on create/update, so the columns need to exist for real.

    Companion EF migration 20260819175124_AddWorkPostGuestCapacity.cs also
    exists in this project for local/fresh databases via `dotnet ef database
    update`, but running that migration as-is against the LIVE database would
    fail: it was auto-generated from the current model, which had already
    drifted ahead of the last real migration (20260811021043_
    AddCategoriesSeedData) via several prior hand-written Manual/*.sql scripts
    - so it also tries to re-CreateTable Conversations/VendorProfileCategories
    and re-AddColumn the VendorProfiles upload paths, all of which already
    exist live. This script only does the two columns that are actually still
    missing.

    HOW TO APPLY: SSMS / Azure Data Studio / sqlcmd / MonsterASP Run T-SQL
    against EventHubDb. Safe to re-run - every column add is guarded.
*/

SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.WorkPosts') AND name = 'MinGuests')
    ALTER TABLE dbo.WorkPosts ADD MinGuests int NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.WorkPosts') AND name = 'MaxGuests')
    ALTER TABLE dbo.WorkPosts ADD MaxGuests int NULL;

PRINT 'AddWorkPostGuestCapacity.sql applied successfully.';
