namespace Zeta.Sample.Api.Validation;

// Context for user registration - loaded async before validation
public record RegisterUserContext(bool EmailExists);
