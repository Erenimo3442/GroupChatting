namespace Application
{
    public interface ITempFileService
    {
        Task<string> SaveTempFileAsync(
            Stream fileStream,
            string originalFileName,
            string mimeType,
            long fileSize,
            Guid userId,
            Guid groupId
        );

        Task PromoteToPermanentAsync(string fileUrl, Guid messageId);
        Task<IEnumerable<string>> GetExpiredTempFilesAsync(int hoursOld);
        Task DeleteTempFileAsync(string fileName);
    }
}
