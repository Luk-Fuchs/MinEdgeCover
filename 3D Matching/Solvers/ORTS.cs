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
        
        public override List<Edge> Run(Dictionary<string, double> parameters)
        {
            solver = Solver.CreateSolver("SCIP");
            x = _graph.Edges.Select(_ => solver.MakeIntVar(0.0, 1, String.Join(" ", _.Vertices))).ToArray();
            for (int i = 0; i < _graph.Vertices.Count; i++)
            {
                Constraint constraint = solver.MakeConstraint(1, 1, "");
                for (int j = 0; j < _graph.Edges.Count; j++)
                {
                    if (_graph.Edges[j].VerticesIds.Contains(_graph.Vertices[i].Id))
                        constraint.SetCoefficient(x[j], 1);
                }
            }

            Objective objective = solver.Objective();
            for (int j = 0; j < _graph.Edges.Count; j++)
            {
                objective.SetCoefficient(x[j], 1);
            }
            objective.SetMinimization();
            var res = new List<Edge>();
            solver.Solve();
            for (int j = 0; j < x.Length; j++)
            {
                if (solver.Variable(j).SolutionValue() != 0)
                    res.Add(_graph.Edges[j]);
                    //Console.WriteLine(x[j].Name());
            }
            return res;
            //Console.WriteLine(solver.Objective().Value());
            //Console.WriteLine(solver.ExportModelAsLpFormat(false));
        }
    }
}
