using System.Security.Claims;
using Application;
using Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController(IGroupService groupService) : BaseApiController
    {
        private readonly IGroupService _groupService = groupService;

        [HttpPost]
        public async Task<ActionResult<GroupResponseDto>> CreateGroup(CreateGroupDto dto)
        {
            var userId = CurrentUserId;

            var groupEntity = await _groupService.CreateGroupAsync(dto, userId);

            var groupResponse = new GroupResponseDto
            {
                Id = groupEntity.Id,
                Name = groupEntity.Name,
                IsPublic = groupEntity.IsPublic,
            };

            return CreatedAtAction(
                nameof(CreateGroup),
                new { id = groupResponse.Id },
                groupResponse
            );
        }

        [AllowAnonymous]
        [HttpGet("public")]
        public async Task<ActionResult<IEnumerable<GroupResponseDto>>> GetPublicGroups()
        {
            var groupEntities = await _groupService.GetPublicGroupsAsync();

            var groupResponses = groupEntities
                .Select(g => new GroupResponseDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    IsPublic = g.IsPublic,
                })
                .ToList();

            return Ok(groupResponses);
        }

        [HttpPost("{groupId}/invite")]
        public async Task<IActionResult> InviteUserToGroup(InviteUserToGroupDto dto, Guid groupId)
        {
            var inviterId = CurrentUserId;

            await _groupService.InviteUserToGroupAsync(groupId, inviterId, dto);
            return Ok(new { message = "User invited successfully." });
        }

        [HttpPost("{groupId}/accept")]
        public async Task<IActionResult> AcceptGroupInvitation(Guid groupId)
        {
            var userId = CurrentUserId;

            await _groupService.AcceptGroupInvitationAsync(groupId, userId);
            return Ok(new { message = "Invitation accepted successfully." });
        }

        [HttpPost("{groupId}/apply")]
        public async Task<IActionResult> ApplyToGroup(Guid groupId)
        {
            var userId = CurrentUserId;

            await _groupService.ApplyToGroupAsync(groupId, userId);
            return Ok(new { message = "Application submitted successfully." });
        }

        [HttpPost("{groupId}/approve")]
        public async Task<IActionResult> ApproveApplication(Guid groupId, ApproveApplicationDto dto)
        {
            var approverId = CurrentUserId;

            await _groupService.ApproveApplicationAsync(groupId, approverId, dto);
            return Ok(new { message = "Application approved successfully." });
        }

        [HttpPost("{groupId}/join")]
        public async Task<IActionResult> JoinPublicGroup(Guid groupId)
        {
            var userId = CurrentUserId;

            await _groupService.JoinPublicGroupAsync(groupId, userId);
            return Ok(new { message = "Joined public group successfully." });
        }
    }
}
