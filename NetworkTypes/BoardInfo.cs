using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTypes 
{
    public class BoardInfo : SerializableType
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public float Spacing { get; set; }
        public float HexWidth { get; set; }
    }
}
