namespace Security.Entities.DTOs
{
    public class ChangePassword
    {
        public required int UserId { get; set; }
        public required string Oldpassword { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }
}
