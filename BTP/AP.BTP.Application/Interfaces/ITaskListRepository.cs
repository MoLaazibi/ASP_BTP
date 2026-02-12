using AP.BTP.Domain;

namespace AP.BTP.Application.Interfaces
{
    public interface ITaskListRepository : IGenericRepository<TaskList>
    {
        TaskList GetByDate(DateTime date);
        Task<TaskList> GetByUserIdAndDate(int userId, DateTime date);
        Task<TaskList> GetByIdWithDetails(int id);
        Task<IEnumerable<TaskList>> GetByUserId(int userId);
        Task<IEnumerable<TaskList>> GetByUserIdAndDateRange(int userId, DateTime start, DateTime end);
        Task<IEnumerable<TaskList>> GetAllWithDateRange(DateTime start, DateTime end);
        Task<IEnumerable<TaskList>> GetByZoneIdsWithTasks(IEnumerable<int> zoneIds);
        Task<int> GetCountByUser(int userId);
        Task<int> GetCountByUserAndWeek(int userId, int year, int week);
        Task<TaskList?> GetByIdWithZoneAndNurserySite(int taskListId);
        Task<List<TaskList>> GetByUserAndWeekWithZoneAndNurserySite(int userId, int year, int week);
        Task<IEnumerable<TaskList>> GetByArchiveStatus(bool isArchived, int pageNr, int pageSize);
        Task<int> CountByArchiveStatus(bool isArchived);

    }
}
