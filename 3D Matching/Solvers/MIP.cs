﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ORT = Google.OrTools.LinearSolver;
using Gurobi;
using Coin = Sonnet;
using COIN;

namespace _3D_Matching.Solvers
{
    class MIP : IMinimumEdgecoveringSolver
    {

        //ORT.Solver solver = ORT.Solver.CreateSolver("SCIP");
         
        Random _random = new Random();
        private string _solver;
        private double _usePerc2D;
        private double _usePerc3D;

        public MIP(double usePerc2D = 1.0 ,double usePerc3D = 1.0, string solver = "GUROBI")
        {
            _usePerc2D = usePerc2D;
            _usePerc3D = usePerc3D;
            _solver = solver;
        }

        public override String Name { get => this.GetType().Name + "|" + _solver + "|" + Math.Round(_usePerc2D * 100) + "%|"+ Math.Round( _usePerc3D *100) +"%"; }
        public override (List<Edge> cover, int iterations)  Run(Dictionary<string, double> parameters)
        {
            var edges = _graph.Edges.Where(_ => ((_random.NextDouble() < _usePerc2D && _.VertexCount == 2)||(_random.NextDouble() < _usePerc3D && _.VertexCount==3)|| _.Vertices.Count==1)).ToList();

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
                    solver.AddConstr(expr, GRB.Equals, 1, "c0");
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
                var solver = ORT.Solver.CreateSolver("SCIP");
                var x = edges.Select(_ => solver.MakeIntVar(0.0, 1, String.Join(" ", _.Vertices))).ToArray();
                for (int i = 0; i < _graph.Vertices.Count; i++)
                {
                    ORT.Constraint constraint = solver.MakeConstraint(1, 1, "");
                    for (int j = 0; j < edges.Count; j++)
                    {
                        if (edges[j].VerticesIds.Contains(_graph.Vertices[i].Id))
                            constraint.SetCoefficient(x[j], 1);
                    }
                }

                ORT.Objective objective = solver.Objective();
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
            else if (_solver == "CoinOR")
            {
                Coin.Model model = new Coin.Model();


                //var solver = Coin.Solver.CreateSolver("SCIP");
                var x = edges.Select(_ => new Coin.Variable(0.0, 1, Coin.VariableType.Integer)).ToArray();
                Coin.Objective obj = new Coin.Objective();
                for (int i = 0; i < _graph.Vertices.Count; i++)
                {
                    var lhs = new Coin.Expression();
                    var rhs = new Coin.Expression() + 1;
                    for (int j = 0; j < edges.Count; j++)
                    {
                        if (edges[j].VerticesIds.Contains(_graph.Vertices[i].Id))
                            lhs += x[j];
                        
                    }
                    var constraint = new Coin.Constraint(i+"", lhs, Coin.ConstraintType.EQ, rhs);
                    model.Add(constraint);
                }


                for (int j = 0; j < edges.Count; j++)
                {
                    obj.SetCoefficient(x[j], 1);
                }

                model.Objective = obj;
                var solver = new Coin.Solver(model, typeof(OsiCbcSolverInterface));
                solver.LogLevel = 0;

                solver.Minimise();

                var res = new List<Edge>();
                for (int j = 0; j < x.Length; j++)
                {
                    if (x[j].Value != 0)
                    {
                        res.Add(edges[j]);
                    }
                }
                return (res, 1);
            }
            return (null, -1);
        }
    }
}
