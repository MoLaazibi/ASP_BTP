namespace AP.BTP.Domain
{
    public class TaskList
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? ZoneId { get; set; }
        public DateTime Date { get; set; }
        public bool IsArchived { get; set; }
        public User? User { get; set; }
        public Zone? Zone { get; set; }
        public ICollection<EmployeeTask> Tasks { get; set; } = [];
    }
}
