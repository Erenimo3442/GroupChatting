using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class SendMessageDto
    {
        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}
