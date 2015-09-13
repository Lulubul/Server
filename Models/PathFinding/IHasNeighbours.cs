using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PathFinding
{
    public interface IHasNeighbours<out TN>
    {
        IEnumerable<TN> Neighbours { get; }
    }
}
