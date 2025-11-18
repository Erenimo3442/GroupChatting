using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Application.Dtos
{
    public class FileUploadDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
