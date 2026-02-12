using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AP.BTP.Infrastructure.Contexts;

namespace AP.BTP.Infrastructure.Repositories
{
    public class TreeImageRepository : GenericRepository<TreeImage>, ITreeImageRepository
    {
        public TreeImageRepository(BTPContext context) : base(context) { }
    }
}

