using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Contracts.Responses;

namespace DnDMapBuilder.Api.Controllers;

/// <summary>
/// Controller for authentication operations (login, register).
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserManagementService _userManagementService;

    public AuthController(IAuthService authService, IUserManagementService userManagementService)
    {
        _authService = authService;
        _userManagementService = userManagementService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        // Create user via user management service
        var userDto = await _userManagementService.RegisterAsync(request);

        if (userDto == null)
        {
            return BadRequest(new ApiResponse<AuthResponse>(
                false,
                null,
                "Registration failed. User may already exist."
            ));
        }

        // Auto-login after successful registration
        var loginRequest = new LoginRequest(request.Email, request.Password);
        var authResponse = await _authService.LoginAsync(loginRequest);

        if (authResponse == null)
        {
            return Ok(new ApiResponse<AuthResponse>(
                true,
                null,
                "Registration successful. Awaiting admin approval."
            ));
        }

        return Ok(new ApiResponse<AuthResponse>(true, authResponse, "Registration successful. Token generated."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized(new ApiResponse<AuthResponse>(
                false,
                null,
                "Invalid credentials or account not approved."
            ));
        }

        return Ok(new ApiResponse<AuthResponse>(true, result, "Login successful."));
    }

    [Authorize(Roles = "admin")]
    [HttpGet("pending-users")]
    [ResponseCache(CacheProfileName = "Short10")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetPendingUsers()
    {
        var users = await _userManagementService.GetPendingUsersAsync();
        return Ok(new ApiResponse<IEnumerable<UserDto>>(true, users));
    }

    [Authorize(Roles = "admin")]
    [HttpPost("approve-user")]
    public async Task<ActionResult<ApiResponse<bool>>> ApproveUser([FromBody] ApproveUserRequest request)
    {
        var result = await _userManagementService.ApproveUserAsync(request.UserId, request.Approved);

        if (!result)
        {
            return NotFound(new ApiResponse<bool>(false, false, "User not found."));
        }

        return Ok(new ApiResponse<bool>(true, true, "User status updated."));
    }
}
