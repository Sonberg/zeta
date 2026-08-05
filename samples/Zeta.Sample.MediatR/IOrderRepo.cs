namespace Zeta.Sample.MediatR;

public interface IOrderRepo
{
    ValueTask<Order?> FindAsync(int id, CancellationToken ct);
}

public sealed record Order(int Id, bool IsOpen);

public sealed class InMemoryOrderRepo : IOrderRepo
{
    private readonly Dictionary<int, Order> _orders = new()
    {
        [42] = new Order(42, IsOpen: true)
    };

    public ValueTask<Order?> FindAsync(int id, CancellationToken ct)
        => new(_orders.GetValueOrDefault(id));
}