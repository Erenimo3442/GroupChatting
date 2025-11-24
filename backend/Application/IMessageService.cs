using Application.Dtos;
using Core;

namespace Application
{
    public interface IMessageService
    {
        Task<MessageResponseDto> SendMessageAsync(
            Guid groupId,
            SendMessageDto sendMessageDto,
            Guid senderId
        );
        Task<List<MessageResponseDto>> GetMessagesAsync(
            Guid groupId,
            GetMessagesDto getMessagesDto
        );
        Task<MessageResponseDto?> UpdateMessageAsync(
            Guid messageId,
            Guid userId,
            UpdateMessageDto dto
        );
        Task<bool> DeleteMessageAsync(Guid messageId, Guid userId);

        Task<FileDownloadResponseDto> DownloadFileAsync(Guid messageId, Guid userId);
    }
}
