using System.Collections.Generic;

namespace NetworkTypes
{
    public enum HeroRace { Human, Orc };
    public enum HeroType { Might, Magic }
    public class Hero : SerializableType
    {
        #region Fields
        private float _mana;
        public float Mana
        {
            get { return _mana; }
            set { _mana = value; }
        }

        private float _maxMana;
        public float MaxMana
        {
            get { return _maxMana; }
            set { _maxMana = value; }
        }

        private float _damage;
        public float Damage
        {
            get { return _damage; }
            set { _damage = value; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        #endregion

        public HeroRace Race { get; set; }
        public HeroType Type { get; set; }
        public List<AbstractCreature> Creatures = new List<AbstractCreature>();
        public List<Creature> InstanceCreatures = new List<Creature>();

        public Hero(string name)
        {
            Mana = 100.0f;
            MaxMana = 100.0f;
            Damage = 50.0f;
            Name = name;
        }

        public void ThrowMagic()
        {

        }

        public void ThrowAttack()
        {

        }
    }
}
