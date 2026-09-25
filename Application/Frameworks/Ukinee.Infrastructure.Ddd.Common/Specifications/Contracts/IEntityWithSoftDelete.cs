namespace Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;

public interface IEntityWithSoftDelete<out TSelf>
{
    public bool IsDeleted { get; }
    public DateTimeOffset? DeletedAt { get; }
    
    public TSelf Delete(DateTimeOffset now);
}
