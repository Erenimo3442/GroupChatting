using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application;
using Application.Dtos;
using BCrypt.Net;
using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure
{
    public class UserService(
        AppDbContext context,
        ITokenService tokenService,
        IConfiguration config
    ) : IUserService
    {
        private readonly AppDbContext _context = context;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IConfiguration _config = config;

        public async Task<User> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                throw new InvalidOperationException("Username is already taken.");
            }

            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new ArgumentException("Username and password cannot be empty.");
            }

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return null;
            }

            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            user.RefreshToken = refreshToken;
            user.TokenCreated = DateTime.UtcNow;
            user.TokenExpires = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            var accessToken = _tokenService.CreateToken(user);

            return new AuthResponseDto { AccessToken = accessToken, RefreshToken = refreshToken };
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenKey = _config["AppSettings:Token"];
            if (string.IsNullOrEmpty(tokenKey))
            {
                throw new Exception("AppSettings:Token is not configured.");
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
                ValidateLifetime = false,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(
                token,
                tokenValidationParameters,
                out var securityToken
            );

            if (securityToken is not JwtSecurityToken)
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto dto)
        {
            // Get user principal from the expired access token
            var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
            var userIdString = principal
                ?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                ?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
                return null;

            // Find the user in the database
            var user = await _context.Users.FindAsync(userId);

            // Validate the refresh token
            if (
                user == null
                || user.RefreshToken != dto.RefreshToken
                || user.TokenExpires <= DateTime.UtcNow
            )
            {
                return null;
            }

            // Generate new tokens and update the user
            var newAccessToken = _tokenService.CreateToken(user);
            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            user.RefreshToken = newRefreshToken;
            user.TokenCreated = DateTime.UtcNow;
            user.TokenExpires = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };
        }
    }
}
