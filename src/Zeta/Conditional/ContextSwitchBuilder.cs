using Zeta.Core;
using Zeta.Schemas;

namespace Zeta.Conditional;

public sealed class ContextSwitchBuilder<T, TContext> where T : class
{
    internal List<(Func<T, TContext, bool> Condition, ISchema<T, TContext> Schema)> Cases { get; } = [];
    internal ISchema<T, TContext>? DefaultSchema { get; private set; }

    public ContextSwitchBuilder<T, TContext> Case(
        Func<T, TContext, bool> condition,
        Func<ObjectContextSchema<T, TContext>, ObjectContextSchema<T, TContext>> configure)
    {
        Cases.Add((condition, configure(new ObjectContextSchema<T, TContext>())));
        return this;
    }

    public ContextSwitchBuilder<T, TContext> Default(
        Func<ObjectContextSchema<T, TContext>, ObjectContextSchema<T, TContext>> configure)
    {
        DefaultSchema = configure(new ObjectContextSchema<T, TContext>());
        return this;
    }
}
