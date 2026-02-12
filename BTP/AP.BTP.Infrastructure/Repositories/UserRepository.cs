using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AP.BTP.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AP.BTP.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly BTPContext context;
        public UserRepository(BTPContext context) : base(context)
        {
            this.context = context;
        }
        public new async Task<IEnumerable<User>> GetAll(int pageNr, int pageSize)
        {
            return await context.UserList
                .Include(u => u.Roles)
                .Include(u => u.TaskLists)
                .Skip((pageNr - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public new async Task<List<User>> GetWithoutNurserySiteByRole(int pageNr, int pageSize)
        {
            var assignedUserIds = context.NurserySites
                .Select(n => n.UserId);

            return await context.UserList
                .Where(u => !assignedUserIds.Contains(u.Id))
                .Skip((pageNr - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<User> GetByAuthId(string authId)
        {
            return await context.UserList
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.AuthId == authId);
        }
        public async Task<User> GetByEmail(string email)
        {
            return await context.UserList
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<List<User>> GetByRole(Role role, int pageNr, int pageSize)
        {
            var query = context.UserList
                .Include(u => u.Roles)
                .Include(u => u.TaskLists)
                .Where(u => u.Roles.Any(r => r.Role == role));

            if (pageSize > 0)
            {
                query = query
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize);
            }

            return await query.ToListAsync();
        }

    }
}
