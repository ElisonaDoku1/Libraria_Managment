using Library.Application.Common;
using Library.Application.Contracts.Clients;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly LibraryDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public ClientService(LibraryDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }





    public async Task<ClientDto> AddClientAsync(AddClientDto dto, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(dto.Username) &&
            await _context.Clients.AnyAsync(c => c.Username == dto.Username, ct))
            throw new ConflictException($"Username '{dto.Username}' is already taken.");

        var client = new Client
        {

            Name = dto.Name,
            Lastname = dto.Lastname,
            Username = dto.Username,
            Password = string.IsNullOrEmpty(dto.Password) ? null : _passwordHasher.Hash(dto.Password),
            Email = dto.Email,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,

        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync(ct);

        return ToDto(client);
    }




    public async Task<IReadOnlyList<ClientDto>> GetAllClientsAsync(CancellationToken ct = default)
    {
        return await _context.Clients
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new ClientDto
            {
                Id = c.Id,
                Name = c.Name,
                Lastname = c.Lastname,
                Username = c.Username,
                Password = null,
                Email = c.Email,
                Address = c.Address,
                City = c.City,
                Country = c.Country,
            })
            .ToListAsync(ct);
    }



    public async Task<ClientDto?> GetClientByIdAsync(int id, CancellationToken ct = default)
    {
        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        return client is null ? null : ToDto(client);
    }



    public async Task<ClientDto> EditClientAsync(ClientDto dto, CancellationToken ct = default)
    {

        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == dto.Id, ct)
            ?? throw new NotFoundException($"Client {dto.Id} was not found.");

        if (!string.IsNullOrWhiteSpace(dto.Username) &&
            //look for someone else — a different client — who already has this username.
            await _context.Clients.AnyAsync(c => c.Id != dto.Id && c.Username == dto.Username, ct))
            throw new ConflictException($"Username '{dto.Username}' is already taken.");

        client.Name = dto.Name;
        client.Lastname = dto.Lastname;
        client.Username = dto.Username;
        client.Email = dto.Email;
        client.Address = dto.Address;
        client.City = dto.City;
        client.Country = dto.Country;

        if (!string.IsNullOrEmpty(dto.Password))
            client.Password = _passwordHasher.Hash(dto.Password);

        await _context.SaveChangesAsync(ct);

        return ToDto(client);
    }



    public async Task DeleteClientAsync(int id, CancellationToken ct = default)
    {
        var client = await _context.Clients
            .Include(c => c.ClientOrderBridges)
            .Include(c => c.RefreshTokens)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException($"Client {id} was not found.");

        if (client.ClientOrderBridges.Count > 0)
            throw new ConflictException("Cannot delete a client that has existing orders.");

        _context.RefreshTokens.RemoveRange(client.RefreshTokens);
        _context.Clients.Remove(client);
        await _context.SaveChangesAsync(ct);
    }



    private static ClientDto ToDto(Client client) => new()
    {
        Id = client.Id,
        Name = client.Name,
        Lastname = client.Lastname,
        Username = client.Username,
        Password = null,
        Email = client.Email,
        Address = client.Address,
        City = client.City,
        Country = client.Country,
    };
}
