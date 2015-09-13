namespace NetworkTypes
{
    class Turn
    {
        public Hero CurrentHero { get; set; }
        public int PlayerOrder { get; set; }
        public byte Index { get; set; }
        public byte MaxCreature { get; private set; }

        public Turn(Hero h, int player, byte index = 0)
        {
            CurrentHero = h;
            PlayerOrder = player;
            Index = index;
            MaxCreature = (byte)(h.InstanceCreatures.Count);
        }
    }
}
