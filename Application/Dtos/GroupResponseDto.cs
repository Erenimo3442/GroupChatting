namespace Application.Dtos
{
    public class GroupResponseDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public bool IsPublic { get; set; }
    }
}
