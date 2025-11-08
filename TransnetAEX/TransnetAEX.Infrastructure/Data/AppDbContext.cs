using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TransnetAEX.Domain.Entities;

namespace TransnetAEX.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<SystemUser, SystemUserRoles, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // -------------------- Identity Tables --------------------
        public DbSet<SystemUser> SystemUsers { get; set; }     
        public DbSet<SystemUserRoles> SystemUserRoles { get; set; }  

        // -------------------- Application Tables --------------------
        public DbSet<HRInitiatedReview> Reviews { get; set; }
        public DbSet<ReviewSection> ReviewSections { get; set; }
        public DbSet<QuestionBank> QuestionBanks { get; set; }
        public DbSet<NotificationLog> Notifications { get; set; }
        public DbSet<ReviewQuestion> ReviewQuestions { get; set; }
        public DbSet<SessionAuditLog> SessionAuditLogs { get; set; }
        public DbSet<UserAccessRequest> UserAccessRequests { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // -------------------- Identity Table Naming --------------------
            builder.Entity<SystemUser>().ToTable("SystemUsers");
            builder.Entity<SystemUserRoles>().ToTable("SystemUserRoles");
            builder.Entity<IdentityUserRole<string>>().ToTable("SystemUserRoleMappings");
            builder.Entity<IdentityUserClaim<string>>().ToTable("SystemUserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("SystemUserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("SystemUserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("SystemRoleClaims");

            // -------------------- Relationships --------------------
            builder.Entity<HRInitiatedReview>()
                .HasMany(r => r.ReviewSections)
                .WithOne(s => s.Review)
                .HasForeignKey(s => s.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<HRInitiatedReview>()
                .HasMany(r => r.Questions)
                .WithOne()
                .HasForeignKey(q => q.SectionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<HRInitiatedReview>()
                .HasMany(r => r.Notifications)
                .WithOne(n => n.Review)
                .HasForeignKey(n => n.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ReviewSection>()
                .HasMany(s => s.Questions)
                .WithOne(q => q.Section)
                .HasForeignKey(q => q.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ReviewQuestion>()
                .HasOne(q => q.Review)
                .WithMany()
                .HasForeignKey(q => q.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
