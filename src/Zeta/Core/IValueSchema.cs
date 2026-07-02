using Zeta.Rules;

namespace Zeta.Core;

/// <summary>
/// Public append hook that lets a single extension method target both the contextless
/// and context-aware variant of a value schema and get the concrete self-type back for
/// fluent chaining.
/// </summary>
public interface IValueSchema<T, TSelf> where TSelf : IValueSchema<T, TSelf>
{
    TSelf AppendRule(IValidationRule<T> rule);
}
