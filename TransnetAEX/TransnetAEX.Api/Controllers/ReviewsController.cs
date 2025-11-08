using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransnetAEX.Api.DTOs.Review;
using TransnetAEX.Api.DTOs.Shared;
using TransnetAEX.Application.Helpers;
using TransnetAEX.Application.Interfaces;

namespace TransnetAEX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // ===============================================================
        // GET ALL REVIEWS
        // ===============================================================
        [HttpGet]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.HR_ROLE}")]
        public async Task<IActionResult> GetAllReviews()
        {
            try
            {
                var reviews = await _reviewService.GetAllReviewsAsync();
                return Ok(ApiResponseDTO<object>.SuccessResponse(reviews, "All reviews retrieved successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseDTO<object>.FailResponse(ex.Message, "Failed to retrieve reviews."));
            }
        }

        // ===============================================================
        // GET REVIEW BY ID
        // ===============================================================
        [HttpGet("{id:int}")]
        //[Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.HR_ROLE}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewById(int id)
        {
            try
            {
                var review = await _reviewService.GetReviewByIdAsync(id);
                if (review == null)
                    return NotFound(ApiResponseDTO<object>.FailResponse("NotFound", "Review not found."));

                return Ok(ApiResponseDTO<object>.SuccessResponse(review, "Review retrieved successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseDTO<object>.FailResponse(ex.Message, "Failed to retrieve review."));
            }
        }

        // ===============================================================
        // INITIATE REVIEW (HR)
        // ===============================================================
        [HttpPost("initiate")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.HR_ROLE}")]
        public async Task<IActionResult> InitiateReview([FromBody] ReviewCreateRequestDto dto)
        {
            if (dto == null)
                return BadRequest(ApiResponseDTO<object>.FailResponse("Invalid data", "Invalid review data."));

            try
            {
                var review = await _reviewService.InitiateReviewAsync(dto.HrEmail, dto.EmployeeEmail, dto.LineManagerEmail);
                return Ok(ApiResponseDTO<object>.SuccessResponse(review, "Review initiated successfully. Employee has been notified."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseDTO<object>.FailResponse(ex.Message, "Failed to initiate review."));
            }
        }

        // ===============================================================
        // EMPLOYEE SUBMIT
        // ===============================================================
        [HttpPut("{id:int}/employee-submit")]
        [AllowAnonymous]
        public async Task<IActionResult> EmployeeSubmit(int id)
        {
            try
            {
                var success = await _reviewService.EmployeeSubmitAsync(id);
                if (!success)
                    return NotFound(ApiResponseDTO<object>.FailResponse("NotFound", "Review not found or already processed."));

                return Ok(ApiResponseDTO<object>.SuccessResponse(null, "Employee submission recorded. Line Manager has been notified."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseDTO<object>.FailResponse(ex.Message, "Error processing employee submission."));
            }
        }

        // ===============================================================
        // LINE MANAGER SUBMIT
        // ===============================================================
        [HttpPut("{id:int}/manager-submit")]
        [AllowAnonymous]
        public async Task<IActionResult> ManagerSubmit(int id)
        {
            try
            {
                var success = await _reviewService.ManagerSubmitAsync(id);
                if (!success)
                    return NotFound(ApiResponseDTO<object>.FailResponse("NotFound", "Review not found or already completed."));

                return Ok(ApiResponseDTO<object>.SuccessResponse(null, "Manager submission recorded. HR has been notified."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseDTO<object>.FailResponse(ex.Message, "Error processing manager submission."));
            }
        }

        // ===============================================================
        // MANUAL COMPLETE (HR)
        // ===============================================================
        [HttpPut("{id:int}/complete")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.HR_ROLE}")]
        public async Task<IActionResult> CompleteReview(int id)
        {
            try
            {
                var success = await _reviewService.MarkReviewCompletedAsync(id);
                if (!success)
                    return NotFound(ApiResponseDTO<object>.FailResponse("NotFound", "Review not found or already completed."));

                return Ok(ApiResponseDTO<object>.SuccessResponse(null, $"Review {id} marked as completed."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseDTO<object>.FailResponse(ex.Message, "Failed to complete review."));
            }
        }

        // ===============================================================
        // GET-INFO
        // ===============================================================

        [HttpGet("my-info")]
        public IActionResult GetMyInfo()
        {
            var email = UserContextHelper.GetUserEmail(HttpContext);
            var name = UserContextHelper.GetUserDisplayName(HttpContext);
            var roles = UserContextHelper.GetUserRoles(HttpContext);

            return Ok(new { email, name, roles });
        }
    }
}
