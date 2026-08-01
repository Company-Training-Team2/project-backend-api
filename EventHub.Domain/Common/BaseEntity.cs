//Defines the identity for every domain object using a unique primary key (Id).
//Being abstract prevents creating a plain instance of BaseEntity directly—it exists only to be inherited.
namespace EventHub.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
}