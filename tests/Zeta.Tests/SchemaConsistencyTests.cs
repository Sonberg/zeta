using System.Reflection;
using Zeta.Core;
using Zeta.Schemas;

namespace Zeta.Tests;

/// <summary>
/// Tests that verify schema types have consistent method signatures.
/// </summary>
public class SchemaConsistencyTests
{
    [Fact]
    public void WithContext_StringSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.String();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<StringSchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_IntSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Int();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<IntSchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_DoubleSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Double();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<DoubleSchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_DecimalSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Decimal();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<DecimalSchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_BoolSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Bool();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<BoolSchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_GuidSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Guid();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<GuidSchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_DateTimeSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.DateTime();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<DateTimeSchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_DateOnlySchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.DateOnly();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<DateOnlySchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_TimeOnlySchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.TimeOnly();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<TimeOnlySchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_ObjectSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Object<TestClass>();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<ObjectSchema<TestClass, object>>(contextAware);
    }

    [Fact]
    public void WithContext_ArraySchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Array(Z.Int());
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<ArraySchema<int, object>>(contextAware);
    }

    [Fact]
    public void WithContext_ListSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.List(Z.String());
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<ListSchema<string, object>>(contextAware);
    }

    private class TestClass { }

    /// <summary>
    /// Ensures all non-abstract classes inheriting from ContextlessSchema have a WithContext method.
    /// This enforces the pattern since we can't use an abstract method due to return type constraints.
    /// </summary>
    [Fact]
    public void AllContextlessSchemas_HaveWithContextMethod()
    {
        var assembly = typeof(ContextlessSchema<>).Assembly;
        var contextlessSchemaType = typeof(ContextlessSchema<>);

        var contextlessSchemaTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => IsSubclassOfGeneric(t, contextlessSchemaType))
            .ToList();

        var missingWithContext = new List<Type>();

        foreach (var type in contextlessSchemaTypes)
        {
            // Look for a public generic method named "WithContext" with one type parameter
            var withContextMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "WithContext"
                    && m.IsGenericMethod
                    && m.GetGenericArguments().Length == 1);

            if (withContextMethod == null)
            {
                missingWithContext.Add(type);
            }
        }

        Assert.True(
            missingWithContext.Count == 0,
            $"The following ContextlessSchema types are missing a WithContext<TContext>() method:\n" +
            string.Join("\n", missingWithContext.Select(t => $"  - {t.FullName}")));
    }

    /// <summary>
    /// Ensures WithContext methods return context-aware schema types (not just ISchema).
    /// </summary>
    [Fact]
    public void AllContextlessSchemas_WithContextReturnsTypedSchema()
    {
        var assembly = typeof(ContextlessSchema<>).Assembly;
        var contextlessSchemaType = typeof(ContextlessSchema<>);

        var contextlessSchemaTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => IsSubclassOfGeneric(t, contextlessSchemaType))
            .ToList();

        var returnsInterfaceOnly = new List<(Type SchemaType, Type ReturnType)>();

        foreach (var type in contextlessSchemaTypes)
        {
            var withContextMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "WithContext"
                    && m.IsGenericMethod
                    && m.GetGenericArguments().Length == 1);

            if (withContextMethod == null) continue;

            var returnType = withContextMethod.ReturnType;

            // Check if return type is an interface (ISchema<T, TContext>)
            // We want concrete types like StringSchema<TContext>, not ISchema<string, TContext>
            if (returnType.IsInterface ||
                (returnType.IsGenericType && returnType.GetGenericTypeDefinition().IsInterface))
            {
                returnsInterfaceOnly.Add((type, returnType));
            }
        }

        Assert.True(
            returnsInterfaceOnly.Count == 0,
            $"The following ContextlessSchema types have WithContext<TContext>() returning an interface instead of a typed schema:\n" +
            string.Join("\n", returnsInterfaceOnly.Select(x => $"  - {x.SchemaType.Name} returns {x.ReturnType.Name}")));
    }

    private static bool IsSubclassOfGeneric(Type type, Type genericBase)
    {
        while (type != null && type != typeof(object))
        {
            var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            if (genericBase == cur)
            {
                return true;
            }
            type = type.BaseType!;
        }
        return false;
    }
}
