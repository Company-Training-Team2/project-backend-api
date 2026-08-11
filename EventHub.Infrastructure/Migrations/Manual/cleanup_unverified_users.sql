-- ============================================================================
-- Cleanup: delete accounts that registered but never verified their email
-- (an OTP was sent and never entered) — as if they never registered at all.
--
-- HOW TO USE
--   1. Run STEP 1 first and actually look at the results. It's a SELECT —
--      completely safe, changes nothing.
--   2. Only if that list looks right, run STEP 2 and STEP 3 (in that exact
--      order — VendorProfiles must go before AspNetUsers, see note below).
--   3. This is a permanent delete. There is no undo outside of a database
--      backup/restore. Take a fresh backup first if there's any doubt.
--
-- SCOPE: targets AspNetUsers.IsEmailVerified = 0. Admin self-registration is
-- blocked at the API level (AuthService.RegisterAsync), so no unverified
-- Admin rows should exist in practice — this doesn't special-case Role for
-- that reason, but double-check the STEP 1 output before deleting regardless.
-- ============================================================================

-- ── STEP 1: preview — run this first, read the output ──────────────────────
SELECT
    Id,
    Email,
    Role,               -- 1 = Customer, 2 = Vendor, 3 = Admin
    CreatedAt,
    EmailVerificationExpiry
FROM AspNetUsers
WHERE IsEmailVerified = 0
ORDER BY CreatedAt DESC;

-- ── STEP 2: delete VendorProfiles for those users first ─────────────────────
-- Required before STEP 3: VendorProfiles.UserId -> AspNetUsers.Id is
-- ON DELETE RESTRICT (VendorProfileConfiguration.cs), so deleting the user
-- first would fail with a foreign-key error for any unverified Vendor
-- registrations. CustomerProfiles don't need this — that relationship is
-- ON DELETE CASCADE and cleans itself up in STEP 3.
DELETE VP
FROM VendorProfiles VP
INNER JOIN AspNetUsers U ON U.Id = VP.UserId
WHERE U.IsEmailVerified = 0;

-- ── STEP 3: delete the unverified users themselves ───────────────────────────
-- Cascades to CustomerProfiles automatically (ON DELETE CASCADE).
DELETE FROM AspNetUsers
WHERE IsEmailVerified = 0;
