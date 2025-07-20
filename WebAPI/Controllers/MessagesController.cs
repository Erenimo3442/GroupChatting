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
    ) : BaseApiController
    {
        private readonly IMessageService _messageService = messageService;
        private readonly IFileStorageService _fileStorageService = fileStorageService;

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageDto sendMessageDto)
        {
            var senderId = CurrentUserId;

            var responseDto = await _messageService.SendMessageAsync(sendMessageDto, senderId);
            return Ok(responseDto);
        }

        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetMessages(
            Guid groupId,
            [FromQuery] GetMessagesDto getMessagesDto
        )
        {
            var responseDtos = await _messageService.GetMessagesAsync(groupId, getMessagesDto);
            return Ok(responseDtos);
        }

        [HttpPut("{messageId}")]
        public async Task<IActionResult> UpdateMessage(
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
        public async Task<IActionResult> UploadFile([FromForm] FileUploadDto uploadDto)
        {
            if (uploadDto.File == null || uploadDto.File.Length == 0)
                return BadRequest("No file uploaded.");

            var senderId = CurrentUserId;

            // Save the file using the storage service
            var fileUrl = await _fileStorageService.SaveFileAsync(
                uploadDto.File.OpenReadStream(),
                uploadDto.File.FileName
            );

            // Create a message DTO to send to the service
            var messageDto = new SendMessageDto
            {
                GroupId = uploadDto.GroupId,
                Content = $"[File]: {uploadDto.File.FileName}",
                FileUrl = fileUrl,
                MimeType = uploadDto.File.ContentType,
            };

            var responseDto = await _messageService.SendMessageAsync(messageDto, senderId);
            return Ok(responseDto);
        }
    }
}
