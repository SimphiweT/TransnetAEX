using Microsoft.EntityFrameworkCore;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories;
using TransnetAEX.Infrastructure.Data;
using TransnetAEX.Infrastructure.EF_Repositories.Base;

namespace TransnetAEX.Infrastructure.EF_Repositories
{
    public class QuestionBankRepository : RepositoryBase<QuestionBank>, IQuestionBankRepository
    {
        public QuestionBankRepository(AppDbContext dbContext) : base(dbContext) { }

        public async Task<List<QuestionBank>> GetBySectionTypeAsync(string sectionType)
        {
            return await appDbContext.QuestionBanks
                .Where(q => q.SectionType == sectionType && q.IsActive)
                .OrderBy(q => q.QuestionId)
                .ToListAsync();
        }
    }
}
