using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class MessageResponseDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? LastEditedAt { get; set; }
        public string? FileUrl { get; set; }
        public string? MimeType { get; set; }

        // Additional properties for better API responses
        public string SenderUsername { get; set; } = string.Empty;
        public bool IsEdited => LastEditedAt.HasValue;
        public bool HasFile => !string.IsNullOrEmpty(FileUrl);
    }
}
