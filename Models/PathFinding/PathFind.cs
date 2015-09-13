using System;
using System.Collections.Generic;

namespace PathFinding
{
    public static class PathFind
    {
        public static Path<TNode> FindPath<TNode>(
            TNode start,
            TNode destination,
            Func<TNode, TNode, double> distance,
            Func<TNode, double> estimate)
            where TNode : IHasNeighbours<TNode>
        {
            var closed = new HashSet<TNode>();
            var queue = new PriorityQueue<double, Path<TNode>>();
            queue.Enqueue(0, new Path<TNode>(start));

            while (!queue.IsEmpty)
            {
                var path = queue.Dequeue();

                if (closed.Contains(path.LastStep))
                    continue;
                if (path.LastStep.Equals(destination))
                    return path;

                closed.Add(path.LastStep);
                foreach (TNode n in path.LastStep.Neighbours)
                {
                    double d = distance(path.LastStep, n);
                    var newPath = path.AddStep(n, d);
                    queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                }
            }

            return null;
        }

        public static List<List<TNode>> MovementRange<TNode>(TNode start, int movement)
            where TNode : IHasNeighbours<TNode>
        {
            var visited = new List<TNode>();
            var fringes = new List<List<TNode>> {new List<TNode>()};
            fringes[0].Add(start);
            visited.Add(start);

            for (var k = 1; k <= movement; k++)
            {
                fringes.Add(new List<TNode>());
                fringes[k] = new List<TNode>();
                foreach (var hex in fringes[k - 1])
                {
                    foreach (var neighbor in hex.Neighbours)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            fringes[k].Add(neighbor);
                        }
                    }
                }
            }
            return fringes;
        }

    }
}
