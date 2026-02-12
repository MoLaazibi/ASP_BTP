using AP.BTP.Domain;

namespace AP.BTP.Application.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<IEnumerable<Category>> GetByArchiveStatus(bool isArchived, int pageNr, int pageSize);
    }
}
