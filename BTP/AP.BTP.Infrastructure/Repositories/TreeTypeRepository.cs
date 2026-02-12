using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AP.BTP.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AP.BTP.Infrastructure.Repositories
{
    public class TreeTypeRepository : GenericRepository<TreeType>, ITreeTypeRepository
    {
        private readonly BTPContext context;
        public TreeTypeRepository(BTPContext context) : base(context)
        {
            this.context = context;
        }
        public async Task<TreeType> GetByName(string name)
        {
            return await context.TreeTypes.FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<TreeType>> GetAll(int pageNr, int pageSize)
        {
            return await context.TreeTypes
                .Include(t => t.TreeImages)
                .Include(t => t.Instructions)
                .Skip((pageNr - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<TreeType> GetByIdWithIncludes(int id)
        {
            return await context.TreeTypes
                .Include(t => t.TreeImages)
                .Include(t => t.Instructions)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<TreeType>> GetByArchiveStatus(bool isArchived, int pageNr, int pageSize)
        {
            return await context.TreeTypes
                .Where(t => t.IsArchived == isArchived)
                .Include(t => t.TreeImages)
                .Include(t => t.Instructions)
                .OrderBy(t => t.Name)
                .Skip((pageNr - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }


        public async Task<int> CountByArchiveStatus(bool isArchived)
        {
            return await context.TreeTypes.CountAsync(t => t.IsArchived == isArchived);
        }

    }
}
