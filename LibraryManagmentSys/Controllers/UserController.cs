using Library.Application.Contracts.Users;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagmentSys.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService) => _userService = userService;

    [HttpPost("AddUser")]
    public async Task<ActionResult<GetUsersDto>> AddUser([FromBody] AddUserDto dto, CancellationToken ct)
        => Ok(await _userService.AddUserAsync(dto, ct));

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet("GetUserById/{id:int}")]
    public async Task<ActionResult<IEnumerable<GetUsersDto>>> GetUserById(int id, CancellationToken ct)
        => Ok(await _userService.GetUserByIdAsync(id, ct));

    [HttpGet("GetAllUsers")]
    public async Task<ActionResult<IEnumerable<GetUsersDto>>> GetAllUsers(CancellationToken ct)
        => Ok(await _userService.GetAllUsersAsync(ct));

    [HttpPut("EditUser")]
    public async Task<ActionResult<GetUsersDto>> EditUser([FromBody] EditUserDto dto, CancellationToken ct)
        => Ok(await _userService.EditUserAsync(dto, ct));

    [HttpDelete("DeleteUser/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken ct)
    {
        await _userService.DeleteUserAsync(id, ct);
        return Ok();
    }



    [HttpPost("AddUserType")]
    public async Task<ActionResult<UserTypeDto>> AddUserType([FromBody] AddUserTypeDto dto, CancellationToken ct)
        => Ok(await _userService.AddUserTypeAsync(dto, ct));

    [HttpGet("GetUserTypes")]
    public async Task<ActionResult<IEnumerable<UserTypeDto>>> GetUserTypes(CancellationToken ct)
        => Ok(await _userService.GetUserTypesAsync(ct));

    [HttpPut("EditUserType")]
    public async Task<ActionResult<UserTypeDto>> EditUserType([FromBody] UserTypeDto dto, CancellationToken ct)
        => Ok(await _userService.EditUserTypeAsync(dto, ct));

    [HttpDelete("DeleteUserType/{id:int}")]
    public async Task<IActionResult> DeleteUserType(int id, CancellationToken ct)
    {
        await _userService.DeleteUserTypeAsync(id, ct);
        return Ok();
    }



    [HttpPost("AddUserRole")]
    public async Task<ActionResult<UserRolesDto>> AddUserRole([FromBody] AddUserRolesDto dto, CancellationToken ct)
        => Ok(await _userService.AddUserRoleAsync(dto, ct));

    [HttpGet("GetUserRoles")]
    public async Task<ActionResult<IEnumerable<UserRolesDto>>> GetUserRoles(CancellationToken ct)
        => Ok(await _userService.GetUserRolesAsync(ct));

    [HttpPut("EditUserRole")]
    public async Task<ActionResult<UserRolesDto>> EditUserRole([FromBody] UserRolesDto dto, CancellationToken ct)
        => Ok(await _userService.EditUserRoleAsync(dto, ct));



    [HttpDelete("DeleteUserRole/{id:int}")]
    public async Task<IActionResult> DeleteUserRole(int id, CancellationToken ct)
    {
        await _userService.DeleteUserRoleAsync(id, ct);
        return Ok();
    }
}
