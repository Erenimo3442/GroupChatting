using System.Security.Claims;
using Application;
using Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController(
        IMessageService messageService,
        IFileStorageService fileStorageService
    ) : ControllerBase
    {
        private readonly IMessageService _messageService = messageService;
        private readonly IFileStorageService _fileStorageService = fileStorageService;

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageDto sendMessageDto)
        {
            var senderIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (
                string.IsNullOrEmpty(senderIdString)
                || !Guid.TryParse(senderIdString, out var senderId)
            )
            {
                return Unauthorized();
            }

            var message = await _messageService.SendMessageAsync(sendMessageDto, senderId);

            return Ok(message);
        }

        [HttpGet]
        [Route("{groupId}")]
        public async Task<IActionResult> GetMessages(
            Guid groupId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchText = null
        )
        {
            var messages = await _messageService.GetMessagesAsync(
                groupId,
                page,
                pageSize,
                searchText
            );
            return Ok(messages);
        }

        [HttpPut("{messageId}")]
        public async Task<IActionResult> UpdateMessage(Guid messageId, [FromBody] string newContent)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var updatedMessage = await _messageService.UpdateMessageAsync(
                messageId,
                userId,
                newContent
            );
            if (updatedMessage == null)
            {
                return NotFound();
            }

            return Ok(updatedMessage);
        }

        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var isDeleted = await _messageService.DeleteMessageAsync(messageId, userId);
            if (!isDeleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] Guid groupId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var senderIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(senderIdString, out var senderId))
            {
                return Unauthorized();
            }

            // Save the file using the storage service
            var fileUrl = await _fileStorageService.SaveFileAsync(
                file.OpenReadStream(),
                file.FileName
            );

            // Create a message DTO to send to the service
            var messageDto = new SendMessageDto
            {
                GroupId = groupId,
                Content = $"[File]: {file.FileName}",
                FileUrl = fileUrl,
                MimeType = file.ContentType,
            };

            // Use your existing message service to save and broadcast
            var message = await _messageService.SendMessageAsync(messageDto, senderId);

            return Ok(message);
        }
    }
}
