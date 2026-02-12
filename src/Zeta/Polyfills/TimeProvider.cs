#if !NET8_0_OR_GREATER

namespace System;

public abstract class TimeProvider
{
    public static TimeProvider System { get; } = new SystemTimeProvider();

    public virtual DateTimeOffset GetUtcNow()
        => DateTimeOffset.UtcNow;

    public virtual DateTimeOffset GetLocalNow()
        => DateTimeOffset.Now;

    private sealed class SystemTimeProvider : TimeProvider
    {
    }
}

#endif