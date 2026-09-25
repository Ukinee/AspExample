using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.SignalR.Server.Contracts;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.SignalR.Server.Services;

public abstract class RouteResolverBase<TTag, TIdentifier, TEntity, TRequest> : IRouteResolver<TIdentifier, TEntity, TRequest>
where TEntity : IEntity<TIdentifier>
{
    private readonly List<Func<TRequest, UserContext, string?>> _requestRules = new();
    private readonly List<Func<TIdentifier, EntityEventType, string?>> _entityRules = new();

    protected virtual string TagName => typeof(TTag).Name;
    protected virtual string EntityName => typeof(TEntity).Name;

    protected string FeatureKey(string feature, string id) =>
        $"{TagName}.{EntityName}.{feature}.{id}";

    protected string Key(string id) =>
        $"{TagName}.{EntityName}.{id}";

    protected void RequestRule(Func<TRequest, string?> rule) =>
        _requestRules.Add((request, _) => rule.Invoke(request));

    protected void RequestRule(Func<TRequest, UserContext, string?> rule) =>
        _requestRules.Add(rule);

    protected void EntityRule(Func<TIdentifier, EntityEventType, string?> rule) =>
        _entityRules.Add(rule);

    protected void EntityRule(Func<TIdentifier, string?> rule) =>
        _entityRules.Add((e, _) => rule(e));

    public virtual IEnumerable<string> GetGroupsForRequest(TRequest request, UserContext userContext)
    {
        var groups = new HashSet<string>();

        foreach (var rule in _requestRules)
        {
            var key = rule.Invoke(request, userContext);

            if (!string.IsNullOrEmpty(key))
                groups.Add(key);
        }

        return groups;
    }

    public virtual IEnumerable<string> GetGroupsForIdentifier(TIdentifier identifier, EntityEventType eventType)
    {
        var groups = new HashSet<string>();

        foreach (var rule in _entityRules)
        {
            var key = rule.Invoke(identifier, eventType);

            if (!string.IsNullOrEmpty(key))
                groups.Add(key);
        }

        return groups;
    }
}
