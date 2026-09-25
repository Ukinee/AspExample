using Ukinee.Infrastructure.Ddd.Common.Entities;

namespace Ukinee.Infrastructure.Ddd.External.Api.Utils;

public static class EndpointsNames
{
    public static string Update<TEntity, TPayload>()
    where TEntity : IEntity
    {
        return $"Update{typeof(TEntity).Name}Using{typeof(TPayload).Name}";
    }
}

public static class RelationalPathUtils
{
    public static string Find<TEntity>(string template)
    where TEntity : IEntity
    {
        return $"{typeof(TEntity).Name}/{template}";
    }

    public static string FindMany<TEntity>()
    where TEntity : IEntity
    {
        return $"{typeof(TEntity).Name}/find";
    }

    public static string FindAll<TEntity>()
    where TEntity : IEntity
    {
        return $"{typeof(TEntity).Name}";
    }

    public static string Create<TEntity>()
    where TEntity : IEntity
    {
        return $"{typeof(TEntity).Name}";
    }

    public static string CreateMany<TEntity>()
    where TEntity : IEntity
    {
        return $"{typeof(TEntity).Name}/createMany";
    }

    public static string EnsureExists<TEntity>(string template)
    where TEntity : IEntity
    {
        return $"{typeof(TEntity).Name}/ensureExists/{template}";
    }

    public static string EnsureExistsMany<TEntity>()
    where TEntity : IEntity
    {
        return $"{typeof(TEntity).Name}/ensureExistsMany";
    }

    public static string Update<TEntity, TPayload>(string template)
    where TEntity : IEntity
    {
        return $"{typeof(TEntity).Name}/updateWith{typeof(TPayload).Name}/{template}";
    }

    public static string UpdateMany<TEntity, TPayload>()
    where TEntity : IEntity
    {
        return $"{typeof(TEntity).Name}/updateManyWith{typeof(TPayload).Name}";
    }

    public static string Delete<TEntity>(string template)
    where TEntity : IEntity
    {
        return $"{typeof(TEntity).Name}/{template}";
    }

    public static string DeleteMany<TEntity>()
    where TEntity : IEntity
    {
        return $"{typeof(TEntity).Name}/deleteMany";
    }

    public static string EffectMany<TEntity, TPayload>()
    where TEntity : IEntity
    {
        return $"{typeof(TEntity).Name}/effectMany/{typeof(TPayload).Name}";
    }
}
