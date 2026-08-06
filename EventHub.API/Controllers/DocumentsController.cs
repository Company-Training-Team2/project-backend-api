using EventHub.Application.DTOs.Document;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.API.Controllers;

/// <summary>
/// Module 7 – Document repository.
///
/// Routes:
///   GET  /api/events/{id}/documents        → list event documents
///   POST /api/events/{id}/documents        → upload (multipart/form-data)
///   GET  /api/documents/{id}/download      → secured download link / stream
///
/// Critical dependency: Object Storage pipeline (S3 / Azure Blob / MinIO)
/// must be configured before file upload and download produce real URLs.
/// The endpoints are fully wired; only the storage integration in
/// DocumentService is deferred until that infrastructure is in place.
/// </summary>
[ApiController]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    // ── GET /api/events/{id}/documents ────────────────────────────────────────

    [HttpGet("api/events/{id}/documents")]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocuments(int id)
    {
        var userId = GetCurrentUserId();

        var result = await _documentService.GetDocumentsAsync(id, userId);

        if (result == null)
            return NotFound(new { message = "Event not found or access denied." });

        return Ok(result);
    }

    // ── POST /api/events/{id}/documents ───────────────────────────────────────

    [HttpPost("api/events/{id}/documents")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DocumentDto>> UploadDocument(
        int id,
        [FromForm] UploadDocumentRequest request)
    {
        var userId = GetCurrentUserId();

        var document = await _documentService.UploadDocumentAsync(id, userId, request);

        if (document == null)
            return NotFound(new { message = "Event not found or access denied." });

        return CreatedAtAction(
            nameof(GetDocuments),
            new { id },
            document);
    }

    // ── GET /api/documents/{id}/download ─────────────────────────────────────

    [HttpGet("api/documents/{id}/download")]
    public async Task<ActionResult<DocumentDownloadResponse>> GetDownloadLink(int id)
    {
        var userId = GetCurrentUserId();

        var result = await _documentService.GetDownloadLinkAsync(id, userId);

        if (result == null)
            return NotFound(new { message = "Document not found or access denied." });

        return Ok(result);
    }
}