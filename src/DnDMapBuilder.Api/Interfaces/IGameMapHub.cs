namespace DnDMapBuilder.Api.Interfaces;

/// <summary>
/// Interface for broadcasting messages to SignalR clients.
/// Abstracts away the Hub implementation details from the Application layer.
/// </summary>
public interface IGameMapHub
{
    /// <summary>
    /// Sends a message to all clients in a SignalR group.
    /// </summary>
    /// <param name="groupName">The name of the group to send to</param>
    /// <param name="methodName">The method name to invoke on clients</param>
    /// <param name="arg1">First argument to pass to the client method</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendAsync(string groupName, string methodName, object? arg1, CancellationToken cancellationToken = default);
}
