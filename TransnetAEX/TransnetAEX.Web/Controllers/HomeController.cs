using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using TransnetAEX.Web.Api_Services.IServices;
using TransnetAEX.Web.Helpers;
using TransnetAEX.Web.Models;
using TransnetAEX.Web.Models.Auth;
using TransnetAEX.Web.Models.Reviews;
using TransnetAEX.Web.Models.Shared;

namespace TransnetAEX.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IAuthService authService;
        private readonly ILogger<HomeController> logger;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration config;

        public HomeController(
            IAuthService _authService,
            ILogger<HomeController> _logger,
            SessionManager _sessionManager,
            IHttpClientFactory _httpClientFactory,
            IConfiguration _config
        ) : base(_sessionManager)
        {
            authService = _authService;
            logger = _logger;
            httpClientFactory = _httpClientFactory;
            config = _config;
        }

        // ======================================================
        // HOME PAGE
        // ======================================================
        public IActionResult Index()
        {
            ForceLayoutForRole(null);
            return View();
        }

        // ======================================================
        // ABOUT & CONTACT
        // ======================================================
        public IActionResult About()
        {
            ForceLayoutForRole(null);
            return View();
        }

        public IActionResult Contact()
        {
            ForceLayoutForRole(null);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(string name, string email, string message)
        {
            ForceLayoutForRole(null);

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                TempData.SetAlert("error", "Please fill in all required fields.", "Contact Form");
                return View();
            }

            try
            {
                var client = httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(config["ApiSettings:BaseUrl"]);

                var payload = new { Name = name, Email = email, Message = message };
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("contact/send", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseBody);

                if (response.IsSuccessStatusCode && result?.success == true)
                {
                    TempData.SetAlert("success", (string)result.message ?? "Message sent successfully!", "Contact Us");
                    ModelState.Clear();
                }
                else
                {
                    TempData.SetAlert("error", (string)result.error ?? "Failed to send message.", "Contact Us");
                }
            }
            catch (Exception ex)
            {
                TempData.SetAlert("error", $"An error occurred: {ex.Message}", "Contact Us");
            }

            return View();
        }

        // ======================================================
        // 🔐 LOGIN
        // ======================================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            ForceLayoutForRole(null);
            return View(new LoginRequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequestViewModel model)
        {
            ForceLayoutForRole(null);

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("error", "Please fill in all required fields.", "Login Failed");
                return View(model);
            }

            var result = await authService.LoginAsync(model);

            if (result == null || !result.Success || result.Data == null)
            {
                TempData.SetAlert("error", result?.Error ?? "Invalid credentials.", "Login Failed");
                return View(model);
            }

            // Store session
            sessionManager.SetSession(
                result.Data.Token,
                result.Data.Role,
                result.Data.UserId,
                result.Data.DisplayName,
                result.Data.Email,
                result.Data.ProfileImageUrl
            );

            TempData.SetAlert("success", $"Welcome {result.Data.DisplayName}", "Login Successful");

            return result.Data.Role switch
            {
                "Admin" => RedirectToAction("AdminDashboard", "Admin"),
                "HR" => RedirectToAction("Dashboard", "HR"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // ======================================================
        // 🚪 LOGOUT
        // ======================================================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await authService.LogoutAsync();
            sessionManager.Clear();
            TempData.SetAlert("info", "You have been logged out.", "Session Ended");
            return RedirectToAction("Login");
        }

        // ======================================================
        // 📝 REQUEST ACCESS
        // ======================================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RequestAccess()
        {
            ForceLayoutForRole(null);
            return View(new SignUpViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> RequestAccess(SignUpViewModel model)
        {
            ForceLayoutForRole(null);

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("error", "Please complete the form correctly.", "Access Request Failed");
                return View(model);
            }

            var response = await authService.RequestAccessAsync(model);
            if (response.Success)
            {
                TempData.SetAlert("success", "Your access request has been submitted successfully.", "Request Sent");
                return RedirectToAction("Login");
            }

            TempData.SetAlert("error", response.Error ?? "Submission failed.", "Access Request");
            return View(model);
        }

        // ======================================================
        // 🔍 EMPLOYEE EXIT REVIEW CHECK
        // ======================================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ViewReview(string? reviewId)
        {
            ForceLayoutForRole(null);

            if (string.IsNullOrWhiteSpace(reviewId))
            {
                TempData.SetAlert("info", "Enter your Review ID to view progress.", "Check Review");
                return RedirectToAction("Index");
            }

            return RedirectToAction("ReviewDetails", new { id = reviewId });
        }

        [HttpGet("review/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ReviewDetails(string id)
        {
            ForceLayoutForRole(null);

            try
            {
                var client = httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(config["ApiSettings:BaseUrl"]);

                var response = await client.GetAsync($"review/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData.SetAlert("error", "Review not found or access denied.", "Invalid ID");
                    return RedirectToAction("Index");
                }

                var json = await response.Content.ReadAsStringAsync();
                var reviewResponse = JsonConvert.DeserializeObject<ApiResponseViewModel<ReviewViewModel>>(json);

                if (reviewResponse?.Success != true || reviewResponse.Data == null)
                {
                    TempData.SetAlert("error", "Unable to load review details.", "Error");
                    return RedirectToAction("Index");
                }

                return View("ReviewDetails", reviewResponse.Data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching review details for ID: {ReviewId}", id);
                TempData.SetAlert("error", "Something went wrong while fetching your review.", "Server Error");
                return RedirectToAction("Index");
            }
        }

        // ======================================================
        // PRIVACY & ERROR
        // ======================================================
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
