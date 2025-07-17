using System.Security.Claims;
using Application;
using Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController(IGroupService groupService) : ControllerBase
    {
        private readonly IGroupService _groupService = groupService;

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateGroup(CreateGroupDto dto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized(); // Or BadRequest
            }

            // Now you have the userId as a Guid
            var group = await _groupService.CreateGroupAsync(dto, userId);

            return Ok(group);
        }
    }
}
