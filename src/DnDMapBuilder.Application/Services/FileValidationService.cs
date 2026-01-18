using DnDMapBuilder.Application.Interfaces;

namespace DnDMapBuilder.Application.Services;

/// <summary>
/// Service for validating file uploads with size and type restrictions.
/// </summary>
public class FileValidationService : IFileValidationService
{
    private static readonly Dictionary<string, (long MaxSize, string[] AllowedMimeTypes)> CategoryConfig =
        new()
        {
            {
                "maps",
                (
                    MaxSize: 5 * 1024 * 1024, // 5MB
                    AllowedMimeTypes: new[] { "image/png", "image/jpeg", "image/webp" }
                )
            },
            {
                "tokens",
                (
                    MaxSize: 2 * 1024 * 1024, // 2MB
                    AllowedMimeTypes: new[] { "image/png", "image/jpeg", "image/webp" }
                )
            },
            {
                "default",
                (
                    MaxSize: 10 * 1024 * 1024, // 10MB
                    AllowedMimeTypes: new[] { "application/octet-stream" }
                )
            }
        };

    /// <summary>
    /// Validates a file's size and type.
    /// </summary>
    public FileValidationResult ValidateFile(string fileName, long fileSize, string contentType, string storageCategory)
    {
        var errors = new List<string>();

        // Validate file name
        if (string.IsNullOrWhiteSpace(fileName))
        {
            errors.Add("File name cannot be empty.");
            return new FileValidationResult(errors.ToArray());
        }

        // Validate file size
        var maxSize = GetMaxFileSizeForCategory(storageCategory);
        if (fileSize == 0)
        {
            errors.Add("File cannot be empty.");
        }
        else if (fileSize > maxSize)
        {
            errors.Add($"File size exceeds the maximum limit of {maxSize / (1024 * 1024)}MB for {storageCategory}.");
        }

        // Validate MIME type
        var allowedMimeTypes = GetAllowedMimeTypesForCategory(storageCategory);
        var normalizedContentType = (contentType ?? "").ToLowerInvariant();
        if (!allowedMimeTypes.Contains(normalizedContentType))
        {
            var supportedTypes = string.Join(", ", allowedMimeTypes);
            errors.Add($"File type '{normalizedContentType}' is not allowed. Supported types: {supportedTypes}");
        }

        return errors.Count > 0 ? new FileValidationResult(errors.ToArray()) : new FileValidationResult();
    }

    /// <summary>
    /// Gets the maximum allowed file size for a storage category.
    /// </summary>
    public long GetMaxFileSizeForCategory(string storageCategory)
    {
        var category = (storageCategory ?? "default").ToLowerInvariant();
        return CategoryConfig.ContainsKey(category) ? CategoryConfig[category].MaxSize : CategoryConfig["default"].MaxSize;
    }

    /// <summary>
    /// Gets the allowed MIME types for a storage category.
    /// </summary>
    public string[] GetAllowedMimeTypesForCategory(string storageCategory)
    {
        var category = (storageCategory ?? "default").ToLowerInvariant();
        return CategoryConfig.ContainsKey(category) ? CategoryConfig[category].AllowedMimeTypes : CategoryConfig["default"].AllowedMimeTypes;
    }
}
