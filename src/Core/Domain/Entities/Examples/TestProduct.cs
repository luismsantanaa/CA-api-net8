using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Base;

namespace Domain.Entities.Examples
{
    [Table("TestProduct", Schema = "Example")]
    public class TestProduct : SoftDelete
    {
        [Column(TypeName = "varchar(75)")]
        [Required]
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "varchar(250)")]
        public string Description { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "varchar(350)")]
        public string? Image { get; set; }
        [Column(TypeName = "numeric(10,2)")]
        [Required]
        public double Price { get; set; } = 0.0;
        [Column(TypeName = "uniqueidentifier")]
        [ForeignKey("TestCategory")]
        [Required]
        public Guid CategoryId { get; set; }

        public TestCategory? Category { get; }
    }
}
