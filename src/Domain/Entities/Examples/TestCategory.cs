using Domain.Base;

namespace Domain.Entities.Examples
{
    public class TestCategory : AuditableEntity
    {
        public string? Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? Image { get; set; } = string.Empty;

        public virtual ICollection<TestProduct>? Products { get; set; }
    }
}
