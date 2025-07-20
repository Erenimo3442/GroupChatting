using Application.Dtos;
using Core;

namespace Application
{
    public interface IGroupService
    {
        Task<Group> CreateGroupAsync(CreateGroupDto dto, Guid userId);

        Task InviteUserToGroupAsync(Guid groupId, Guid inviterId, InviteUserToGroupDto dto);

        Task<bool> IsUserMemberOfGroupAsync(Guid groupId, Guid userId);

        Task<List<Group>> GetPublicGroupsAsync();

        Task AcceptGroupInvitationAsync(Guid groupId, Guid userId);

        Task ApplyToGroupAsync(Guid groupId, Guid userId);

        Task ApproveApplicationAsync(Guid groupId, Guid approverId, ApproveApplicationDto dto);

        Task JoinPublicGroupAsync(Guid groupId, Guid userId);
    }
}
