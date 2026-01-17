using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DnDMapBuilder.Application.Interfaces;
using DnDMapBuilder.Contracts.DTOs;
using DnDMapBuilder.Contracts.Requests;
using DnDMapBuilder.Contracts.Responses;

namespace DnDMapBuilder.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MissionsController : ControllerBase
{
    private readonly IMissionService _missionService;

    public MissionsController(IMissionService missionService)
    {
        _missionService = missionService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MissionDto>>> GetMission(string id)
    {
        var mission = await _missionService.GetByIdAsync(id, GetUserId());
        
        if (mission == null)
        {
            return NotFound(new ApiResponse<MissionDto>(false, null, "Mission not found."));
        }

        return Ok(new ApiResponse<MissionDto>(true, mission));
    }

    [HttpGet("campaign/{campaignId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MissionDto>>>> GetMissionsByCampaign(string campaignId)
    {
        var missions = await _missionService.GetByCampaignIdAsync(campaignId, GetUserId());
        return Ok(new ApiResponse<IEnumerable<MissionDto>>(true, missions));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MissionDto>>> CreateMission([FromBody] CreateMissionRequest request)
    {
        try
        {
            var mission = await _missionService.CreateAsync(request, GetUserId());
            return CreatedAtAction(nameof(GetMission), new { id = mission.Id }, new ApiResponse<MissionDto>(true, mission, "Mission created."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<MissionDto>>> UpdateMission(string id, [FromBody] UpdateMissionRequest request)
    {
        var mission = await _missionService.UpdateAsync(id, request, GetUserId());
        
        if (mission == null)
        {
            return NotFound(new ApiResponse<MissionDto>(false, null, "Mission not found."));
        }

        return Ok(new ApiResponse<MissionDto>(true, mission, "Mission updated."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMission(string id)
    {
        var result = await _missionService.DeleteAsync(id, GetUserId());
        
        if (!result)
        {
            return NotFound(new ApiResponse<bool>(false, false, "Mission not found."));
        }

        return Ok(new ApiResponse<bool>(true, true, "Mission deleted."));
    }
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MapsController : ControllerBase
{
    private readonly IGameMapService _mapService;
    private readonly IFileStorageService _fileStorageService;

    public MapsController(IGameMapService mapService, IFileStorageService fileStorageService)
    {
        _mapService = mapService;
        _fileStorageService = fileStorageService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();

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

    [HttpGet("mission/{missionId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<GameMapDto>>>> GetMapsByMission(string missionId)
    {
        var maps = await _mapService.GetByMissionIdAsync(missionId, GetUserId());
        return Ok(new ApiResponse<IEnumerable<GameMapDto>>(true, maps));
    }

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

[Authorize]
[ApiController]
[Route("api/[controller]")]
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

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<TokenDefinitionDto>>>> GetUserTokens()
    {
        var tokens = await _tokenService.GetUserTokensAsync(GetUserId());
        return Ok(new ApiResponse<IEnumerable<TokenDefinitionDto>>(true, tokens));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TokenDefinitionDto>>> GetToken(string id)
    {
        var token = await _tokenService.GetByIdAsync(id, GetUserId());
        
        if (token == null)
        {
            return NotFound(new ApiResponse<TokenDefinitionDto>(false, null, "Token not found."));
        }

        return Ok(new ApiResponse<TokenDefinitionDto>(true, token));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TokenDefinitionDto>>> CreateToken([FromBody] CreateTokenDefinitionRequest request)
    {
        var token = await _tokenService.CreateAsync(request, GetUserId());
        return CreatedAtAction(nameof(GetToken), new { id = token.Id }, new ApiResponse<TokenDefinitionDto>(true, token, "Token created."));
    }

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
