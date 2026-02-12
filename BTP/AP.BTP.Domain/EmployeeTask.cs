using System.ComponentModel.DataAnnotations;

namespace AP.BTP.Domain
{
    public class EmployeeTask
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Beschrijving is verplicht")]
        public string Description { get; set; }
        [Range(1, 4, ErrorMessage = "Verwachte duur moet 1 tot 4 uur zijn.")]
        public int PlannedDuration { get; set; }
        public DateTime PlannedStartTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public int Order { get; set; }
        public int? TaskListId { get; set; }
        public int? CategoryId { get; set; }
        public TaskList TaskList { get; set; }
    }
}
