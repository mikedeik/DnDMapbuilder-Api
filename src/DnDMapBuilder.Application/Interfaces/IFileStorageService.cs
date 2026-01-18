namespace DnDMapBuilder.Application.Interfaces;

/// <summary>
/// Service interface for file storage operations.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage and returns the file ID for database reference.
    /// </summary>
    /// <param name="file">The file stream to upload</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <param name="storageCategory">Category for organization (e.g., "maps", "tokens")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated file ID</returns>
    Task<string> UploadAsync(Stream file, string fileName, string contentType, string storageCategory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the public URL for a stored file.
    /// </summary>
    /// <param name="fileId">The file ID returned from upload</param>
    /// <param name="storageCategory">The category where the file is stored</param>
    /// <returns>Public URL to the file</returns>
    string GetPublicUrl(string fileId, string storageCategory);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="fileId">The file ID to delete</param>
    /// <param name="storageCategory">The category where the file is stored</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteAsync(string fileId, string storageCategory, CancellationToken cancellationToken = default);
}
