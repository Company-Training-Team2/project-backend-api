namespace EventHub.Application.DTOs.Document;

/// <summary>
/// Returned by GET /api/documents/{id}/download.
/// When blob storage is configured this carries a short-lived pre-signed URL.
/// Until then, FileUrl mirrors the stored value (may be null).
/// </summary>
public class DocumentDownloadResponse
{
    public int DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Pre-signed or direct download URL.
    /// Null when the blob-storage pipeline has not yet been configured.
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// UTC expiry of the pre-signed URL. Null for direct/permanent links.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}