using EventHub.Application.DTOs.Vendor;
using EventHub.Application.DTOs.WorkPost;

namespace EventHub.Application.Interfaces;

public interface IVendorService
{
	// ── Dashboard ────────────────────────────────────────────────────────────
	Task<VendorDashboardDto> GetDashboardAsync(int userId);

	// ── WorkPost (Service) CRUD ───────────────────────────────────────────────
	Task<IEnumerable<VendorWorkPostDto>> GetMyWorkPostsAsync(int userId);
	Task<VendorWorkPostDto> GetWorkPostByIdAsync(int userId, int workPostId);
	Task<VendorWorkPostDto> CreateWorkPostAsync(int userId, CreateWorkPostDto dto);
	Task<VendorWorkPostDto> UpdateWorkPostAsync(int userId, int workPostId, UpdateWorkPostDto dto);
	Task DeleteWorkPostAsync(int userId, int workPostId);

	// ── WorkPost Images ───────────────────────────────────────────────────────
	/// <summary>POST /api/vendor/services/{id}/images — attach images to a WorkPost.</summary>
	Task<IEnumerable<WorkPostImageDto>> UploadWorkPostImagesAsync(
		int userId,
		int workPostId,
		UploadWorkPostImagesRequest request);

	// ── Availability ──────────────────────────────────────────────────────────
	Task<IEnumerable<VendorAvailabilityDto>> GetAvailabilityAsync(int userId);
	Task UpdateAvailabilityAsync(int userId, UpdateAvailabilityDto dto);

	// ── Bookings ──────────────────────────────────────────────────────────────
	Task<IEnumerable<VendorBookingDto>> GetBookingsAsync(int userId, string? status);
	Task<VendorBookingDto> ApproveBookingAsync(int userId, int bookingId);
	Task<VendorBookingDto> DeclineBookingAsync(int userId, int bookingId);

	/// <summary>PUT /api/vendor/bookings/{id}/complete — marks a Paid booking Completed and triggers its Payout.</summary>
	Task<VendorBookingDto> CompleteBookingAsync(int userId, int bookingId);

	// ── Analytics ────────────────────────────────────────────────────────────
	Task<VendorAnalyticsDto> GetAnalyticsAsync(int userId);

	// ── Reviews ───────────────────────────────────────────────────────────────
	/// <summary>GET /api/vendor/reviews — every review left across all of this vendor's WorkPosts.</summary>
	Task<IEnumerable<VendorReviewDto>> GetReviewsAsync(int userId);

	// ── Profile ───────────────────────────────────────────────────────────────
	Task<VendorProfileDto> GetProfileAsync(int userId);
	Task<VendorProfileDto> UpdateProfileAsync(int userId, UpdateVendorProfileDto dto);

	/// <summary>POST /api/vendor/profile/logo — replaces the storefront logo with an
	/// uploaded image file. Not part of UpdateProfileAsync's plain-JSON payload
	/// (that no longer accepts a LogoUrl string at all — see UpdateVendorProfileDto).</summary>
	Task<VendorProfileDto> UploadLogoAsync(int userId, Microsoft.AspNetCore.Http.IFormFile file);

	// ── Verification documents (admin KYC review) ─────────────────────────────
	/// <summary>Resolves the opaque IFileStorageService.SavePrivateAsync path for
	/// one of a vendor's KYC documents ("commercial-registration" | "national-id"
	/// | "business-license"). Returns null if the vendor never uploaded that
	/// document, or if vendorProfileId does not exist. Callers stream it back
	/// via IFileStorageService.StreamPrivateAsync.</summary>
	Task<string?> GetVerificationDocumentPathAsync(int vendorProfileId, string documentType);
}