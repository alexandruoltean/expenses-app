using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.DTOs;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly ExpenseContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ExpenseContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return null;
            }

            // Hash password (BCrypt handles salt internally)
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = registerDto.Email,
                Username = registerDto.Username,
                PasswordHash = passwordHash,
                Salt = "", // BCrypt includes salt in the hash
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:ExpiryInDays"));

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Username = user.Username,
                UserId = user.Id,
                ExpiresAt = expiresAt
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
            {
                return null;
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:ExpiryInDays"));

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Username = user.Username,
                UserId = user.Id,
                ExpiresAt = expiresAt
            };
        }

        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:ExpiryInDays")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}