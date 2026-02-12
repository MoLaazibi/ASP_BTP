using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AP.BTP.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AP.BTP.Infrastructure.Repositories
{
    public class InstructionRepository : GenericRepository<Instruction>, IInstructionRepository
    {
        private readonly BTPContext _context;

        public InstructionRepository(BTPContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Instruction?> GetLatestByTreeTypeAndSeason(int treeTypeId, string season)
        {
            return await _context.Set<Instruction>()
                .Where(i => i.TreeTypeId == treeTypeId && i.Season == season)
                .OrderByDescending(i => i.UploadTime)
                .FirstOrDefaultAsync();
        }
    }
}
