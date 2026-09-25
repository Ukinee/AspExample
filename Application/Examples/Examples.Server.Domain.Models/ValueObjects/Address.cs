namespace Examples.Server.Domain.Models.ValueObjects;

public record Address(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country
);
