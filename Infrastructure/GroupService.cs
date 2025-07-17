using Application;
using Application.Dtos;

namespace Infrastructure
{
    public class GroupService(AppDbContext context) : IGroupService
    {
        private readonly AppDbContext _context = context;

        public async Task<Group> CreateGroupAsync(CreateGroupDto dto, Guid userId)
        {
            var group = new Group { Name = dto.Name, IsPublic = dto.IsPublic };

            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();

            return group;
        }
    }
}
