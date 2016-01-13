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
                foreach (var neighbour in path.LastStep.Neighbours)
                {
                    var d = distance(path.LastStep, neighbour);
                    var newPath = path.AddStep(neighbour, d);
                    queue.Enqueue(newPath.TotalCost + estimate(neighbour), newPath);
                }
            }

            return new Path<TNode>();
        }

    }
}
