using System.Collections.Generic;
using System.Linq;
using Entities;

namespace DataAccess
{
    public class CreaturesRepository
    {
        private readonly HeroesContext _context;
        public CreaturesRepository(HeroesContext context)
        {
            _context = context;
        }

        public List<Creature> GetCreatures()
        {
            return _context.Creatures.ToList();
        }

        public Creature GetCreatureWithName(string name)
        {
            return _context.Creatures.SingleOrDefault(x => x.Name == name);
        }

        public Dictionary<string, Creature> GetCreaturesByName(IEnumerable<string> names)
        {
            return _context.Creatures.Where(x => names.Contains(x.Name)).ToDictionary(x => x.Name);
        } 
    }
}
