using Domain.Base;

namespace Domain.Entities.Shared
{
    public class MailNotificationTemplate : AuditableEntity
    {
        public string? Description { get; set; }
        public string? Suject { get; set; }
        public string? BodyHtml { get; set; }
        public string? PathImages { get; set; }
    }
}
