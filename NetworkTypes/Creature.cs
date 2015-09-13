namespace NetworkTypes
{
    public enum CreatureType { Melee, Range }
    public enum CreatureStatus { Alive, Death }

    public class Creature : AbstractCreature
    {
        public CreatureStatus Status { get; set; }

        public int Index { get; set; }
        public GamePiece Piece;
    }
}
