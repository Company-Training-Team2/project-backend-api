using Microsoft.AspNetCore.Http;

namespace EventHub.Application.DTOs.WorkPost;

/// <summary>
/// Multipart/form-data body for POST /api/vendor/services/{id}/images.
/// </summary>
public class UploadWorkPostImagesRequest
{
    /// <summary>One or more image files to attach to the WorkPost.</summary>
    public List<IFormFile> Images { get; set; } = new();

    /// <summary>
    /// When true the first uploaded image becomes (or replaces) the primary
    /// image for this WorkPost.
    /// </summary>
    public bool SetFirstAsPrimary { get; set; } = false;
}
