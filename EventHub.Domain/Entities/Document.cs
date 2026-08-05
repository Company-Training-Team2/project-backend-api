using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

/// <summary>
/// A file (contract, invoice, receipt) attached to an event.
/// Per audit Module 7. Requires Blob Storage pipeline (S3 / Azure Blob / MinIO)
/// before FileUrl can be populated.
/// </summary>
public class Document : AuditableEntity
{
    public int EventId { get; set; }

    public DocumentType Type { get; set; }

    public string FileName { get; set; } = string.Empty;

    /// <summary>Blob storage URL. Null until file is uploaded.</summary>
    public string? FileUrl { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>For invoice/receipt documents.</summary>
    public decimal? Amount { get; set; }

    public string? Status { get; set; }

    // ─── Navigation ───────────────────────────────────────────────────────────
    public Event Event { get; set; } = null!;
}