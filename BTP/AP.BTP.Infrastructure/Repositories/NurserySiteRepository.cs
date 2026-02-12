using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AP.BTP.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AP.BTP.Infrastructure.Repositories
{
    public class NurserySiteRepository : GenericRepository<NurserySite>, INurserySiteRepository
    {
        private readonly BTPContext _context;

        public NurserySiteRepository(BTPContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NurserySite>> GetAll(int pageNr, int pageSize)
        {
            return await _context.NurserySites
                .Include(ns => ns.GroundPlan)
                .Include(ns => ns.Address)
                .Include(ns => ns.Zones)
                .OrderBy(ns => ns.Name)
                .Skip((pageNr - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<NurserySite>> GetByArchiveStatus(bool isArchived, int pageNr, int pageSize)
        {
            return await _context.NurserySites
                .Include(ns => ns.GroundPlan)
                .Include(ns => ns.Address)
                .Include(ns => ns.Zones)
                .Where(ns => ns.IsArchived == isArchived)
                .OrderBy(ns => ns.Name)
                .Skip((pageNr - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<NurserySite> GetByIdWithDetails(int id)
        {
            return await _context.NurserySites
                .Include(ns => ns.Address)
                .Include(ns => ns.GroundPlan)
                .Include(ns => ns.Zones)
                .FirstOrDefaultAsync(ns => ns.Id == id);
        }
    }
}
