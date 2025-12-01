using Application;
using Application.Dtos;
using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure
{
    public class TempFileService : ITempFileService
    {
        private readonly AppDbContext _dbContext;
        private readonly IFileStorageService _fileStorageService;
        private readonly string _uploadPath;

        public TempFileService(
            AppDbContext db_dbContext,
            IFileStorageService fileStorageService,
            IConfiguration configuration
        )
        {
            _dbContext = db_dbContext;
            _fileStorageService = fileStorageService;
            _uploadPath = configuration["FileStorage:UploadPath"] ?? "uploads";
        }

        public async Task<string> SaveTempFileAsync(
            Stream fileStream,
            string originalFileName,
            string mimeType,
            long fileSize,
            Guid userId,
            Guid groupId
        )
        {
            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(originalFileName)}";
            var tempPath = Path.Combine(_uploadPath, "temp", fileName);

            // Save physical file
            await _fileStorageService.SaveFileAsync(fileStream, tempPath);

            // Track in database
            var uploadRecord = new TempFileUpload
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                OriginalName = originalFileName,
                MimeType = mimeType,
                Size = fileSize,
                UserId = userId,
                GroupId = groupId,
                UploadedAt = DateTime.UtcNow,
                TempPath = tempPath,
            };

            await _dbContext.TempFileUploads.AddAsync(uploadRecord);
            await _dbContext.SaveChangesAsync();

            return $"/uploads/temp/{fileName}";
        }

        public async Task PromoteToPermanentAsync(string fileName, Guid messageId)
        {
            var record = await _dbContext.TempFileUploads.FirstOrDefaultAsync(t =>
                t.FileName == fileName
            );
            if (record != null)
            {
                _dbContext.TempFileUploads.Remove(record);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<string>> GetExpiredTempFilesAsync(int hoursOld)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hoursOld);
            return await _dbContext
                .TempFileUploads.Where(t => t.UploadedAt < cutoffTime)
                .Select(t => t.FileName)
                .ToListAsync();
        }

        public async Task DeleteTempFileAsync(string fileName)
        {
            var record = await _dbContext.TempFileUploads.FirstOrDefaultAsync(t =>
                t.FileName == fileName
            );
            if (record != null)
            {
                _dbContext.TempFileUploads.Remove(record);
                await _dbContext.SaveChangesAsync();
            }
        }

        private TempFileUploadDto MapToDto(TempFileUpload record)
        {
            return new TempFileUploadDto
            {
                FileName = record.FileName,
                OriginalName = record.OriginalName,
                Size = record.Size,
                UploadedAt = record.UploadedAt,
            };
        }
    }
}
