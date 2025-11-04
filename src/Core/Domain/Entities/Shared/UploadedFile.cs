using Domain.Base;

namespace Domain.Entities.Shared
{
    public class UploadedFile : AuditableEntity
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Extension { get; set; }
        public decimal? Size { get; set; }
        public string? Path { get; set; }
        public string? Reference { get; set; }
        public string? Comment { get; set; }
    }
}
