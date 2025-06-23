using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        string GenerateJwtToken(User user);
    }
}