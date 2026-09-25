namespace Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;

public interface IEntityWithPublicRead<out TSelf>
{
    public bool IsAvailableForPublicRead { get; }

    public TSelf OpenPublicRead();
    public TSelf ClosePublicRead();
}
