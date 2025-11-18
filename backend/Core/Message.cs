namespace Core
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? LastEditedAt { get; set; } = null;
        public string? FileUrl { get; set; } = null;
        public string? MimeType { get; set; } = null;
    }
}
