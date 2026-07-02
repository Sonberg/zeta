using Zeta.Core;
using Zeta.Rules.Enum;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Enum validators. Work on both contextless and context-aware enum schemas.
/// </summary>
public static class EnumSchemaExtensions
{
    public static TSelf Defined<TEnum, TSelf>(this IValueSchema<TEnum, TSelf> schema, string? message = null)
        where TEnum : struct, Enum
        where TSelf : IValueSchema<TEnum, TSelf>
        => schema.AppendRule(new DefinedRule<TEnum>(message));

    public static TSelf OneOf<TEnum, TSelf>(this IValueSchema<TEnum, TSelf> schema, params TEnum[] allowed)
        where TEnum : struct, Enum
        where TSelf : IValueSchema<TEnum, TSelf>
        => schema.OneOf((IReadOnlyCollection<TEnum>)allowed, null);

    public static TSelf OneOf<TEnum, TSelf>(this IValueSchema<TEnum, TSelf> schema, IReadOnlyCollection<TEnum> allowed, string? message = null)
        where TEnum : struct, Enum
        where TSelf : IValueSchema<TEnum, TSelf>
        => schema.AppendRule(new OneOfRule<TEnum>(allowed, message));
}
