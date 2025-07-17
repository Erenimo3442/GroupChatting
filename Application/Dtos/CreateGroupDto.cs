using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class CreateGroupDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public bool IsPublic { get; set; } = false;
    }
}
