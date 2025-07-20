using Core;

namespace Application
{
    public interface IMongoMessageDbService
    {
        Task CreateAsync(Message newMessage);
        Task<List<Message>> GetMessagesAsync(
            Guid groupId,
            int page,
            int pageSize,
            string? searchText
        );
        Task<Message?> GetByIdAsync(Guid id);
        Task UpdateAsync(Guid id, Message updatedMessage);
    }
}
