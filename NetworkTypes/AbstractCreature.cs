namespace NetworkTypes
{

    public class AbstractCreature : SerializableType
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
        public CreatureType Type { get; set; }
        public Team Team { get; set; }
        public CreatureStatus Status { get; set; }
        public int Index { get; set; }
        public Point Piece { get; set; }

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
