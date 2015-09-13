using NetworkTypes;

namespace Models
{
    public class AbstractCreature : SerializableType
    {
        public CreatureType Type { get; set; }
        public Team Team { get; set; }
        #region Fields
        private float _damage;
        public float Damage
        {
            get { return _damage; }
            set { _damage = value; }
        }

        private float _luck;
        public float Luck
        {
            get { return _luck; }
            set { _luck = value; }
        }

        private float _range;
        public float Range
        {
            get { return _range; }
            set { _range = value; }
        }

        private int _speed;
        public int Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private int _maxhealth;
        public int MaxHealth
        {
            get { return _maxhealth; }
            set { _maxhealth = value; }
        }

        private float _health;
        public float Health
        {
            get { return _health; }
            set { _health = value; }
        }

        private int _count;
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        private int _armor;
        public int Armor
        {
            get { return _armor; }
            set { _armor = value; }
        }
        #endregion

        public AbstractCreature() {
            //TODO: Move this in config file
            Damage = 45;
            Luck = 20;
            Speed = 3;
            Health = 250.0f;
            MaxHealth = 250;
            Count = 5;
            Armor = 10;
        }
    }
}
