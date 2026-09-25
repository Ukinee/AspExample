using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.SignalR.Server.Contracts;

public enum EntityEventType
{
    Created,
    Updated,
    Removed,
}

public interface IRouteResolver<in TIdentifier, in TEntity, in TRequest>
where TEntity : IEntity<TIdentifier>
{
    public IEnumerable<string> GetGroupsForRequest(TRequest request, UserContext userContext);
    public IEnumerable<string> GetGroupsForIdentifier(TIdentifier identifier, EntityEventType eventType);
}
