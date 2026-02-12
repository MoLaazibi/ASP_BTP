using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AP.BTP.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AP.BTP.Infrastructure.Repositories
{
    public class TaskListRepository : GenericRepository<TaskList>, ITaskListRepository
    {
        private readonly BTPContext context;
        public TaskListRepository(BTPContext context) : base(context)
        {
            this.context = context;
        }

        private async Task AutoArchiveElapsedTaskLists()
        {
            var now = DateTime.Now;
            var candidates = await context.TaskList
                .Include(tl => tl.Tasks)
                .Where(tl => !tl.IsArchived && tl.Tasks.Any() && tl.Date.Date <= now.Date)
                .ToListAsync();

            var updated = false;
            foreach (var tl in candidates)
            {
                if (tl.Tasks == null || !tl.Tasks.Any()) continue;
                var lastPlannedEnd = tl.Tasks.Max(t => t.PlannedStartTime.AddHours(t.PlannedDuration));
                var allCompleted = tl.Tasks.All(t => t.StopTime != null);
                var shouldArchive = allCompleted || now > lastPlannedEnd;
                if (!shouldArchive) continue;

                foreach (var task in tl.Tasks.Where(t => t.StopTime == null))
                {
                    task.StopTime = now;
                    context.Entry(task).State = EntityState.Modified;
                }

                tl.IsArchived = true;
                context.Entry(tl).State = EntityState.Modified;
                updated = true;
            }

            if (updated)
                await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TaskList>> GetAll(int pageNr, int pageSize)
        {
            await AutoArchiveElapsedTaskLists();
            return await context.TaskList
                .Include(tl => tl.Tasks)
                .Skip((pageNr - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<TaskList> GetById(int id)
        {
            return await context.TaskList
               .Include(t => t.Tasks)
               .FirstOrDefaultAsync(t => t.Id == id);
        }

        public TaskList GetByDate(DateTime date)
        {
            return context.TaskList.FirstOrDefault(tl => tl.Date == date);
        }

        public async Task<TaskList> GetByUserIdAndDate(int userId, DateTime date)
        {
            return await context.TaskList
                .Include(tl => tl.Zone)
                .FirstOrDefaultAsync(tl => tl.UserId == userId && tl.Date.Date == date);
        }
        public async Task<IEnumerable<TaskList>> GetByUserIdAndDateRange(int userId, DateTime start, DateTime end)
        {
            return await context.TaskList
                .Include(t => t.Zone)
                .Where(t => t.UserId == userId && t.Date >= start && t.Date <= end)
                .ToListAsync();
        }
        public async Task<IEnumerable<TaskList>> GetAllWithDateRange(DateTime start, DateTime end)
        {
            return await context.TaskList
                .Where(t => t.Date >= start && t.Date <= end)
                .ToListAsync();
        }
        public async Task<TaskList> GetByIdWithDetails(int id)
        {
            return await context.TaskList
                .Include(tl => tl.Tasks)
                .FirstOrDefaultAsync(ns => ns.Id == id);
        }
        public async Task<IEnumerable<TaskList>> GetByUserId(int userId)
        {
            return await context.TaskList
                .Where(tl => tl.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskList>> GetByZoneIdsWithTasks(IEnumerable<int> zoneIds)
        {
            var zoneList = zoneIds?.ToList() ?? new List<int>();
            if (!zoneList.Any()) return Enumerable.Empty<TaskList>();

            return await context.TaskList
                .Include(tl => tl.Tasks)
                .Where(tl => tl.ZoneId.HasValue && zoneList.Contains(tl.ZoneId.Value))
                .ToListAsync();
        }

        public async Task<int> GetCountByUser(int userId)
        {
            return await context.TaskList
                .Where(tl => tl.UserId == userId)
                .CountAsync();
        }
        public async Task<int> GetCountByUserAndWeek(int userId, int year, int week)
        {
            var iso = CultureInfo.CurrentCulture;

            var taskLists = await context.TaskList
                .Where(tl => tl.UserId == userId)
                .ToListAsync();

            var count = taskLists
                .Count(tl =>
                {
                    int tlWeek = iso.Calendar.GetWeekOfYear(
                        tl.Date,
                        CalendarWeekRule.FirstFourDayWeek,
                        DayOfWeek.Monday);

                    return tl.Date.Year == year && tlWeek == week;
                });
            return count;
        }

        public async Task<TaskList?> GetByIdWithZoneAndNurserySite(int taskListId)
        {
            return await context.TaskList
                .Include(tl => tl.Zone)
                    .ThenInclude(z => z.NurserySite)
                .FirstOrDefaultAsync(tl => tl.Id == taskListId);
        }

        public async Task<List<TaskList>> GetByUserAndWeekWithZoneAndNurserySite(int userId, int year, int week)
        {
            var iso = CultureInfo.CurrentCulture;

            var taskLists = await context.TaskList
                .Include(tl => tl.Zone)
                    .ThenInclude(z => z.NurserySite)
                .Where(tl => tl.UserId == userId && tl.Date.Year == year)
                .ToListAsync();

            return taskLists
                .Where(tl => iso.Calendar.GetWeekOfYear(
                    tl.Date,
                    CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Monday) == week)
                .ToList();
        }

        private static IQueryable<TaskList> ApplyArchiveFilter(IQueryable<TaskList> query, bool isArchived)
        {
            return query.Where(tl => tl.IsArchived == isArchived);
        }

        public async Task<IEnumerable<TaskList>> GetByArchiveStatus(bool isArchived, int pageNr, int pageSize)
        {
            await AutoArchiveElapsedTaskLists();
            var query = ApplyArchiveFilter(context.TaskList.Include(tl => tl.Tasks), isArchived);
            return await query
                .Skip((pageNr - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountByArchiveStatus(bool isArchived)
        {
            await AutoArchiveElapsedTaskLists();
            var query = ApplyArchiveFilter(context.TaskList.Include(tl => tl.Tasks), isArchived);
            return await query.CountAsync();
        }
    }
}
