namespace EventHub.Application.DTOs.Document;

/// <summary>
/// Represents a stored document (Contract / Invoice / Receipt) attached to an event.
/// FileUrl is null until the blob-storage pipeline is wired (S3 / Azure Blob / MinIO).
/// </summary>
public class DocumentDto
{
    public int Id { get; set; }
    public int EventId { get; set; }

    /// <summary>Contract | Invoice | Receipt</summary>
    public string Type { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    /// <summary>Blob storage URL. Null until the upload pipeline is configured.</summary>
    public string? FileUrl { get; set; }

    public DateTime UploadedAt { get; set; }

    /// <summary>Relevant for Invoice / Receipt documents.</summary>
    public decimal? Amount { get; set; }

    public string? Status { get; set; }
}