using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [Authorize]
        [HttpGet("protected")]
        public IActionResult GetProtectedData()
        {
            var username = User.Identity?.Name;
            return Ok(new { message = $"Hello {username}, you are authorized!" });
        }
    }
}
