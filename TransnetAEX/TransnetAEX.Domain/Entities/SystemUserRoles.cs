using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TransnetAEX.Domain.Entities
{
    public class SystemUserRoles : IdentityRole
    {
        [MaxLength(150)]
        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}
