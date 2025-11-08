using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using TransnetAEX.Api.Extensions;
using TransnetAEX.Api.Middleware;
using TransnetAEX.Application.Helpers;
using TransnetAEX.Application.Interfaces;
using TransnetAEX.Application.Services;
using TransnetAEX.Domain.Configurations;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories.Base;
using TransnetAEX.Infrastructure.Data;
using TransnetAEX.Infrastructure.EF_Repositories.Base;
using TransnetAEX.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Kestrel Configuration --------------------
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5038);
    options.Limits.MaxRequestBodySize = 100_000_000; // 100 MB
});

// -------------------- Services --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// -------------------- Database Context --------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// -------------------- Identity Configuration --------------------
builder.Services.AddIdentity<SystemUser, SystemUserRoles>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// -------------------- JWT Authentication --------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"] ?? throw new InvalidOperationException("JWT key missing"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSection["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

// -------------------- Authorization --------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole(ConstantsUtils.ADMIN_ROLE));
    options.AddPolicy("RequireHrRole", policy => policy.RequireRole(ConstantsUtils.HR_ROLE));
});
// -------------------- DI --------------------
// Repositories
builder.Services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();

// Application Services
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IQuestionBankService, QuestionBankService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailService, GraphEmailService>();

// -------------------- Swagger / OpenAPI --------------------
var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Transnet AEX API",
        Version = "v1",
        Description = "Automated Exit Management System for HR, Line Managers, and Employees",
        Contact = new OpenApiContact
        {
            Name = "Nxumalo Simphiwe",
            Email = "Simphiwe.Nxumalo3@transnet.net",
            Url = new Uri("https://github.com/SimphiweT")
        }
    });

    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    // JWT Security Definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and your token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// -------------------- CORS --------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// -------------------- File Upload Limits --------------------
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100_000_000; // 100 MB
});

// -------------------- Build App --------------------
var app = builder.Build();

// -------------------- Middleware --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGlobalExceptionHandler(); //our new middleware


app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
// Custom JWT middleware (after auth)
app.UseMiddleware<JwtMiddleware>();
app.MapControllers();

// -------------------- Global Exception Logging --------------------
AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
    Console.WriteLine($"[Unhandled Exception] {e.ExceptionObject}");

TaskScheduler.UnobservedTaskException += (sender, e) =>
    Console.WriteLine($"[Unobserved Task Exception] {e.Exception}");

// -------------------- Seed Database --------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.SeedAsync(services, maxRetries: 3);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database seeding failed after retries: {ex.Message}");
        throw;
    }
}

// -------------------- Run --------------------
app.Run();
