using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using Entities;

namespace DataAccess
{
    public class UserRepository : Repository
    {
        private readonly HeroesContext _context;

        public UserRepository(HeroesContext dbContext)
        {
            _context = dbContext;
        }

        public IEnumerable<User> Get()
        {
            return _context.Users;
        }

    }
}
