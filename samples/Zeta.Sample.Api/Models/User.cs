namespace Zeta.Sample.Api.Models;

public record User(string Name, string Email);

public record UserWithAddress(string Name, string Email, Address Address);

public record Address(string Street, string City, string ZipCode);