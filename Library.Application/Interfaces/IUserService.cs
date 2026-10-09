using Library.Application.Contracts.Users;

namespace Library.Application.Interfaces;

public interface IUserService
{
    Task<GetUsersDto> AddUserAsync(AddUserDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<GetUsersDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<GetUsersDto>> GetUserByIdAsync(int id, CancellationToken ct = default);
    Task<GetUsersDto> EditUserAsync(EditUserDto dto, CancellationToken ct = default);
    Task DeleteUserAsync(int id, CancellationToken ct = default);

    Task<UserTypeDto> AddUserTypeAsync(AddUserTypeDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<UserTypeDto>> GetUserTypesAsync(CancellationToken ct = default);
    Task<UserTypeDto> EditUserTypeAsync(UserTypeDto dto, CancellationToken ct = default);
    Task DeleteUserTypeAsync(int id, CancellationToken ct = default);

    Task<UserRolesDto> AddUserRoleAsync(AddUserRolesDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<UserRolesDto>> GetUserRolesAsync(CancellationToken ct = default);
    Task<UserRolesDto> EditUserRoleAsync(UserRolesDto dto, CancellationToken ct = default);
    Task DeleteUserRoleAsync(int id, CancellationToken ct = default);
}
