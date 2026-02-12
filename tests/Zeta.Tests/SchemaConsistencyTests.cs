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
    public void Using_StringSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.String();
        var contextAware = contextless.Using<object>();

        Assert.IsType<StringContextSchema<object>>(contextAware);
    }

    [Fact]
    public void Using_IntSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Int();
        var contextAware = contextless.Using<object>();

        Assert.IsType<IntContextSchema<object>>(contextAware);
    }

    [Fact]
    public void Using_DoubleSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Double();
        var contextAware = contextless.Using<object>();

        Assert.IsType<DoubleContextSchema<object>>(contextAware);
    }

    [Fact]
    public void Using_DecimalSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Decimal();
        var contextAware = contextless.Using<object>();

        Assert.IsType<DecimalContextSchema<object>>(contextAware);
    }

    [Fact]
    public void Using_BoolSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Bool();
        var contextAware = contextless.Using<object>();

        Assert.IsType<BoolContextSchema<object>>(contextAware);
    }

    [Fact]
    public void Using_GuidSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Guid();
        var contextAware = contextless.Using<object>();

        Assert.IsType<GuidContextSchema<object>>(contextAware);
    }

    [Fact]
    public void Using_DateTimeSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.DateTime();
        var contextAware = contextless.Using<object>();

        Assert.IsType<DateTimeContextSchema<object>>(contextAware);
    }

    [Fact]
    public void Using_DateOnlySchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.DateOnly();
        var contextAware = contextless.Using<object>();

        Assert.IsType<DateOnlyContextSchema<object>>(contextAware);
    }

    [Fact]
    public void Using_TimeOnlySchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.TimeOnly();
        var contextAware = contextless.Using<object>();

        Assert.IsType<TimeOnlyContextSchema<object>>(contextAware);
    }

    [Fact]
    public void Using_ObjectSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Object<TestClass>();
        var contextAware = contextless.Using<object>();

        Assert.IsType<ObjectContextSchema<TestClass, object>>(contextAware);
    }

    [Fact]
    public void Using_ArraySchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Collection<int>();
        var contextAware = contextless.Using<object>();

        Assert.IsType<CollectionContextSchema<int, object>>(contextAware);
    }

    [Fact]
    public void Using_ListSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Collection<string>();
        var contextAware = contextless.Using<object>();

        Assert.IsType<CollectionContextSchema<string, object>>(contextAware);
    }

    private class TestClass { }

    /// <summary>
    /// Ensures all non-abstract classes inheriting from ContextlessSchema have a Using method.
    /// This enforces the pattern since we can't use an abstract method due to return type constraints.
    /// </summary>
    [Fact]
    public void AllContextlessSchemas_HaveUsingMethod()
    {
        var assembly = typeof(ContextlessSchema<,>).Assembly;
        var contextlessSchemaType = typeof(ContextlessSchema<,>);

        var contextlessSchemaTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.IsPublic)
            .Where(t => IsSubclassOfGeneric(t, contextlessSchemaType))
            .ToList();

        var missingUsing = new List<Type>();

        foreach (var type in contextlessSchemaTypes)
        {
            // Look for a public generic method named "Using" with one type parameter
            var usingMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "Using"
                    && m.IsGenericMethod
                    && m.GetGenericArguments().Length == 1);

            if (usingMethod == null)
            {
                missingUsing.Add(type);
            }
        }

        Assert.True(
            missingUsing.Count == 0,
            $"The following ContextlessSchema types are missing a Using<TContext>() method:\n" +
            string.Join("\n", missingUsing.Select(t => $"  - {t.FullName}")));
    }

    /// <summary>
    /// Ensures Using methods return context-aware schema types (not just ISchema).
    /// </summary>
    [Fact]
    public void AllContextlessSchemas_UsingReturnsTypedSchema()
    {
        var assembly = typeof(ContextlessSchema<,>).Assembly;
        var contextlessSchemaType = typeof(ContextlessSchema<,>);

        var contextlessSchemaTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.IsPublic)
            .Where(t => IsSubclassOfGeneric(t, contextlessSchemaType))
            .ToList();

        var returnsInterfaceOnly = new List<(Type SchemaType, Type ReturnType)>();

        foreach (var type in contextlessSchemaTypes)
        {
            var usingMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "Using"
                    && m.IsGenericMethod
                    && m.GetGenericArguments().Length == 1);

            if (usingMethod == null) continue;

            var returnType = usingMethod.ReturnType;

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
            $"The following ContextlessSchema types have Using<TContext>() returning an interface instead of a typed schema:\n" +
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

    /// <summary>
    /// Discovers all primitive schema types by finding matching Contextless/Context schema pairs.
    /// Excludes container types (Object, Array, List, Nullable) which have different Field semantics.
    /// </summary>
    private static IReadOnlyList<PrimitiveSchemaInfo> DiscoverPrimitiveSchemaTypes()
    {
        var assembly = typeof(ContextlessSchema<,>).Assembly;
        var results = new List<PrimitiveSchemaInfo>();

        // Container types that should not have Field overloads (they use generic Field<TProperty>)
        var excludedPrefixes = new[] { "Object", "Array", "List", "Nullable" };

        // Find all non-generic contextless schema types
        var contextlessTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsGenericType)
            .Where(t => t.Name.EndsWith("ContextlessSchema"))
            .Where(t => !excludedPrefixes.Any(p => t.Name.StartsWith(p)))
            .ToList();

        foreach (var contextlessType in contextlessTypes)
        {
            // Extract the prefix (e.g., "String" from "StringContextlessSchema")
            var prefix = contextlessType.Name.Replace("ContextlessSchema", "");

            // Find corresponding context-aware schema (e.g., "StringContextSchema`1")
            var contextTypeName = $"{prefix}ContextSchema`1";
            var contextType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name == contextTypeName);

            if (contextType == null) continue;

            // Extract the value type from ISchema<T> interface
            var schemaInterface = contextlessType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISchema<>));

            if (schemaInterface == null) continue;

            var valueType = schemaInterface.GetGenericArguments()[0];

            results.Add(new PrimitiveSchemaInfo(
                valueType,
                contextlessType,
                contextType,
                prefix));
        }

        return results;
    }

    private record PrimitiveSchemaInfo(
        Type ValueType,
        Type ContextlessSchemaType,
        Type ContextSchemaType,
        string SchemaPrefix);

    /// <summary>
    /// Ensures every primitive schema type has a corresponding context-aware schema type.
    /// </summary>
    [Fact]
    public void AllPrimitiveSchemas_HaveMatchingContextAndContextlessPairs()
    {
        var assembly = typeof(ContextlessSchema<,>).Assembly;
        var excludedPrefixes = new[] { "Object", "Array", "List", "Nullable" };

        var contextlessTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsGenericType)
            .Where(t => t.Name.EndsWith("ContextlessSchema"))
            .Where(t => !excludedPrefixes.Any(p => t.Name.StartsWith(p)))
            .ToList();

        var missingContextTypes = new List<string>();

        foreach (var contextlessType in contextlessTypes)
        {
            var prefix = contextlessType.Name.Replace("ContextlessSchema", "");
            var contextTypeName = $"{prefix}ContextSchema`1";
            var contextType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name == contextTypeName);

            if (contextType == null)
            {
                missingContextTypes.Add($"{prefix}ContextSchema<TContext> (for {contextlessType.Name})");
            }
        }

        Assert.True(
            missingContextTypes.Count == 0,
            $"Missing context-aware schema types:\n" +
            string.Join("\n", missingContextTypes.Select(t => $"  - {t}")));
    }

    /// <summary>
    /// Ensures ObjectContextlessSchema has Field overloads for ALL discovered primitive schema types.
    /// This test auto-discovers types, so adding a new schema type will cause it to fail until
    /// the corresponding Field overload is added.
    /// </summary>
    [Fact]
    public void ObjectContextlessSchema_HasFieldOverloads_ForAllDiscoveredPrimitiveTypes()
    {
        var primitiveSchemas = DiscoverPrimitiveSchemaTypes();
        var objectSchemaType = typeof(ObjectContextlessSchema<>);

        var fieldMethods = objectSchemaType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == "Field" && !m.IsGenericMethod)
            .ToList();

        var missingOverloads = new List<PrimitiveSchemaInfo>();

        foreach (var schema in primitiveSchemas)
        {
            var hasOverload = fieldMethods.Any(m => HasFieldOverloadForType(m, schema.ValueType));

            if (!hasOverload)
            {
                missingOverloads.Add(schema);
            }
        }

        Assert.True(
            missingOverloads.Count == 0,
            $"ObjectContextlessSchema<T> is missing Field overloads for:\n" +
            string.Join("\n", missingOverloads.Select(s =>
                $"  - {s.ValueType.Name} (add: Field(Expression<Func<T, {s.ValueType.Name}>>, Func<{s.ContextlessSchemaType.Name}, {s.ContextlessSchemaType.Name}>))")));
    }

    /// <summary>
    /// Ensures ObjectContextSchema has Field overloads for ALL discovered primitive schema types.
    /// This maintains strict parity with ObjectContextlessSchema.
    /// </summary>
    [Fact]
    public void ObjectContextSchema_HasFieldOverloads_ForAllDiscoveredPrimitiveTypes()
    {
        var primitiveSchemas = DiscoverPrimitiveSchemaTypes();
        var objectSchemaType = typeof(ObjectContextSchema<,>);

        var fieldMethods = objectSchemaType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == "Field" && !m.IsGenericMethod)
            .ToList();

        var missingOverloads = new List<PrimitiveSchemaInfo>();

        foreach (var schema in primitiveSchemas)
        {
            var hasOverload = fieldMethods.Any(m => HasFieldOverloadForType(m, schema.ValueType));

            if (!hasOverload)
            {
                missingOverloads.Add(schema);
            }
        }

        Assert.True(
            missingOverloads.Count == 0,
            $"ObjectContextSchema<T, TContext> is missing Field overloads for:\n" +
            string.Join("\n", missingOverloads.Select(s =>
                $"  - {s.ValueType.Name} (add: Field(Expression<Func<T, {s.ValueType.Name}>>, Func<{s.SchemaPrefix}ContextSchema<TContext>, {s.SchemaPrefix}ContextSchema<TContext>>))")));
    }

    /// <summary>
    /// Ensures Field overloads in ObjectContextlessSchema and ObjectContextSchema mirror each other.
    /// Both must have the exact same set of non-generic Field overloads (by value type).
    /// </summary>
    [Fact]
    public void ObjectSchemas_FieldOverloads_AreMirrored()
    {
        var contextlessType = typeof(ObjectContextlessSchema<>);
        var contextType = typeof(ObjectContextSchema<,>);

        var contextlessValueTypes = GetFieldOverloadValueTypes(contextlessType);
        var contextValueTypes = GetFieldOverloadValueTypes(contextType);

        var onlyInContextless = contextlessValueTypes.Except(contextValueTypes).ToList();
        var onlyInContext = contextValueTypes.Except(contextlessValueTypes).ToList();

        var errors = new List<string>();

        if (onlyInContextless.Count > 0)
        {
            errors.Add($"Field overloads in ObjectContextlessSchema but NOT in ObjectContextSchema:\n" +
                string.Join("\n", onlyInContextless.Select(t => $"    - {t.Name}")));
        }

        if (onlyInContext.Count > 0)
        {
            errors.Add($"Field overloads in ObjectContextSchema but NOT in ObjectContextlessSchema:\n" +
                string.Join("\n", onlyInContext.Select(t => $"    - {t.Name}")));
        }

        Assert.True(
            errors.Count == 0,
            $"ObjectContextlessSchema and ObjectContextSchema Field overloads are not mirrored:\n" +
            string.Join("\n", errors));
    }

    private static bool HasFieldOverloadForType(MethodInfo method, Type valueType)
    {
        var parameters = method.GetParameters();
        if (parameters.Length != 2) return false;

        var firstParam = parameters[0].ParameterType;
        if (!firstParam.IsGenericType) return false;
        if (firstParam.GetGenericTypeDefinition() != typeof(System.Linq.Expressions.Expression<>)) return false;

        var funcType = firstParam.GetGenericArguments()[0];
        if (!funcType.IsGenericType) return false;

        var funcArgs = funcType.GetGenericArguments();
        // funcArgs[0] is T (generic param), funcArgs[1] is the property type
        return funcArgs.Length == 2 && funcArgs[1] == valueType;
    }

    private static HashSet<Type> GetFieldOverloadValueTypes(Type objectSchemaType)
    {
        var fieldMethods = objectSchemaType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == "Field" && !m.IsGenericMethod);

        var valueTypes = new HashSet<Type>();

        foreach (var method in fieldMethods)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != 2) continue;

            var firstParam = parameters[0].ParameterType;
            if (!firstParam.IsGenericType) continue;
            if (firstParam.GetGenericTypeDefinition() != typeof(System.Linq.Expressions.Expression<>)) continue;

            var funcType = firstParam.GetGenericArguments()[0];
            if (!funcType.IsGenericType) continue;

            var funcArgs = funcType.GetGenericArguments();
            if (funcArgs.Length == 2)
            {
                valueTypes.Add(funcArgs[1]);
            }
        }

        return valueTypes;
    }

    /// <summary>
    /// Ensures validation methods on contextless schemas are mirrored on context-aware schemas.
    /// For example, if StringContextlessSchema has MinLength(int), StringContextSchema must also have MinLength(int).
    /// </summary>
    [Fact]
    public void AllPrimitiveSchemas_ValidationMethods_AreMirrored()
    {
        var primitiveSchemas = DiscoverPrimitiveSchemaTypes();
        var errors = new List<string>();

        // Methods that are structural/inherited and should be excluded from comparison
        var excludedMethods = new HashSet<string>
        {
            "ValidateAsync",
            "Using",
            "Use",
            "GetType",
            "ToString",
            "Equals",
            "GetHashCode"
        };

        foreach (var schema in primitiveSchemas)
        {
            var contextlessType = schema.ContextlessSchemaType;
            var contextType = schema.ContextSchemaType;

            // Get validation methods from contextless schema (declared on this type, not inherited)
            var contextlessMethods = contextlessType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName) // Exclude property accessors
                .Where(m => !excludedMethods.Contains(m.Name))
                .ToList();

            // Get methods from context-aware schema
            var contextMethods = contextType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .Where(m => !excludedMethods.Contains(m.Name))
                .ToList();

            // Check each contextless method has a matching context method
            foreach (var contextlessMethod in contextlessMethods)
            {
                var matchingContextMethod = FindMatchingMethod(contextlessMethod, contextMethods);

                if (matchingContextMethod == null)
                {
                    var paramSignature = string.Join(", ", contextlessMethod.GetParameters()
                        .Select(p => $"{GetFriendlyTypeName(p.ParameterType)} {p.Name}"));

                    errors.Add($"{contextType.Name} is missing: {contextlessMethod.Name}({paramSignature})");
                }
            }

            // Check each context method has a matching contextless method (except context-aware overloads)
            foreach (var contextMethod in contextMethods)
            {
                // Skip if this is a context-aware overload (has TContext in parameters)
                if (IsContextAwareOverload(contextMethod))
                    continue;

                var matchingContextlessMethod = FindMatchingMethod(contextMethod, contextlessMethods);

                if (matchingContextlessMethod == null)
                {
                    var paramSignature = string.Join(", ", contextMethod.GetParameters()
                        .Select(p => $"{GetFriendlyTypeName(p.ParameterType)} {p.Name}"));

                    errors.Add($"{contextlessType.Name} is missing: {contextMethod.Name}({paramSignature})");
                }
            }
        }

        Assert.True(
            errors.Count == 0,
            $"Schema validation methods are not mirrored:\n" +
            string.Join("\n", errors.Select(e => $"  - {e}")));
    }

    private static MethodInfo? FindMatchingMethod(MethodInfo source, List<MethodInfo> candidates)
    {
        var sourceParams = source.GetParameters();

        return candidates.FirstOrDefault(candidate =>
        {
            if (candidate.Name != source.Name)
                return false;

            var candidateParams = candidate.GetParameters();

            if (candidateParams.Length != sourceParams.Length)
                return false;

            // Compare parameter types (ignoring generic type arguments)
            for (int i = 0; i < sourceParams.Length; i++)
            {
                if (!AreParameterTypesCompatible(sourceParams[i].ParameterType, candidateParams[i].ParameterType))
                    return false;
            }

            return true;
        });
    }

    private static bool AreParameterTypesCompatible(Type type1, Type type2)
    {
        // Direct match
        if (type1 == type2)
            return true;

        // Handle nullable reference types (string vs string?)
        var underlying1 = Nullable.GetUnderlyingType(type1) ?? type1;
        var underlying2 = Nullable.GetUnderlyingType(type2) ?? type2;

        if (underlying1 == underlying2)
            return true;

        // Handle generic types - compare definitions
        if (type1.IsGenericType && type2.IsGenericType)
        {
            return type1.GetGenericTypeDefinition() == type2.GetGenericTypeDefinition();
        }

        return false;
    }

    private static bool IsContextAwareOverload(MethodInfo method)
    {
        // Get the TContext type parameter from the declaring type
        var declaringType = method.DeclaringType;
        if (declaringType == null || !declaringType.IsGenericType)
            return false;

        var contextTypeParam = declaringType.GetGenericArguments().FirstOrDefault();
        if (contextTypeParam == null)
            return false;

        // Check if any parameter type references TContext
        foreach (var param in method.GetParameters())
        {
            if (TypeReferencesGenericParam(param.ParameterType, contextTypeParam))
                return true;
        }

        return false;
    }

    private static bool TypeReferencesGenericParam(Type type, Type genericParam)
    {
        if (type == genericParam)
            return true;

        if (type.IsGenericType)
        {
            foreach (var arg in type.GetGenericArguments())
            {
                if (TypeReferencesGenericParam(arg, genericParam))
                    return true;
            }
        }

        return false;
    }

    private static string GetFriendlyTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var genericDef = type.GetGenericTypeDefinition();
        var args = type.GetGenericArguments();

        if (genericDef == typeof(Nullable<>))
            return $"{args[0].Name}?";

        var baseName = type.Name.Split('`')[0];
        return $"{baseName}<{string.Join(", ", args.Select(GetFriendlyTypeName))}>";
    }
}
