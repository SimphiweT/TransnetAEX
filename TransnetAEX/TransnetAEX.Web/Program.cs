using Microsoft.AspNetCore.Http.Features;
using TransnetAEX.Web.Api_Services.IServices;
using TransnetAEX.Web.Api_Services.Services;
using TransnetAEX.Web.Helpers;

var builder = WebApplication.CreateBuilder(args);

// === File Upload Limits ===
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104_857_600; // 100 MB
});

// === API Configuration ===
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// === MVC + Session ===
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// === Dependency Injection ===
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SessionManager>();
builder.Services.AddTransient<AuthHeaderHandler>();

// === Service Registrations ===
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAccessService, AccessService>();

// === Typed HttpClients with SSL bypass and auth handler ===
builder.Services.AddHttpClient<IAuthService, AuthService>()
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            // Development only: ignore SSL validation
            ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
        });

builder.Services.AddHttpClient<IUserService, UserService>()
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
        });

builder.Services.AddHttpClient<IAccessService, AccessService>()
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
        });

// === Global Exception Logging ===
AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
    Console.WriteLine("Unhandled Exception: " + args.ExceptionObject);

TaskScheduler.UnobservedTaskException += (sender, args) =>
    Console.WriteLine("Unobserved Task Exception: " + args.Exception.Message);

// === Build App ===
var app = builder.Build();

// === Middleware ===
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseHttpMethodOverride();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// === Routes ===
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
