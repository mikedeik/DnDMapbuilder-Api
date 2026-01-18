using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DnDMapBuilder.Infrastructure.Telemetry;

/// <summary>
/// Service for recording custom telemetry events and metrics using OpenTelemetry.
/// </summary>
public class TelemetryService : ITelemetryService
{
    private readonly ActivitySource _activitySource;
    private readonly Meter _authenticationMeter;
    private readonly Meter _campaignMeter;
    private readonly Meter _missionMeter;
    private readonly Meter _mapMeter;
    private readonly Meter _tokenMeter;
    private readonly Meter _fileMeter;

    // Counters for various operations
    private readonly Counter<int> _authenticationAttempts;
    private readonly Counter<int> _authenticationSuccesses;
    private readonly Counter<int> _authenticationFailures;

    private readonly Counter<int> _tokenIssued;
    private readonly Counter<int> _tokenRefreshed;
    private readonly Counter<int> _tokenRevoked;

    private readonly Counter<int> _campaignCreated;
    private readonly Counter<int> _campaignUpdated;
    private readonly Counter<int> _campaignDeleted;
    private readonly Histogram<long> _campaignOperationDuration;

    private readonly Counter<int> _missionCreated;
    private readonly Counter<int> _missionUpdated;
    private readonly Counter<int> _missionDeleted;
    private readonly Histogram<long> _missionOperationDuration;

    private readonly Counter<int> _mapCreated;
    private readonly Counter<int> _mapUpdated;
    private readonly Counter<int> _mapDeleted;
    private readonly Histogram<long> _mapOperationDuration;

    private readonly Counter<int> _tokenInstanceCreated;
    private readonly Counter<int> _tokenInstanceUpdated;
    private readonly Counter<int> _tokenInstanceDeleted;
    private readonly Histogram<long> _tokenInstanceOperationDuration;

    private readonly Counter<int> _fileUploadsSuccessful;
    private readonly Counter<int> _fileUploadsFailed;
    private readonly Histogram<long> _fileUploadDuration;
    private readonly Histogram<long> _fileUploadSize;

    /// <summary>
    /// Initializes a new instance of the TelemetryService class.
    /// </summary>
    public TelemetryService()
    {
        // Create activity source for distributed tracing
        _activitySource = new ActivitySource("DnDMapBuilder.Services");

        // Create meters for different domains
        _authenticationMeter = new Meter("DnDMapBuilder.Authentication", "1.0.0");
        _campaignMeter = new Meter("DnDMapBuilder.CampaignOperations", "1.0.0");
        _missionMeter = new Meter("DnDMapBuilder.MissionOperations", "1.0.0");
        _mapMeter = new Meter("DnDMapBuilder.MapOperations", "1.0.0");
        _tokenMeter = new Meter("DnDMapBuilder.TokenOperations", "1.0.0");
        _fileMeter = new Meter("DnDMapBuilder.FileOperations", "1.0.0");

        // Initialize authentication counters
        _authenticationAttempts = _authenticationMeter.CreateCounter<int>(
            "authentication.attempts.total",
            description: "Total authentication attempts");
        _authenticationSuccesses = _authenticationMeter.CreateCounter<int>(
            "authentication.successes.total",
            description: "Total successful authentications");
        _authenticationFailures = _authenticationMeter.CreateCounter<int>(
            "authentication.failures.total",
            description: "Total failed authentications");

        // Initialize token counters
        _tokenIssued = _tokenMeter.CreateCounter<int>(
            "token.issued.total",
            description: "Total tokens issued");
        _tokenRefreshed = _tokenMeter.CreateCounter<int>(
            "token.refreshed.total",
            description: "Total tokens refreshed");
        _tokenRevoked = _tokenMeter.CreateCounter<int>(
            "token.revoked.total",
            description: "Total tokens revoked");

        // Initialize campaign counters and histograms
        _campaignCreated = _campaignMeter.CreateCounter<int>(
            "campaign.created.total",
            description: "Total campaigns created");
        _campaignUpdated = _campaignMeter.CreateCounter<int>(
            "campaign.updated.total",
            description: "Total campaigns updated");
        _campaignDeleted = _campaignMeter.CreateCounter<int>(
            "campaign.deleted.total",
            description: "Total campaigns deleted");
        _campaignOperationDuration = _campaignMeter.CreateHistogram<long>(
            "campaign.operation.duration_ms",
            unit: "ms",
            description: "Duration of campaign operations in milliseconds");

        // Initialize mission counters and histograms
        _missionCreated = _missionMeter.CreateCounter<int>(
            "mission.created.total",
            description: "Total missions created");
        _missionUpdated = _missionMeter.CreateCounter<int>(
            "mission.updated.total",
            description: "Total missions updated");
        _missionDeleted = _missionMeter.CreateCounter<int>(
            "mission.deleted.total",
            description: "Total missions deleted");
        _missionOperationDuration = _missionMeter.CreateHistogram<long>(
            "mission.operation.duration_ms",
            unit: "ms",
            description: "Duration of mission operations in milliseconds");

        // Initialize map counters and histograms
        _mapCreated = _mapMeter.CreateCounter<int>(
            "map.created.total",
            description: "Total maps created");
        _mapUpdated = _mapMeter.CreateCounter<int>(
            "map.updated.total",
            description: "Total maps updated");
        _mapDeleted = _mapMeter.CreateCounter<int>(
            "map.deleted.total",
            description: "Total maps deleted");
        _mapOperationDuration = _mapMeter.CreateHistogram<long>(
            "map.operation.duration_ms",
            unit: "ms",
            description: "Duration of map operations in milliseconds");

        // Initialize token instance counters and histograms
        _tokenInstanceCreated = _tokenMeter.CreateCounter<int>(
            "token_instance.created.total",
            description: "Total token instances created");
        _tokenInstanceUpdated = _tokenMeter.CreateCounter<int>(
            "token_instance.updated.total",
            description: "Total token instances updated");
        _tokenInstanceDeleted = _tokenMeter.CreateCounter<int>(
            "token_instance.deleted.total",
            description: "Total token instances deleted");
        _tokenInstanceOperationDuration = _tokenMeter.CreateHistogram<long>(
            "token_instance.operation.duration_ms",
            unit: "ms",
            description: "Duration of token instance operations in milliseconds");

        // Initialize file operation counters and histograms
        _fileUploadsSuccessful = _fileMeter.CreateCounter<int>(
            "file.uploads.successful.total",
            description: "Total successful file uploads");
        _fileUploadsFailed = _fileMeter.CreateCounter<int>(
            "file.uploads.failed.total",
            description: "Total failed file uploads");
        _fileUploadDuration = _fileMeter.CreateHistogram<long>(
            "file.upload.duration_ms",
            unit: "ms",
            description: "Duration of file uploads in milliseconds");
        _fileUploadSize = _fileMeter.CreateHistogram<long>(
            "file.upload.size_bytes",
            unit: "bytes",
            description: "Size of uploaded files in bytes");
    }

