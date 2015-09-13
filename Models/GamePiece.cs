using System;

namespace Models
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
