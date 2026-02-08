using System.Linq.Expressions;
using Zeta.Schemas;
using Zeta.Validators;

namespace Zeta.Conditional;

public sealed class ConditionalBuilder<T, TContext> where T : class
{
    internal List<IFieldContextValidator<T, TContext>> Validators { get; } = [];

    public ConditionalBuilder<T, TContext> Require<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        string? message = null)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        Validators.Add(new RequiredFieldContextContextValidator<T, TProperty, TContext>(propertyName, getter, message));
        return this;
    }

    public ConditionalBuilder<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty, TContext> schema)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        Validators.Add(new FieldContextContextValidator<T, TProperty, TContext>(propertyName, getter, schema));
        return this;
    }

    public ConditionalBuilder<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty> schema)
    {
        return Field(propertySelector, new SchemaAdapter<TProperty, TContext>(schema));
    }

    // ==================== Select Methods ====================

    /// <summary>
    /// Validates a string property using an inline schema builder.
    /// For nullable strings, call .AllowNull() on the schema: s => s.MinLength(1).AllowNull()
    /// </summary>
    public ConditionalBuilder<T, TContext> Select(
        Expression<Func<T, string?>> propertySelector,
        Func<StringContextlessSchema, ISchema<string>> schemaBuilder)
    {
        return Field(propertySelector, schemaBuilder(Z.String()));
    }

    /// <summary>
    /// Validates an int property using an inline schema builder.
    /// </summary>
    public ConditionalBuilder<T, TContext> Select(
        Expression<Func<T, int>> propertySelector,
        Func<IntContextlessSchema, IntContextlessSchema> schemaBuilder)
    {
        return Field(propertySelector, schemaBuilder(Z.Int()));
    }
    

    /// <summary>
    /// Validates a double property using an inline schema builder.
    /// </summary>
    public ConditionalBuilder<T, TContext> Select(
        Expression<Func<T, double>> propertySelector,
        Func<DoubleContextlessSchema, DoubleContextlessSchema> schemaBuilder)
    {
        return Field(propertySelector, schemaBuilder(Z.Double()));
    }

    /// <summary>
    /// Validates a decimal property using an inline schema builder.
    /// </summary>
    public ConditionalBuilder<T, TContext> Select(
        Expression<Func<T, decimal>> propertySelector,
        Func<DecimalContextlessSchema, DecimalContextlessSchema> schemaBuilder)
    {
        return Field(propertySelector, schemaBuilder(Z.Decimal()));
    }
}