using AP.BTP.Domain;

namespace AP.BTP.Application.CQRS.Users
{

    public class UserDTO
    {
        public int Id { get; set; }
        public string AuthId { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();
        public int TaskListsCount { get; set; }
        public int? PreferredNurserySiteId { get; set; }
    }
}
