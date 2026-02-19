namespace Zeta.Validators;

internal sealed class DelegatedFieldContextValidator<TInstance, TProperty, TContext> : IFieldContextValidator<TInstance, TContext>
{
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = [];

    private readonly string _name;
    private readonly Func<TInstance, TProperty?> _getter;
    private readonly Func<TInstance, TContext, IServiceProvider, ISchema<TProperty, TContext>> _factory;

    public DelegatedFieldContextValidator(
        string name,
        Func<TInstance, TProperty?> getter,
        Func<TInstance, TContext, IServiceProvider, ISchema<TProperty, TContext>> factory)
    {
        _name = name;
        if (!string.IsNullOrEmpty(_name) && char.IsUpper(_name[0]))
            _name = char.ToLower(_name[0]) + _name.Substring(1);
        _getter = getter;
        _factory = factory;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TInstance instance, ValidationContext<TContext> context)
    {
        var serviceProvider = context.ServiceProvider
            ?? throw new InvalidOperationException("IServiceProvider is required for delegated schema factories.");

        var value = _getter(instance);
        var schema = _factory(instance, context.Data, serviceProvider);
        
        var wrapper = Adapters.NullableAdapterFactory.CreateContextWrapper(schema);
        
        var fieldExecution = context.Push(_name);
        var result = await wrapper.ValidateAsync(value, fieldExecution);
        return result.IsSuccess ? EmptyErrors : result.Errors;
    }
}
