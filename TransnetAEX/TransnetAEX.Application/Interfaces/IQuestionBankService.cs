using TransnetAEX.Domain.Entities;

namespace TransnetAEX.Application.Interfaces
{
    public interface IQuestionBankService
    {
        Task<List<QuestionBank>> GetQuestionsBySectionAsync(string sectionType);
        Task<QuestionBank> AddQuestionAsync(QuestionBank question);
        Task<bool> DeactivateQuestionAsync(int id);
    }
}
