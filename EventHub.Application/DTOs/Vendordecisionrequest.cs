namespace EventHub.Application.DTOs;

/// <summary>
/// Body sent to PUT /api/admin/vendors/{id}/approve|reject|request-changes.
/// Reason is optional for approve, recommended for reject/request-changes.
/// </summary>
public class VendorDecisionRequest
{
    public string? Reason { get; set; }
}