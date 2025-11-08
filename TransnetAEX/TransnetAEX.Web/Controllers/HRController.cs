using AngleSharp.Dom;
using Microsoft.AspNetCore.Mvc;
using TransnetAEX.Web.Api_Services.IServices;
using TransnetAEX.Web.Helpers;
using TransnetAEX.Web.Models.Admin;
using TransnetAEX.Web.Models.Auth;
using TransnetAEX.Web.Models.HR;

namespace TransnetAEX.Web.Controllers
{
    public class HrController : BaseController
    {
        private readonly IAuthService authService;
        private readonly IUserService userService;
        private readonly IAccessService accessService;

        public HrController(
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
        public async Task<IActionResult> HRDashboard()
        {
            ForceHRLayout();

            var users = await userService.GetAllUsersAsync();

            var stats = new StaffDashboardViewModel
            {
                TotalReviewComplete = 0,
                TotalReviewIncomplete = 0,
                TotalReviewPending = 0,
                TotaEmployeeQuestions =  0,
                TotaLineManagerQuestions =  0
            };

            return View(stats);
        }

        /*====================================================================================================================================
        *** PROFILE ***
       ======================================================================================================================================*/
        #region PROFILE

        [HttpGet]
        public async Task<IActionResult> ManageProfile()
        {
            ForceHRLayout();

            var userId = sessionManager.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                TempData.SetAlert("error", "Session expired. Please log in again.", "Profile");
                return RedirectToAction("Login", "Home");
            }

            var result = await userService.GetUserByIdAsync(userId);
            if (result == null || !result.Success || result.Data == null)
            {
                TempData.SetAlert("error", "Unable to load your profile.", "Profile");
                return RedirectToAction(nameof(HRDashboard));
            }

            var vm = new DetailsAndUpdateUserViewModel
            {
                UserDetails = result.Data,
                UpdateUser = new UpdateUserViewModel
                {
                    UserId = result.Data.UserId!,
                    UsernameDisplay = result.Data.UsernameDisplay,
                    UserEmail = result.Data.UserEmail,
                    StudentStaffNumber = result.Data.StudentStaffNumber,
                    ContactNumber = result.Data.ContactNumber,
                    Description = result.Data.Description,
                    UserRole = result.Data.UserRole
                }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageProfile(UpdateUserViewModel model)
        {
            ForceHRLayout();

            // Always ensure the UserId is present
            if (string.IsNullOrWhiteSpace(model.UserId))
                model.UserId = sessionManager.GetUserId() ?? string.Empty;

            if (!ModelState.IsValid)
            {
                // Re-fetch details so the left card is filled
                var details = await userService.GetUserByIdAsync(model.UserId);
                var vm = new DetailsAndUpdateUserViewModel
                {
                    UserDetails = details.Success ? details.Data : new UserViewModel
                    {
                        UserId = model.UserId,
                        UsernameDisplay = model.UsernameDisplay,
                        UserEmail = model.UserEmail,
                        StudentStaffNumber = model.StudentStaffNumber,
                        ContactNumber = model.ContactNumber,
                        Description = model.Description,
                        UserRole = sessionManager.GetRole()
                    },
                    UpdateUser = model
                };

                TempData.SetAlert("warning", "Please correct highlighted errors.", "Profile");
                return View(vm);
            }

            var result = await userService.UpdateUserAsync(model);

            if (result.Success)
            {
                TempData.SetAlert("success", "Profile updated successfully!", "Profile");

                // keep header/avatar in sync
                sessionManager.SetUserDisplayName(model.UsernameDisplay ?? "");
                if (!string.IsNullOrWhiteSpace(result.Data?.ProfileImageFileName))
                    sessionManager.SetProfilePictureUrl(result.Data.ProfileImageFileName);

                return RedirectToAction(nameof(ManageProfile));
            }

            // ⬇️ On API error: still show details on the left
            var current = await userService.GetUserByIdAsync(model.UserId);
            var backVm = new DetailsAndUpdateUserViewModel
            {
                UserDetails = current.Success ? current.Data : new UserViewModel
                {
                    UserId = model.UserId,
                    UsernameDisplay = model.UsernameDisplay,
                    UserEmail = model.UserEmail,
                    StudentStaffNumber = model.StudentStaffNumber,
                    ContactNumber = model.ContactNumber,
                    Description = model.Description,
                    UserRole = sessionManager.GetRole()
                },
                UpdateUser = model
            };

            TempData.SetAlert("error", result.Error ?? "Failed to update profile.", "Profile");
            return View(backVm);
        }



        #endregion

        /*====================================================================================================================================
                                                        *** CHANGE PASSWORD ***
        ======================================================================================================================================*/
        #region CHANGE PASSWORD

        [HttpGet]
        public IActionResult ChangePassword()
        {
            ForceHRLayout();

            var vm = new ChangePasswordViewModel
            {
                UserId = sessionManager.GetUserId()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            ForceHRLayout();

            if (!ModelState.IsValid)
            {
                TempData.SetAlert("warning", "Please correct the highlighted errors.", "Change Password");
                return View(model);
            }

            var result = await userService.ChangePasswordAsync(model);

            if (result.Success)
            {
                TempData.SetAlert("success", "Password changed successfully! Please log in again.", "Account");
                await authService.LogoutAsync(); // clear session if needed
                return RedirectToAction("Login", "Home");
            }

            TempData.SetAlert("error", result.Error ?? "Failed to change password. Please try again.", "Account");
            return View(model);
        }

        #endregion
        /*====================================================================================================================
                                LOGOUT
        ====================================================================================================================*/
        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            ForceHRLayout();

            var result = await authService.LogoutAsync();
            sessionManager.Clear();

            var msg = result.Success ? "You have been signed out successfully." : "Session ended.";
            TempData.SetAlert("info", msg, "Session Ended");

            return RedirectToAction("Login", "Home");
        }

        /*====================================================================================================================
                                HELPERS
        ====================================================================================================================*/
        private void ForceHRLayout()
        {
            var redirect = RequireRole("HR");
            if (redirect != null)
                RedirectToAction(redirect.ToString());
            ForceLayout();
        }
        /*====================================================================================================================================
                                *** EXPORT DASHBOARD  ***
       =====================================================================================================================================*/

        #region Export Dashboard
        [HttpGet]

        public async Task<IActionResult> ExportDashboardExcel()
        {
            ForceHRLayout();

            // Get dashboard data
            var assetsResponse = await assetService.GetAllAssetsAsync();
            var categoriesResponse = await categoryService.GetAllCategoriesWithAssetsAsync();
            var roomsResponse = await roomService.GetAllRoomsAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();

            // ===== SUMMARY SHEET =====
            var summary = workbook.Worksheets.Add("Summary");
            summary.Cell("A1").Value = "Dashboard Summary";
            summary.Range("A1:D1").Merge().Style
                .Font.SetBold().Font.FontSize = 16;
            summary.Cell("A3").Value = "Total Assets";
            summary.Cell("B3").Value = assetsResponse.Data?.Count ?? 0;
            summary.Cell("A4").Value = "Total Categories";
            summary.Cell("B4").Value = categoriesResponse.Data?.Count ?? 0;
            summary.Cell("A5").Value = "Total Rooms";
            summary.Cell("B5").Value = roomsResponse.Data?.Count ?? 0;

            // ===== CATEGORY CHART DATA =====
            var wsCategory = workbook.Worksheets.Add("Assets by Category");
            wsCategory.Cell(1, 1).Value = "Category";
            wsCategory.Cell(1, 2).Value = "Asset Count";

            int row = 2;
            foreach (var c in categoriesResponse.Data ?? new())
            {
                wsCategory.Cell(row, 1).Value = c.AssetCategoryName ?? "Uncategorized";
                wsCategory.Cell(row, 2).Value = c.Assets?.Count ?? 0;
                row++;
            }

            wsCategory.Columns().AdjustToContents();

            // ===== ROOM CHART DATA =====
            var wsRoom = workbook.Worksheets.Add("Assets by Room");
            wsRoom.Cell(1, 1).Value = "Room";
            wsRoom.Cell(1, 2).Value = "Asset Count";
            row = 2;
            foreach (var r in roomsResponse.Data ?? new())
            {
                wsRoom.Cell(row, 1).Value = r.RoomName ?? "Unnamed Room";
                wsRoom.Cell(row, 2).Value = r.Assets?.Count ?? 0;
                row++;
            }

            wsRoom.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"StaffDashboard_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }
        [HttpGet]
        public async Task<IActionResult> ExportDashboardPdf()
        {

            ForceHRLayout();
            var assetsResponse = await assetService.GetAllAssetsAsync();
            var categoriesResponse = await categoryService.GetAllCategoriesWithAssetsAsync();
            var roomsResponse = await roomService.GetAllRoomsAsync();

            using var stream = new MemoryStream();
            using (var writer = new IText.Kernel.Pdf.PdfWriter(stream))
            using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
            {
                var document = new iText.Layout.Document(pdf);
                var pageSize = pdf.GetDefaultPageSize();
                var width = pageSize.GetWidth();

                // ==== FONTS ====
                var fontRegular = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                var fontBold = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

                // ==== COLORS ====
                var blue = new iText.Kernel.Colors.DeviceRgb(40, 123, 255);   // Kovsie Blue
                var peach = new iText.Kernel.Colors.DeviceRgb(255, 182, 123); // Kovsie Peach
                var white = iText.Kernel.Colors.ColorConstants.WHITE;

                // ==== HEADER BAR (Gradient Effect) ====
                var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(pdf.AddNewPage());
                var gradient = new iText.Kernel.Pdf.Extgstate.PdfExtGState();
                gradient.SetFillOpacity(0.9f);
                canvas.SaveState()
                      .SetExtGState(gradient)
                      .SetFillColor(blue)
                      .Rectangle(0, pageSize.GetTop() - 60, width, 60)
                      .Fill()
                      .RestoreState();

                // ==== ADD LOGO (Optional) ====
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "icon_logo_transparent.png");
                if (System.IO.File.Exists(logoPath))
                {
                    var logo = new iText.Layout.Element.Image(iText.IO.Image.ImageDataFactory.Create(logoPath))
                        .ScaleToFit(60, 60)
                        .SetFixedPosition(40, pageSize.GetTop() - 55);
                    document.Add(logo);
                }

                // ==== TITLE ON HEADER ====
                var title = new iText.Layout.Element.Paragraph("KOVSIE ASSETS TRACKER")
                    .SetFont(fontBold)
                    .SetFontSize(18)
                    .SetFontColor(white)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFixedPosition(0, pageSize.GetTop() - 45, width);
                document.Add(title);

                // === ADD SPACING BELOW HEADER ===
                document.Add(new iText.Layout.Element.Paragraph("\n\n\n"));

                // ==== SUMMARY SECTION ====
                document.Add(new iText.Layout.Element.Paragraph("Dashboard Summary")
                    .SetFont(fontBold)
                    .SetFontSize(14)
                    .SetFontColor(blue)
                    .SetMarginBottom(8));

                document.Add(new iText.Layout.Element.Paragraph(
                    $"Total Assets: {assetsResponse.Data?.Count ?? 0}\n" +
                    $"Total Categories: {categoriesResponse.Data?.Count ?? 0}\n" +
                    $"Total Rooms: {roomsResponse.Data?.Count ?? 0}\n"
                ).SetFont(fontRegular).SetFontSize(12)
                 .SetMarginBottom(20));

                // ==== CATEGORY TABLE ====
                document.Add(new iText.Layout.Element.Paragraph("Assets by Category")
                    .SetFont(fontBold)
                    .SetFontSize(13)
                    .SetFontColor(blue)
                    .SetMarginBottom(5));

                var categoryTable = new iText.Layout.Element.Table(new float[] { 3, 2 }).UseAllAvailableWidth();
                categoryTable.AddHeaderCell(new iText.Layout.Element.Cell()
                    .Add(new iText.Layout.Element.Paragraph("Category").SetFont(fontBold).SetFontColor(white))
                    .SetBackgroundColor(blue).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                categoryTable.AddHeaderCell(new iText.Layout.Element.Cell()
                    .Add(new iText.Layout.Element.Paragraph("Asset Count").SetFont(fontBold).SetFontColor(white))
                    .SetBackgroundColor(blue).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                foreach (var c in categoriesResponse.Data ?? new())
                {
                    categoryTable.AddCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(c.AssetCategoryName ?? "Uncategorized").SetFont(fontRegular)));
                    categoryTable.AddCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph((c.Assets?.Count ?? 0).ToString()).SetFont(fontRegular))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                }

                document.Add(categoryTable);
                document.Add(new iText.Layout.Element.Paragraph("\n"));

                // ==== ROOM TABLE ====
                document.Add(new iText.Layout.Element.Paragraph("Assets by Room")
                    .SetFont(fontBold)
                    .SetFontSize(13)
                    .SetFontColor(blue)
                    .SetMarginBottom(5));

                var roomTable = new iText.Layout.Element.Table(new float[] { 3, 2 }).UseAllAvailableWidth();
                roomTable.AddHeaderCell(new iText.Layout.Element.Cell()
                    .Add(new iText.Layout.Element.Paragraph("Room").SetFont(fontBold).SetFontColor(white))
                    .SetBackgroundColor(blue).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                roomTable.AddHeaderCell(new iText.Layout.Element.Cell()
                    .Add(new iText.Layout.Element.Paragraph("Asset Count").SetFont(fontBold).SetFontColor(white))
                    .SetBackgroundColor(blue).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                foreach (var r in roomsResponse.Data ?? new())
                {
                    roomTable.AddCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(r.RoomName ?? "Unnamed Room").SetFont(fontRegular)));
                    roomTable.AddCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph((r.Assets?.Count ?? 0).ToString()).SetFont(fontRegular))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                }

                document.Add(roomTable);

                // ==== FOOTER ====
                document.Add(new iText.Layout.Element.Paragraph(
                    $"\nReport Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .SetFont(fontRegular)
                    .SetFontSize(10)
                    .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginTop(30));

                document.Close();
            }

            return File(stream.ToArray(), "application/pdf", $"StaffDashboard_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
        #endregion
    }
}
