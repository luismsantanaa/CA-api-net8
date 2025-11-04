namespace Shared.Services.Configurations
{
    public class EMailSettings
    {
        public required string From { get; set; }
        public required string Host { get; set; }
        public required int Port { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string? DisplayName { get; set; }
    }
}
