using System.Security.Claims;
using Application;
using Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/groups/{groupId}/messages")]
    public class MessagesController(
        IMessageService messageService,
        IFileStorageService fileStorageService
    ) : BaseApiController
    {
        private readonly IMessageService _messageService = messageService;
        private readonly IFileStorageService _fileStorageService = fileStorageService;

        [HttpPost]
        public async Task<ActionResult<MessageResponseDto>> SendMessage(
            Guid groupId,
            [FromBody] SendMessageDto sendMessageDto
        )
        {
            var senderId = CurrentUserId;

            var responseDto = await _messageService.SendMessageAsync(
                groupId,
                sendMessageDto,
                senderId
            );
            return Ok(responseDto);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageResponseDto>>> GetMessages(
            Guid groupId,
            [FromQuery] GetMessagesDto getMessagesDto
        )
        {
            var responseDtos = await _messageService.GetMessagesAsync(groupId, getMessagesDto);
            return Ok(responseDtos);
        }

        [HttpPut("{messageId}")]
        public async Task<ActionResult<MessageResponseDto>> UpdateMessage(
            Guid messageId,
            [FromBody] UpdateMessageDto updateDto
        )
        {
            var userId = CurrentUserId;

            var responseDto = await _messageService.UpdateMessageAsync(
                messageId,
                userId,
                updateDto
            );
            if (responseDto == null)
            {
                return NotFound();
            }

            return Ok(responseDto);
        }

        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            var userId = CurrentUserId;

            var isDeleted = await _messageService.DeleteMessageAsync(messageId, userId);
            return isDeleted ? NoContent() : NotFound();
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<MessageResponseDto>> UploadFile(
            [FromRoute] Guid groupId,
            IFormFile file,
            string? content = null
        )
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var senderId = CurrentUserId;

            // Save the file using the storage service
            var fileUrl = await _fileStorageService.SaveFileAsync(
                file.OpenReadStream(),
                file.FileName
            );

            var providedContent = string.IsNullOrWhiteSpace(content)
                ? $"[File]: {file.FileName}"
                : content.Trim();

            // Create a message DTO to send to the service
            var messageDto = new SendMessageDto
            {
                Content = providedContent,
                FileUrl = fileUrl,
                MimeType = file.ContentType,
            };

            var responseDto = await _messageService.SendMessageAsync(groupId, messageDto, senderId);
            return Ok(responseDto);
        }

        [HttpGet("download/{messageId}")]
        public async Task<IActionResult> DownloadFile(Guid messageId)
        {
            var userId = CurrentUserId;

            try
            {
                var fileResponse = await _messageService.DownloadFileAsync(messageId, userId);

                return File(
                    fileResponse.FileStream,
                    fileResponse.ContentType,
                    fileResponse.FileName
                );
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex) when (ex.Message.Contains("must be a member"))
            {
                return Forbid();
            }
        }
    }
}
