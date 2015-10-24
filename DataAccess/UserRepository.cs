using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using Entities;

namespace DataAccess
{
    public class UserRepository
    {
        private readonly HeroesContext _context;
        public UserRepository(HeroesContext context)
        {
            _context = context;
        }

        public User Get(string username, string password)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public User Add(string username, string password)
        {
            var user = _context.Users.Add(new User
            {
                Username = username,
                Password = password
            });
            _context.SaveChanges();
            return user;
        }
    }
}
