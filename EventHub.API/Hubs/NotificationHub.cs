using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EventHub.API.Hubs;

/// <summary>
/// Audit Module 10: real-time channel for the Notifications inbox
/// (push updates without polling). Clients connect to /hubs/notifications with
/// a JWT bearer token (see Program.cs JwtBearerEvents.OnMessageReceived).
///
/// SignalR's default IUserIdProvider maps ClaimTypes.NameIdentifier to
/// Context.UserIdentifier, and our JWT already carries that claim (see
/// JwtHelper / HomeService's use of ClaimTypes.NameIdentifier), so server-side
/// code can push straight to Clients.User(userId) — no manual group management
/// needed. This hub itself has no client-invokable methods; it's a pure
/// server -> client push channel.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
}
