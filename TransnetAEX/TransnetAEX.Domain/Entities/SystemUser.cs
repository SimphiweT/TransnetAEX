using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TransnetAEX.Domain.Entities
{
    /// <summary>
    /// Represents an authenticated user within the Transnet AEX system.
    /// Extends IdentityUser for ASP.NET Identity integration.
    /// </summary>
    public class SystemUser : IdentityUser
    {
        /// <summary>
        /// The employee's SAP number (required for all users).
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string SAP_Number { get; set; } = string.Empty;

        /// <summary>
        /// The display name shown across dashboards, emails, and audit logs.
        /// </summary>
        [Required]
        [MaxLength(120)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Optional contact number for direct communication.
        /// </summary>
        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        /// <summary>
        /// Optional short bio or description for context (e.g., role summary).
        /// </summary>
        [MaxLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// Relative or absolute path to the user’s profile picture.
        /// </summary>
        public string? ProfilePicture { get; set; }

        /// <summary>
        /// Refresh token for persistent authentication sessions.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Expiration date of the user's refresh token.
        /// </summary>
        public DateTime? RefreshTokenExpiry { get; set; }

        /// <summary>
        /// Tracks when the user account was created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Tracks the most recent update to the user profile.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }
    }
}
