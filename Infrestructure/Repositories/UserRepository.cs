using Domain.Entities;
using Domain.Interfaces;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        public List<User> GetAll()
        {
            return new List<User>
            {
                new User { Id = 1, Name = "Ana" },
                new User { Id = 2, Name = "Juan" }
            };
        }
    }
}