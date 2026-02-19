using Zeta.Sample.Blazor.Models;
using Zeta.Core;
using Zeta;

namespace Zeta.Sample.Blazor.Validation;

public static class RegistrationSchema
{
    public static readonly ISchema<RegistrationRequest> Instance = Z.Schema<RegistrationRequest>()
        .Property(x => x.FullName, s => s.MinLength(2).MaxLength(100))
        .Property(x => x.Email, s => s.Email())
        .Property(x => x.Age, s => s.Min(18).Max(120))
        .Property(x => x.Password, s => s.MinLength(8).MaxLength(128))
        .Refine(
            x => !x.Password.Contains(x.Email, StringComparison.OrdinalIgnoreCase),
            "Password must not contain the email address.",
            "password_contains_email");
}
