/*
    SeedDemoData.sql
    ---------------------------------------------------------------------------
    Demo/test data so the site has something real to browse and click through:
    10 vendor accounts (5 in Cairo, 5 in Alexandria) with real, approved
    listings, images, tiered packages, and open availability dates — plus
    sample Events/Bookings/Payments/Reviews/Guests/Checklist/Expenses/
    Documents/Favorites attached to two EXISTING customer accounts
    (ayshakassem59@gmail.com, nourtarekhelmy11@gmail.com).

    Vendor business names are intentionally fictional (not real Cairo/
    Alexandria businesses) — inventing pricing, contact details, and
    reviews under an actual company's real name would misrepresent that
    company on the platform without its knowledge or consent. These are
    designed to feel authentic to the two cities instead.

    All 10 vendor accounts share one password so you can log into any of
    them to test the vendor portal: Vendor@123

    Safe to re-run — every block is guarded by an existence check.
*/

SET NOCOUNT ON;


-- ─── Vendor: Zamalek Garden Palace (Cairo) ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Email = N'zamalek-garden-palace@eventhub.test')
BEGIN
    DECLARE @UserId INT, @VendorProfileId INT, @WorkPostId INT;

    INSERT INTO dbo.AspNetUsers
        (Role, IsDeleted, IsActive, IsEmailVerified, IsMfaEnabled, CreatedAt, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (2, 0, 1, 1, 0, GETUTCDATE(), N'zamalek-garden-palace@eventhub.test', N'ZAMALEK-GARDEN-PALACE@EVENTHUB.TEST', N'zamalek-garden-palace@eventhub.test', N'ZAMALEK-GARDEN-PALACE@EVENTHUB.TEST', 1, N'AQAAAAEAAYagAAAAEBX6x3wjo67KuvBWSX4s6p7O9nOlOfc3Q9v2ZoqRW/984ye1l7OFKzpuP4fAnSJa/g==', CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), N'+201073812029', 0, 0, 1, 0);
    SET @UserId = SCOPE_IDENTITY();

    INSERT INTO dbo.VendorProfiles (UserId, BusinessName, BioDescription, PhoneNumber, City, IsVerified, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@UserId, N'Zamalek Garden Palace', N'A restored 1930s riverside villa on Zamalek''s quiet north end, with a landscaped garden marquee for up to 300 guests and a Nile-facing terrace for golden-hour photos.', N'+201073812029', N'Cairo', 1, 2, 0, GETUTCDATE());
    SET @VendorProfileId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPosts (VendorProfileId, CategoryId, Title, Description, Price, City, Address, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@VendorProfileId, 3, N'Zamalek Garden Palace — Riverside Wedding Venue', N'A restored 1930s riverside villa on Zamalek''s quiet north end, with a landscaped garden marquee for up to 300 guests and a Nile-facing terrace for golden-hour photos.', 45000, N'Cairo', N'14 Brazil St, Zamalek, Cairo', 2, 0, GETUTCDATE());
    SET @WorkPostId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPostImages (WorkPostId, ImageUrl, IsPrimary, UploadedAt) VALUES
        (@WorkPostId, N'https://images.unsplash.com/photo-1414235077428-338989a2e8c0?auto=format&fit=crop&w=800&q=80', 1, GETUTCDATE()),
        (@WorkPostId, N'https://images.unsplash.com/photo-1478147427282-58a87a120781?auto=format&fit=crop&w=800&q=80', 0, GETUTCDATE());

    INSERT INTO dbo.ServicePackages (WorkPostId, Name, Description, Price, Includes, IsActive, CreatedAt) VALUES
        (@WorkPostId, N'Garden Only (Half-Day)', NULL, 25000, N'6-hour access, garden marquee, basic lighting rig', 1, GETUTCDATE()),
        (@WorkPostId, N'Full Estate (Full-Day)', NULL, 45000, N'12-hour access, garden + indoor hall, in-house sound system, valet parking', 1, GETUTCDATE()),
        (@WorkPostId, N'Full Estate + Catering Partner', NULL, 78000, N'Everything in Full Estate, plus a 5-course plated dinner for up to 200 guests', 1, GETUTCDATE());

    INSERT INTO dbo.WorkPostAvailabilities (WorkPostId, Date, IsAvailable, Notes) VALUES
        (@WorkPostId, DATEADD(day, 8, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 15, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 22, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 29, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 36, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 45, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 60, CAST(GETUTCDATE() AS date)), 1, NULL);
END
GO

-- ─── Vendor: Nile Terrace Catering (Cairo) ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Email = N'nile-terrace-catering@eventhub.test')
BEGIN
    DECLARE @UserId INT, @VendorProfileId INT, @WorkPostId INT;

    INSERT INTO dbo.AspNetUsers
        (Role, IsDeleted, IsActive, IsEmailVerified, IsMfaEnabled, CreatedAt, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (2, 0, 1, 1, 0, GETUTCDATE(), N'nile-terrace-catering@eventhub.test', N'NILE-TERRACE-CATERING@EVENTHUB.TEST', N'nile-terrace-catering@eventhub.test', N'NILE-TERRACE-CATERING@EVENTHUB.TEST', 1, N'AQAAAAEAAYagAAAAEFkaUfofe7y+OK8d7ciC+i3xhhytEKxgNbfw/eSkyxFORyIXkYN45xrEBFaYFBBWFA==', CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), N'+201079400633', 0, 0, 1, 0);
    SET @UserId = SCOPE_IDENTITY();

    INSERT INTO dbo.VendorProfiles (UserId, BusinessName, BioDescription, PhoneNumber, City, IsVerified, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@UserId, N'Nile Terrace Catering', N'Family-run catering house serving Cairo weddings since 2011, known for a modern Egyptian-Mediterranean menu and a live cooking-station buffet option.', N'+201079400633', N'Cairo', 1, 2, 0, GETUTCDATE());
    SET @VendorProfileId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPosts (VendorProfileId, CategoryId, Title, Description, Price, City, Address, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@VendorProfileId, 1, N'Nile Terrace Catering — Plated & Buffet Wedding Menus', N'Family-run catering house serving Cairo weddings since 2011, known for a modern Egyptian-Mediterranean menu and a live cooking-station buffet option.', 350, N'Cairo', N'9 Nile Corniche, Maadi, Cairo', 2, 0, GETUTCDATE());
    SET @WorkPostId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPostImages (WorkPostId, ImageUrl, IsPrimary, UploadedAt) VALUES
        (@WorkPostId, N'https://images.unsplash.com/photo-1487070183336-b863922373d4?auto=format&fit=crop&w=800&q=80', 1, GETUTCDATE()),
        (@WorkPostId, N'https://images.unsplash.com/photo-1487412947147-5cebf100ffc2?auto=format&fit=crop&w=800&q=80', 0, GETUTCDATE());

    INSERT INTO dbo.ServicePackages (WorkPostId, Name, Description, Price, Includes, IsActive, CreatedAt) VALUES
        (@WorkPostId, N'Silver Buffet', NULL, 250, N'3 mains, 4 sides, soft drinks — buffet style', 1, GETUTCDATE()),
        (@WorkPostId, N'Gold Plated Menu', NULL, 350, N'4-course plated dinner, welcome mocktail, dessert table', 1, GETUTCDATE()),
        (@WorkPostId, N'Platinum Live Stations', NULL, 520, N'Live carving + pasta + dessert stations, full bar mocktails, dedicated waitstaff', 1, GETUTCDATE());

    INSERT INTO dbo.WorkPostAvailabilities (WorkPostId, Date, IsAvailable, Notes) VALUES
        (@WorkPostId, DATEADD(day, 8, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 15, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 22, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 29, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 36, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 45, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 60, CAST(GETUTCDATE() AS date)), 1, NULL);
