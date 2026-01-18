namespace DnDMapBuilder.Application.Interfaces;

/// <summary>
/// Service for validating file uploads.
/// </summary>
public interface IFileValidationService
{
    /// <summary>
    /// Validates a file's size and type.
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <param name="fileSize">The file size in bytes</param>
    /// <param name="contentType">The MIME type of the file</param>
    /// <param name="storageCategory">The storage category (e.g., "maps", "tokens")</param>
    /// <returns>Validation result with any error messages</returns>
    FileValidationResult ValidateFile(string fileName, long fileSize, string contentType, string storageCategory);

    /// <summary>
    /// Gets the maximum allowed file size for a storage category.
    /// </summary>
    /// <param name="storageCategory">The storage category</param>
    /// <returns>Maximum file size in bytes</returns>
    long GetMaxFileSizeForCategory(string storageCategory);

    /// <summary>
    /// Gets the allowed MIME types for a storage category.
    /// </summary>
    /// <param name="storageCategory">The storage category</param>
    /// <returns>Array of allowed MIME types</returns>
    string[] GetAllowedMimeTypesForCategory(string storageCategory);
}

/// <summary>
/// Result of file validation.
/// </summary>
public class FileValidationResult
{
    /// <summary>
    /// Initializes a new instance of the FileValidationResult class with success.
    /// </summary>
    public FileValidationResult()
    {
        IsValid = true;
        Errors = new List<string>();
    }

    /// <summary>
    /// Initializes a new instance of the FileValidationResult class with an error.
    /// </summary>
    /// <param name="error">The validation error message</param>
    public FileValidationResult(string error) : this(new[] { error })
    {
    }

    /// <summary>
    /// Initializes a new instance of the FileValidationResult class with errors.
    /// </summary>
    /// <param name="errors">The validation error messages</param>
    public FileValidationResult(string[] errors)
    {
        IsValid = false;
        Errors = new List<string>(errors);
    }

    /// <summary>
    /// Gets a value indicating whether the file passed validation.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the validation error messages.
    /// </summary>
    public List<string> Errors { get; }
}
