using System.Security.Cryptography;
using Common.Infrastructure.Contracts;

namespace Common.Infrastructure.Services;

public class FileHashService : IFileHashService
{
    public string Calculate(Stream stream)
    {
        using var hashAlgorithm = SHA256.Create();

        stream.Seek(0, SeekOrigin.Begin);
        var hash = hashAlgorithm.ComputeHash(stream);
        stream.Seek(0, SeekOrigin.Begin);

        return Convert.ToHexString(hash);
    }
}
