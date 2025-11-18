using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class MessageResponseDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public bool IsDeleted { get; set; }

        public DateTime? LastEditedAt { get; set; }
        public string? FileUrl { get; set; }
        public string? MimeType { get; set; }

        // Additional properties for better API responses
        [Required]
        public string SenderUsername { get; set; } = string.Empty;

        [Required]
        public bool IsEdited => LastEditedAt.HasValue;

        [Required]
        public bool HasFile => !string.IsNullOrEmpty(FileUrl);
    }
}
