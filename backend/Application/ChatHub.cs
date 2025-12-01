using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Application
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IGroupService _groupService;

        public ChatHub(IGroupService groupService)
        {
            _groupService = groupService;
        }

        public async Task JoinGroup(string groupId)
        {
            var userId = GetUserId();
            var parsedGroupId = ParseGroupId(groupId);

            var isMember = await _groupService.IsUserMemberOfGroupAsync(parsedGroupId, userId);
            if (!isMember)
            {
                throw new HubException("You are not a member of this group.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        }

        public async Task SendMessageToGroup(string groupId, string message)
        {
            var userId = GetUserId();
            var parsedGroupId = ParseGroupId(groupId);

            var isMember = await _groupService.IsUserMemberOfGroupAsync(parsedGroupId, userId);
            if (!isMember)
            {
                throw new HubException("You are not authorized to send messages to this group.");
            }

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

        private Guid GetUserId()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(userId, out var parsedUserId))
            {
                return parsedUserId;
            }

            throw new HubException("User identity is not available.");
        }

        private static Guid ParseGroupId(string groupId)
        {
            if (Guid.TryParse(groupId, out var parsedGroupId))
            {
                return parsedGroupId;
            }

            throw new HubException("Invalid group identifier.");
        }
    }
}