END
GO

-- ─── Vendor: Cairo Frame Studio (Cairo) ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Email = N'cairo-frame-studio@eventhub.test')
BEGIN
    DECLARE @UserId INT, @VendorProfileId INT, @WorkPostId INT;

    INSERT INTO dbo.AspNetUsers
        (Role, IsDeleted, IsActive, IsEmailVerified, IsMfaEnabled, CreatedAt, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (2, 0, 1, 1, 0, GETUTCDATE(), N'cairo-frame-studio@eventhub.test', N'CAIRO-FRAME-STUDIO@EVENTHUB.TEST', N'cairo-frame-studio@eventhub.test', N'CAIRO-FRAME-STUDIO@EVENTHUB.TEST', 1, N'AQAAAAEAAYagAAAAEPw7trmRILZkQl7C23NzdkkSy16U0t7ravpZ8tyTVrBwYOI/5P1m1w9jlbiv1r07kg==', CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), N'+201029913194', 0, 0, 1, 0);
    SET @UserId = SCOPE_IDENTITY();

    INSERT INTO dbo.VendorProfiles (UserId, BusinessName, BioDescription, PhoneNumber, City, IsVerified, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@UserId, N'Cairo Frame Studio', N'A three-photographer studio specializing in documentary-style wedding coverage across Cairo and the North Coast, with a same-week sneak-peek gallery.', N'+201029913194', N'Cairo', 1, 2, 0, GETUTCDATE());
    SET @VendorProfileId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPosts (VendorProfileId, CategoryId, Title, Description, Price, City, Address, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@VendorProfileId, 4, N'Cairo Frame Studio — Wedding & Engagement Photography', N'A three-photographer studio specializing in documentary-style wedding coverage across Cairo and the North Coast, with a same-week sneak-peek gallery.', 12000, N'Cairo', N'22 Talaat Harb St, Downtown Cairo', 2, 0, GETUTCDATE());
    SET @WorkPostId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPostImages (WorkPostId, ImageUrl, IsPrimary, UploadedAt) VALUES
        (@WorkPostId, N'https://images.unsplash.com/photo-1492684223066-81342ee5ff30?auto=format&fit=crop&w=800&q=80', 1, GETUTCDATE()),
        (@WorkPostId, N'https://images.unsplash.com/photo-1511285560929-80b456fea0bc?auto=format&fit=crop&w=800&q=80', 0, GETUTCDATE());

    INSERT INTO dbo.ServicePackages (WorkPostId, Name, Description, Price, Includes, IsActive, CreatedAt) VALUES
        (@WorkPostId, N'Essential (4 Hours)', NULL, 7000, N'1 photographer, 4 hours coverage, 200+ edited photos', 1, GETUTCDATE()),
        (@WorkPostId, N'Signature (8 Hours)', NULL, 12000, N'2 photographers, 8 hours coverage, 500+ edited photos, engagement session', 1, GETUTCDATE()),
        (@WorkPostId, N'Premium (Full Day + Album)', NULL, 19000, N'2 photographers + 1 videographer, full-day coverage, printed album', 1, GETUTCDATE());

    INSERT INTO dbo.WorkPostAvailabilities (WorkPostId, Date, IsAvailable, Notes) VALUES
        (@WorkPostId, DATEADD(day, 8, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 15, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 22, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 29, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 36, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 45, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 60, CAST(GETUTCDATE() AS date)), 1, NULL);
END
GO

-- ─── Vendor: Qasr El Nil Events (Cairo) ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Email = N'qasr-el-nil-events@eventhub.test')
BEGIN
    DECLARE @UserId INT, @VendorProfileId INT, @WorkPostId INT;

    INSERT INTO dbo.AspNetUsers
        (Role, IsDeleted, IsActive, IsEmailVerified, IsMfaEnabled, CreatedAt, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (2, 0, 1, 1, 0, GETUTCDATE(), N'qasr-el-nil-events@eventhub.test', N'QASR-EL-NIL-EVENTS@EVENTHUB.TEST', N'qasr-el-nil-events@eventhub.test', N'QASR-EL-NIL-EVENTS@EVENTHUB.TEST', 1, N'AQAAAAEAAYagAAAAEM3aXYOzJ0OJlmYB7df9LwLxh8QnaGEBVeTwcC1lI97rh9LQEN1cx77JrHkGtamP0A==', CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), N'+201000527428', 0, 0, 1, 0);
    SET @UserId = SCOPE_IDENTITY();

    INSERT INTO dbo.VendorProfiles (UserId, BusinessName, BioDescription, PhoneNumber, City, IsVerified, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@UserId, N'Qasr El Nil Events', N'Boutique planning studio managing every vendor, timeline, and on-site detail for 15-20 weddings a year across Cairo''s top venues.', N'+201000527428', N'Cairo', 1, 2, 0, GETUTCDATE());
    SET @VendorProfileId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPosts (VendorProfileId, CategoryId, Title, Description, Price, City, Address, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@VendorProfileId, 8, N'Qasr El Nil Events — Full-Service Wedding Planning', N'Boutique planning studio managing every vendor, timeline, and on-site detail for 15-20 weddings a year across Cairo''s top venues.', 60000, N'Cairo', N'5 Qasr El Nil St, Garden City, Cairo', 2, 0, GETUTCDATE());
    SET @WorkPostId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPostImages (WorkPostId, ImageUrl, IsPrimary, UploadedAt) VALUES
        (@WorkPostId, N'https://images.unsplash.com/photo-1511578314322-379afb476865?auto=format&fit=crop&w=800&q=80', 1, GETUTCDATE()),
        (@WorkPostId, N'https://images.unsplash.com/photo-1511795409834-ef04bbd61622?auto=format&fit=crop&w=800&q=80', 0, GETUTCDATE());

    INSERT INTO dbo.ServicePackages (WorkPostId, Name, Description, Price, Includes, IsActive, CreatedAt) VALUES
        (@WorkPostId, N'Day-of Coordination', NULL, 20000, N'On-site coordination for the event day only, timeline management', 1, GETUTCDATE()),
        (@WorkPostId, N'Partial Planning', NULL, 40000, N'Vendor shortlist + booking support, starting 3 months out', 1, GETUTCDATE()),
        (@WorkPostId, N'Full-Service Planning', NULL, 60000, N'End-to-end planning from budget to execution, dedicated planner', 1, GETUTCDATE());

    INSERT INTO dbo.WorkPostAvailabilities (WorkPostId, Date, IsAvailable, Notes) VALUES
        (@WorkPostId, DATEADD(day, 8, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 15, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 22, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 29, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 36, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 45, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 60, CAST(GETUTCDATE() AS date)), 1, NULL);
