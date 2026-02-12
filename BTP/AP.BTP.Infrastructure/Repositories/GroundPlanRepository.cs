using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AP.BTP.Infrastructure.Contexts;

namespace AP.BTP.Infrastructure.Repositories
{
    public class GroundPlanRepository : GenericRepository<GroundPlan>, IGroundPlanRepository
    {
        public GroundPlanRepository(BTPContext context) : base(context)
        {
        }
    }
}
