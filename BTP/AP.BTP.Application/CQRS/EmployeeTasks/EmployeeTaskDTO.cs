namespace AP.BTP.Application.CQRS.EmployeeTasks
{
    public class EmployeeTaskDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int PlannedDuration { get; set; }
        public DateTime PlannedStartTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public int Order { get; set; }
        public int? TaskListId { get; set; }
        public int? CategoryId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
