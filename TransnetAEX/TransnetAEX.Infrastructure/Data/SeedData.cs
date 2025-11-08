using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TransnetAEX.Domain.Entities;

namespace TransnetAEX.Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(IServiceProvider services, int maxRetries = 3)
        {
            int attempts = 0;
            while (attempts < maxRetries)
            {
                try
                {
                    using var scope = services.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<SystemUserRoles>>();
                    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<SystemUser>>();

                    // Apply migrations
                    if ((await context.Database.GetPendingMigrationsAsync()).Any())
                    {
                        Console.WriteLine("Applying pending migrations...");
                        await context.Database.MigrateAsync();
                    }

                    // Seed entities
                    await SeedRolesAsync(roleMgr);
                    await SeedAdminUserAsync(userMgr, roleMgr);
                    await SeedQuestionBankAsync(context);
                    await SeedSampleExitReviewsAsync(context);

                    Console.WriteLine("✅ Database seeding completed successfully.");
                    return;
                }
                catch (Exception ex)
                {
                    attempts++;
                    Console.WriteLine($"⚠️ Seeding attempt {attempts} failed: {ex.Message}");
                    if (attempts >= maxRetries)
                        throw;
                    await Task.Delay(2000);
                }
            }
        }

        // ----------------- ROLES -----------------
        private static async Task SeedRolesAsync(RoleManager<SystemUserRoles> roleMgr)
        {
            string[] roles = { "Admin", "HR"};

            foreach (var roleName in roles)
            {
                if (!await roleMgr.RoleExistsAsync(roleName))
                {
                    var role = new SystemUserRoles
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper(),
                        Description = $"{roleName} default role",
                        CreatedDate = DateTime.UtcNow
                    };
                    await roleMgr.CreateAsync(role);
                    Console.WriteLine($"✅ Role created: {roleName}");
                }
            }
        }

        // ----------------- ADMIN USER -----------------
        private static async Task SeedAdminUserAsync(UserManager<SystemUser> userMgr, RoleManager<SystemUserRoles> roleMgr)
        {
            var adminEmail = "nxumalosimphiweterrence@gmail.com";
            var existingUser = await userMgr.FindByEmailAsync(adminEmail);
            if (existingUser == null)
            {
                var adminUser = new SystemUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    SAP_Number = "000000",
                    ContactNumber = "0100000000",
                    Description = "System Administrator",
                    CreatedDate = DateTime.UtcNow
                };

                var result = await userMgr.CreateAsync(adminUser, "Admin@1234");
                if (result.Succeeded)
                {
                    await userMgr.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("✅ Admin user created and assigned to Admin role.");
                }
            }

            var adminEmail2 = "simphiweterrencenxumalo@gmail.com";
            var existingUser2 = await userMgr.FindByEmailAsync(adminEmail2);
            if (existingUser2 == null)
            {
                var adminUser = new SystemUser
                {
                    UserName = adminEmail2,
                    Email = adminEmail2,
                    EmailConfirmed = true,
                    SAP_Number = "000001",
                    ContactNumber = "0100000001",
                    Description = "HR Staff",
                    CreatedDate = DateTime.UtcNow
                };

                var result = await userMgr.CreateAsync(adminUser, "Hr@1234");
                if (result.Succeeded)
                {
                    await userMgr.AddToRoleAsync(adminUser, "HR");
                    Console.WriteLine("✅ Hr user created and assigned to Hr role.");
                }
            }

            var adminEmail3 = "blackdoorsdev@gmail.com";
            var existingUser3 = await userMgr.FindByEmailAsync(adminEmail3);
            if (existingUser3 == null)
            {
                var adminUser = new SystemUser
                {
                    UserName = adminEmail3,
                    Email = adminEmail3,
                    EmailConfirmed = true,
                    SAP_Number = "000002",
                    ContactNumber = "0100000002",
                    Description = "HR Staff",
                    CreatedDate = DateTime.UtcNow
                };

                var result = await userMgr.CreateAsync(adminUser, "Hr@1234");
                if (result.Succeeded)
                {
                    await userMgr.AddToRoleAsync(adminUser, "HR");
                    Console.WriteLine("✅ Hr user created and assigned to Hr role.");
                }
            }
        }

        // ----------------- QUESTION BANK -----------------
        private static async Task SeedQuestionBankAsync(AppDbContext context)
        {
            if (!context.QuestionBanks.Any())
            {
                var questions = new List<QuestionBank>
                {
                    new QuestionBank
                    {
                        SectionType = "Employee",
                        QuestionText = "How satisfied were you with your work environment?",
                        InputType = "Rating",
                        Options = "Poor,Fair,Good,Excellent",
                        IsRequired = true
                    },
                    new QuestionBank
                    {
                        SectionType = "Employee",
                        QuestionText = "What improvements would you recommend for the company?",
                        InputType = "TextArea"
                    },
                    new QuestionBank
                    {
                        SectionType = "LineManager",
                        QuestionText = "How was the employee’s performance during their final months?",
                        InputType = "Rating",
                        Options = "Poor,Fair,Good,Excellent"
                    },
                    new QuestionBank
                    {
                        SectionType = "LineManager",
                        QuestionText = "Were all assigned handover tasks completed satisfactorily?",
                        InputType = "Dropdown",
                        Options = "Yes,No,Partially"
                    }
                };

                await context.QuestionBanks.AddRangeAsync(questions);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Default QuestionBank entries seeded.");
            }
        }

        // ----------------- SAMPLE EXIT REVIEW -----------------
        private static async Task SeedSampleExitReviewsAsync(AppDbContext context)
        {
            if (!context.Reviews.Any())
            {
                var review = new HRInitiatedReview
                {
                    HrEmail = "hr@transnetaex.co.za",
                    EmployeeEmail = "employee@transnetaex.co.za",
                    LineManagerEmail = "manager@transnetaex.co.za",
                    Status = "Pending",
                    CreatedDate = DateTime.UtcNow
                };

                var employeeSection = new ReviewSection
                {
                    SectionType = "Employee",
                    ReviewerEmail = review.EmployeeEmail,
                    CreatedDate = DateTime.UtcNow
                };

                var managerSection = new ReviewSection
                {
                    SectionType = "LineManager",
                    ReviewerEmail = review.LineManagerEmail,
                    CreatedDate = DateTime.UtcNow
                };

                review.ReviewSections.Add(employeeSection);
                review.ReviewSections.Add(managerSection);

                review.Notifications.Add(new NotificationLog
                {
                    RecipientEmail = review.EmployeeEmail,
                    NotificationType = "EmployeeLink",
                    Status = "Sent",
                    CreatedDate = DateTime.UtcNow
                });

                review.Notifications.Add(new NotificationLog
                {
                    RecipientEmail = review.LineManagerEmail,
                    NotificationType = "ManagerLink",
                    Status = "Sent",
                    CreatedDate = DateTime.UtcNow
                });

                await context.Reviews.AddAsync(review);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Sample HR-initiated review, sections, and notifications seeded.");
            }
        }
    }
}
