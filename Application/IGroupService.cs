using Application.Dtos;

namespace Application
{
    public interface IGroupService
    {
        Task<Group> CreateGroupAsync(CreateGroupDto dto, Guid userId);
    }
}
