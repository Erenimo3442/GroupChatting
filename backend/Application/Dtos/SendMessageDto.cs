using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class SendMessageDto
    {
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public string? MimeType { get; set; }
    }
}
