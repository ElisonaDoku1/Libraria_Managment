using Library.Application.Common;
using Library.Application.Contracts.Orders;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly LibraryDbContext _context;

    public OrderService(LibraryDbContext context) => _context = context;

    public async Task<OrderItemDto> CreateOrderAsync(OrderDto dto, CancellationToken ct = default)
    {
        var book = await _context.Books.FindAsync([dto.BookId], ct)
            ?? throw new NotFoundException($"Book {dto.BookId} was not found.");

        var client = await _context.Clients.FindAsync([dto.ClientId], ct)
            ?? throw new NotFoundException($"Client {dto.ClientId} was not found.");

        var order = new Order
        {
            Price = book.Price,
            OrderDate = DateTime.UtcNow,
            DeliveryType = dto.DeliveryType,
            PaymentType = dto.PaymentType,
            Note = dto.Note,
            Address = client.Address,
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(ct);

        _context.ClientOrderBridges.Add(new ClientOrderBridge
        {
            BookId = book.Id,
            ClientId = client.Id,
            OrderId = order.Id,
        });
        await _context.SaveChangesAsync(ct);

        return await GetOrderByIdAsync(order.Id, ct)
            ?? throw new NotFoundException($"Order {order.Id} was not found.");
    }

    public async Task<IReadOnlyList<OrderItemDto>> GetAllOrdersAsync(CancellationToken ct = default)
    {
        return await Query()
            .OrderByDescending(b => b.OrderDate)
            .ToListAsync(ct);
    }

    public async Task<OrderItemDto?> GetOrderByIdAsync(int orderId, CancellationToken ct = default)
    {
        return await Query()
            .FirstOrDefaultAsync(b => b.OrderId == orderId, ct);
    }

    public async Task<IReadOnlyList<OrderItemDto>> GetOrdersByClientIdAsync(int clientId, CancellationToken ct = default)
    {
        return await Query()
            .Where(b => b.ClientId == clientId)
            .OrderByDescending(b => b.OrderDate)
            .ToListAsync(ct);
    }

    public async Task<OrderItemDto> EditOrderAsync(EditOrderDto dto, CancellationToken ct = default)
    {
        //Find the order by id
        var order = await _context.Orders
            .Include(o => o.ClientOrderBridges)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId, ct)
            ?? throw new NotFoundException($"Order {dto.OrderId} was not found.");

        var bridge = order.ClientOrderBridges.FirstOrDefault()
            ?? throw new NotFoundException($"Order {dto.OrderId} has no linked book/client.");

        var book = await _context.Books.FindAsync([dto.BookId], ct)
            ?? throw new NotFoundException($"Book {dto.BookId} was not found.");

        var client = await _context.Clients.FindAsync([dto.ClientId], ct)
            ?? throw new NotFoundException($"Client {dto.ClientId} was not found.");

        order.DeliveryType = dto.DeliveryType;
        order.PaymentType = dto.PaymentType;
        order.Note = dto.Note;
        order.Price = book.Price;

        bridge.BookId = book.Id;
        bridge.ClientId = client.Id;

        await _context.SaveChangesAsync(ct);

        return await GetOrderByIdAsync(order.Id, ct)
            ?? throw new NotFoundException($"Order {order.Id} was not found.");
    }

    public async Task DeleteOrderAsync(int orderId, CancellationToken ct = default)
    {
        var order = await _context.Orders
            .Include(o => o.ClientOrderBridges)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new NotFoundException($"Order {orderId} was not found.");

        _context.ClientOrderBridges.RemoveRange(order.ClientOrderBridges);
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync(ct);
    }

    private IQueryable<OrderItemDto> Query() =>
        _context.ClientOrderBridges
            .AsNoTracking()
            .Include(b => b.Order)
            .Include(b => b.Client)
            .Include(b => b.Book)
            .Select(b => new OrderItemDto
            {
                OrderId = b.OrderId,
                Price = b.Order!.Price,
                OrderDate = b.Order.OrderDate,
                DeliveryType = b.Order.DeliveryType,
                PaymentType = b.Order.PaymentType,
                Note = b.Order.Note,
                Address = b.Order.Address,
                ClientOrderBridgeId = b.Id,
                ClientId = b.ClientId,
                ClientName = b.Client!.Name + " " + b.Client.Lastname,
                ClientEmail = b.Client.Email,
                BookId = b.BookId,
                BookTitle = b.Book!.Title,
                BookAuthor = b.Book.Author,
                BookPrice = b.Book.Price,
                BookPages = b.Book.Pages,
                BookPublicationYear = b.Book.PublicationYear,
                BookDescription = b.Book.Description,
            });
}
