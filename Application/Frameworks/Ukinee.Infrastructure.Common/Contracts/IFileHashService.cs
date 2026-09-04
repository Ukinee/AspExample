namespace Common.Infrastructure.Contracts;

public interface IFileHashService
{
    public string Calculate(Stream stream);
}

public interface IFileHashService<TConsumer> : IFileHashService;
