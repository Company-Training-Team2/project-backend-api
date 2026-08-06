using EventHub.Application.DTOs.Document;

namespace EventHub.Application.Interfaces;

/// <summary>
/// Module 7 – Document repository.
/// File upload requires an object-storage pipeline (S3 / Azure Blob / MinIO)
/// to be configured before FileUrl can be populated.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Lists all documents attached to the event.
    /// Returns null when the event is not found or does not belong to the user.
    /// </summary>
    Task<IEnumerable<DocumentDto>?> GetDocumentsAsync(int eventId, int userId);

    /// <summary>
    /// Registers a document record and streams the file to blob storage.
    /// Returns null when the event is not found or does not belong to the user.
    /// </summary>
    Task<DocumentDto?> UploadDocumentAsync(
        int eventId,
        int userId,
        UploadDocumentRequest request);

    /// <summary>
    /// Generates a secured download link / stream for the document.
    /// Returns null when the document is not found or does not belong to the user's event.
    /// </summary>
    Task<DocumentDownloadResponse?> GetDownloadLinkAsync(int documentId, int userId);
}