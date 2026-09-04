namespace Ukinee.Infrastructure.Ddd.Common.Entities;

public interface IEntity
{
    public object GetIdentifier();
}

public interface IEntity<out TIdentifier> : IEntity
{
    public TIdentifier Identifier { get; }

    object IEntity.GetIdentifier()
    {
        return Identifier!;
    }
}
