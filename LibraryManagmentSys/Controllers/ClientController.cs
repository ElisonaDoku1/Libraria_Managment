using Library.Application.Contracts.Clients;
using Library.Application.Contracts.Orders;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagmentSys.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Staff")]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly IOrderService _orderService;

    public ClientController(IClientService clientService, IOrderService orderService)
    {
        _clientService = clientService;
        _orderService = orderService;
    }





    [HttpPost("AddClient")]
    public async Task<ActionResult<ClientDto>> AddClient([FromBody] AddClientDto dto, CancellationToken ct)
        => Ok(await _clientService.AddClientAsync(dto, ct));

    
    
    [HttpGet("GetAllClients")]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAllClients(CancellationToken ct)
        => Ok(await _clientService.GetAllClientsAsync(ct));



    [HttpGet("GetClientById/{id:int}")]
    public async Task<ActionResult<ClientDto>> GetClientById(int id, CancellationToken ct)
    {
        var client = await _clientService.GetClientByIdAsync(id, ct);
        return client is null ? NotFound() : Ok(client);
    }

    
    
    [HttpGet("GetOrdersByClientId/{clientId:int}")]
    public async Task<ActionResult<IEnumerable<OrderItemDto>>> GetOrdersByClientId(int clientId, CancellationToken ct)
        => Ok(await _orderService.GetOrdersByClientIdAsync(clientId, ct));

    [HttpPut("EditClient")]
    public async Task<ActionResult<ClientDto>> EditClient([FromBody] ClientDto dto, CancellationToken ct)
        => Ok(await _clientService.EditClientAsync(dto, ct));

    
    
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("DeleteClient/{id:int}")]
    public async Task<IActionResult> DeleteClient(int id, CancellationToken ct)
    {
        await _clientService.DeleteClientAsync(id, ct);
        return Ok();
    }

    [HttpPost("CreateOrder")]
    public async Task<ActionResult<OrderItemDto>> CreateOrder([FromBody] OrderDto dto, CancellationToken ct)
        => Ok(await _orderService.CreateOrderAsync(dto, ct));

    [HttpGet("GetAllOrders")]
    public async Task<ActionResult<IEnumerable<OrderItemDto>>> GetAllOrders(CancellationToken ct)
        => Ok(await _orderService.GetAllOrdersAsync(ct));

    
    [HttpGet("GetOrderById/{orderId:int}")]
    public async Task<ActionResult<OrderItemDto>> GetOrderById(int orderId, CancellationToken ct)
    {
        
        var order = await _orderService.GetOrderByIdAsync(orderId, ct);
        
        
        return order is null ? NotFound() : Ok(order);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("DeleteOrder/{id}")]
    public async Task<IActionResult> DeleteOrder(string id, [FromQuery] int? orderId, CancellationToken ct)
    {
        var resolvedId = orderId ?? (int.TryParse(id, out var parsed) ? parsed : (int?)null);
        
        
        if (resolvedId is null)
            return BadRequest("A valid order id must be provided.");

        await _orderService.DeleteOrderAsync(resolvedId.Value, ct);
        
        return Ok();
    }

    [HttpPut("EditOrder")]
    public async Task<ActionResult<OrderItemDto>> EditOrder([FromBody] EditOrderDto dto, CancellationToken ct)
        => Ok(await _orderService.EditOrderAsync(dto, ct));
}
