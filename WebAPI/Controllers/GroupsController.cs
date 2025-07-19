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
    public class GroupsController(IGroupService groupService) : ControllerBase
    {
        private readonly IGroupService _groupService = groupService;

        [HttpPost]
        public async Task<IActionResult> CreateGroup(CreateGroupDto dto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

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

        [HttpGet("public")]
        public async Task<IActionResult> GetPublicGroups()
        {
            var groups = await _groupService.GetPublicGroupsAsync();
            return Ok(groups);
        }

        [HttpPost("{groupId}/invite/{inviteeId}")]
        public async Task<IActionResult> InviteUserToGroup(Guid groupId, Guid inviteeId)
        {
            var inviterIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (
                string.IsNullOrEmpty(inviterIdString)
                || !Guid.TryParse(inviterIdString, out var inviterId)
            )
            {
                return Unauthorized();
            }

            try
            {
                await _groupService.InviteUserToGroupAsync(groupId, inviterId, inviteeId);
                return Ok("User invited successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{groupId}/accept")]
        public async Task<IActionResult> AcceptGroupInvitation(Guid groupId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                await _groupService.AcceptGroupInvitationAsync(groupId, userId);
                return Ok("Invitation accepted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{groupId}/apply")]
        public async Task<IActionResult> ApplyToGroup(Guid groupId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                await _groupService.ApplyToGroupAsync(groupId, userId);
                return Ok("Application submitted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{groupId}/approve/{applicantId}")]
        public async Task<IActionResult> ApproveApplication(Guid groupId, Guid applicantId)
        {
            var approverIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (
                string.IsNullOrEmpty(approverIdString)
                || !Guid.TryParse(approverIdString, out var approverId)
            )
            {
                return Unauthorized();
            }

            try
            {
                await _groupService.ApproveApplicationAsync(groupId, approverId, applicantId);
                return Ok("Application approved successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{groupId}/join")]
        public async Task<IActionResult> JoinPublicGroup(Guid groupId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                await _groupService.JoinPublicGroupAsync(groupId, userId);
                return Ok("Joined public group successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
