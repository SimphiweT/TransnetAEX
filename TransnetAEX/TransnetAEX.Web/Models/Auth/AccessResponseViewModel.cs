using System;

namespace TransnetAEX.Web.Models.Auth
{
    /// <summary>
    /// Represents a user access request and its current review status for display in the MVC frontend.
    /// </summary>
    public class AccessResponseViewModel
    {
        public int Id { get; set; }

        /// <summary>Full name of the requester.</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Email address of the requester.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>The role the requester applied for.</summary>
        public string RoleApplied { get; set; } = string.Empty;

        /// <summary>Message or motivation submitted by the requester.</summary>
        public string? Message { get; set; }

        /// <summary>Relative path or URL to the uploaded PDF document.</summary>
        public string? PdfFilePath { get; set; }

        /// <summary>Indicates if the request has been approved.</summary>
        public bool IsApproved { get; set; }

        /// <summary>The name or email of the administrator who approved the request (if any).</summary>
        public string? ApprovedBy { get; set; }

        /// <summary>The name or email of the administrator who declined the request (if any).</summary>
        public string? DeclinedBy { get; set; }

        /// <summary>Date when the request was created.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Date when the request was last updated (either approved or declined).</summary>
        public DateTime? UpdatedAt { get; set; }

        // ===========================
        // 🧠 Helper Properties
        // ===========================

        /// <summary>Derived string status for UI convenience (Pending / Approved / Declined).</summary>
        public string Status
        {
            get
            {
                if (IsApproved && !string.IsNullOrEmpty(ApprovedBy)) return "Approved";
                if (!IsApproved && !string.IsNullOrEmpty(DeclinedBy)) return "Declined";
                return "Pending";
            }
        }

        /// <summary>Readable label for display tables and dashboards.</summary>
        public string StatusDisplay
        {
            get
            {
                return Status switch
                {
                    "Approved" => $"✅ Approved by {ApprovedBy}",
                    "Declined" => $"❌ Declined by {DeclinedBy}",
                    _ => "🕓 Pending Review"
                };
            }
        }

        /// <summary>Time difference string for showing how long ago it was created/updated.</summary>
        public string TimeSince
        {
            get
            {
                var date = UpdatedAt ?? CreatedAt;
                var diff = DateTime.UtcNow - date;
                if (diff.TotalDays >= 1) return $"{(int)diff.TotalDays} day(s) ago";
                if (diff.TotalHours >= 1) return $"{(int)diff.TotalHours} hour(s) ago";
                return $"{(int)diff.TotalMinutes} minute(s) ago";
            }
        }
    }
}
