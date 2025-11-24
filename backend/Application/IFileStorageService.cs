using Application.Dtos;

namespace Application
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(Stream fileStream, string fileName);
        Task<FileDownloadResponseDto> GetFileAsync(string fileUrl, DateTime timestamp);
    }
}
