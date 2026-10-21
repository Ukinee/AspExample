namespace Ukinee.Infrastructure.Ddd.Common.EntityFeatures.Contracts;

public interface IEntityWithVersion
{
    public long RowVersion { get; }
}
