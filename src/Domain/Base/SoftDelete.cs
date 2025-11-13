using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Base
{
    public abstract class SoftDelete : TraceableEntity
    {
        public bool? IsDeleted { get; set; } = false;
        [Column(TypeName = "uniqueidentifier")]
        public string? DeletedBy { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
