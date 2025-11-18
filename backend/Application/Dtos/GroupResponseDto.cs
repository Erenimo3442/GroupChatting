using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class GroupResponseDto
    {
        [Required]
        public Guid Id { get; set; }

        public string? Name { get; set; }

        [Required]
        public bool IsPublic { get; set; }
    }
}
