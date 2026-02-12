using AP.BTP.Application.CQRS.EmployeeTasks;

namespace AP.BTP.Application.CQRS.TaskLists
{
    public class TaskListDTO
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? ZoneId { get; set; }
        public string? ZoneName { get; set; }
        public DateTime Date { get; set; }
        public List<EmployeeTaskDTO> Tasks { get; set; } = new List<EmployeeTaskDTO>();
        public bool IsArchived { get; set; }
    }
}