END
GO

-- ─── Vendor: Cairo Sound Ensemble (Cairo) ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Email = N'cairo-sound-ensemble@eventhub.test')
BEGIN
    DECLARE @UserId INT, @VendorProfileId INT, @WorkPostId INT;

    INSERT INTO dbo.AspNetUsers
        (Role, IsDeleted, IsActive, IsEmailVerified, IsMfaEnabled, CreatedAt, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (2, 0, 1, 1, 0, GETUTCDATE(), N'cairo-sound-ensemble@eventhub.test', N'CAIRO-SOUND-ENSEMBLE@EVENTHUB.TEST', N'cairo-sound-ensemble@eventhub.test', N'CAIRO-SOUND-ENSEMBLE@EVENTHUB.TEST', 1, N'AQAAAAEAAYagAAAAED3mTgwsfvbiMWQLp1G4ahjFPlFd/CkEtvnXN2FQOtQf4QWn9kdKKmJk+dh25Wfv3g==', CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), N'+201038221481', 0, 0, 1, 0);
    SET @UserId = SCOPE_IDENTITY();

    INSERT INTO dbo.VendorProfiles (UserId, BusinessName, BioDescription, PhoneNumber, City, IsVerified, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@UserId, N'Cairo Sound Ensemble', N'A 6-piece live band and resident DJ duo covering Arabic classics, modern pop, and first-dance sets for weddings and corporate galas.', N'+201038221481', N'Cairo', 1, 2, 0, GETUTCDATE());
    SET @VendorProfileId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPosts (VendorProfileId, CategoryId, Title, Description, Price, City, Address, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@VendorProfileId, 7, N'Cairo Sound Ensemble — Live Band & DJ', N'A 6-piece live band and resident DJ duo covering Arabic classics, modern pop, and first-dance sets for weddings and corporate galas.', 15000, N'Cairo', N'18 El Ahram St, Heliopolis, Cairo', 2, 0, GETUTCDATE());
    SET @WorkPostId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPostImages (WorkPostId, ImageUrl, IsPrimary, UploadedAt) VALUES
        (@WorkPostId, N'https://images.unsplash.com/photo-1519167758481-83f550bb49b3?auto=format&fit=crop&w=800&q=80', 1, GETUTCDATE()),
        (@WorkPostId, N'https://images.unsplash.com/photo-1519225421980-715cb0215aed?auto=format&fit=crop&w=800&q=80', 0, GETUTCDATE());

    INSERT INTO dbo.ServicePackages (WorkPostId, Name, Description, Price, Includes, IsActive, CreatedAt) VALUES
        (@WorkPostId, N'DJ Set (4 Hours)', NULL, 8000, N'Resident DJ, sound system for up to 200 guests', 1, GETUTCDATE()),
        (@WorkPostId, N'Live Trio + DJ', NULL, 15000, N'3-piece live band for ceremony/dinner, DJ for the after-party', 1, GETUTCDATE()),
        (@WorkPostId, N'Full Ensemble', NULL, 24000, N'6-piece live band, DJ, MC, and full PA/lighting rig', 1, GETUTCDATE());

    INSERT INTO dbo.WorkPostAvailabilities (WorkPostId, Date, IsAvailable, Notes) VALUES
        (@WorkPostId, DATEADD(day, 8, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 15, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 22, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 29, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 36, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 45, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 60, CAST(GETUTCDATE() AS date)), 1, NULL);
END
GO

-- ─── Vendor: Corniche Grand Hall (Alexandria) ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Email = N'corniche-grand-hall@eventhub.test')
BEGIN
    DECLARE @UserId INT, @VendorProfileId INT, @WorkPostId INT;

    INSERT INTO dbo.AspNetUsers
        (Role, IsDeleted, IsActive, IsEmailVerified, IsMfaEnabled, CreatedAt, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (2, 0, 1, 1, 0, GETUTCDATE(), N'corniche-grand-hall@eventhub.test', N'CORNICHE-GRAND-HALL@EVENTHUB.TEST', N'corniche-grand-hall@eventhub.test', N'CORNICHE-GRAND-HALL@EVENTHUB.TEST', 1, N'AQAAAAEAAYagAAAAEN7tdj/J2/Jh2PVjUJxVElvwziKfHENWt0+t+j/aG+MNXk34Gt8/afmslPwcs1GQkw==', CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), N'+201081193947', 0, 0, 1, 0);
    SET @UserId = SCOPE_IDENTITY();

    INSERT INTO dbo.VendorProfiles (UserId, BusinessName, BioDescription, PhoneNumber, City, IsVerified, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@UserId, N'Corniche Grand Hall', N'A seafront ballroom on the Alexandria Corniche with floor-to-ceiling sea views, seating up to 400, and an in-house AV team.', N'+201081193947', N'Alexandria', 1, 2, 0, GETUTCDATE());
    SET @VendorProfileId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPosts (VendorProfileId, CategoryId, Title, Description, Price, City, Address, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@VendorProfileId, 3, N'Corniche Grand Hall — Seafront Ballroom', N'A seafront ballroom on the Alexandria Corniche with floor-to-ceiling sea views, seating up to 400, and an in-house AV team.', 55000, N'Alexandria', N'Corniche Rd, Sidi Gaber, Alexandria', 2, 0, GETUTCDATE());
    SET @WorkPostId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPostImages (WorkPostId, ImageUrl, IsPrimary, UploadedAt) VALUES
        (@WorkPostId, N'https://images.unsplash.com/photo-1519671482749-fd09be7ccebf?auto=format&fit=crop&w=800&q=80', 1, GETUTCDATE()),
        (@WorkPostId, N'https://images.unsplash.com/photo-1519741497674-611481863552?auto=format&fit=crop&w=800&q=80', 0, GETUTCDATE());

    INSERT INTO dbo.ServicePackages (WorkPostId, Name, Description, Price, Includes, IsActive, CreatedAt) VALUES
        (@WorkPostId, N'Ballroom Only (Half-Day)', NULL, 32000, N'6-hour access, standard round-table seating for 250', 1, GETUTCDATE()),
        (@WorkPostId, N'Ballroom Full-Day', NULL, 55000, N'12-hour access, sea-view terrace, in-house AV and lighting', 1, GETUTCDATE()),
        (@WorkPostId, N'Ballroom + Bridal Suite', NULL, 68000, N'Everything in Full-Day, plus a private bridal suite for the day', 1, GETUTCDATE());

    INSERT INTO dbo.WorkPostAvailabilities (WorkPostId, Date, IsAvailable, Notes) VALUES
        (@WorkPostId, DATEADD(day, 8, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 15, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 22, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 29, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 36, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 45, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 60, CAST(GETUTCDATE() AS date)), 1, NULL);
