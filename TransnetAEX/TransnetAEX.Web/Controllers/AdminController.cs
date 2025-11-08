using Microsoft.AspNetCore.Mvc;
using TransnetAEX.Web.Api_Services.IServices;
using TransnetAEX.Web.Helpers;
using TransnetAEX.Web.Models.Auth;
using TransnetAEX.Web.Models.Admin;

namespace TransnetAEX.Web.Controllers
{
    public class AdminController : BaseController
    {
        private readonly IAuthService authService;
        private readonly IUserService userService;
        private readonly IAccessService accessService;

        public AdminController(
            IAuthService _authService,
            IUserService _userService,
            IAccessService _accessService,
            SessionManager _sessionManager)
            : base(_sessionManager)
        {
            authService = _authService;
            userService = _userService;
            accessService = _accessService;
        }

        /*====================================================================================================================
                                        DASHBOARD
        ====================================================================================================================*/
        [HttpGet]
        public async Task<IActionResult> AdminDashboard()
        {
            ForceAdminLayout();

            var users = await userService.GetAllUsersAsync();

            var stats = new DashboardStatsViewModel
            {
                TotalUsers = users.Data?.Count ?? 0,
                TotalReviewPending = 0,
                TotalReviewComplete = 0,
                TotalReviewIncomplete = 0
            };

            return View(stats);
        }

        /*====================================================================================================================
                                        MANAGE USERS
        ====================================================================================================================*/
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            ForceAdminLayout();
            var result = await userService.GetAllUsersAsync();
            var vm = new CreateAndListUserViewModel
            {
                Users = result.Data ?? new List<UserViewModel>()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUsers(CreateAndListUserViewModel model)
        {
            ForceAdminLayout();

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("error", "Invalid form data.", "Manage Users");
                return View(model);
            }

            var result = await userService.RegisterUserAsync(model.CreateUser);

            if (result.Success)
            {
                TempData.SetAlert("success", "User created successfully!", "Manage Users");
                return RedirectToAction(nameof(ManageUsers));
            }

            TempData.SetAlert("error", result.Error ?? "Failed to create user.", "Manage Users");
            model.Users = (await userService.GetAllUsersAsync()).Data ?? new List<UserViewModel>();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ViewAndUpdateUserDetails(string userId)
        {
            ForceAdminLayout();

            if (string.IsNullOrEmpty(userId))
            {
                TempData.SetAlert("error", "User ID missing.", "Manage Users");
                return RedirectToAction(nameof(ManageUsers));
            }

            var result = await userService.GetUserByIdAsync(userId);
            if (!result.Success || result.Data == null)
            {
                TempData.SetAlert("error", "User not found.", "Manage Users");
                return RedirectToAction(nameof(ManageUsers));
            }

            var vm = new DetailsAndUpdateUserViewModel
            {
                UserDetails = result.Data,
                UpdateUser = new UpdateUserViewModel
                {
                    UserId = result.Data.UserId,
                    UsernameDisplay = result.Data.UsernameDisplay,
                    UserEmail = result.Data.UserEmail,
                    ContactNumber = result.Data.ContactNumber,
                    StudentStaffNumber = result.Data.StudentStaffNumber,
                    Description = result.Data.Description,
                    UserRole = result.Data.UserRole
                }
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ViewAndUpdateUserDetails(UpdateUserViewModel model)
        {
            ForceAdminLayout();

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("warning", "Please correct highlighted fields.", "Manage Users");
                var existing = await userService.GetUserByIdAsync(model.UserId);
                return View(new DetailsAndUpdateUserViewModel
                {
                    UserDetails = existing.Data,
                    UpdateUser = model
                });
            }

            var update = await userService.UpdateUserAsync(model);
            if (update.Success)
            {
                TempData.SetAlert("success", "User updated successfully!", "Manage Users");
                return RedirectToAction(nameof(ViewAndUpdateUserDetails), new { userId = model.UserId });
            }

            TempData.SetAlert("error", update.Error ?? "Failed to update user.", "Manage Users");
            return RedirectToAction(nameof(ViewAndUpdateUserDetails), new { userId = model.UserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            ForceAdminLayout();

            var result = await userService.DeleteUserAsync(userId);
            if (result.Success)
                TempData.SetAlert("success", "User deleted successfully!", "Manage Users");
            else
                TempData.SetAlert("error", result.Error ?? "Failed to delete user.", "Manage Users");

            return RedirectToAction(nameof(ManageUsers));
        }

        /*====================================================================================================================
                                        MANAGE ACCESS REQUESTS
        ====================================================================================================================*/
        [HttpGet]
        public async Task<IActionResult> ManageAccessRequests()
        {
            ForceAdminLayout();

            var result = await accessService.GetAllAccessRequestsAsync();
            if (!result.Success)
            {
                TempData.SetAlert("error", result.Error ?? "Unable to fetch access requests.", "Access Requests");
                return View(new List<AccessResponseViewModel>());
            }

            return View(result.Data ?? new List<AccessResponseViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> ApproveAccessRequest(int id)
        {
            ForceAdminLayout();
            var result = await accessService.ApproveAccessRequestAsync(id);

            if (result.Success)
                TempData.SetAlert("success", "Access request approved successfully!", "Access Requests");
            else
                TempData.SetAlert("error", result.Error ?? "Failed to approve access request.", "Access Requests");

            return RedirectToAction(nameof(ManageAccessRequests));
        }

        [HttpPost]
        public async Task<IActionResult> DeclineAccessRequest(int id, string reason)
        {
            ForceAdminLayout();
            var result = await accessService.DeclineAccessRequestAsync(id, reason);

            if (result.Success)
                TempData.SetAlert("success", "Access request declined successfully!", "Access Requests");
            else
                TempData.SetAlert("error", result.Error ?? "Failed to decline request.", "Access Requests");

            return RedirectToAction(nameof(ManageAccessRequests));
        }

        /*====================================================================================================================
                                        LOGOUT
        ====================================================================================================================*/
        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            ForceAdminLayout();

            var result = await authService.LogoutAsync();
            sessionManager.Clear();

            var msg = result.Success ? "You have been signed out successfully." : "Session ended.";
            TempData.SetAlert("info", msg, "Session Ended");

            return RedirectToAction("Login", "Home");
        }

        /*====================================================================================================================
                                        HELPERS
        ====================================================================================================================*/
        private void ForceAdminLayout()
        {
            var redirect = RequireRole("Admin");
            if (redirect != null)
                RedirectToAction(redirect.ToString());
            ForceLayout();
        }
    }
}
