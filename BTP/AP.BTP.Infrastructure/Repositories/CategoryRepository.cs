using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AP.BTP.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AP.BTP.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly BTPContext context;
        public CategoryRepository(BTPContext context) : base(context)
        {
            this.context = context;
        }
        public async Task<IEnumerable<Category>> GetByArchiveStatus(bool isArchived, int pageNr, int pageSize)
        {
            return await context.Categories
                .Where(c => c.IsArchived == isArchived)
                .Skip((pageNr - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
