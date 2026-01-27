namespace DnDMapBuilder.Data.Entities;

/// <summary>
/// Publication status for game maps used in live view feature.
/// </summary>
public enum PublicationStatus
{
    /// <summary>Draft maps are editable but not broadcast to live views.</summary>
    Draft = 0,

    /// <summary>Live maps are broadcast in real-time to connected clients.</summary>
    Live = 1
}
