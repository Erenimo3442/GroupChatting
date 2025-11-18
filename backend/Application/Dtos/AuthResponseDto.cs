using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class AuthResponseDto
    {
        [Required]
        public string AccessToken { get; set; } = string.Empty;

        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
