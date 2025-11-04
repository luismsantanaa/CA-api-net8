using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Security.Entities
{
    public class RefreshToken : BaseEntity
    {
        [Required]
        public string? UserId { get; set; }
        [Required]
        public string? Token { get; set; }
        [Required]
        public string? JwtId { get; set; }
        [Required]
        public bool IsUsed { get; set; }
        [Required]
        public bool IsRevoked { get; set; }
        [Required]
        public DateTime ExpireDate { get; set; }

        [ForeignKey(nameof(UserId))]
        public IdentityUser? User { get; set; }
    }
}
