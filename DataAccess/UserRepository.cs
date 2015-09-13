using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

        public User Get(string username, string password)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public int Add(string username, string password)
        {
            _context.Users.Add(new User() {Username = username, Password = password});
            return _context.SaveChanges();
        }
    }
}
