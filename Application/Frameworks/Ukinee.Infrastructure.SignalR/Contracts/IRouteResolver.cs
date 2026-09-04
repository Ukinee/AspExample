using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.SignalR.Contracts;

public enum EntityEventType
{
    Created,
    Updated,
    Removed,
}

public interface IRouteResolver<in TEntity, in TRequest>
{
    public IEnumerable<string> GetGroupsForRequest(TRequest request, UserContext userContext);
    public IEnumerable<string> GetGroupsForEntity(TEntity entity, EntityEventType eventType);
}
