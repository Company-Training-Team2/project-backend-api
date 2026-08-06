using EventHub.Application.DTOs.Document;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;

namespace EventHub.Application.Services;

/// <summary>
/// Module 7 – Document repository service.
///
/// Architecture note:
///   File upload and download require an Object Storage pipeline
///   (AWS S3 / Azure Blob / MinIO). Until that infrastructure is configured:
///     - UploadDocumentAsync persists the metadata record and stores the
///       original FileName; FileUrl is left null.
///     - GetDownloadLinkAsync returns the stored FileUrl (also null until
///       blob storage is wired).
///
///   When integrating blob storage, inject your storage client here and:
///     1. In UploadDocumentAsync: stream request.File to the bucket,
///        assign the resulting URL to document.FileUrl before SaveChanges.
///     2. In GetDownloadLinkAsync: generate a short-lived pre-signed URL
///        from the stored key and populate DownloadUrl / ExpiresAt.
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventService _eventService;

    public DocumentService(IUnitOfWork unitOfWork, IEventService eventService)
    {
        _unitOfWork = unitOfWork;
        _eventService = eventService;
    }

    // ── GET /api/events/{id}/documents ────────────────────────────────────────

    public async Task<IEnumerable<DocumentDto>?> GetDocumentsAsync(
        int eventId,
        int userId)
    {
        var owned = await _eventService.EventBelongsToUserAsync(eventId, userId);
        if (!owned)
            return null;

        var documents = await _unitOfWork
            .Repository<Document>()
            .FindAsync(d => d.EventId == eventId);

        return documents
            .OrderByDescending(d => d.UploadedAt)
            .Select(MapToDto);
    }

    // ── POST /api/events/{id}/documents ───────────────────────────────────────

    public async Task<DocumentDto?> UploadDocumentAsync(
        int eventId,
        int userId,
        UploadDocumentRequest request)
    {
        var owned = await _eventService.EventBelongsToUserAsync(eventId, userId);
        if (!owned)
            return null;

        // TODO: When blob storage is configured, stream request.File to
        // the bucket here and capture the resulting URL as fileUrl.
        string? fileUrl = null;

        var document = new Document
        {
            EventId = eventId,
            Type = ParseDocumentType(request.Type),
            FileName = request.File.FileName,
            FileUrl = fileUrl,
            UploadedAt = DateTime.UtcNow,
            Amount = request.Amount,
            Status = request.Status
        };

        await _unitOfWork.Repository<Document>().AddAsync(document);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(document);
    }

    // ── GET /api/documents/{id}/download ─────────────────────────────────────

    public async Task<DocumentDownloadResponse?> GetDownloadLinkAsync(
        int documentId,
        int userId)
    {
        var document = await _unitOfWork
            .Repository<Document>()
            .GetByIdAsync(documentId);

        if (document == null)
            return null;

        var owned = await _eventService.EventBelongsToUserAsync(document.EventId, userId);
        if (!owned)
            return null;

        // TODO: When blob storage is configured, generate a short-lived
        // pre-signed URL from document.FileUrl / storage key and set ExpiresAt.
        return new DocumentDownloadResponse
        {
            DocumentId = document.Id,
            FileName = document.FileName,
            DownloadUrl = document.FileUrl,   // null until blob storage is wired
            ExpiresAt = null
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static DocumentDto MapToDto(Document d) => new()
    {
        Id = d.Id,
        EventId = d.EventId,
        Type = d.Type.ToString(),
        FileName = d.FileName,
        FileUrl = d.FileUrl,
        UploadedAt = d.UploadedAt,
        Amount = d.Amount,
        Status = d.Status
    };

    private static DocumentType ParseDocumentType(string type) =>
        Enum.TryParse<DocumentType>(type, ignoreCase: true, out var t)
            ? t
            : DocumentType.Contract;
}