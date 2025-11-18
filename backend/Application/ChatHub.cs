using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Application
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task JoinGroup(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        }

        public async Task SendMessageToGroup(string groupId, string message)
        {
            // For testing purposes - in production, you'd validate group membership here
            var username = Context.User?.Identity?.Name ?? "Anonymous";
            await Clients
                .Group(groupId)
                .SendAsync(
                    "ReceiveMessage",
                    new
                    {
                        senderUsername = username,
                        content = message,
                        timestamp = DateTime.UtcNow,
                    }
                );
        }

        public async Task LeaveGroup(string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
        }
    }
}
