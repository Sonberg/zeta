namespace Zeta.Validators;

internal sealed class DelegatedFieldContextlessValidator<TInstance, TProperty> : IFieldContextlessValidator<TInstance>
{
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = [];

    private readonly string _name;
    private readonly Func<TInstance, IServiceProvider, ISchema<TProperty>> _factory;

    public DelegatedFieldContextlessValidator(
        string name,
        Func<TInstance, TProperty?> getter,
        Func<TInstance, IServiceProvider, ISchema<TProperty>> factory)
    {
        _name = name;
        if (!string.IsNullOrEmpty(_name) && char.IsUpper(_name[0]))
            _name = char.ToLower(_name[0]) + _name.Substring(1);
        _getter = getter!;
        _factory = factory;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TInstance instance, ValidationContext execution)
    {
        var serviceProvider = execution.ServiceProvider
            ?? throw new InvalidOperationException("IServiceProvider is required for delegated schema factories.");

        var value = _getter(instance);
        var schema = _factory(instance, serviceProvider);
        
        // Handle nullable properties correctly by creating a wrapper if not already nullable
        var wrapper = Adapters.NullableAdapterFactory.CreateContextlessWrapper(schema);
        
        var fieldExecution = execution.Push(_name);
        var result = await wrapper.ValidateAsync(value, fieldExecution);
        return result.IsSuccess ? EmptyErrors : result.Errors;
    }
}
