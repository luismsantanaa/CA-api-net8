using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Security.Entities
{
    public class AppUser : BaseEntity
    {
        [Required]
        public string? UserId { get; set; }

        [Column(Order = 2, TypeName = "varchar(25)")]
        public string? Codigo { get; set; }

        [Column(Order = 3, TypeName = "varchar(75)")]
        public string FullName { get; set; } = string.Empty;

        [Column(Order = 4, TypeName = "varchar(50)")]
        public string Email { get; set; } = string.Empty;

        [Column(Order = 5, TypeName = "varchar(50)")]
        public string Department { get; set; } = string.Empty;

        [Column(Order = 6, TypeName = "varchar(75)")]
        public string Position { get; set; } = string.Empty;

        [Column(Order = 7, TypeName = "varchar(50)")]
        public string? Company { get; set; }

        [Column(Order = 8, TypeName = "varchar(50)")]
        public string Office { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public IdentityUser? User { get; set; }
    }
}
