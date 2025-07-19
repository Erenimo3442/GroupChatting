using Application.Dtos;
using Core;

namespace Application
{
    public interface IMessageService
    {
        Task<Message> SendMessageAsync(SendMessageDto sendMessageDto, Guid senderId);

        Task<List<Message>> GetMessagesAsync(
            Guid groupId,
            int page,
            int pageSize,
            string? searchText
        );

        Task<Message?> UpdateMessageAsync(Guid messageId, Guid userId, string newContent);

        Task<bool> DeleteMessageAsync(Guid messageId, Guid userId);
    }
}
