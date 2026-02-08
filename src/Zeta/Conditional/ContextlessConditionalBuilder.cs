using System.Linq.Expressions;
using Zeta.Schemas;
using Zeta.Validators;

namespace Zeta.Conditional;

public sealed class ContextlessConditionalBuilder<T> where T : class
{
    internal List<IFieldContextlessValidator<T>> Validators { get; } = [];

    public ContextlessConditionalBuilder<T> Require<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        string? message = null)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        Validators.Add(new RequiredFieldContextlessValidator<T, TProperty>(propertyName, getter, message));
        return this;
    }

    public ContextlessConditionalBuilder<T> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty> schema)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        Validators.Add(new FieldContextlessValidator<T, TProperty>(propertyName, getter, schema));
        return this;
    }

    // ==================== Select Methods ====================

    /// <summary>
    /// Validates a string property using an inline schema builder.
    /// For nullable strings, call .Nullable() on the schema: s => s.MinLength(1).Nullable()
    /// </summary>
    public ContextlessConditionalBuilder<T> Select(
        Expression<Func<T, string?>> propertySelector,
        Func<StringContextlessSchema, ISchema<string>> schemaBuilder)
    {
        return Field(propertySelector, schemaBuilder(Z.String()));
    }

    /// <summary>
    /// Validates an int property using an inline schema builder.
    /// </summary>
    public ContextlessConditionalBuilder<T> Select(
        Expression<Func<T, int>> propertySelector,
        Func<IntContextlessSchema, IntContextlessSchema> schemaBuilder)
    {
        return Field(propertySelector, schemaBuilder(Z.Int()));
    }

    /// <summary>
    /// Validates a double property using an inline schema builder.
    /// </summary>
    public ContextlessConditionalBuilder<T> Select(
        Expression<Func<T, double>> propertySelector,
        Func<DoubleContextlessSchema, DoubleContextlessSchema> schemaBuilder)
    {
        return Field(propertySelector, schemaBuilder(Z.Double()));
    }

    /// <summary>
    /// Validates a decimal property using an inline schema builder.
    /// </summary>
    public ContextlessConditionalBuilder<T> Select(
        Expression<Func<T, decimal>> propertySelector,
        Func<DecimalContextlessSchema, DecimalContextlessSchema> schemaBuilder)
    {
        return Field(propertySelector, schemaBuilder(Z.Decimal()));
    }
}