END
GO

-- ─── Vendor: Alexandria Bloom Atelier (Alexandria) ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Email = N'alexandria-bloom-atelier@eventhub.test')
BEGIN
    DECLARE @UserId INT, @VendorProfileId INT, @WorkPostId INT;

    INSERT INTO dbo.AspNetUsers
        (Role, IsDeleted, IsActive, IsEmailVerified, IsMfaEnabled, CreatedAt, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (2, 0, 1, 1, 0, GETUTCDATE(), N'alexandria-bloom-atelier@eventhub.test', N'ALEXANDRIA-BLOOM-ATELIER@EVENTHUB.TEST', N'alexandria-bloom-atelier@eventhub.test', N'ALEXANDRIA-BLOOM-ATELIER@EVENTHUB.TEST', 1, N'AQAAAAEAAYagAAAAEHPH8Af2holHQ5DFVBZiwOx//4GRfIoXkN9GbW9vsr0u505/CtuPx8NCUqhkjQlwFQ==', CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), N'+201027888137', 0, 0, 1, 0);
    SET @UserId = SCOPE_IDENTITY();

    INSERT INTO dbo.VendorProfiles (UserId, BusinessName, BioDescription, PhoneNumber, City, IsVerified, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@UserId, N'Alexandria Bloom Atelier', N'A floral design studio sourcing seasonal blooms from the Delta for bridal bouquets, ceremony arches, and reception centerpieces.', N'+201027888137', N'Alexandria', 1, 2, 0, GETUTCDATE());
    SET @VendorProfileId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPosts (VendorProfileId, CategoryId, Title, Description, Price, City, Address, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@VendorProfileId, 2, N'Alexandria Bloom Atelier — Bridal & Event Florals', N'A floral design studio sourcing seasonal blooms from the Delta for bridal bouquets, ceremony arches, and reception centerpieces.', 8000, N'Alexandria', N'31 El Horreya Rd, Sporting, Alexandria', 2, 0, GETUTCDATE());
    SET @WorkPostId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPostImages (WorkPostId, ImageUrl, IsPrimary, UploadedAt) VALUES
        (@WorkPostId, N'https://images.unsplash.com/photo-1520854221256-17451cc331bf?auto=format&fit=crop&w=800&q=80', 1, GETUTCDATE()),
        (@WorkPostId, N'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=800&q=80', 0, GETUTCDATE());

    INSERT INTO dbo.ServicePackages (WorkPostId, Name, Description, Price, Includes, IsActive, CreatedAt) VALUES
        (@WorkPostId, N'Ceremony Only', NULL, 4500, N'Bridal bouquet, 2 bridesmaid bouquets, ceremony arch', 1, GETUTCDATE()),
        (@WorkPostId, N'Ceremony + Reception', NULL, 8000, N'Everything in Ceremony, plus 10 table centerpieces', 1, GETUTCDATE()),
        (@WorkPostId, N'Full Floral Design', NULL, 14000, N'Full venue floral design including stage backdrop and hanging installations', 1, GETUTCDATE());

    INSERT INTO dbo.WorkPostAvailabilities (WorkPostId, Date, IsAvailable, Notes) VALUES
        (@WorkPostId, DATEADD(day, 8, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 15, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 22, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 29, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 36, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 45, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 60, CAST(GETUTCDATE() AS date)), 1, NULL);
END
GO

