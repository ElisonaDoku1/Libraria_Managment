using Library.Application.Contracts.Orders;

namespace Library.Application.Interfaces;

public interface IOrderService
{
    Task<OrderItemDto> CreateOrderAsync(OrderDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<OrderItemDto>> GetAllOrdersAsync(CancellationToken ct = default);
    Task<OrderItemDto?> GetOrderByIdAsync(int orderId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderItemDto>> GetOrdersByClientIdAsync(int clientId, CancellationToken ct = default);
    Task<OrderItemDto> EditOrderAsync(EditOrderDto dto, CancellationToken ct = default);
    Task DeleteOrderAsync(int orderId, CancellationToken ct = default);
}
