﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace DataAccess
{
    public class Repository
    {
        private UserRepository _userRepository;
        private readonly HeroesContext _context;

        public Repository()
        {
            _context = new HeroesContext();
        }

        public UserRepository Users
        {
            get { return _userRepository ?? (_userRepository = new UserRepository(_context)); }
        }
    }
}
