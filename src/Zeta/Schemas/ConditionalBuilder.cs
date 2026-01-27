using System.Linq.Expressions;

namespace Zeta.Schemas;

public sealed class ConditionalBuilder<T, TContext> where T : class
{
    internal List<IFieldValidator<T, TContext>> Validators { get; } = [];

    public ConditionalBuilder<T, TContext> Require<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        string? message = null)
    {
        var propertyName = ObjectSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectSchema<T>.CreateGetter(propertySelector);
        Validators.Add(new RequiredFieldValidator<T, TProperty, TContext>(propertyName, getter, message));
        return this;
    }

    public ConditionalBuilder<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty, TContext> schema)
    {
        var propertyName = ObjectSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectSchema<T>.CreateGetter(propertySelector);
        Validators.Add(new FieldValidator<T, TProperty, TContext>(propertyName, getter, schema));
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
    /// For nullable strings, call .Nullable() on the schema: s => s.MinLength(1).Nullable()
    /// </summary>
    public ConditionalBuilder<T, TContext> Select(
        Expression<Func<T, string?>> propertySelector,
        Func<StringSchema, ISchema<string>> schemaBuilder)
    {
        return Field(propertySelector, schemaBuilder(Z.String()));
    }

    /// <summary>
    /// Validates an int property using an inline schema builder.
    /// </summary>
    public ConditionalBuilder<T, TContext> Select(
        Expression<Func<T, int>> propertySelector,
        Func<IntSchema, IntSchema> schemaBuilder)
    {
        return Field(propertySelector, schemaBuilder(Z.Int()));
    }

    /// <summary>
    /// Validates a nullable int property using an inline schema builder.
    /// </summary>
    public ConditionalBuilder<T, TContext> Select(
        Expression<Func<T, int?>> propertySelector,
        Func<IntSchema, IntSchema> schemaBuilder)
    {
        return Field(propertySelector, schemaBuilder(Z.Int()).Nullable());
    }

    /// <summary>
    /// Validates a double property using an inline schema builder.
    /// </summary>
    public ConditionalBuilder<T, TContext> Select(
        Expression<Func<T, double>> propertySelector,
        Func<DoubleSchema, DoubleSchema> schemaBuilder)
    {
        return Field(propertySelector, schemaBuilder(Z.Double()));
    }

    /// <summary>
    /// Validates a nullable double property using an inline schema builder.
    /// </summary>
    public ConditionalBuilder<T, TContext> Select(
        Expression<Func<T, double?>> propertySelector,
        Func<DoubleSchema, DoubleSchema> schemaBuilder)
    {
        return Field(propertySelector, schemaBuilder(Z.Double()).Nullable());
    }

    /// <summary>
    /// Validates a decimal property using an inline schema builder.
    /// </summary>
    public ConditionalBuilder<T, TContext> Select(
        Expression<Func<T, decimal>> propertySelector,
        Func<DecimalSchema, DecimalSchema> schemaBuilder)
    {
        return Field(propertySelector, schemaBuilder(Z.Decimal()));
    }

    /// <summary>
    /// Validates a nullable decimal property using an inline schema builder.
    /// </summary>
    public ConditionalBuilder<T, TContext> Select(
        Expression<Func<T, decimal?>> propertySelector,
        Func<DecimalSchema, DecimalSchema> schemaBuilder)
    {
        return Field(propertySelector, schemaBuilder(Z.Decimal()).Nullable());
    }
}