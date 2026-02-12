using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AP.BTP.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AP.BTP.Infrastructure.Repositories
{
    public class ZoneRepository : GenericRepository<Zone>, IZoneRepository
    {
        private readonly BTPContext context;
        public ZoneRepository(BTPContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<Zone?> GetByIdWithNurserySite(int zoneId)
        {
            return await context.Zones
                .Include(z => z.NurserySite)
                .FirstOrDefaultAsync(z => z.Id == zoneId);
        }

    }
}
