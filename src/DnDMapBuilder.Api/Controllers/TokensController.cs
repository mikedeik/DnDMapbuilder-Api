using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Contracts.Responses;

namespace DnDMapBuilder.Api.Controllers;

/// <summary>
/// Controller for managing token definitions and token images.
/// </summary>
[ApiVersion("1.0")]
[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class TokensController : ControllerBase
{
    private readonly ITokenDefinitionService _tokenService;
    private readonly IFileStorageService _fileStorageService;

    public TokensController(ITokenDefinitionService tokenService, IFileStorageService fileStorageService)
    {
        _tokenService = tokenService;
        _fileStorageService = fileStorageService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();

    /// <summary>
    /// Gets all token definitions for the current user.
    /// </summary>
    /// <returns>Collection of user's token definitions</returns>
    [HttpGet]
    [ResponseCache(CacheProfileName = "Long300")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TokenDefinitionDto>>>> GetUserTokens()
    {
        var tokens = await _tokenService.GetUserTokensAsync(GetUserId());
        return Ok(new ApiResponse<IEnumerable<TokenDefinitionDto>>(true, tokens));
    }

    /// <summary>
    /// Gets a token definition by ID.
    /// </summary>
    /// <param name="id">The token definition ID</param>
    /// <returns>The token definition details or 404 if not found</returns>
    [HttpGet("{id}")]
    [ResponseCache(CacheProfileName = "Long300")]
    public async Task<ActionResult<ApiResponse<TokenDefinitionDto>>> GetToken(string id)
    {
        var token = await _tokenService.GetByIdAsync(id, GetUserId());

        if (token == null)
        {
            return NotFound(new ApiResponse<TokenDefinitionDto>(false, null, "Token not found."));
        }

        return Ok(new ApiResponse<TokenDefinitionDto>(true, token));
    }

    /// <summary>
    /// Creates a new token definition.
    /// </summary>
    /// <param name="request">Create token request</param>
    /// <returns>The created token definition details</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TokenDefinitionDto>>> CreateToken([FromBody] CreateTokenDefinitionRequest request)
    {
        var token = await _tokenService.CreateAsync(request, GetUserId());
        return CreatedAtAction(nameof(GetToken), new { id = token.Id }, new ApiResponse<TokenDefinitionDto>(true, token, "Token created."));
    }

    /// <summary>
    /// Updates an existing token definition.
    /// </summary>
    /// <param name="id">The token definition ID to update</param>
    /// <param name="request">Update token request</param>
    /// <returns>The updated token definition details or 404 if not found</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TokenDefinitionDto>>> UpdateToken(string id, [FromBody] UpdateTokenDefinitionRequest request)
    {
        var token = await _tokenService.UpdateAsync(id, request, GetUserId());

        if (token == null)
        {
            return NotFound(new ApiResponse<TokenDefinitionDto>(false, null, "Token not found."));
        }

        return Ok(new ApiResponse<TokenDefinitionDto>(true, token, "Token updated."));
    }

    /// <summary>
    /// Deletes a token definition.
    /// </summary>
    /// <param name="id">The token definition ID to delete</param>
    /// <returns>Success indicator or 404 if not found</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteToken(string id)
    {
        var result = await _tokenService.DeleteAsync(id, GetUserId());

        if (!result)
        {
            return NotFound(new ApiResponse<bool>(false, false, "Token not found."));
        }

        return Ok(new ApiResponse<bool>(true, true, "Token deleted."));
    }

    /// <summary>
    /// Uploads an image for a token definition.
    /// </summary>
    /// <param name="id">The token definition ID</param>
    /// <param name="image">The image file to upload</param>
    /// <returns>Upload response with file details or error status</returns>
    [EnableRateLimiting("fileUpload")]
    [HttpPost("{id}/image")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ImageUploadResponse>>> UploadTokenImage(string id, IFormFile image)
    {
        try
        {
            // Validate file
            if (image == null || image.Length == 0)
                return BadRequest(new ApiResponse<ImageUploadResponse>(false, null, "No file provided."));

            // Validate file size (2MB max for tokens)
            const long maxFileSize = 2 * 1024 * 1024;
            if (image.Length > maxFileSize)
                return BadRequest(new ApiResponse<ImageUploadResponse>(false, null, "File size exceeds 2MB limit."));

            // Validate MIME type
            var allowedMimeTypes = new[] { "image/png", "image/jpeg", "image/webp" };
            if (!allowedMimeTypes.Contains(image.ContentType?.ToLower() ?? ""))
                return BadRequest(new ApiResponse<ImageUploadResponse>(false, null, "Invalid file format. Allowed: PNG, JPEG, WebP."));

            // Get token to verify ownership
            var token = await _tokenService.GetByIdAsync(id, GetUserId());
            if (token == null)
                return NotFound(new ApiResponse<ImageUploadResponse>(false, null, "Token not found."));

            // Upload file
            var fileId = await _fileStorageService.UploadAsync(
                image.OpenReadStream(),
                image.FileName,
                image.ContentType,
                "tokens"
            );

            // Update token with file metadata
            var updatedToken = token with
            {
                ImageFileId = fileId,
                ImageContentType = image.ContentType,
                ImageFileSize = image.Length,
                ImageUrl = _fileStorageService.GetPublicUrl(fileId, "tokens")
            };

            // Update database
            var result = await _tokenService.UpdateAsync(id, new UpdateTokenDefinitionRequest(
                updatedToken.Name,
                updatedToken.ImageUrl,
                updatedToken.Size,
                updatedToken.Type
            ), GetUserId());

            var response = new ImageUploadResponse(fileId, result.ImageUrl ?? "", image.ContentType ?? "application/octet-stream", image.Length);
            return Ok(new ApiResponse<ImageUploadResponse>(true, response, "Image uploaded successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<ImageUploadResponse>(false, null, $"Error uploading image: {ex.Message}"));
        }
    }
}
