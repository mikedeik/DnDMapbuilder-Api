using System.Diagnostics;

namespace DnDMapBuilder.Infrastructure.Telemetry;

/// <summary>
/// Service for recording custom telemetry events and metrics.
/// </summary>
public interface ITelemetryService
{
    /// <summary>
    /// Records an authentication attempt.
    /// </summary>
    /// <param name="userId">The user ID (if successful)</param>
    /// <param name="success">Whether the authentication was successful</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    void RecordAuthenticationAttempt(int? userId, bool success, long durationMs);

    /// <summary>
    /// Records a token operation.
    /// </summary>
    /// <param name="operationType">Type of token operation (issue, refresh, revoke)</param>
    /// <param name="success">Whether the operation was successful</param>
    void RecordTokenOperation(string operationType, bool success);

    /// <summary>
    /// Records a campaign CRUD operation.
    /// </summary>
    /// <param name="operationType">Type of operation (Create, Read, Update, Delete)</param>
    /// <param name="campaignId">The campaign ID</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    void RecordCampaignOperation(string operationType, int campaignId, long durationMs);

    /// <summary>
    /// Records a mission CRUD operation.
    /// </summary>
    /// <param name="operationType">Type of operation (Create, Read, Update, Delete)</param>
    /// <param name="missionId">The mission ID</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    void RecordMissionOperation(string operationType, int missionId, long durationMs);

    /// <summary>
    /// Records a game map CRUD operation.
    /// </summary>
    /// <param name="operationType">Type of operation (Create, Read, Update, Delete)</param>
    /// <param name="mapId">The map ID</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    void RecordMapOperation(string operationType, int mapId, long durationMs);

    /// <summary>
    /// Records a token instance operation.
    /// </summary>
    /// <param name="operationType">Type of operation (Create, Read, Update, Delete)</param>
    /// <param name="tokenId">The token instance ID</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    void RecordTokenInstanceOperation(string operationType, int tokenId, long durationMs);

    /// <summary>
    /// Records a file upload operation.
    /// </summary>
    /// <param name="fileName">Name of the uploaded file</param>
    /// <param name="fileSizeBytes">Size of the file in bytes</param>
    /// <param name="success">Whether the upload was successful</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    void RecordFileUpload(string fileName, long fileSizeBytes, bool success, long durationMs);

    /// <summary>
    /// Creates an activity for manual tracing.
    /// </summary>
    /// <param name="activityName">Name of the activity</param>
    /// <returns>The created activity, or null if tracing is not enabled</returns>
    Activity? StartActivity(string activityName);
}
