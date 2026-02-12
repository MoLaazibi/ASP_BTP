using AP.BTP.Domain;

namespace AP.BTP.Application.Interfaces
{
    public interface INurserySiteRepository : IGenericRepository<NurserySite>
    {
        public Task<NurserySite> GetByIdWithDetails(int id);
        public Task<IEnumerable<NurserySite>> GetByArchiveStatus(bool isArchived, int pageNr, int pageSize);
    }
}
