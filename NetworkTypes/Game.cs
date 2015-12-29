
using System.Collections.Generic;
using System.Linq;

namespace NetworkTypes
{
    public class Game
    {
        public Tile[,] GameBoard;

        public int Width;
        public int Height;

        public Game(int width, int height)
        {
            Width = width;
            Height = height;
            InitialiseGameBoard();

            BlockOutTiles(5, 3);
            BlockOutTiles(6, 3);
            BlockOutTiles(5, 4);
        }

        private void InitialiseGameBoard()
        {
            GameBoard = new Tile[Width, Height];

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    GameBoard[x, y] = new Tile(x, y);
                }
            }

            AllTiles.ToList().ForEach(o => o.FindNeighbours(GameBoard));
        }

        public void BlockOutTiles(int x, int y)
        {
            GameBoard[x, y].CanPass = false;
        }

        public IEnumerable<Tile> AllTiles
        {
            get
            {
                for (var x = 0; x < Width; x++)
                    for (var y = 0; y < Height; y++)
                        yield return GameBoard[x, y];
            }
        }

    }
}
