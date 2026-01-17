namespace DnDMapBuilder.Contracts.Requests;

/// <summary>
/// Response for file upload operations.
/// </summary>
public record ImageUploadResponse(
    string FileId,
    string Url,
    string ContentType,
    long FileSize
);
