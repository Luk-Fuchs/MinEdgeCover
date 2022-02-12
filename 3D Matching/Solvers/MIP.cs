using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.LinearSolver;
using Gurobi;

namespace _3D_Matching.Solvers
{
    class MIP : IMinimumEdgecoveringSolver
    {

        Solver solver = Solver.CreateSolver("SCIP");
        
        Random _random = new Random();
        private string _solver;
        private double _usePercentOfAllEdges;

        public MIP(string solver = "GUROBI",double usePercentOfAllEdges = 1.0)
        {
            _usePercentOfAllEdges = usePercentOfAllEdges;
            _solver = solver;
        }

        public override String Name { get => this.GetType().Name + "|" + _solver +"|"+( _usePercentOfAllEdges *100) +"%"; }
        public override (List<Edge> cover, int iterations)  Run(Dictionary<string, double> parameters)
        {
            var edges = _graph.Edges.Where(_ => (_random.NextDouble() < _usePercentOfAllEdges || _.Vertices.Count!=3)).ToList();

            if (_solver == "GUROBI")
            {
                GRBEnv env = new GRBEnv(true);
                env.Set("OutputFlag", "0");
                env.Start();

                GRBModel solver = new GRBModel(env);
                var x = edges.Select(_ => solver.AddVar(0.0, 1.0, 1.0,GRB.BINARY,String.Join(" ", _.Vertices))).ToArray();
                for (int i = 0; i < _graph.Vertices.Count; i++)
                {
                    //var constraint = solver.MakeConstraint(1, 1, "");
                    GRBLinExpr expr = 0.0;
                    for (int j = 0; j < edges.Count; j++)
                    {
                        if (edges[j].VerticesIds.Contains(_graph.Vertices[i].Id))
                            //constraint.SetCoefficient(x[j], 1);
                            expr.AddTerm(1.0, x[j]);
                    }
                    solver.AddConstr(expr, GRB.EQUAL, 1, "c0");
                }
                solver.Optimize();
                var res = new List<Edge>();
                for (int j = 0; j < x.Length; j++)
                {
                    if (x[j].X!=0)//Variable(j).SolutionValue() != 0)
                        res.Add(edges[j]);
                }
                return (res, 1);
            }
            else if (_solver == "ORT")
            {
                solver = Solver.CreateSolver("SCIP");
                var x = edges.Select(_ => solver.MakeIntVar(0.0, 1, String.Join(" ", _.Vertices))).ToArray();
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
                return (res, 1);
            }
            return (null, -1);
        }
    }
}
