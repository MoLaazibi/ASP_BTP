using AP.BTP.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace AP.BTP.API.Services
{
    public class TaskContextService
    {
        private readonly BTPContext _db;
        private readonly ILogger<TaskContextService> _logger;

        public TaskContextService(BTPContext db, ILogger<TaskContextService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<string?> GetWeeklyTaskSummaryAsync(
            ClaimsPrincipal user,
            string? overrideEmail,
            string? overrideAuthId,
            int? overrideUserId,
            DateTime? targetDate,
            CancellationToken cancellationToken)
        {
            var email = GetEmail(user, overrideEmail);
            var authId = GetAuthId(user, overrideAuthId);
            var userId = await GetUserIdAsync(user, overrideUserId, email, authId, cancellationToken);

            var today = targetDate?.Date ?? DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            var weekEnd = weekStart.AddDays(7);

            _logger.LogInformation("TaskContext resolve start: email={Email}, authId={AuthId}, userId={UserId}, weekStart={WeekStart}, weekEnd={WeekEnd}",
                email ?? "<null>", authId ?? "<null>", userId?.ToString() ?? "<null>", weekStart, weekEnd.AddDays(-1));

            if (!userId.HasValue)
            {
                _logger.LogInformation("Geen gebruiker gevonden voor context (claims/overrides ontbreken of matchen niet).");
                throw new InvalidOperationException("Geen gebruiker bekend voor takencontext.");
            }

            // Fetch all relevant task lists once
            var allTasks = await _db.TaskList
                .Where(tl => tl.UserId == userId)
                .SelectMany(tl => tl.Tasks.Select(t => new TaskInfo {
                    PlannedDuration = t.PlannedDuration,
                    StopTime = t.StopTime,
                    Date = tl.Date,
                    IsArchived = tl.IsArchived
                }))
                .ToListAsync(cancellationToken);

            var summary = ComputeTaskSummary(allTasks, weekStart, weekEnd, today);

            var userLabel = email ?? authId ?? $"UserId:{userId}";
            return BuildSummaryString(userLabel, weekStart, weekEnd, summary);
        }

        #region Helper

        public class TaskInfo
        {
            public int PlannedDuration { get; set; }
            public DateTime? StopTime { get; set; }
            public DateTime Date { get; set; }
            public bool IsArchived { get; set; }
        }

        private static string? GetEmail(ClaimsPrincipal? user, string? overrideEmail) =>
            !string.IsNullOrWhiteSpace(overrideEmail)
                ? overrideEmail
                : user?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value;

        private static string? GetAuthId(ClaimsPrincipal? user, string? overrideAuthId) =>
            !string.IsNullOrWhiteSpace(overrideAuthId)
                ? overrideAuthId
                : user?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;

        private async Task<int?> GetUserIdAsync(ClaimsPrincipal? user, int? overrideUserId, string? email, string? authId, CancellationToken cancellationToken)
        {
            if (overrideUserId.HasValue) return overrideUserId;

            int? userId = null;
            var idClaim = user?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "user_id" || c.Type == ClaimTypes.Name);
            if (idClaim != null && int.TryParse(idClaim.Value, out var parsed))
                userId = parsed;

            if (userId.HasValue || !string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(authId))
            {
                var userEntity = await _db.UserList.AsNoTracking().FirstOrDefaultAsync(u =>
                    (userId.HasValue && u.Id == userId.Value) ||
                    (!string.IsNullOrEmpty(email) && u.Email == email) ||
                    (!string.IsNullOrEmpty(authId) && u.AuthId == authId),
                    cancellationToken);

                return userEntity?.Id ?? userId;
            }

            return null;
        }

        private static (int taskCount, int completedWeek, int completedTotal, int plannedToday, int completedToday,
            int longWeekOver2, int longWeekOver3, int longTotalOver2, int longTotalOver3,
            int archivedWeek, int openWeek, int archivedTotal, int openTotal)
            ComputeTaskSummary(List<TaskInfo> tasks, DateTime weekStart, DateTime weekEnd, DateTime today)
        {
            int taskCount = tasks.Count(t => t.Date >= weekStart && t.Date < weekEnd);
            int completedWeek = tasks.Count(t => t.Date >= weekStart && t.Date < weekEnd && t.StopTime != null);
            int completedTotal = tasks.Count(t => t.StopTime != null);
            int plannedToday = tasks.Count(t => t.Date >= today && t.Date < today.AddDays(1));
            int completedToday = tasks.Count(t => t.Date >= today && t.Date < today.AddDays(1) && t.StopTime != null);

            int longWeekOver2 = tasks.Count(t => t.Date >= weekStart && t.Date < weekEnd && t.PlannedDuration > 2);
            int longWeekOver3 = tasks.Count(t => t.Date >= weekStart && t.Date < weekEnd && t.PlannedDuration > 3);
            int longTotalOver2 = tasks.Count(t => t.PlannedDuration > 2);
            int longTotalOver3 = tasks.Count(t => t.PlannedDuration > 3);

            int archivedWeek = tasks.Count(t => t.Date >= weekStart && t.Date < weekEnd && t.IsArchived);
            int openWeek = tasks.Count(t => t.Date >= weekStart && t.Date < weekEnd && !t.IsArchived);
            int archivedTotal = tasks.Count(t => t.IsArchived);
            int openTotal = tasks.Count(t => !t.IsArchived);

            return (taskCount, completedWeek, completedTotal, plannedToday, completedToday,
                longWeekOver2, longWeekOver3, longTotalOver2, longTotalOver3,
                archivedWeek, openWeek, archivedTotal, openTotal);
        }

        private static string BuildSummaryString(string userLabel, DateTime weekStart, DateTime weekEnd,
            (int taskCount, int completedWeek, int completedTotal, int plannedToday, int completedToday,
             int longWeekOver2, int longWeekOver3, int longTotalOver2, int longTotalOver3,
             int archivedWeek, int openWeek, int archivedTotal, int openTotal) summary)
        {
            var sb = new StringBuilder();
            sb.Append($"Gebruiker: {userLabel}. ");
            sb.Append($"Week {weekStart:yyyy-MM-dd} t/m {weekEnd.AddDays(-1):yyyy-MM-dd}: ");
            sb.Append($"taken gepland: {summary.taskCount}, afgerond: {summary.completedWeek}. ");
            sb.Append($"Vandaag gepland: {summary.plannedToday}, afgerond: {summary.completedToday}. ");
            sb.Append($"Totaal afgerond: {summary.completedTotal}.");
            sb.Append($" Lang (>2u): week {summary.longWeekOver2}, totaal {summary.longTotalOver2}; ");
            sb.Append($">3u: week {summary.longWeekOver3}, totaal {summary.longTotalOver3}.");
            sb.Append($" Lijsten: open week {summary.openWeek}, klaar week {summary.archivedWeek}, open totaal {summary.openTotal}, klaar totaal {summary.archivedTotal}.");
            return sb.ToString();
        }

        #endregion
    }
}
