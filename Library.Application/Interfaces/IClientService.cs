using Library.Application.Contracts.Clients;

namespace Library.Application.Interfaces;

public interface IClientService
{
    Task<ClientDto> AddClientAsync(AddClientDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<ClientDto>> GetAllClientsAsync(CancellationToken ct = default);
    Task<ClientDto?> GetClientByIdAsync(int id, CancellationToken ct = default);
    Task<ClientDto> EditClientAsync(ClientDto dto, CancellationToken ct = default);
    Task DeleteClientAsync(int id, CancellationToken ct = default);
}
