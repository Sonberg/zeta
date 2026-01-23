namespace Zeta.Sample.Api.Models;

// Basic user registration
public record RegisterUserRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string? Name,
    int Age);

// User with optional address (conditional validation)
public record CreateUserRequest(
    string Email,
    string Name,
    bool HasAddress,
    AddressDto? Address);

public record AddressDto(
    string Street,
    string City,
    string State,
    string ZipCode,
    string? Country);

// User profile update
public record UpdateProfileRequest(
    string? Name,
    string? PhoneNumber,
    DateOnly? DateOfBirth);
