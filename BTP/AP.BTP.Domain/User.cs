namespace AP.BTP.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string AuthId { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<UserRole> Roles { get; set; } = new();
        public ICollection<TaskList> TaskLists { get; set; } = new List<TaskList>();
        public NurserySite NurserySite { get; set; }
        public int? PreferredNurserySiteId { get; set; }
        public NurserySite PreferredNurserySite { get; set; }
    }
}
