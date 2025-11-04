using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Base
{
    public abstract class AuditableEntity : BaseEntity
    {
        [Column(TypeName = "uniqueidentifier")]
        [Required]
        public string? CreatedBy { get; set; }
        [Required]
        public DateTime CreatedOn { get; set; }
        [Column(TypeName = "uniqueidentifier")]
        public string? LastModifiedBy { get; set; } = null;
        public DateTime? LastModifiedOn { get; set; } = null;
        [Required]
        public int Version { get; set; } = 1;
    }
}
