namespace Zeta.AspNetCore;

/// <summary>
/// Indicates that a parameter should be skipped by Zeta validation.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ZetaIgnoreAttribute : Attribute
{
}

/// <summary>
/// Specifies that a parameter should be validated using a specific schema type.
/// The schema type must implement ISchema&lt;T&gt; where T is the parameter type.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ZetaValidateAttribute : Attribute
{
    /// <summary>
    /// The type of the schema to use for validation.
    /// Must be a class that implements ISchema&lt;T&gt; and can be resolved from DI or instantiated.
    /// </summary>
    public Type SchemaType { get; }

    /// <summary>
    /// Creates a new ZetaValidateAttribute with the specified schema type.
    /// </summary>
    /// <param name="schemaType">The schema type to use for validation.</param>
    public ZetaValidateAttribute(Type schemaType)
    {
        SchemaType = schemaType ?? throw new ArgumentNullException(nameof(schemaType));
    }
}
