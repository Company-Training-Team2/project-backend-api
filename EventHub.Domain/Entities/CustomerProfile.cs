using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

public class CustomerProfile : AuditableEntity
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}