-- ─── Vendor: Mediterranean Glam Studio (Alexandria) ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Email = N'mediterranean-glam-studio@eventhub.test')
BEGIN
    DECLARE @UserId INT, @VendorProfileId INT, @WorkPostId INT;

    INSERT INTO dbo.AspNetUsers
        (Role, IsDeleted, IsActive, IsEmailVerified, IsMfaEnabled, CreatedAt, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (2, 0, 1, 1, 0, GETUTCDATE(), N'mediterranean-glam-studio@eventhub.test', N'MEDITERRANEAN-GLAM-STUDIO@EVENTHUB.TEST', N'mediterranean-glam-studio@eventhub.test', N'MEDITERRANEAN-GLAM-STUDIO@EVENTHUB.TEST', 1, N'AQAAAAEAAYagAAAAELgrJxBUtebLC8ItJBhJ9Cw4jhuzUMXsLZaQWXFP37rPpGn3Vn5n8CoLafjUNZJgRg==', CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), N'+201004307689', 0, 0, 1, 0);
    SET @UserId = SCOPE_IDENTITY();

    INSERT INTO dbo.VendorProfiles (UserId, BusinessName, BioDescription, PhoneNumber, City, IsVerified, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@UserId, N'Mediterranean Glam Studio', N'A bridal beauty studio offering airbrush makeup and hair styling, with a mobile team available for on-location sessions across Alexandria.', N'+201004307689', N'Alexandria', 1, 2, 0, GETUTCDATE());
    SET @VendorProfileId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPosts (VendorProfileId, CategoryId, Title, Description, Price, City, Address, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@VendorProfileId, 5, N'Mediterranean Glam Studio — Bridal Makeup & Hair', N'A bridal beauty studio offering airbrush makeup and hair styling, with a mobile team available for on-location sessions across Alexandria.', 4500, N'Alexandria', N'12 Fouad St, Smouha, Alexandria', 2, 0, GETUTCDATE());
    SET @WorkPostId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPostImages (WorkPostId, ImageUrl, IsPrimary, UploadedAt) VALUES
        (@WorkPostId, N'https://images.unsplash.com/photo-1541643600914-78b084683601?auto=format&fit=crop&w=800&q=80', 1, GETUTCDATE()),
        (@WorkPostId, N'https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=800&q=80', 0, GETUTCDATE());

    INSERT INTO dbo.ServicePackages (WorkPostId, Name, Description, Price, Includes, IsActive, CreatedAt) VALUES
        (@WorkPostId, N'Bride Only', NULL, 2500, N'Trial session + wedding-day airbrush makeup and hair', 1, GETUTCDATE()),
        (@WorkPostId, N'Bride + 2 Bridesmaids', NULL, 4500, N'Bridal package plus makeup and hair for 2 bridesmaids', 1, GETUTCDATE()),
        (@WorkPostId, N'Bridal Party (up to 5)', NULL, 8500, N'Bridal package plus makeup and hair for up to 5 bridesmaids', 1, GETUTCDATE());

    INSERT INTO dbo.WorkPostAvailabilities (WorkPostId, Date, IsAvailable, Notes) VALUES
        (@WorkPostId, DATEADD(day, 8, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 15, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 22, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 29, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 36, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 45, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 60, CAST(GETUTCDATE() AS date)), 1, NULL);
END
GO

-- ─── Vendor: Alexandria Elegance Decor (Alexandria) ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Email = N'alexandria-elegance-decor@eventhub.test')
BEGIN
    DECLARE @UserId INT, @VendorProfileId INT, @WorkPostId INT;

    INSERT INTO dbo.AspNetUsers
        (Role, IsDeleted, IsActive, IsEmailVerified, IsMfaEnabled, CreatedAt, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (2, 0, 1, 1, 0, GETUTCDATE(), N'alexandria-elegance-decor@eventhub.test', N'ALEXANDRIA-ELEGANCE-DECOR@EVENTHUB.TEST', N'alexandria-elegance-decor@eventhub.test', N'ALEXANDRIA-ELEGANCE-DECOR@EVENTHUB.TEST', 1, N'AQAAAAEAAYagAAAAEI3MzwKS2PT2doUbrKcUd1MyBKIYUMbBzZZ+FKuniYH3OHmxpPpXc/yj/bSH9XIFyQ==', CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), N'+201063034534', 0, 0, 1, 0);
    SET @UserId = SCOPE_IDENTITY();

    INSERT INTO dbo.VendorProfiles (UserId, BusinessName, BioDescription, PhoneNumber, City, IsVerified, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@UserId, N'Alexandria Elegance Decor', N'Full-service event styling covering table settings, stage backdrops, and lighting design for weddings, engagements, and corporate events.', N'+201063034534', N'Alexandria', 1, 2, 0, GETUTCDATE());
    SET @VendorProfileId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPosts (VendorProfileId, CategoryId, Title, Description, Price, City, Address, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@VendorProfileId, 6, N'Alexandria Elegance Decor — Event Styling & Decor', N'Full-service event styling covering table settings, stage backdrops, and lighting design for weddings, engagements, and corporate events.', 20000, N'Alexandria', N'7 Abu Qir Rd, Roushdy, Alexandria', 2, 0, GETUTCDATE());
    SET @WorkPostId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPostImages (WorkPostId, ImageUrl, IsPrimary, UploadedAt) VALUES
        (@WorkPostId, N'https://images.unsplash.com/photo-1560518883-ce09059eeffa?auto=format&fit=crop&w=800&q=80', 1, GETUTCDATE()),
        (@WorkPostId, N'https://images.unsplash.com/photo-1586338468230-7c6be2619af6?auto=format&fit=crop&w=800&q=80', 0, GETUTCDATE());

    INSERT INTO dbo.ServicePackages (WorkPostId, Name, Description, Price, Includes, IsActive, CreatedAt) VALUES
        (@WorkPostId, N'Basic Styling', NULL, 10000, N'Table settings and chair covers for up to 150 guests', 1, GETUTCDATE()),
        (@WorkPostId, N'Premium Styling', NULL, 20000, N'Table settings, stage backdrop, and uplighting for up to 250 guests', 1, GETUTCDATE()),
        (@WorkPostId, N'Luxury Full Decor', NULL, 35000, N'Complete venue transformation including ceiling drapery and custom backdrop', 1, GETUTCDATE());

    INSERT INTO dbo.WorkPostAvailabilities (WorkPostId, Date, IsAvailable, Notes) VALUES
        (@WorkPostId, DATEADD(day, 8, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 15, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 22, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 29, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 36, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 45, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 60, CAST(GETUTCDATE() AS date)), 1, NULL);
END
GO

