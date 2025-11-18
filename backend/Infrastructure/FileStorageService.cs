using System.IO;
using System.Threading.Tasks;
using Application;
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
    }
}
