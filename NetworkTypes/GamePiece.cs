using System;

namespace NetworkTypes
{
    [Serializable]
    public class GamePiece : SpacialObject
    {
        public GamePiece(Point location)
            : base(location)
        {

        }
    }
}
