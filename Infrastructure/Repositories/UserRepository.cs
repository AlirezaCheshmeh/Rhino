using Application.Common;
using Application.Database;
using Application.Repositories;
using Domain.Entities.Users;


namespace Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(IApplicationDataContext context) : base(context) { }

    }
}
