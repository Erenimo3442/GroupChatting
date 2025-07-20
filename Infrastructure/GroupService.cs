using Application;
using Application.Dtos;
using Application.Exceptions;
using Core;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class GroupService(AppDbContext context) : IGroupService
    {
        private readonly AppDbContext _context = context;

        public async Task<Group> CreateGroupAsync(CreateGroupDto dto, Guid userId)
        {
            var group = new Group { Name = dto.Name, IsPublic = dto.IsPublic };

            // Add the creator as a member of the group
            var groupMember = new GroupMember
            {
                UserId = userId,
                Group = group,
                User =
                    await _context.Users.FindAsync(userId)
                    ?? throw new NotFoundException("User not found"),
                Role = "Admin",
            };

            await _context.Groups.AddAsync(group);
            await _context.GroupMembers.AddAsync(groupMember);
            await _context.SaveChangesAsync();

            return group;
        }

        public async Task InviteUserToGroupAsync(
            Guid groupId,
            Guid inviterId,
            InviteUserToGroupDto dto
        )
        {
            var group =
                await _context.Groups.FindAsync(groupId)
                ?? throw new NotFoundException("Group not found");
            var inviter =
                await _context.Users.FindAsync(inviterId)
                ?? throw new NotFoundException("Inviter not found");
            var invitee =
                await _context.Users.FindAsync(dto.InviteeId)
                ?? throw new NotFoundException("Invitee not found");
            var inviterMember =
                await _context.GroupMembers.FirstOrDefaultAsync(m =>
                    m.GroupId == groupId && m.UserId == inviterId
                ) ?? throw new NotFoundException("Inviter is not a member of the group");

            if (inviterMember.Role != "Admin")
            {
                throw new ForbiddenAccessException(
                    "Inviter does not have permission to invite users to this group"
                );
            }

            if (await IsUserMemberOfGroupAsync(groupId, dto.InviteeId))
            {
                throw new Exception("Invitee is already a member of the group");
            }

            var groupMember = new GroupMember
            {
                UserId = dto.InviteeId,
                GroupId = groupId,
                Group = group,
                User = invitee,
                Role = "Member",
                MembershipStatus = "Invited",
            };

            await _context.GroupMembers.AddAsync(groupMember);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsUserMemberOfGroupAsync(Guid groupId, Guid userId)
        {
            return await _context.GroupMembers.AnyAsync(m =>
                m.GroupId == groupId && m.UserId == userId
            );
        }

        public async Task<List<Group>> GetPublicGroupsAsync()
        {
            return await _context.Groups.Where(g => g.IsPublic).ToListAsync();
        }

        public async Task AcceptGroupInvitationAsync(Guid groupId, Guid userId)
        {
            var groupMember =
                await _context.GroupMembers.FirstOrDefaultAsync(m =>
                    m.GroupId == groupId && m.UserId == userId && m.MembershipStatus == "Invited"
                )
                ?? throw new NotFoundException(
                    "No invitation found for this user in the specified group"
                );

            groupMember.MembershipStatus = "Active";
            await _context.SaveChangesAsync();
        }

        public async Task ApplyToGroupAsync(Guid groupId, Guid userId)
        {
            var group =
                await _context.Groups.FindAsync(groupId)
                ?? throw new NotFoundException("Group not found");
            var user =
                await _context.Users.FindAsync(userId)
                ?? throw new NotFoundException("User not found");

            if (await IsUserMemberOfGroupAsync(groupId, userId))
            {
                throw new Exception("User is already a member of the group");
            }

            var groupMember = new GroupMember
            {
                UserId = userId,
                GroupId = groupId,
                Group = group,
                User = user,
                Role = "Member",
                MembershipStatus = "PendingApproval",
            };

            await _context.GroupMembers.AddAsync(groupMember);
            await _context.SaveChangesAsync();
        }

        public async Task ApproveApplicationAsync(
            Guid groupId,
            Guid approverId,
            ApproveApplicationDto dto
        )
        {
            var group =
                await _context.Groups.FindAsync(groupId)
                ?? throw new NotFoundException("Group not found");
            var approver =
                await _context.Users.FindAsync(approverId)
                ?? throw new NotFoundException("Approver not found");
            var applicant =
                await _context.Users.FindAsync(dto.ApplicantId)
                ?? throw new NotFoundException("Applicant not found");

            var approverMember =
                await _context.GroupMembers.FirstOrDefaultAsync(m =>
                    m.GroupId == groupId && m.UserId == approverId
                ) ?? throw new NotFoundException("Approver is not a member of the group");

            if (approverMember.Role != "Admin")
            {
                throw new ForbiddenAccessException(
                    "Approver does not have permission to approve applications for this group"
                );
            }

            var applicantMember =
                await _context.GroupMembers.FirstOrDefaultAsync(m =>
                    m.GroupId == groupId
                    && m.UserId == dto.ApplicantId
                    && m.MembershipStatus == "PendingApproval"
                )
                ?? throw new NotFoundException(
                    "No pending application found for this user in the specified group"
                );

            applicantMember.MembershipStatus = "Active";
            await _context.SaveChangesAsync();
        }

        public async Task JoinPublicGroupAsync(Guid groupId, Guid userId)
        {
            var group =
                await _context.Groups.FindAsync(groupId)
                ?? throw new NotFoundException("Group not found");
            var user =
                await _context.Users.FindAsync(userId)
                ?? throw new NotFoundException("User not found");

            if (await IsUserMemberOfGroupAsync(groupId, userId))
            {
                throw new Exception("User is already a member of the group");
            }

            if (!group.IsPublic)
            {
                throw new ForbiddenAccessException("Only public groups can be joined directly.");
            }

            var groupMember = new GroupMember
            {
                UserId = userId,
                GroupId = groupId,
                Group = group,
                User = user,
                Role = "Member",
                MembershipStatus = "Active",
            };

            await _context.GroupMembers.AddAsync(groupMember);
            await _context.SaveChangesAsync();
        }
    }
}
