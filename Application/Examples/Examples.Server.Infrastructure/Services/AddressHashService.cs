using System.IO.Hashing;
using System.Text;
using Examples.Server.Domain.Models.Contracts;
using Examples.Server.Domain.Models.ValueObjects;

namespace Examples.Server.Infrastructure.Services;

public class AddressHashService : IAddressHashService
{
    public string Calculate(Address address)
    {
        var data = $"{address.Country}{address.State}{address.City}{address.Street}{address.ZipCode}";

        var dataBytes = Encoding.UTF8.GetBytes(data);
        var hashBytes = XxHash64.HashToUInt64(dataBytes);

        return hashBytes.ToString();
    }
}
