using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.SignalR.Server.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.SignalR.Server.Services;

public class DefaultRouteResolver<TTag, TIdentifier, TEntity, TRequest> : RouteResolverBase<TTag, TIdentifier, TEntity, TRequest>
where TRequest : ISignalRSubscribeRequest
where TEntity : IEntity<TIdentifier>
{
    public DefaultRouteResolver(AuthorizationPolicy<TIdentifier, TEntity> authorizationPolicy)
    {
        var prefix = Prefix;

        RequestRule((request, userContext) => authorizationPolicy.UserTagsFactory.Invoke(userContext, request.AccessModes).Select(tag => tag.ToGroupKey(prefix)));
        EntityRule(entity => authorizationPolicy.EntityTagsFactory.Invoke(entity).Select(tag => tag.ToGroupKey(prefix)));
    }
}

public abstract class RouteResolverBase<TTag, TIdentifier, TEntity, TRequest> : IRouteResolver<TIdentifier, TEntity, TRequest>
where TEntity : IEntity<TIdentifier>
{
    private readonly List<Func<TRequest, UserContext, IEnumerable<string>?>> _requestRules = new();
    private readonly List<Func<TEntity, EntityEventType, IEnumerable<string>?>> _entityRules = new();

    protected string TagName => typeof(TTag).Name;
    protected string EntityName => typeof(TEntity).Name;

    protected string Prefix => $"{TagName}.{EntityName}";

    protected string Key(params IEnumerable<string> parameters) =>
        string.Join(".", new string[] { TagName, EntityName }.Concat(parameters));

    protected string Key(string id) =>
        $"{TagName}.{EntityName}.{id}";

    protected void RequestRule(Func<TRequest, IEnumerable<string>?> rule) =>
        _requestRules.Add((request, _) => rule.Invoke(request));

    protected void RequestRule(Func<TRequest, UserContext, IEnumerable<string>?> rule) =>
        _requestRules.Add(rule);

    protected void EntityRule(Func<TEntity, EntityEventType, IEnumerable<string>?> rule) =>
        _entityRules.Add(rule);

    protected void EntityRule(Func<TEntity, IEnumerable<string>?> rule) =>
        _entityRules.Add((e, _) => rule(e));

    public virtual IEnumerable<string> GetGroupsForRequest(TRequest request, UserContext userContext)
    {
        var groups = new HashSet<string>();

        foreach (var rule in _requestRules)
        {
            var keys = rule.Invoke(request, userContext);

            if (keys == null)
                continue;

            foreach (var key in keys)
                groups.Add(key);
        }

        return groups;
    }

    public virtual IEnumerable<string> GetGroupsForEntities(TEntity identifier, EntityEventType eventType)
    {
        var groups = new HashSet<string>();

        foreach (var rule in _entityRules)
        {
            var keys = rule.Invoke(identifier, eventType);

            if (keys == null)
                continue;

            foreach (var key in keys)
                groups.Add(key);
        }

        return groups;
    }
}
