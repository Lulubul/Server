using System.Collections.Generic;
using NetworkTypes;

namespace Server
{
    public interface IDataCollector
    {
        BoardInfo GetBoardInfo();
    }

    public class DataCollector : IDataCollector
    {
        public BoardInfo GetBoardInfo()
        {
            return new BoardInfo
            {
                Height = 7,
                Width = 12,
                HexWidth = 4f,
                Spacing = 3.46f
            };
        }
    }
}
