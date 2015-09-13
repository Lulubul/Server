using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace PathFinding
{
    public class Path<TNode> : IEnumerable<TNode>
    {
        public TNode LastStep { get; private set; }
        public Path<TNode> PreviousSteps { get; private set; }
        public double TotalCost { get; private set; }

        private Path(TNode lastStep, Path<TNode> previousSteps, double totalCost)
        {
            LastStep = lastStep;
            PreviousSteps = previousSteps;
            TotalCost = totalCost;
        }

        public Path(TNode start) : this(start, null, 0) { }

        public Path<TNode> AddStep(TNode step, double stepCost)
        {
            return new Path<TNode>(step, this, TotalCost + stepCost);
        }

        public IEnumerator<TNode> GetEnumerator()
        {
            for (var p = this; p != null; p = p.PreviousSteps)
                yield return p.LastStep;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
