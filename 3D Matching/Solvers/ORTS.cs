using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.LinearSolver;

namespace _3D_Matching.Solvers
{
    class ORTS : IMinimumEdgecoveringSolver
    {

        Solver solver = Solver.CreateSolver("SCIP");
        Variable[] x;
        Random _random = new Random();
        private double _usePercentOfAllEdges;

        public ORTS(double usePercentOfAllEdges = 1.0)
        {
            _usePercentOfAllEdges = usePercentOfAllEdges;
        }

        public override String Name { get => this.GetType().Name + "|" +( _usePercentOfAllEdges *100) +"%"; }
        public override (List<Edge> cover, int iterations)  Run(Dictionary<string, double> parameters)
        {
            solver = Solver.CreateSolver("SCIP");
            var edges = _graph.Edges.Where(_ => (_random.NextDouble() < _usePercentOfAllEdges || _.Vertices.Count!=3)).ToList();
            x = edges.Select(_ => solver.MakeIntVar(0.0, 1, String.Join(" ", _.Vertices))).ToArray();
            for (int i = 0; i < _graph.Vertices.Count; i++)
            {
                Constraint constraint = solver.MakeConstraint(1, 1, "");
                for (int j = 0; j < edges.Count; j++)
                {
                    if (edges[j].VerticesIds.Contains(_graph.Vertices[i].Id))
                        constraint.SetCoefficient(x[j], 1);
                }
            }

            Objective objective = solver.Objective();
            for (int j = 0; j < edges.Count; j++)
            {
                objective.SetCoefficient(x[j], 1);
            }
            objective.SetMinimization();
            var res = new List<Edge>();
            solver.Solve();
            for (int j = 0; j < x.Length; j++)
            {
                if (solver.Variable(j).SolutionValue() != 0)
                    res.Add(edges[j]);
            }
            return (res,1);
        }
    }
}
