//Inherits Id, CreatedAt, and UpdatedAt through the chain.

//Enables Soft Deletes: instead of deleting a row permanently (DELETE FROM Table), your application sets IsDeleted = true and logs DeletedAt.

namespace EventHub.Domain.Common;

public abstract class SoftDeletableEntity : AuditableEntity
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
}
