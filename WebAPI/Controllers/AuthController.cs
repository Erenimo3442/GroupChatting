using Application;
using Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IUserService userService, ITokenService tokenService)
        : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly ITokenService _tokenService = tokenService;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            try
            {
                var user = await _userService.RegisterAsync(
                    registerDto.Username,
                    registerDto.Password
                );
                // Return a 201 Created status without the user object for security
                return StatusCode(201, new { message = "User registered successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _userService.LoginAsync(loginDto.Username, loginDto.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            // Generate and return the JWT token
            var token = _tokenService.CreateToken(user);

            return Ok(new { token = token });
        }
    }
}
