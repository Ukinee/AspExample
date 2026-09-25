using System.Security.Cryptography;
using Ukinee.Infrastructure.Common.Contracts;

namespace Ukinee.Infrastructure.Common.Services;

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
