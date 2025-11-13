using System.ComponentModel.DataAnnotations.Schema;
using Domain.Base;

namespace Domain.Entities.Examples
{
    [Table("TestProduct", Schema = "Example")]
    public class TestProduct : SoftDelete
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? Image { get; set; }

        public double Price { get; set; } = 0.0;

        public int Stock { get; set; } = 0;

        public Guid CategoryId { get; set; }

        public virtual TestCategory? Category { get; }
    }
}
