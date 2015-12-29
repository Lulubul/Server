namespace NetworkTypes
{
    public abstract class SpacialObject 
    {
        private Point _location;
        public Point Location
        {
            get { return _location; }
            set { X = value.X; Y = value.Y; _location = value; }
        }
        public int X { get; set; }
        public int Y { get; set; }

        protected SpacialObject(int x, int y)
            : this(new Point(x, y))
        {
        }

        protected SpacialObject(Point location)
        {
            Location = location;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}]", X, Y);
        }
    }
}
