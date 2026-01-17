using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Contracts.Responses;

namespace DnDMapBuilder.Api.Controllers;

/// <summary>
/// Controller for managing game maps and map images.
/// </summary>
[ApiVersion("1.0")]
[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class GameMapsController : ControllerBase
{
    private readonly IGameMapService _mapService;
    private readonly IFileStorageService _fileStorageService;

    public GameMapsController(IGameMapService mapService, IFileStorageService fileStorageService)
    {
        _mapService = mapService;
        _fileStorageService = fileStorageService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();

    /// <summary>
    /// Gets a game map by ID.
    /// </summary>
    /// <param name="id">The game map ID</param>
    /// <returns>The game map details or 404 if not found</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GameMapDto>>> GetMap(string id)
    {
        var map = await _mapService.GetByIdAsync(id, GetUserId());

        if (map == null)
        {
            return NotFound(new ApiResponse<GameMapDto>(false, null, "Map not found."));
        }

        return Ok(new ApiResponse<GameMapDto>(true, map));
    }

    /// <summary>
    /// Gets all game maps for a specific mission.
    /// </summary>
    /// <param name="missionId">The mission ID</param>
    /// <returns>Collection of game maps in the mission</returns>
    [HttpGet("mission/{missionId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<GameMapDto>>>> GetMapsByMission(string missionId)
    {
        var maps = await _mapService.GetByMissionIdAsync(missionId, GetUserId());
        return Ok(new ApiResponse<IEnumerable<GameMapDto>>(true, maps));
    }

    /// <summary>
    /// Creates a new game map in a mission.
    /// </summary>
    /// <param name="request">Create map request</param>
    /// <returns>The created game map details or 403 if unauthorized</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<GameMapDto>>> CreateMap([FromBody] CreateMapRequest request)
    {
        try
        {
            var map = await _mapService.CreateAsync(request, GetUserId());
            return CreatedAtAction(nameof(GetMap), new { id = map.Id }, new ApiResponse<GameMapDto>(true, map, "Map created."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing game map.
    /// </summary>
    /// <param name="id">The game map ID to update</param>
    /// <param name="request">Update map request</param>
    /// <returns>The updated game map details or 404 if not found</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<GameMapDto>>> UpdateMap(string id, [FromBody] UpdateMapRequest request)
    {
        var map = await _mapService.UpdateAsync(id, request, GetUserId());

        if (map == null)
        {
            return NotFound(new ApiResponse<GameMapDto>(false, null, "Map not found."));
        }

        return Ok(new ApiResponse<GameMapDto>(true, map, "Map updated."));
    }

    /// <summary>
    /// Deletes a game map.
    /// </summary>
    /// <param name="id">The game map ID to delete</param>
    /// <returns>Success indicator or 404 if not found</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMap(string id)
    {
        var result = await _mapService.DeleteAsync(id, GetUserId());

        if (!result)
        {
            return NotFound(new ApiResponse<bool>(false, false, "Map not found."));
        }

        return Ok(new ApiResponse<bool>(true, true, "Map deleted."));
    }

    /// <summary>
    /// Uploads an image for a game map.
    /// </summary>
    /// <param name="id">The game map ID</param>
    /// <param name="image">The image file to upload</param>
    /// <returns>Upload response with file details or error status</returns>
    [HttpPost("{id}/image")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ImageUploadResponse>>> UploadMapImage(string id, IFormFile image)
    {
        try
        {
            // Validate file
            if (image == null || image.Length == 0)
                return BadRequest(new ApiResponse<ImageUploadResponse>(false, null, "No file provided."));

            // Validate file size (5MB max for maps)
            const long maxFileSize = 5 * 1024 * 1024;
            if (image.Length > maxFileSize)
                return BadRequest(new ApiResponse<ImageUploadResponse>(false, null, "File size exceeds 5MB limit."));

            // Validate MIME type
            var allowedMimeTypes = new[] { "image/png", "image/jpeg", "image/webp" };
            if (!allowedMimeTypes.Contains(image.ContentType?.ToLower() ?? ""))
                return BadRequest(new ApiResponse<ImageUploadResponse>(false, null, "Invalid file format. Allowed: PNG, JPEG, WebP."));

            // Get map to verify ownership
            var map = await _mapService.GetByIdAsync(id, GetUserId());
            if (map == null)
                return NotFound(new ApiResponse<ImageUploadResponse>(false, null, "Map not found."));

            // Upload file
            var fileId = await _fileStorageService.UploadAsync(
                image.OpenReadStream(),
                image.FileName,
                image.ContentType,
                "maps"
            );

            // Update map with file metadata
            var updatedMap = map with
            {
                ImageFileId = fileId,
                ImageContentType = image.ContentType,
                ImageFileSize = image.Length,
                ImageUrl = _fileStorageService.GetPublicUrl(fileId, "maps")
            };

            // Update database
            var tokenRequests = updatedMap.Tokens
                .Select(t => new MapTokenInstanceRequest(t.TokenId, t.X, t.Y))
                .ToList();

            var result = await _mapService.UpdateAsync(id, new UpdateMapRequest(
                updatedMap.Name,
                updatedMap.ImageUrl,
                updatedMap.Rows,
                updatedMap.Cols,
                tokenRequests,
                updatedMap.GridColor,
                updatedMap.GridOpacity
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
