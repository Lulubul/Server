using System.Collections.Generic;
using System.Linq;
using Entities;

namespace DataAccess
{
    public class HeroesRepository
    {
        private readonly HeroesContext _context;
        public HeroesRepository(HeroesContext context)
        {
            _context = context;
        }

        public List<Hero> GetHeroes()
        {
            return _context.Heroes.ToList();
        }

        public Hero GetHeroWithName(string name)
        {
            return _context.Heroes.SingleOrDefault(x => x.Name == name);
        }

        public IEnumerable<Hero> GetHeroByNames(List<string> names)
        {
            return _context.Heroes.Where(x => names.Contains(x.Name));
        }
    }
}
