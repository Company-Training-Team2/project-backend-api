using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EventHub.Application.DTOs.Document;

public class UploadDocumentRequest
{
    /// <summary>Accepted values: Contract, Invoice, Receipt.</summary>
    [Required]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The binary file to upload. The controller receives it as
    /// IFormFile from a multipart/form-data POST.
    /// </summary>
    [Required]
    public IFormFile File { get; set; } = null!;

    /// <summary>For Invoice / Receipt documents.</summary>
    public decimal? Amount { get; set; }

    public string? Status { get; set; }
}