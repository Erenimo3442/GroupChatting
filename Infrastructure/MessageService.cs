using Application;
using Application.Dtos;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure
{
    public class MessageService(
        MongoMessageDbService mongoMessageDbService,
        IHubContext<ChatHub> hubContext
    ) : IMessageService
    {
        private readonly MongoMessageDbService _mongoMessageDbService = mongoMessageDbService;
        private readonly IHubContext<ChatHub> _hubContext = hubContext;

        public async Task<Message> SendMessageAsync(SendMessageDto sendMessageDto, Guid senderId)
        {
            var message = new Message
            {
                Id = Guid.NewGuid(),
                Content = sendMessageDto.Content,
                GroupId = sendMessageDto.GroupId,
                UserId = senderId,
                Timestamp = DateTime.UtcNow,
            };

            // 2. Save the message to the database
            await _mongoMessageDbService.CreateAsync(message);

            // 3. Broadcast the new message to clients in the group
            // The client-side will listen for the "ReceiveMessage" event.
            await _hubContext
                .Clients.Group(sendMessageDto.GroupId.ToString())
                .SendAsync("ReceiveMessage", message);

            return message;
        }

        public async Task<List<Message>> GetMessagesAsync(
            Guid groupId,
            int page,
            int pageSize,
            string? searchText
        )
        {
            return await _mongoMessageDbService.GetMessagesAsync(
                groupId,
                page,
                pageSize,
                searchText
            );
        }

        public async Task<Message?> UpdateMessageAsync(
            Guid messageId,
            Guid userId,
            string newContent
        )
        {
            var message = await _mongoMessageDbService.GetByIdAsync(messageId);

            if (message == null || message.UserId != userId)
            {
                return null;
            }

            message.Content = newContent;
            message.LastEditedAt = DateTime.UtcNow;

            await _mongoMessageDbService.UpdateAsync(messageId, message);

            // Notify clients about the updated message
            await _hubContext
                .Clients.Group(message.GroupId.ToString())
                .SendAsync("MessageUpdated", message);

            return message;
        }

        public async Task<bool> DeleteMessageAsync(Guid messageId, Guid userId)
        {
            var message = await _mongoMessageDbService.GetByIdAsync(messageId);

            if (message == null || message.UserId != userId)
            {
                return false;
            }

            message.IsDeleted = true;

            await _mongoMessageDbService.UpdateAsync(messageId, message);

            // Notify clients about the deleted message
            await _hubContext
                .Clients.Group(message.GroupId.ToString())
                .SendAsync("MessageDeleted", messageId);

            return true;
        }
    }
}
