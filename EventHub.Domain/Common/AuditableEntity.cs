
//Inherits Id from BaseEntity.
//Adds timestamp tracking for auditing:

//CreatedAt: Set once when the record is created.

//UpdatedAt: Nullable(DateTime ?), updated every time the record changes.

namespace EventHub.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
