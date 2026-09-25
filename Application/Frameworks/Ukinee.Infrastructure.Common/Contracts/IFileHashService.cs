namespace Ukinee.Infrastructure.Common.Contracts;

public interface IFileHashService
{
    public string Calculate(Stream stream);
}

public interface IFileHashService<TConsumer> : IFileHashService;
