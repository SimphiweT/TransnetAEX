using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories.Base;

namespace TransnetAEX.Domain.IRepositories
{
    public interface IQuestionBankRepository : IRepositoryBase<QuestionBank>
    {
        Task<List<QuestionBank>> GetBySectionTypeAsync(string sectionType);
    }
}
