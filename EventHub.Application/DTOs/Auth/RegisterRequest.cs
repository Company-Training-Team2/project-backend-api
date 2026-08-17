using System.ComponentModel.DataAnnotations;
using EventHub.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EventHub.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } // Customer or Vendor only

    // ─── Customer fields ──────────────────────────────────────────────────────

    /// <summary>
    /// REG-CUS-003: Enforces a maximum length of 100 characters (matching the
    /// CustomerProfiles.FullName column) so that oversized names are rejected
    /// at the API boundary rather than causing a DB truncation or silent accept.
    /// MinLength(2) ensures single-character names are also rejected.
    /// </summary>
    [MinLength(2), MaxLength(100)]
    public string? FullName { get; set; }

    public string? City { get; set; }

    /// <summary>
    /// Added per audit Module 1: Add PhoneNumber to RegisterRequest.
    /// REG-CUS-006: MaxLength(15) matches the E.164 standard maximum ('+' plus
    /// 14 digits) and the CustomerProfiles.PhoneNumber column cap, so a 15-digit
    /// value submitted without a leading '+' is rejected at the API boundary.
    /// </summary>
    [Phone, MinLength(7), MaxLength(15)]
    public string? PhoneNumber { get; set; }

    // ─── Idempotency ──────────────────────────────────────────────────────────
    /// <summary>
    /// REG-CUS-013: Optional client-supplied key (UUID recommended) that the
    /// backend uses to de-duplicate rapid multi-click submissions.  The frontend
    /// generates this once per registration attempt; repeated requests carrying
    /// the same key within the dedup window receive the same 200 response
    /// without creating a second account.
    /// </summary>
    [MaxLength(64)]
    public string? IdempotencyKey { get; set; }

    // ─── Vendor fields ────────────────────────────────────────────────────────
    public string? BusinessName { get; set; }

    public string? BioDescription { get; set; }

    /// <summary>
    /// Vendor-only. Ids from GET /api/categories for the services this vendor
    /// offers. Extras beyond 3 are ignored and unknown ids are silently
    /// dropped rather than failing registration — see AuthService.RegisterAsync.
    /// </summary>
    public List<int>? CategoryIds { get; set; }

    // ─── Vendor uploads ───────────────────────────────────────────────────────
    // Optional. This binds from multipart/form-data (see AuthController.Register's
    // [FromForm]), not JSON — a customer registration posting the usual JSON
    // body simply leaves these null. Saved by AuthService.RegisterAsync via
    // IFileStorageService; VendorProfile.LogoUrl/CoverImageUrl end up public,
    // the three verification documents end up private (see VendorProfile.cs).

    /// <summary>Public. Shown on the vendor's storefront.</summary>
    public IFormFile? BusinessLogo { get; set; }

    /// <summary>Public. Shown on the vendor's storefront.</summary>
    public IFormFile? CoverImage { get; set; }

    /// <summary>Private - reviewed by admin during KYC approval only.</summary>
    public IFormFile? CommercialRegistration { get; set; }

    /// <summary>Private - reviewed by admin during KYC approval only.</summary>
    public IFormFile? NationalId { get; set; }

    /// <summary>Private - reviewed by admin during KYC approval only.</summary>
    public IFormFile? BusinessLicense { get; set; }
}