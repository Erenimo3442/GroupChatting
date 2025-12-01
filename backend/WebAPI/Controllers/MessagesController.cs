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
        IFileStorageService fileStorageService,
        ITempFileService tempFileService
    ) : BaseApiController
    {
        private readonly IMessageService _messageService = messageService;
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly ITempFileService _tempFileService = tempFileService;

        [HttpPost]
        public async Task<ActionResult<MessageResponseDto>> SendMessage(
            Guid groupId,
            [FromBody] SendMessageDto sendMessageDto
        )
        {
            // Validate: at least content OR fileUrl must be provided
            if (
                string.IsNullOrWhiteSpace(sendMessageDto.Content)
                && string.IsNullOrWhiteSpace(sendMessageDto.FileUrl)
            )
            {
                return BadRequest("Message content or file URL must be provided.");
            }

            var senderId = CurrentUserId;

            var responseDto = await _messageService.SendMessageAsync(
                groupId,
                sendMessageDto,
                senderId
            );

            if (!string.IsNullOrWhiteSpace(sendMessageDto.FileUrl))
            {
                await _tempFileService.PromoteToPermanentAsync(
                    sendMessageDto.FileUrl,
                    responseDto.Id
                );
            }
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
        public async Task<ActionResult<FileUploadResponseDto>> UploadFile(
            [FromRoute] Guid groupId,
            IFormFile file
        )
        {
            // Validation
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "application/pdf" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest("File type not allowed");

            if (file.Length > 10 * 1024 * 1024) // 10MB limit
                return BadRequest("File too large (max 10MB)");

            var userId = CurrentUserId;

            // Temp file service
            var fileUrl = await _tempFileService.SaveTempFileAsync(
                file.OpenReadStream(),
                file.FileName,
                file.ContentType,
                file.Length,
                userId,
                groupId
            );

            return Ok(
                new FileUploadResponseDto
                {
                    FileUrl = fileUrl,
                    FileName = file.FileName,
                    MimeType = file.ContentType,
                    FileSize = file.Length,
                }
            );
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
