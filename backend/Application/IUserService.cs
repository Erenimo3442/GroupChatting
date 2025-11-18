using Application.Dtos;
using Core;

namespace Application
{
    public interface IUserService
    {
        Task<User> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
        Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto dto);
    }
}
