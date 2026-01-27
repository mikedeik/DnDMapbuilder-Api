using DnDMapBuilder.Contracts.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DnDMapBuilder.Api.Hubs;

/// <summary>
/// Adapter that implements IGameMapHub interface using IHubContext for broadcasting.
/// Decouples the Application layer from SignalR implementation details.
/// </summary>
public class GameMapHubAdapter : IGameMapHub
{
    private readonly IHubContext<GameMapHub> _hubContext;

    public GameMapHubAdapter(IHubContext<GameMapHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendAsync(string groupName, string methodName, object? arg1, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(groupName).SendAsync(methodName, arg1, cancellationToken);
    }
}
