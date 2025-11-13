using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Shared
{
    [Table("AuditLogs", Schema = "Shared")]
    public class AuditLog
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public Guid Id { get; set; }

        [Column(Order = 1, TypeName = "uniqueidentifier")]
        [Required]
        public required Guid UserId { get; set; }

        [Column(Order = 2, TypeName = "varchar(10)")]
        [Required]
        public required string Type { get; set; }

        [Column(Order = 3, TypeName = "varchar(50)")]
        [Required]
        public required string TableName { get; set; }

        [Column(TypeName = "DateTime2")]
        [Required]
        public required DateTime DateTime { get; set; }

        [Column(Order = 4, TypeName = "varchar(max)")]
        [Required]
        public string? OldValues { get; set; }

        [Column(Order = 5, TypeName = "varchar(max)")]
        [Required]
        public string? NewValues { get; set; }

        [Column(Order = 6, TypeName = "varchar(max)")]
        [Required]
        public string? AffectedColumns { get; set; }

        [Column(Order = 7, TypeName = "varchar(50)")]
        [Required]
        public required string PrimaryKey { get; set; }
    }
}
