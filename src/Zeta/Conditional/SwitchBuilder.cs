using Zeta.Core;
using Zeta.Schemas;

namespace Zeta.Conditional;

public sealed class SwitchBuilder<T> where T : class
{
    internal List<(Func<T, bool> Condition, ISchema<T> Schema)> Cases { get; } = [];
    internal ISchema<T>? DefaultSchema { get; private set; }

    public SwitchBuilder<T> Case(
        Func<T, bool> condition,
        Func<ObjectContextlessSchema<T>, ObjectContextlessSchema<T>> configure)
    {
        Cases.Add((condition, configure(new ObjectContextlessSchema<T>())));
        return this;
    }

    public SwitchBuilder<T> Default(
        Func<ObjectContextlessSchema<T>, ObjectContextlessSchema<T>> configure)
    {
        DefaultSchema = configure(new ObjectContextlessSchema<T>());
        return this;
    }
}
