using Ukinee.Infrastructure.SignalR.Contracts;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.SignalR.Services;

public abstract class RouteResolverBase<TTag, TEntity, TRequest> : IRouteResolver<TEntity, TRequest>
{
    private readonly List<Func<TRequest, UserContext, string?>> _requestRules = new();
    private readonly List<Func<TEntity, EntityEventType, string?>> _entityRules = new();

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

    protected void EntityRule(Func<TEntity, EntityEventType, string?> rule) =>
        _entityRules.Add(rule);

    protected void EntityRule(Func<TEntity, string?> rule) =>
        _entityRules.Add((e, _) => rule(e));

    public virtual IEnumerable<string> GetGroupsForRequest(TRequest request, UserContext userContext)
    {
        var groups = new List<string>();

        foreach (var rule in _requestRules)
        {
            var key = rule.Invoke(request, userContext);

            if (!string.IsNullOrEmpty(key))
                groups.Add(key);
        }

        return groups;
    }

    public virtual IEnumerable<string> GetGroupsForEntity(TEntity entity, EntityEventType eventType)
    {
        var groups = new HashSet<string>();

        foreach (var rule in _entityRules)
        {
            var key = rule.Invoke(entity, eventType);

            if (!string.IsNullOrEmpty(key))
                groups.Add(key);
        }

        return groups;
    }
}
