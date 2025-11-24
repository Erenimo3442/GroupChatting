using System.IO;
using System.Threading.Tasks;
using Application;
using Application.Dtos;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure
{
    public class FileStorageService(IWebHostEnvironment env) : IFileStorageService
    {
        private readonly IWebHostEnvironment _env = env;

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
        {
            // The wwwroot folder is the default location for serving static files
            var uploadsFolderPath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");

            // Ensure the directory exists
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            // Create a unique filename to avoid conflicts
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

            // Save the file to disk
            using (var newFileStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(newFileStream);
            }

            // Return the public URL for the file
            return $"/uploads/{uniqueFileName}";
        }

        public async Task<FileDownloadResponseDto> GetFileAsync(string fileUrl, DateTime timestamp)
        {
            // Extract filename from URL (format: /uploads/filename)
            var originalFileName = Path.GetFileName(fileUrl);
            var uploadsFolderPath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
            var filePath = Path.Combine(uploadsFolderPath, originalFileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", originalFileName);
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var contentType = GetContentType(originalFileName);
            var extension = Path.GetExtension(originalFileName);

            // Format: "CRTalk file - yyyy-MM-dd HH-mm-ss.extension"
            var formattedTimestamp = timestamp.ToString("yyyy-MM-dd HH-mm-ss");
            var downloadFileName = $"CRTalk file - {formattedTimestamp}{extension}";

            return new FileDownloadResponseDto
            {
                FileStream = memory,
                FileName = downloadFileName,
                ContentType = contentType,
            };
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".doc" => "application/msword",
                ".docx" =>
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".7z" => "application/x-7z-compressed",
                ".mp4" => "video/mp4",
                ".mp3" => "audio/mpeg",
                _ => "application/octet-stream",
            };
        }
    }
}
