using Models;

namespace NetworkTypes
{
    class  NextTurn : SerializableType
    {
        public string Team { get; set; }
        public Point CreaturePoint { get; set; }
    }
}
