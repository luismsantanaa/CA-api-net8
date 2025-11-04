using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Base;

namespace Domain.Entities.Examples
{
    [Table("TestCategory", Schema = "Example")]
    public class TestCategory : AuditableEntity
    {
        [Column(TypeName = "varchar(150)")]
        [Required]
        public string? Name { get; set; }
        [Column(TypeName = "varchar(300)")]
        public string? Image { get; set; }

        public virtual ICollection<TestProduct>? Products { get; set; }
    }
}
