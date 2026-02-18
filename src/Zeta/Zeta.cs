using Zeta.Core;
using Zeta.Schemas;

namespace Zeta;

/// <summary>
/// Entry point for creating Zeta schemas.
/// </summary>
public static class Z
{
    /// <summary>
    /// Creates a schema for validating string values.
    /// </summary>
    public static StringContextlessSchema String() => new();

    /// <summary>
    /// Creates a schema for validating integer values.
    /// </summary>
    public static IntContextlessSchema Int() => new();

    /// <summary>
    /// Creates a schema for validating double-precision floating-point values.
    /// </summary>
    public static DoubleContextlessSchema Double() => new();

    /// <summary>
    /// Creates a schema for validating decimal values.
    /// </summary>
    public static DecimalContextlessSchema Decimal() => new();

    /// <summary>
    /// Creates a schema for validating object values of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to validate.</typeparam>
    public static ObjectContextlessSchema<T> Object<T>() where T : class => new();

    /// <summary>
    /// Creates a schema for validating object values of type <typeparamref name="T"/>.
    /// Alias of <see cref="Object{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to validate.</typeparam>
    public static ObjectContextlessSchema<T> Schema<T>() where T : class => Object<T>();

    /// <summary>
    /// Creates a schema for validating collections of elements of type <typeparamref name="TElement"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of elements in the collection.</typeparam>
    public static CollectionContextlessSchema<TElement> Collection<TElement>() => new(null, new ContextlessRuleEngine<ICollection<TElement>>());

    /// <summary>
    /// Creates a schema for validating collections with a pre-defined element schema.
    /// </summary>
    /// <typeparam name="TElement">The type of elements in the collection.</typeparam>
    /// <param name="elementSchema">The schema to use for validating each element in the collection.</param>
    public static CollectionContextlessSchema<TElement> Collection<TElement>(ISchema<TElement> elementSchema) => new(elementSchema, new ContextlessRuleEngine<ICollection<TElement>>());
    
    /// <summary>
    /// Creates a schema for validating DateTime values.
    /// </summary>
    public static DateTimeContextlessSchema DateTime() => new();

#if !NETSTANDARD2_0
    /// <summary>
    /// Creates a schema for validating DateOnly values.
    /// </summary>
    public static DateOnlyContextlessSchema DateOnly() => new();

    /// <summary>
    /// Creates a schema for validating TimeOnly values.
    /// </summary>
    public static TimeOnlyContextlessSchema TimeOnly() => new();
#endif

    /// <summary>
    /// Creates a schema for validating GUID values.
    /// </summary>
    public static GuidContextlessSchema Guid() => new();

    /// <summary>
    /// Creates a schema for validating boolean values.
    /// </summary>
    public static BoolContextlessSchema Bool() => new();

    /// <summary>
    /// Creates an empty validation context.
    /// </summary>
    public static ValidationContext Context() => ValidationContext.Empty;

    /// <summary>
    /// Creates a validation context with the specified context data.
    /// </summary>
    /// <typeparam name="TContext">The type of context data.</typeparam>
    /// <param name="value">The context data value.</param>
    public static ValidationContext<TContext> Context<TContext>(TContext value) => new(value);
}
