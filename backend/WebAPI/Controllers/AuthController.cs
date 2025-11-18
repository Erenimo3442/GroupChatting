using Application;
using Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            await _userService.RegisterAsync(registerDto);
            return StatusCode(201, new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            var authResponse = await _userService.LoginAsync(loginDto);

            if (authResponse == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            return Ok(authResponse);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenDto dto)
        {
            var authResponse = await _userService.RefreshTokenAsync(dto);

            if (authResponse == null)
            {
                return Unauthorized(new { message = "Invalid refresh token." });
            }

            return Ok(authResponse);
        }
    }
}
