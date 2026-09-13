using ExceptionHandling.DTO;

namespace ExceptionHandling.Service
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterDto registerDto);
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
        Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
        Task LogoutAsync(RefreshTokenDto dto);
    }
}
