using AP.BTP.Domain;

namespace AP.BTP.Application.Interfaces
{
    public interface IEmployeeTaskRepository : IGenericRepository<EmployeeTask>
    {
        EmployeeTask GetByDescription(string description);
        Task<IEnumerable<EmployeeTask>> GetByTaskListId(int taskListId);
    }
}