using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AP.BTP.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AP.BTP.Infrastructure.Repositories
{
    public class EmployeeTaskRepository : GenericRepository<EmployeeTask>, IEmployeeTaskRepository
    {
        private readonly BTPContext context;
        public EmployeeTaskRepository(BTPContext context) : base(context)
        {
            this.context = context;
        }
        public EmployeeTask GetByDescription(string description)
        {
            return context.EmployeeTask.FirstOrDefault(t => t.Description == description);
        }
        public async Task<IEnumerable<EmployeeTask>> GetByTaskListId(int taskListId)
        {
            return await context.EmployeeTask
                .Include(t => t.TaskList)
                .Where(t => t.TaskListId == taskListId)
                .ToListAsync();
        }
    }
}
