namespace Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;

public interface ISpecificationForSoftDelete<out TSelf>
{
    public bool IsDeleted { get; }
    public DateTimeOffset DeletedAt { get; }
    
    public TSelf Delete(DateTimeOffset now);
}
