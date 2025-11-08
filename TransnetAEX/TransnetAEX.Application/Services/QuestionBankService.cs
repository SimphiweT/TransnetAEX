using TransnetAEX.Application.Interfaces;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories.Base;

namespace TransnetAEX.Application.Services
{
    public class QuestionBankService : IQuestionBankService
    {
        private readonly IRepositoryWrapper _repo;

        public QuestionBankService(IRepositoryWrapper repo)
        {
            _repo = repo;
        }

        public async Task<List<QuestionBank>> GetQuestionsBySectionAsync(string sectionType)
        {
            return await _repo.Question.GetBySectionTypeAsync(sectionType);
        }

        public async Task<QuestionBank> AddQuestionAsync(QuestionBank question)
        {
            await _repo.Question.CreateAsync(question);
            await _repo.SaveAsync();
            return question;
        }

        public async Task<bool> DeactivateQuestionAsync(int id)
        {
            var question = await _repo.Question.GetByIdAsync(id);
            if (question == null) return false;

            question.IsActive = false;
            question.UpdatedDate = DateTime.UtcNow;

            await _repo.Question.UpdateAsync(question);
            await _repo.SaveAsync();
            return true;
        }
    }
}
