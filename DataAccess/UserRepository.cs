using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Entities;

namespace DataAccess
{
    public class UserRepository
    {
        private readonly HeroesContext _context;
        private readonly SHA256 _sha256;
        public UserRepository(HeroesContext context)
        {
            _context = context;
            _sha256 = SHA256Managed.Create();
        }

        public User Get(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return null;
            var passwordHash = GetSha256(password);
            return _context.Users.FirstOrDefault(u => u.Username == username && u.Password == passwordHash);
        }

        public User GetUserByID(int userID)
        {
            return _context.Users.FirstOrDefault(u => u.Id == userID);
        }

        public User Add(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return null;
            var passwordHash = GetSha256(password);
            var user = _context.Users.Add(new User
            {
                Username = username,
                Password = passwordHash
            });
            _context.SaveChanges();
            return user;
        }

        private string GetSha256(string password)
        {
            var hash = new StringBuilder();
            var crypto = _sha256.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
    }
}
