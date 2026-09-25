using Examples.Server.Domain.Models.ValueObjects;

namespace Examples.Server.Domain.Models.Contracts;

public interface IAddressHashService
{
    public string Calculate(Address address);
}
