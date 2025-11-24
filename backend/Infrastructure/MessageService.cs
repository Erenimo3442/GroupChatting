using Application;
using Application.Dtos;
using Application.Exceptions;
using Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class MessageService(
        IMongoMessageDbService mongoMessageDbService,
        IHubContext<ChatHub> hubContext,
        AppDbContext context,
        IFileStorageService fileStorageService,
        IGroupService groupService
    ) : IMessageService
    {
        private readonly AppDbContext _context = context;
        private readonly IMongoMessageDbService _mongoMessageDbService = mongoMessageDbService;
        private readonly IHubContext<ChatHub> _hubContext = hubContext;
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly IGroupService _groupService = groupService;

        public async Task<MessageResponseDto> SendMessageAsync(
            Guid groupId,
            SendMessageDto sendMessageDto,
            Guid senderId
        )
        {
            // Validate the sender is a member of the group
            var isMember = await _context.GroupMembers.AnyAsync(gm =>
                gm.GroupId == groupId
                && gm.UserId == senderId
                && gm.MembershipStatus.Equals("Active")
            );

            if (!isMember)
            {
                throw new ForbiddenAccessException("Sender is not a member of the group.");
            }

            var message = new Message
            {
                Id = Guid.NewGuid(),
                Content = sendMessageDto.Content,
                GroupId = groupId,
                UserId = senderId,
                Timestamp = DateTime.UtcNow,
                FileUrl = sendMessageDto.FileUrl,
                MimeType = sendMessageDto.MimeType,
            };

            // Save the message to the database
            await _mongoMessageDbService.CreateAsync(message);

            // Get sender username for response
            var senderUser = await _context.Users.FindAsync(senderId);
            var senderUsername = senderUser?.Username ?? "Unknown";

            // Create response DTO
            var responseDto = MapToResponseDto(message, senderUsername);

            // Broadcast the new message to clients in the group
            // The client-side will listen for the "ReceiveMessage" event.
            await _hubContext
                .Clients.Group(groupId.ToString())
                .SendAsync("ReceiveMessage", responseDto);

            return responseDto;
        }

        public async Task<List<MessageResponseDto>> GetMessagesAsync(
            Guid groupId,
            GetMessagesDto getMessagesDto
        )
        {
            var messages = await _mongoMessageDbService.GetMessagesAsync(
                groupId,
                getMessagesDto.Page,
                getMessagesDto.PageSize,
                getMessagesDto.SearchText
            );

            // Get all unique user IDs from messages
            var userIds = messages.Select(m => m.UserId).Distinct().ToList();

            // Get usernames in a single query
            var users = await _context
                .Users.Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            // Map to response DTOs
            var responseDtos = messages
                .Select(message =>
                {
                    var senderUsername = users.GetValueOrDefault(message.UserId, "Unknown");
                    return MapToResponseDto(message, senderUsername);
                })
                .ToList();

            return responseDtos;
        }

        public async Task<MessageResponseDto?> UpdateMessageAsync(
            Guid messageId,
            Guid userId,
            UpdateMessageDto dto
        )
        {
            var message = await _mongoMessageDbService.GetByIdAsync(messageId);

            if (message == null || message.UserId != userId)
            {
                return null;
            }

            message.Content = dto.NewContent;
            message.LastEditedAt = DateTime.UtcNow;

            await _mongoMessageDbService.UpdateAsync(messageId, message);

            // Get sender username for response
            var senderUser = await _context.Users.FindAsync(userId);
            var senderUsername = senderUser?.Username ?? "Unknown";

            // Create response DTO
            var responseDto = MapToResponseDto(message, senderUsername);

            // Notify clients about the updated message
            await _hubContext
                .Clients.Group(message.GroupId.ToString())
                .SendAsync("MessageUpdated", responseDto);

            return responseDto;
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

        public async Task<FileDownloadResponseDto> DownloadFileAsync(Guid messageId, Guid userId)
        {
            var message = await _mongoMessageDbService.GetByIdAsync(messageId);

            if (message == null)
            {
                throw new NotFoundException("Message not found.");
            }

            if (string.IsNullOrEmpty(message.FileUrl))
            {
                throw new NotFoundException("No file attached to this message.");
            }

            // Verify user is a member of the group
            var isMember = await _groupService.IsUserMemberOfGroupAsync(message.GroupId, userId);
            if (!isMember)
            {
                throw new ForbiddenAccessException(
                    "You must be a member of the group to download this file."
                );
            }

            // Retrieve the file from storage with message timestamp for proper naming
            return await _fileStorageService.GetFileAsync(message.FileUrl, message.Timestamp);
        }

        private static MessageResponseDto MapToResponseDto(Message message, string senderUsername)
        {
            return new MessageResponseDto
            {
                Id = message.Id,
                Content = message.Content,
                Timestamp = message.Timestamp,
                UserId = message.UserId,
                GroupId = message.GroupId,
                IsDeleted = message.IsDeleted,
                LastEditedAt = message.LastEditedAt,
                FileUrl = message.FileUrl,
                MimeType = message.MimeType,
                SenderUsername = senderUsername,
            };
        }
    }
}