-- ─── Vendor: Anfoushi Seaside Catering (Alexandria) ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Email = N'anfoushi-seaside-catering@eventhub.test')
BEGIN
    DECLARE @UserId INT, @VendorProfileId INT, @WorkPostId INT;

    INSERT INTO dbo.AspNetUsers
        (Role, IsDeleted, IsActive, IsEmailVerified, IsMfaEnabled, CreatedAt, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES (2, 0, 1, 1, 0, GETUTCDATE(), N'anfoushi-seaside-catering@eventhub.test', N'ANFOUSHI-SEASIDE-CATERING@EVENTHUB.TEST', N'anfoushi-seaside-catering@eventhub.test', N'ANFOUSHI-SEASIDE-CATERING@EVENTHUB.TEST', 1, N'AQAAAAEAAYagAAAAEFXichNVWTbKkR0wccyvQGWa0u6xKonbDakoWxNK1UPSaosS4RM3EMote8tdTtIwxw==', CONVERT(nvarchar(36), NEWID()), CONVERT(nvarchar(36), NEWID()), N'+201097142896', 0, 0, 1, 0);
    SET @UserId = SCOPE_IDENTITY();

    INSERT INTO dbo.VendorProfiles (UserId, BusinessName, BioDescription, PhoneNumber, City, IsVerified, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@UserId, N'Anfoushi Seaside Catering', N'Alexandria''s go-to caterer for seafood-forward wedding menus, with a dedicated mezze and grill station option.', N'+201097142896', N'Alexandria', 1, 2, 0, GETUTCDATE());
    SET @VendorProfileId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPosts (VendorProfileId, CategoryId, Title, Description, Price, City, Address, ApprovalStatus, IsDeleted, CreatedAt)
    VALUES (@VendorProfileId, 1, N'Anfoushi Seaside Catering — Seafood & Mediterranean Menus', N'Alexandria''s go-to caterer for seafood-forward wedding menus, with a dedicated mezze and grill station option.', 300, N'Alexandria', N'3 Anfoushi Square, Alexandria', 2, 0, GETUTCDATE());
    SET @WorkPostId = SCOPE_IDENTITY();

    INSERT INTO dbo.WorkPostImages (WorkPostId, ImageUrl, IsPrimary, UploadedAt) VALUES
        (@WorkPostId, N'https://images.unsplash.com/photo-1487412947147-5cebf100ffc2?auto=format&fit=crop&w=800&q=80', 1, GETUTCDATE()),
        (@WorkPostId, N'https://images.unsplash.com/photo-1519225421980-715cb0215aed?auto=format&fit=crop&w=800&q=80', 0, GETUTCDATE());

    INSERT INTO dbo.ServicePackages (WorkPostId, Name, Description, Price, Includes, IsActive, CreatedAt) VALUES
        (@WorkPostId, N'Mezze & Grill Buffet', NULL, 220, N'Mezze spread, grilled meats and seafood station, soft drinks', 1, GETUTCDATE()),
        (@WorkPostId, N'Seaside Plated Menu', NULL, 300, N'3-course plated seafood-forward menu, welcome drink', 1, GETUTCDATE()),
        (@WorkPostId, N'Platinum Seafood Feast', NULL, 450, N'Live seafood station, raw bar, full plated dinner, dessert table', 1, GETUTCDATE());

    INSERT INTO dbo.WorkPostAvailabilities (WorkPostId, Date, IsAvailable, Notes) VALUES
        (@WorkPostId, DATEADD(day, 8, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 15, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 22, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 29, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 36, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 45, CAST(GETUTCDATE() AS date)), 1, NULL),
        (@WorkPostId, DATEADD(day, 60, CAST(GETUTCDATE() AS date)), 1, NULL);
END
GO

-- ─── Event for ayshakassem59@gmail.com: Aysha & Karim's Wedding ─────────────────────────────────────
IF EXISTS (SELECT 1 FROM dbo.AspNetUsers u JOIN dbo.CustomerProfiles cp ON cp.UserId = u.Id WHERE u.Email = N'ayshakassem59@gmail.com')
   AND NOT EXISTS (SELECT 1 FROM dbo.Events e JOIN dbo.CustomerProfiles cp ON cp.Id = e.CustomerId JOIN dbo.AspNetUsers u ON u.Id = cp.UserId WHERE u.Email = N'ayshakassem59@gmail.com' AND e.Name = N'Aysha & Karim''s Wedding')
BEGIN
    DECLARE @CustomerProfileId INT, @EventId INT;
    SELECT @CustomerProfileId = cp.Id FROM dbo.CustomerProfiles cp JOIN dbo.AspNetUsers u ON u.Id = cp.UserId WHERE u.Email = N'ayshakassem59@gmail.com';

    INSERT INTO dbo.Events (CustomerId, Name, EventType, TargetDate, GuestCount, TotalBudget, City, Location, Notes, IsDeleted, CreatedAt)
    VALUES (@CustomerProfileId, N'Aysha & Karim''s Wedding', 1, DATEADD(day, 120, CAST(GETUTCDATE() AS date)), 180, 300000, N'Cairo', N'Zamalek Garden Palace', N'Outdoor ceremony, indoor reception. Nile-view photos at sunset.', 0, GETUTCDATE());
    SET @EventId = SCOPE_IDENTITY();

    DECLARE @BookingId0 INT;
    DECLARE @WorkPostId0 INT = (SELECT TOP 1 wp.Id FROM dbo.WorkPosts wp JOIN dbo.VendorProfiles vp ON vp.Id = wp.VendorProfileId WHERE vp.BusinessName = N'Zamalek Garden Palace');
    INSERT INTO dbo.Bookings (CustomerId, EventId, WorkPostId, BookingDate, Status, TotalPrice, Quantity, Notes, CreatedAt)
    VALUES (@CustomerProfileId, @EventId, @WorkPostId0, DATEADD(day, -10, CAST(GETUTCDATE() AS date)), 3, 45000, 180, NULL, GETUTCDATE());
    SET @BookingId0 = SCOPE_IDENTITY();
    INSERT INTO dbo.Payments (BookingId, Amount, PaymentMethod, PaymentStatus, PaidAt, TransactionId, PaymentGateway, GrossAmount, CommissionRateSnapshot, PlatformFeeAmount, VendorPayoutAmount)
    VALUES (@BookingId0, 45000, 5, 2, DATEADD(day, -10, CAST(GETUTCDATE() AS date)), N'TXN-ZAMALEK-GA-0', N'Paymob', 45000, 0.10, 4500.0, 40500.0);
    INSERT INTO dbo.Reviews (BookingId, Rating, Comment, CreatedAt)
    VALUES (@BookingId0, 5, N'Absolutely stunning venue — the garden looked like something out of a magazine. The team was incredibly responsive.', GETUTCDATE());

    DECLARE @BookingId1 INT;
    DECLARE @WorkPostId1 INT = (SELECT TOP 1 wp.Id FROM dbo.WorkPosts wp JOIN dbo.VendorProfiles vp ON vp.Id = wp.VendorProfileId WHERE vp.BusinessName = N'Nile Terrace Catering');
    INSERT INTO dbo.Bookings (CustomerId, EventId, WorkPostId, BookingDate, Status, TotalPrice, Quantity, Notes, CreatedAt)
    VALUES (@CustomerProfileId, @EventId, @WorkPostId1, DATEADD(day, 15, CAST(GETUTCDATE() AS date)), 6, 63000, 180, NULL, GETUTCDATE());
    SET @BookingId1 = SCOPE_IDENTITY();
    INSERT INTO dbo.Payments (BookingId, Amount, PaymentMethod, PaymentStatus, PaidAt, TransactionId, PaymentGateway, GrossAmount, CommissionRateSnapshot, PlatformFeeAmount, VendorPayoutAmount)
    VALUES (@BookingId1, 63000, 5, 2, DATEADD(day, 15, CAST(GETUTCDATE() AS date)), N'TXN-NILE-TERRA-1', N'Paymob', 63000, 0.10, 6300.0, 56700.0);

    DECLARE @BookingId2 INT;
    DECLARE @WorkPostId2 INT = (SELECT TOP 1 wp.Id FROM dbo.WorkPosts wp JOIN dbo.VendorProfiles vp ON vp.Id = wp.VendorProfileId WHERE vp.BusinessName = N'Cairo Frame Studio');
    INSERT INTO dbo.Bookings (CustomerId, EventId, WorkPostId, BookingDate, Status, TotalPrice, Quantity, Notes, CreatedAt)
    VALUES (@CustomerProfileId, @EventId, @WorkPostId2, DATEADD(day, 15, CAST(GETUTCDATE() AS date)), 1, 12000, 1, NULL, GETUTCDATE());
    SET @BookingId2 = SCOPE_IDENTITY();

    DECLARE @BookingId3 INT;
    DECLARE @WorkPostId3 INT = (SELECT TOP 1 wp.Id FROM dbo.WorkPosts wp JOIN dbo.VendorProfiles vp ON vp.Id = wp.VendorProfileId WHERE vp.BusinessName = N'Cairo Sound Ensemble');
    INSERT INTO dbo.Bookings (CustomerId, EventId, WorkPostId, BookingDate, Status, TotalPrice, Quantity, Notes, CreatedAt)
    VALUES (@CustomerProfileId, @EventId, @WorkPostId3, DATEADD(day, 15, CAST(GETUTCDATE() AS date)), 2, 15000, 1, NULL, GETUTCDATE());
    SET @BookingId3 = SCOPE_IDENTITY();

    INSERT INTO dbo.Guests (EventId, Name, Email, PhoneNumber, RSVPStatus, CreatedAt) VALUES
        (@EventId, N'Mona El Sayed', N'mona.elsayed@example.com', NULL, 2, GETUTCDATE()),
        (@EventId, N'Omar Farouk', N'omar.farouk@example.com', NULL, 2, GETUTCDATE()),
        (@EventId, N'Laila Hassan', N'laila.hassan@example.com', NULL, 2, GETUTCDATE()),
        (@EventId, N'Youssef Adel', N'youssef.adel@example.com', NULL, 1, GETUTCDATE()),
        (@EventId, N'Nadine Kamal', N'nadine.kamal@example.com', NULL, 1, GETUTCDATE()),
        (@EventId, N'Hossam Zaki', N'hossam.zaki@example.com', NULL, 3, GETUTCDATE()),
        (@EventId, N'Rana Mostafa', NULL, NULL, 2, GETUTCDATE()),
        (@EventId, N'Kareem Nabil', NULL, NULL, 1, GETUTCDATE()),
        (@EventId, N'Salma Tarek', NULL, NULL, 2, GETUTCDATE()),
        (@EventId, N'Ahmed Gaber', NULL, NULL, 3, GETUTCDATE());

    INSERT INTO dbo.ChecklistItems (EventId, Title, Description, DueDate, Priority, IsCompleted, Category, CreatedAt) VALUES
        (@EventId, N'Book ceremony venue', N'Confirm final headcount with Zamalek Garden Palace', DATEADD(day, -20, CAST(GETUTCDATE() AS date)), 3, 1, N'Venue', GETUTCDATE()),
        (@EventId, N'Finalize catering menu', N'Choose between Gold and Platinum tier with Nile Terrace', DATEADD(day, -5, CAST(GETUTCDATE() AS date)), 3, 1, N'Catering', GETUTCDATE()),
        (@EventId, N'Send save-the-dates', N'Digital + printed for the top 50 guests', DATEADD(day, 5, CAST(GETUTCDATE() AS date)), 2, 0, N'Guests', GETUTCDATE()),
        (@EventId, N'Choose wedding photographer package', N'Decide between Signature and Premium tier', DATEADD(day, 10, CAST(GETUTCDATE() AS date)), 3, 0, N'Photography', GETUTCDATE()),
        (@EventId, N'Confirm live band setlist', N'Send first-dance song to Cairo Sound Ensemble', DATEADD(day, 20, CAST(GETUTCDATE() AS date)), 2, 0, N'Music', GETUTCDATE()),
        (@EventId, N'Order wedding invitations', N'Design proof due back from printer', DATEADD(day, 30, CAST(GETUTCDATE() AS date)), 1, 0, NULL, GETUTCDATE());

    INSERT INTO dbo.Expenses (EventId, Category, Description, Amount, Status, Date, BookingId, CreatedAt) VALUES
        (@EventId, N'Venue', N'Zamalek Garden Palace — full estate booking', 45000, 2, DATEADD(day, -10, CAST(GETUTCDATE() AS date)), @BookingId0, GETUTCDATE()),
        (@EventId, N'Catering', N'Nile Terrace Catering — Gold Plated Menu deposit', 63000, 2, DATEADD(day, 15, CAST(GETUTCDATE() AS date)), @BookingId1, GETUTCDATE()),
        (@EventId, N'Attire', N'Bridal gown fitting deposit', 12000, 2, DATEADD(day, -15, CAST(GETUTCDATE() AS date)), NULL, GETUTCDATE()),
        (@EventId, N'Stationery', N'Wedding invitations — 150 sets', 4500, 1, DATEADD(day, 30, CAST(GETUTCDATE() AS date)), NULL, GETUTCDATE()),
        (@EventId, N'Miscellaneous', N'Bridal party gifts', 6000, 1, DATEADD(day, 40, CAST(GETUTCDATE() AS date)), NULL, GETUTCDATE());

    INSERT INTO dbo.Documents (EventId, Type, FileName, FileUrl, Amount, Status, UploadedAt, CreatedAt) VALUES
        (@EventId, 1, N'Zamalek_Garden_Palace_Contract.pdf', NULL, NULL, N'Signed', GETUTCDATE(), GETUTCDATE()),
        (@EventId, 2, N'Nile_Terrace_Catering_Invoice.pdf', NULL, 63000, N'Paid', GETUTCDATE(), GETUTCDATE());

    INSERT INTO dbo.Favorites (CustomerId, WorkPostId) VALUES
        (@CustomerProfileId, (SELECT TOP 1 wp.Id FROM dbo.WorkPosts wp JOIN dbo.VendorProfiles vp ON vp.Id = wp.VendorProfileId WHERE vp.BusinessName = N'Alexandria Bloom Atelier')),
        (@CustomerProfileId, (SELECT TOP 1 wp.Id FROM dbo.WorkPosts wp JOIN dbo.VendorProfiles vp ON vp.Id = wp.VendorProfileId WHERE vp.BusinessName = N'Mediterranean Glam Studio'));

END
GO

-- ─── Event for nourtarekhelmy11@gmail.com: Nour's 30th Birthday Celebration ─────────────────────────────────────
IF EXISTS (SELECT 1 FROM dbo.AspNetUsers u JOIN dbo.CustomerProfiles cp ON cp.UserId = u.Id WHERE u.Email = N'nourtarekhelmy11@gmail.com')
   AND NOT EXISTS (SELECT 1 FROM dbo.Events e JOIN dbo.CustomerProfiles cp ON cp.Id = e.CustomerId JOIN dbo.AspNetUsers u ON u.Id = cp.UserId WHERE u.Email = N'nourtarekhelmy11@gmail.com' AND e.Name = N'Nour''s 30th Birthday Celebration')
BEGIN
    DECLARE @CustomerProfileId INT, @EventId INT;
    SELECT @CustomerProfileId = cp.Id FROM dbo.CustomerProfiles cp JOIN dbo.AspNetUsers u ON u.Id = cp.UserId WHERE u.Email = N'nourtarekhelmy11@gmail.com';

    INSERT INTO dbo.Events (CustomerId, Name, EventType, TargetDate, GuestCount, TotalBudget, City, Location, Notes, IsDeleted, CreatedAt)
    VALUES (@CustomerProfileId, N'Nour''s 30th Birthday Celebration', 2, DATEADD(day, 45, CAST(GETUTCDATE() AS date)), 60, 60000, N'Alexandria', N'Corniche Grand Hall', N'Seaside evening party, cocktail-style.', 0, GETUTCDATE());
    SET @EventId = SCOPE_IDENTITY();

    DECLARE @BookingId0 INT;
    DECLARE @WorkPostId0 INT = (SELECT TOP 1 wp.Id FROM dbo.WorkPosts wp JOIN dbo.VendorProfiles vp ON vp.Id = wp.VendorProfileId WHERE vp.BusinessName = N'Alexandria Elegance Decor');
    INSERT INTO dbo.Bookings (CustomerId, EventId, WorkPostId, BookingDate, Status, TotalPrice, Quantity, Notes, CreatedAt)
    VALUES (@CustomerProfileId, @EventId, @WorkPostId0, DATEADD(day, -3, CAST(GETUTCDATE() AS date)), 3, 20000, 60, NULL, GETUTCDATE());
    SET @BookingId0 = SCOPE_IDENTITY();
    INSERT INTO dbo.Payments (BookingId, Amount, PaymentMethod, PaymentStatus, PaidAt, TransactionId, PaymentGateway, GrossAmount, CommissionRateSnapshot, PlatformFeeAmount, VendorPayoutAmount)
    VALUES (@BookingId0, 20000, 5, 2, DATEADD(day, -3, CAST(GETUTCDATE() AS date)), N'TXN-ALEXANDRIA-0', N'Paymob', 20000, 0.10, 2000.0, 18000.0);
    INSERT INTO dbo.Reviews (BookingId, Rating, Comment, CreatedAt)
    VALUES (@BookingId0, 4, N'Beautiful styling, arrived right on time and the uplighting made the whole hall glow.', GETUTCDATE());

    DECLARE @BookingId1 INT;
    DECLARE @WorkPostId1 INT = (SELECT TOP 1 wp.Id FROM dbo.WorkPosts wp JOIN dbo.VendorProfiles vp ON vp.Id = wp.VendorProfileId WHERE vp.BusinessName = N'Mediterranean Glam Studio');
    INSERT INTO dbo.Bookings (CustomerId, EventId, WorkPostId, BookingDate, Status, TotalPrice, Quantity, Notes, CreatedAt)
    VALUES (@CustomerProfileId, @EventId, @WorkPostId1, DATEADD(day, 40, CAST(GETUTCDATE() AS date)), 1, 4500, 1, NULL, GETUTCDATE());
    SET @BookingId1 = SCOPE_IDENTITY();

    INSERT INTO dbo.Guests (EventId, Name, Email, PhoneNumber, RSVPStatus, CreatedAt) VALUES
        (@EventId, N'Farida Samir', NULL, NULL, 2, GETUTCDATE()),
        (@EventId, N'Tarek Mansour', NULL, NULL, 2, GETUTCDATE()),
        (@EventId, N'Dina Hesham', NULL, NULL, 1, GETUTCDATE()),
        (@EventId, N'Amr Sherif', NULL, NULL, 3, GETUTCDATE());

    INSERT INTO dbo.ChecklistItems (EventId, Title, Description, DueDate, Priority, IsCompleted, Category, CreatedAt) VALUES
        (@EventId, N'Book decor team', N'Confirm uplighting colors', DATEADD(day, -10, CAST(GETUTCDATE() AS date)), 3, 1, N'Decoration', GETUTCDATE()),
        (@EventId, N'Reserve hall', N'Corniche Grand Hall — evening slot', DATEADD(day, -15, CAST(GETUTCDATE() AS date)), 3, 1, N'Venue', GETUTCDATE()),
        (@EventId, N'Send digital invites', N'Via WhatsApp broadcast', DATEADD(day, 5, CAST(GETUTCDATE() AS date)), 2, 0, N'Guests', GETUTCDATE());

    INSERT INTO dbo.Expenses (EventId, Category, Description, Amount, Status, Date, BookingId, CreatedAt) VALUES
        (@EventId, N'Decoration', N'Alexandria Elegance Decor — Premium Styling', 20000, 2, DATEADD(day, -3, CAST(GETUTCDATE() AS date)), @BookingId0, GETUTCDATE()),
        (@EventId, N'Entertainment', N'Playlist curation + speaker rental', 3000, 1, DATEADD(day, 20, CAST(GETUTCDATE() AS date)), NULL, GETUTCDATE());

    INSERT INTO dbo.Documents (EventId, Type, FileName, FileUrl, Amount, Status, UploadedAt, CreatedAt) VALUES
        (@EventId, 3, N'Alexandria_Elegance_Decor_Receipt.pdf', NULL, 20000, N'Paid', GETUTCDATE(), GETUTCDATE());

    INSERT INTO dbo.Favorites (CustomerId, WorkPostId) VALUES
        (@CustomerProfileId, (SELECT TOP 1 wp.Id FROM dbo.WorkPosts wp JOIN dbo.VendorProfiles vp ON vp.Id = wp.VendorProfileId WHERE vp.BusinessName = N'Cairo Frame Studio'));

END
GO

PRINT 'SeedDemoData.sql applied successfully.';
