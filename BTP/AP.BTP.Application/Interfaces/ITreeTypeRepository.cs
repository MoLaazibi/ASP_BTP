using AP.BTP.Domain;

namespace AP.BTP.Application.Interfaces
{
    public interface ITreeTypeRepository : IGenericRepository<TreeType>
    {
        Task<TreeType> GetByName(string name);
        Task<TreeType> GetByIdWithIncludes(int id);
        Task<IEnumerable<TreeType>> GetAll(int pageNr, int pageSize);
        Task<IEnumerable<TreeType>> GetByArchiveStatus(bool isArchived, int pageNr, int pageSize);
        Task<int> CountByArchiveStatus(bool isArchived);
    }
}
