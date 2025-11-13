namespace Shared.Services.Configurations
{
    public class MailRequest
    {
        public List<string> To { get; set; } = [];
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public List<string>? Cc { get; set; }
        public List<string>? Attach { get; set; }
        public bool IsNotification { get; set; } = false;
    }
}
