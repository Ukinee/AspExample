namespace Ukinee.Infrastructure.Ddd.Common.EntityFeatures.Contracts;

public interface IEntityWithPublicRead<out TSelf>
{
    public bool IsAvailableForPublicRead { get; }

    public TSelf OpenPublicRead();
    public TSelf ClosePublicRead();
}
