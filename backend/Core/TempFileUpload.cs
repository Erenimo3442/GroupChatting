namespace Core
{
    public class TempFileUpload
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public long Size { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
        public DateTime UploadedAt { get; set; }
        public string TempPath { get; set; } = string.Empty;
    }
}
