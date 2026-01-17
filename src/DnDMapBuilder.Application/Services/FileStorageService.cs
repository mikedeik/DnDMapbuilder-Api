using Microsoft.Extensions.Logging;
using DnDMapBuilder.Application.Interfaces;

namespace DnDMapBuilder.Application.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseStoragePath;
    private readonly string _basePublicUrl;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(string baseStoragePath, string basePublicUrl, ILogger<LocalFileStorageService> logger)
    {
        _baseStoragePath = baseStoragePath ?? throw new ArgumentNullException(nameof(baseStoragePath));
        _basePublicUrl = basePublicUrl ?? throw new ArgumentNullException(nameof(basePublicUrl));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        EnsureStorageDirectoriesExist();
    }

    public async Task<string> UploadAsync(Stream file, string fileName, string contentType, string storageCategory)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File stream is empty", nameof(file));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required", nameof(fileName));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type is required", nameof(contentType));

        if (string.IsNullOrWhiteSpace(storageCategory))
            throw new ArgumentException("Storage category is required", nameof(storageCategory));

        // Validate MIME type
        var allowedMimeTypes = new[] { "image/png", "image/jpeg", "image/webp" };
        if (!allowedMimeTypes.Contains(contentType.ToLower()))
            throw new InvalidOperationException($"MIME type '{contentType}' is not allowed");

        // Generate file ID with extension
        var fileExtension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(fileExtension))
            fileExtension = GetExtensionFromMimeType(contentType);

        var fileId = $"{Guid.NewGuid()}{fileExtension}";
        var categoryPath = Path.Combine(_baseStoragePath, storageCategory);
        var fullFilePath = Path.Combine(categoryPath, fileId);

        // Ensure directory exists
        if (!Directory.Exists(categoryPath))
            Directory.CreateDirectory(categoryPath);

        try
        {
            // Save file to disk using streaming (no full buffering)
            using (var fileStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await file.CopyToAsync(fileStream);
            }

            _logger.LogInformation($"File uploaded successfully: {fileId} to category {storageCategory}");
            return fileId;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error uploading file: {ex.Message}");
            if (File.Exists(fullFilePath))
                File.Delete(fullFilePath);
            throw;
        }
    }

    public string GetPublicUrl(string fileId, string storageCategory)
    {
        if (string.IsNullOrWhiteSpace(fileId))
            throw new ArgumentException("File ID is required", nameof(fileId));

        if (string.IsNullOrWhiteSpace(storageCategory))
            throw new ArgumentException("Storage category is required", nameof(storageCategory));

        // Prevent path traversal attacks
        if (fileId.Contains("..") || fileId.Contains("/") || fileId.Contains("\\"))
            throw new InvalidOperationException("Invalid file ID");

        return $"{_basePublicUrl}/{storageCategory}/{fileId}";
    }

    public async Task<bool> DeleteAsync(string fileId, string storageCategory)
    {
        if (string.IsNullOrWhiteSpace(fileId))
            throw new ArgumentException("File ID is required", nameof(fileId));

        if (string.IsNullOrWhiteSpace(storageCategory))
            throw new ArgumentException("Storage category is required", nameof(storageCategory));

        // Prevent path traversal attacks
        if (fileId.Contains("..") || fileId.Contains("/") || fileId.Contains("\\"))
            throw new InvalidOperationException("Invalid file ID");

        var categoryPath = Path.Combine(_baseStoragePath, storageCategory);
        var fullFilePath = Path.Combine(categoryPath, fileId);

        try
        {
            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);
                _logger.LogInformation($"File deleted successfully: {fileId} from category {storageCategory}");
                return true;
            }

            _logger.LogWarning($"File not found for deletion: {fileId} in category {storageCategory}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting file: {ex.Message}");
            throw;
        }
    }

    private void EnsureStorageDirectoriesExist()
    {
        var categories = new[] { "maps", "tokens" };
        foreach (var category in categories)
        {
            var categoryPath = Path.Combine(_baseStoragePath, category);
            if (!Directory.Exists(categoryPath))
                Directory.CreateDirectory(categoryPath);
        }
    }

    private string GetExtensionFromMimeType(string mimeType)
    {
        return mimeType.ToLower() switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/webp" => ".webp",
            _ => ".bin"
        };
    }
}
