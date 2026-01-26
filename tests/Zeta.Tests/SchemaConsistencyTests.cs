using System.Reflection;
using Zeta.Schemas;

namespace Zeta.Tests;

/// <summary>
/// Tests that verify method parity between schema types and their ContextPromotedSchema extension methods.
/// When a validation method is added to a schema type, a corresponding extension method should be added
/// to SchemaExtensions for ContextPromotedSchema to maintain API consistency.
/// </summary>
public class SchemaConsistencyTests
{
    // Methods that are excluded from parity checks - these are either internal or have different semantics
    private static readonly HashSet<string> ExcludedMethods = new()
    {
        "Refine",           // Has different signatures for contextless vs context-aware
        "ValidateAsync",    // Core interface method, not a validation rule
        "GetType",          // Object method
        "ToString",         // Object method
        "Equals",           // Object method
        "GetHashCode",      // Object method
    };

    [Theory]
    [InlineData(typeof(StringSchema), typeof(ContextPromotedSchema<string, object>))]
    public void StringSchema_AllValidationMethodsHaveExtensions(Type schemaType, Type promotedType)
    {
        AssertMethodParity(schemaType, promotedType, "string");
    }

    [Theory]
    [InlineData(typeof(IntSchema), typeof(ContextPromotedSchema<int, object>))]
    public void IntSchema_AllValidationMethodsHaveExtensions(Type schemaType, Type promotedType)
    {
        AssertMethodParity(schemaType, promotedType, "int");
    }

    [Theory]
    [InlineData(typeof(DoubleSchema), typeof(ContextPromotedSchema<double, object>))]
    public void DoubleSchema_AllValidationMethodsHaveExtensions(Type schemaType, Type promotedType)
    {
        AssertMethodParity(schemaType, promotedType, "double");
    }

    [Theory]
    [InlineData(typeof(DecimalSchema), typeof(ContextPromotedSchema<decimal, object>))]
    public void DecimalSchema_AllValidationMethodsHaveExtensions(Type schemaType, Type promotedType)
    {
        AssertMethodParity(schemaType, promotedType, "decimal");
    }

    [Fact]
    public void ArraySchema_AllValidationMethodsHaveExtensions()
    {
        // ArraySchema<TElement> needs special handling due to the generic element type
        var schemaType = typeof(ArraySchema<>).MakeGenericType(typeof(int));
        var promotedType = typeof(ContextPromotedSchema<,>).MakeGenericType(typeof(int[]), typeof(object));
        AssertMethodParity(schemaType, promotedType, "array");
    }

    [Fact]
    public void ListSchema_AllValidationMethodsHaveExtensions()
    {
        // ListSchema<TElement> needs special handling due to the generic element type
        var schemaType = typeof(ListSchema<>).MakeGenericType(typeof(int));
        var promotedType = typeof(ContextPromotedSchema<,>).MakeGenericType(typeof(List<int>), typeof(object));
        AssertMethodParity(schemaType, promotedType, "list");
    }

    private static void AssertMethodParity(Type schemaType, Type promotedType, string typeName)
    {
        // Get validation methods from the schema type (instance methods that return the schema type itself)
        var schemaMethods = schemaType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.ReturnType == schemaType || m.ReturnType.IsAssignableTo(schemaType))
            .Where(m => !ExcludedMethods.Contains(m.Name))
            .Select(m => m.Name)
            .Distinct()
            .ToHashSet();

        // Get extension methods for the promoted type from SchemaExtensions
        var extensionsType = typeof(SchemaExtensions);
        var extensionMethods = extensionsType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.GetParameters().Length > 0)
            .Where(m => IsExtensionMethodForPromotedType(m, promotedType))
            .Select(m => m.Name)
            .Distinct()
            .ToHashSet();

        var missing = schemaMethods.Except(extensionMethods).ToList();

        if (missing.Count > 0)
        {
            Assert.Fail(
                $"The following {typeName} validation methods are missing corresponding " +
                $"ContextPromotedSchema extension methods in SchemaExtensions.cs:\n" +
                $"  - {string.Join("\n  - ", missing)}\n\n" +
                $"To fix: Add extension methods for ContextPromotedSchema<{typeName}, TContext> " +
                $"that match the signatures of these methods.");
        }
    }

    private static bool IsExtensionMethodForPromotedType(MethodInfo method, Type promotedType)
    {
        var firstParam = method.GetParameters().FirstOrDefault();
        if (firstParam == null) return false;

        var paramType = firstParam.ParameterType;

        // Check if it's directly the promoted type
        if (paramType == promotedType) return true;

        // Check if it's a generic ContextPromotedSchema<T, TContext> that matches our value type
        if (paramType.IsGenericType)
        {
            var genericDef = paramType.GetGenericTypeDefinition();
            if (genericDef == typeof(ContextPromotedSchema<,>))
            {
                // Check if the first type argument matches (the value type)
                var valueType = paramType.GetGenericArguments()[0];
                var targetValueType = promotedType.IsGenericType
                    ? promotedType.GetGenericArguments()[0]
                    : promotedType;

                // For array types like int[], we need to check if valueType is an array with matching element
                if (valueType.IsArray && targetValueType.IsArray)
                {
                    return valueType.GetElementType() == targetValueType.GetElementType() ||
                           IsOpenGenericMatch(valueType.GetElementType(), targetValueType.GetElementType());
                }

                // For List<T>, we need to check the generic argument
                if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>) &&
                    targetValueType.IsGenericType && targetValueType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listElementType = valueType.GetGenericArguments()[0];
                    var targetListElementType = targetValueType.GetGenericArguments()[0];
                    return listElementType == targetListElementType ||
                           IsOpenGenericMatch(listElementType, targetListElementType);
                }

                return valueType == targetValueType;
            }
        }

        // Check if method is generic and could be applied to the promoted type
        if (method.IsGenericMethodDefinition)
        {
            try
            {
                // Try to determine if this generic method could work for our type
                var typeArgs = method.GetGenericArguments();
                if (typeArgs.Length >= 1)
                {
                    // Check if the first parameter's generic type structure could match
                    var genericParamType = firstParam.ParameterType;
                    if (genericParamType.IsGenericType)
                    {
                        var genericDef = genericParamType.GetGenericTypeDefinition();
                        if (genericDef == typeof(ContextPromotedSchema<,>))
                        {
                            var valueTypeArg = genericParamType.GetGenericArguments()[0];

                            // For arrays: ContextPromotedSchema<TElement[], TContext>
                            if (valueTypeArg.IsArray && promotedType.IsGenericType)
                            {
                                var targetValueType = promotedType.GetGenericArguments()[0];
                                if (targetValueType.IsArray)
                                    return true;
                            }

                            // For lists: ContextPromotedSchema<List<TElement>, TContext>
                            if (valueTypeArg.IsGenericType &&
                                valueTypeArg.GetGenericTypeDefinition() == typeof(List<>) &&
                                promotedType.IsGenericType)
                            {
                                var targetValueType = promotedType.GetGenericArguments()[0];
                                if (targetValueType.IsGenericType &&
                                    targetValueType.GetGenericTypeDefinition() == typeof(List<>))
                                    return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                // If we can't resolve the generic, skip it
            }
        }

        return false;
    }

    private static bool IsOpenGenericMatch(Type? type1, Type? type2)
    {
        if (type1 == null || type2 == null) return false;
        if (type1.IsGenericParameter || type2.IsGenericParameter) return true;
        return type1 == type2;
    }
}
