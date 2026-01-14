using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Contracts.Responses;

namespace DnDMapBuilder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        
        if (result == null)
        {
            return BadRequest(new ApiResponse<AuthResponse>(
                false,
                null,
                "Registration failed. User may already exist."
            ));
        }

        return Ok(new ApiResponse<AuthResponse>(true, result, "Registration successful. Awaiting admin approval."));
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
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetPendingUsers()
    {
        var users = await _authService.GetPendingUsersAsync();
        return Ok(new ApiResponse<IEnumerable<UserDto>>(true, users));
    }

    [Authorize(Roles = "admin")]
    [HttpPost("approve-user")]
    public async Task<ActionResult<ApiResponse<bool>>> ApproveUser([FromBody] ApproveUserRequest request)
    {
        var result = await _authService.ApproveUserAsync(request.UserId, request.Approved);
        
        if (!result)
        {
            return NotFound(new ApiResponse<bool>(false, false, "User not found."));
        }

        return Ok(new ApiResponse<bool>(true, true, "User status updated."));
    }
}
