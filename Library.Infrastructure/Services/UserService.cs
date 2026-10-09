using Library.Application.Common;
using Library.Application.Contracts.Users;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly LibraryDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(LibraryDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<GetUsersDto> AddUserAsync(AddUserDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Username))
            throw new ValidationAppException("Username is required.");

        if (await _context.Users.AnyAsync(u => u.Username == dto.Username, ct))
            throw new ConflictException($"Username '{dto.Username}' is already taken.");

        var userType = await _context.UserTypes.FindAsync([dto.UserTypeId], ct)
            ?? throw new NotFoundException($"User type {dto.UserTypeId} was not found.");

        var user = new User
        {
            Name = dto.Name,
            Lastname = dto.Lastname,
            Username = dto.Username,
            Password = _passwordHasher.Hash(dto.Password ?? throw new ValidationAppException("Password is required.")),
            EmployeeId = $"EMP-{Guid.NewGuid():N}".ToUpperInvariant()[..12],
            UserTypeId = userType.Id,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        if (dto.RoleIds is { Count: > 0 })
        {
            var validRoleIds = await _context.UserRoles.Where(r => dto.RoleIds.Contains(r.Id)).Select(r => r.Id).ToListAsync(ct);
            foreach (var roleId in validRoleIds)
                _context.UserRoleBridges.Add(new UserRoleBridge { UserId = user.Id, UserRoleId = roleId });

            await _context.SaveChangesAsync(ct);
        }

        return (await GetUserByIdAsync(user.Id, ct)).Single();
    }

    public async Task<IReadOnlyList<GetUsersDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserType)
            .Include(u => u.UserRoleBridges).ThenInclude(b => b.UserRole)
            .OrderBy(u => u.Id)
            .ToListAsync(ct);

        return users.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<GetUsersDto>> GetUserByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserType)
            .Include(u => u.UserRoleBridges).ThenInclude(b => b.UserRole)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        return user is null ? [] : [ToDto(user)];
    }

    public async Task<GetUsersDto> EditUserAsync(EditUserDto dto, CancellationToken ct = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoleBridges)
            .FirstOrDefaultAsync(u => u.Id == dto.Id, ct)
            ?? throw new NotFoundException($"User {dto.Id} was not found.");

        if (!string.IsNullOrWhiteSpace(dto.Username) &&
            await _context.Users.AnyAsync(u => u.Id != dto.Id && u.Username == dto.Username, ct))
            throw new ConflictException($"Username '{dto.Username}' is already taken.");

        var userType = await _context.UserTypes.FindAsync([dto.UserTypeId], ct)
            ?? throw new NotFoundException($"User type {dto.UserTypeId} was not found.");

        user.Name = dto.Name;
        user.Lastname = dto.Lastname;
        user.Username = dto.Username;
        user.UserTypeId = userType.Id;

        if (!string.IsNullOrEmpty(dto.Password))
            user.Password = _passwordHasher.Hash(dto.Password);

        if (dto.UserRoles is not null)
        {
            _context.UserRoleBridges.RemoveRange(user.UserRoleBridges);

            var validRoleIds = await _context.UserRoles.Where(r => dto.UserRoles.Contains(r.Id)).Select(r => r.Id).ToListAsync(ct);
            foreach (var roleId in validRoleIds)
                _context.UserRoleBridges.Add(new UserRoleBridge { UserId = user.Id, UserRoleId = roleId });
        }

        await _context.SaveChangesAsync(ct);

        return (await GetUserByIdAsync(user.Id, ct)).Single();
    }

    public async Task DeleteUserAsync(int id, CancellationToken ct = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoleBridges)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException($"User {id} was not found.");

        _context.UserRoleBridges.RemoveRange(user.UserRoleBridges);
        _context.RefreshTokens.RemoveRange(user.RefreshTokens);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<UserTypeDto> AddUserTypeAsync(AddUserTypeDto dto, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(dto.UserTypeName) &&
            await _context.UserTypes.AnyAsync(t => t.UserTypeName == dto.UserTypeName, ct))
            throw new ConflictException($"User type '{dto.UserTypeName}' already exists.");

        var userType = new UserType { UserTypeName = dto.UserTypeName };
        _context.UserTypes.Add(userType);
        await _context.SaveChangesAsync(ct);

        return new UserTypeDto { Id = userType.Id, UserTypeName = userType.UserTypeName };
    }

    public async Task<IReadOnlyList<UserTypeDto>> GetUserTypesAsync(CancellationToken ct = default)
    {
        return await _context.UserTypes
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .Select(t => new UserTypeDto { Id = t.Id, UserTypeName = t.UserTypeName })
            .ToListAsync(ct);
    }

    public async Task<UserTypeDto> EditUserTypeAsync(UserTypeDto dto, CancellationToken ct = default)
    {
        var userType = await _context.UserTypes.FindAsync([dto.Id], ct)
            ?? throw new NotFoundException($"User type {dto.Id} was not found.");

        if (!string.IsNullOrWhiteSpace(dto.UserTypeName) &&
            await _context.UserTypes.AnyAsync(t => t.Id != dto.Id && t.UserTypeName == dto.UserTypeName, ct))
            throw new ConflictException($"User type '{dto.UserTypeName}' already exists.");

        userType.UserTypeName = dto.UserTypeName;
        await _context.SaveChangesAsync(ct);

        return new UserTypeDto { Id = userType.Id, UserTypeName = userType.UserTypeName };
    }

    public async Task DeleteUserTypeAsync(int id, CancellationToken ct = default)
    {
        var userType = await _context.UserTypes.FindAsync([id], ct)
            ?? throw new NotFoundException($"User type {id} was not found.");

        var inUse = await _context.Users.AnyAsync(u => u.UserTypeId == id, ct);
        if (inUse)
            throw new ConflictException("Cannot delete a user type that is still assigned to users.");

        _context.UserTypes.Remove(userType);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<UserRolesDto> AddUserRoleAsync(AddUserRolesDto dto, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(dto.Role) &&
            await _context.UserRoles.AnyAsync(r => r.Role == dto.Role, ct))
            throw new ConflictException($"User role '{dto.Role}' already exists.");

        var role = new UserRoles { Role = dto.Role };
        _context.UserRoles.Add(role);
        await _context.SaveChangesAsync(ct);

        return new UserRolesDto { Id = role.Id, Role = role.Role };
    }

    public async Task<IReadOnlyList<UserRolesDto>> GetUserRolesAsync(CancellationToken ct = default)
    {
        return await _context.UserRoles
            .AsNoTracking()
            .OrderBy(r => r.Id)
            .Select(r => new UserRolesDto { Id = r.Id, Role = r.Role })
            .ToListAsync(ct);
    }

    public async Task<UserRolesDto> EditUserRoleAsync(UserRolesDto dto, CancellationToken ct = default)
    {
        var role = await _context.UserRoles.FindAsync([dto.Id], ct)
            ?? throw new NotFoundException($"User role {dto.Id} was not found.");

        if (!string.IsNullOrWhiteSpace(dto.Role) &&
            await _context.UserRoles.AnyAsync(r => r.Id != dto.Id && r.Role == dto.Role, ct))
            throw new ConflictException($"User role '{dto.Role}' already exists.");

        role.Role = dto.Role;
        await _context.SaveChangesAsync(ct);

        return new UserRolesDto { Id = role.Id, Role = role.Role };
    }

    public async Task DeleteUserRoleAsync(int id, CancellationToken ct = default)
    {
        var role = await _context.UserRoles.FindAsync([id], ct)
            ?? throw new NotFoundException($"User role {id} was not found.");

        var bridges = _context.UserRoleBridges.Where(b => b.UserRoleId == id);
        _context.UserRoleBridges.RemoveRange(bridges);

        _context.UserRoles.Remove(role);
        await _context.SaveChangesAsync(ct);
    }

    private static GetUsersDto ToDto(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Lastname = user.Lastname,
        Username = user.Username,
        EmployeeId = user.EmployeeId,
        UserType = user.UserType?.UserTypeName,
        Roles = user.UserRoleBridges.Select(b => b.UserRole?.Role ?? string.Empty).Where(r => r.Length > 0).ToList(),
    };
}
    