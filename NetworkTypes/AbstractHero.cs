using System;
using System.Collections.Generic;

namespace NetworkTypes
{
    public class AbstractHero : SerializableType
    {
        #region Fields
        public float Damage { get; set; }
        public float Luck { get; set; }
        public string Name { get; set; }
        public float Range { get; set; }
        public float Health { get; set; }
        public int MaxHealth { get; set; }
        public int Speed { get; set; }
        public int Count { get; set; }
        public int Armor { get; set; }
        #endregion

        public HeroRace Race { get; set; }
        public HeroType Type { get; set; }
        public List<AbstractCreature> Creatures { get; set; }

        public AbstractHero(HeroType heroType, HeroRace race, string name)
        {
            //TO:DO move in database
            Race = race;
            Type = heroType;
            Name = name;
            Damage = 10;
            Luck = 10;
            Range = 10;
            Health = 10;
            MaxHealth = 10;
            Speed = 10;
            Count = 10;
            Armor = 10;
            Creatures = new List<AbstractCreature>();
        }
    }
}
