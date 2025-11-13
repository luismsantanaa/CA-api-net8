namespace Security.Entities
{
    public class RegistrationResponse
    {
        public required Guid UserId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Token { get; set; }
        public required string RefreshToken { get; set; }
    }
}
