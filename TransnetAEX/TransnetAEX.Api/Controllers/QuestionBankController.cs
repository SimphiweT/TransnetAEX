using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransnetAEX.Api.DTOs.Question;
using TransnetAEX.Application.Helpers;
using TransnetAEX.Application.Interfaces;
using TransnetAEX.Domain.Entities;

namespace TransnetAEX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionBankController : ControllerBase
    {
        private readonly IQuestionBankService _questionService;

        public QuestionBankController(IQuestionBankService questionService)
        {
            _questionService = questionService;
        }

        // -------------------- GET: api/questionbank/{sectionType} --------------------
        [HttpGet("{sectionType}")]
        //[Authorize(Roles = "Admin,HR,LineManager,Employee")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.HR_ROLE}")]
        public async Task<IActionResult> GetQuestionsBySection(string sectionType)
        {
            var questions = await _questionService.GetQuestionsBySectionAsync(sectionType);
            return Ok(questions);
        }

        // -------------------- POST: api/questionbank --------------------
        [HttpPost]
        //[Authorize(Roles = "Admin,HR")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.HR_ROLE}")]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newQuestion = new QuestionBank
            {
                SectionType = dto.SectionType,
                QuestionText = dto.QuestionText,
                InputType = dto.InputType,
                Options = dto.Options,
                IsRequired = dto.IsRequired,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _questionService.AddQuestionAsync(newQuestion);
            return CreatedAtAction(nameof(GetQuestionsBySection), new { sectionType = dto.SectionType }, created);
        }

        // -------------------- PUT: api/questionbank/{id}/deactivate --------------------
        [HttpPut("{id:int}/deactivate")]
        //[Authorize(Roles = "Admin,HR")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.HR_ROLE}")]
        public async Task<IActionResult> DeactivateQuestion(int id)
        {
            var success = await _questionService.DeactivateQuestionAsync(id);
            if (!success)
                return NotFound(new { Message = "Question not found." });

            return Ok(new { Message = "Question deactivated successfully." });
        }
    }
}
