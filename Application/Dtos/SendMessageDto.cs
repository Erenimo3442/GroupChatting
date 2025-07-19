using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class SendMessageDto
    {
        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? FileUrl { get; set; } = null;
        public string? MimeType { get; set; } = null;
    }
}
