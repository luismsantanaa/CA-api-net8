namespace Security.Entities.DTOs
{
    public class AuthResponse
    {
        // Base
        public string? Id { get; set; }
        public string? Code { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string? ConcurrencyStamp { get; set; }
        public string? PasswordHash { get; set; }
        public string? AccessToken { get; set; }
        public bool? IsActive { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public bool Success { get; set; }
        public List<string>? Errors { get; set; }

    }
}
