using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EntityFeatures.Contracts;

namespace Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories.Transacted;

internal static class VersionPolicy<TEntity>
where TEntity : IEntity
{
    public static long GetVersionOrDefault(TEntity entity, long defaultVersion = 0)
    {
        if (entity is not IEntityWithVersion<TEntity> versioned)
            return defaultVersion;

        return versioned.GlobalVersion;
    }

    public static TEntity Materialize(TEntity entity, long version)
    {
        if (entity is not IEntityWithVersion<TEntity> versioned)
            return entity;

        if (versioned.GlobalVersion == version)
            return entity;

        return versioned.WithVersion(version);
    }
}
