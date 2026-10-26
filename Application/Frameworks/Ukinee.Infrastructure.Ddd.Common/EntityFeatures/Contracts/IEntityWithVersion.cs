namespace Ukinee.Infrastructure.Ddd.Common.EntityFeatures.Contracts;

public interface IEntityWithVersion
{
    public long GlobalVersion { get; }
}

public interface IEntityWithVersion<out TSelf> : IEntityWithVersion
{
    TSelf WithVersion(long version);
}