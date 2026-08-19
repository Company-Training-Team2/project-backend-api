namespace EventHub.Application.DTOs.Admin;

/// <summary>
/// Body sent to PUT /api/admin/workposts/{id}/approve|reject.
/// Reason is optional for approve, recommended for reject — same shape and
/// convention as VendorDecisionRequest, kept separate since a service
/// listing decision is conceptually distinct from a vendor-account decision.
/// </summary>
public class WorkPostDecisionRequest
{
    public string? Reason { get; set; }
}