    public void RecordAuthenticationAttempt(int? userId, bool success, long durationMs)
    {
        _authenticationAttempts.Add(1);

        if (success && userId.HasValue)
        {
            _authenticationSuccesses.Add(1);
        }
        else
        {
            _authenticationFailures.Add(1);
        }
    }

    public void RecordTokenOperation(string operationType, bool success)
    {
        var countLabel = operationType.ToLowerInvariant() switch
        {
            "issue" => _tokenIssued,
            "refresh" => _tokenRefreshed,
            "revoke" => _tokenRevoked,
            _ => _tokenIssued
        };

        countLabel.Add(1);
    }

    public void RecordCampaignOperation(string operationType, int campaignId, long durationMs)
    {
        switch (operationType.ToLowerInvariant())
        {
            case "create":
                _campaignCreated.Add(1);
                break;
            case "update":
                _campaignUpdated.Add(1);
                break;
            case "delete":
                _campaignDeleted.Add(1);
                break;
        }

        _campaignOperationDuration.Record(durationMs, new KeyValuePair<string, object?>("operation", operationType));
    }

    public void RecordMissionOperation(string operationType, int missionId, long durationMs)
    {
        switch (operationType.ToLowerInvariant())
        {
            case "create":
                _missionCreated.Add(1);
                break;
            case "update":
                _missionUpdated.Add(1);
                break;
            case "delete":
                _missionDeleted.Add(1);
                break;
        }

        _missionOperationDuration.Record(durationMs, new KeyValuePair<string, object?>("operation", operationType));
    }

    public void RecordMapOperation(string operationType, int mapId, long durationMs)
    {
        switch (operationType.ToLowerInvariant())
        {
            case "create":
                _mapCreated.Add(1);
                break;
            case "update":
                _mapUpdated.Add(1);
                break;
            case "delete":
                _mapDeleted.Add(1);
                break;
        }

        _mapOperationDuration.Record(durationMs, new KeyValuePair<string, object?>("operation", operationType));
    }

    public void RecordTokenInstanceOperation(string operationType, int tokenId, long durationMs)
    {
        switch (operationType.ToLowerInvariant())
        {
            case "create":
                _tokenInstanceCreated.Add(1);
                break;
            case "update":
                _tokenInstanceUpdated.Add(1);
                break;
            case "delete":
                _tokenInstanceDeleted.Add(1);
                break;
        }

        _tokenInstanceOperationDuration.Record(durationMs, new KeyValuePair<string, object?>("operation", operationType));
    }

    public void RecordFileUpload(string fileName, long fileSizeBytes, bool success, long durationMs)
    {
        if (success)
        {
            _fileUploadsSuccessful.Add(1);
        }
        else
        {
            _fileUploadsFailed.Add(1);
        }

        _fileUploadDuration.Record(durationMs, new KeyValuePair<string, object?>("file_name", fileName));
        _fileUploadSize.Record(fileSizeBytes, new KeyValuePair<string, object?>("file_name", fileName));
    }

    public Activity? StartActivity(string activityName)
    {
        return _activitySource.StartActivity(activityName);
    }
}
