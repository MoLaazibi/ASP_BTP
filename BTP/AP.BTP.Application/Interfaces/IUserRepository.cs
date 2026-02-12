using AP.BTP.Domain;

namespace AP.BTP.Application.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetByAuthId(string authId);

        Task<User> GetByEmail(string email);

        Task<List<User>> GetByRole(Role role, int pageNr, int pageSize);

        Task<List<User>> GetWithoutNurserySiteByRole(int pageNr, int pageSize);
    }
}
