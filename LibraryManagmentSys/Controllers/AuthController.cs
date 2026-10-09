using Library.Application.Contracts.Auth;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagmentSys.Controllers;

[ApiController]
[Route("Auth")]
public class AuthController : ControllerBase
{
    private const string RefreshCookieName = "refreshToken";

    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;


    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginDto dto, CancellationToken ct)
    {

        var result = await _authService.LoginAsync(dto, ct);
        SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAt);

        return Ok(result.Response);

    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResultDto>> RefreshToken(CancellationToken ct)
    {
        Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken);
        var result = await _authService.RefreshAsync(refreshToken, ct);
        SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAt);

        return Ok(result.Response);
    }

    [AllowAnonymous]
    [HttpGet("token-status")]
    public async Task<ActionResult<TokenStatusDto>> TokenStatus(CancellationToken ct)
    {
        Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken);

        return Ok(await _authService.GetTokenStatusAsync(refreshToken, ct));

    }


    
    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        //Look in the cookies that came with this request. Is there one called refreshToken? If so, put its value into a variable called refreshToken
        Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken);
        //Read the token out of the incoming cookie.
        await _authService.LogoutAsync(refreshToken, ct);

        // must carry the same attributes the cookie was set with, or a cross-site browser may ignore the deletion
        Response.Cookies.Delete(RefreshCookieName, RefreshCookieOptions());
        return Ok();

    }

    private void SetRefreshCookie(string token, DateTime expiresAt)
    {

        var options = RefreshCookieOptions();
        options.Expires = expiresAt;

        Response.Cookies.Append(RefreshCookieName, token, options);
    }

    private static CookieOptions RefreshCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Path = "/Auth",
    };

}
