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
    public class MessagesController(IMessageService messageService) : ControllerBase
    {
        private readonly IMessageService _messageService = messageService;

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
    }
}
