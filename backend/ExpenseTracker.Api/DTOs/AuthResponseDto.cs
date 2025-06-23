namespace ExpenseTracker.Api.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}