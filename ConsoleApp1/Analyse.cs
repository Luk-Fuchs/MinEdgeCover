using _3D_Matching.Solvers;
using _3D_Matching.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Gurobi;
using _3D_Matching.Tests;
using _3D_Matching;

namespace test
{
    class Analyse
    {
        static void Main(string[] args)
        {

            var graphs = SolverTester.RunGraphGeneration(allowAllAsSingle: true,
                                                        forceCompletness: false,
                                                        removeDegreeOne: true,
                                                        addAllPossibleEdges: false,
                                                        iterations: 2);
            var graph = graphs[0];


            //var canBeEdges = ComputePossiblePairingEdges(graph);
            var haveToEdges =ComputeuniquePairingEdges(graph);
        }

        private static List<Edge> ComputeuniquePairingEdges(Graph graph)
        {
            var edges = graph.Edges;
            {
                var haveToEdges = new List<Edge>();
                var lastObjective = double.MaxValue;
                int p = 0;
                var res = new List<Edge>();
                foreach (var edge in edges)
                {
                    edge.canBeInPerfectMatching = false;
                }
                while (true)
                {

                    edges = graph.Edges;

                    GRBEnv env = new GRBEnv(true);
                    env.Set("OutputFlag", "0");
                    env.Start();

                    GRBModel solver = new GRBModel(env);
                    var x = edges.Select(_ => solver.AddVar(0.0, 1.0, _.canBeInPerfectMatching ? 1 : 0.999, GRB.BINARY, String.Join(" ", _.Vertices))).ToArray();
                    for (int i = 0; i < graph.Vertices.Count; i++)
                    {
                        //var constraint = solver.MakeConstraint(1, 1, "");
                        GRBLinExpr expr = 0.0;
                        for (int j = 0; j < edges.Count; j++)
                        {
                            if (edges[j].VerticesIds.Contains(graph.Vertices[i].Id))
                                //constraint.SetCoefficient(x[j], 1);
                                expr.AddTerm(1.0, x[j]);
                        }
                        solver.AddConstr(expr, GRB.EQUAL, 1, "c0");
                    }
                    solver.Optimize();
                    res = new List<Edge>();
                    for (int j = 0; j < x.Length; j++)
                    {
                        if (x[j].X != 0)
                            res.Add(edges[j]);
                    }

                    haveToEdges = new List<Edge>();
                    if (lastObjective != double.MaxValue)
                    {
                        foreach (var edge in edges)
                        {
                            if (edge.canBeInPerfectMatching && res.Contains(edge))
                            {
                                edge.canBeInPerfectMatching = true;
                                continue;
                            }
                            edge.canBeInPerfectMatching = false;
                        }
                    }
                    else
                    {
                        foreach (var edge in res)
                        {
                            edge.canBeInPerfectMatching = true;
                        }
                    }
                    if (lastObjective == solver.ObjVal)
                    {
                        //Console.WriteLine(p);
                        break;
                    }
                    lastObjective = solver.ObjVal;
                }

                foreach (var edge in res)
                {
                    if (edge.canBeInPerfectMatching)
                    {
                        haveToEdges.Add(edge);
                    }
                }
                graph.ResetVertexAdjEdges();
                //Console.Write("[" + String.Join(",", haveToEdges.Select(e => "[" + String.Join(",", e.Vertices.Select(v => "[" + v.Interval[0] + "," + v.Interval[1] + "]")) + "]")) + "],");
                //Console.WriteLine(String.Join(",", haveToEdges.SelectMany(_ => _.Vertices).Select(_ => _.AdjEdges.Count).OrderBy(_ => _)));
                return haveToEdges;
            }
        }

        private static List<Edge> ComputePossiblePairingEdges(Graph graph)
        {
            bool newPossibleEdgeWasFound = true;
            var edges = graph.Edges;
            while (newPossibleEdgeWasFound)
            {
                GRBEnv env = new GRBEnv(true);
                env.Set("OutputFlag", "0");
                env.Start();
                GRBModel solver = new GRBModel(env);
                var x = edges.Select(_ => solver.AddVar(0.0, 1.0, _.canBeInPerfectMatching ? 1 : 0.999, GRB.BINARY, String.Join(" ", _.Vertices))).ToArray();
                for (int i = 0; i < graph.Vertices.Count; i++)
                {
                    GRBLinExpr expr = 0.0;
                    for (int j = 0; j < edges.Count; j++)
                    {
                        if (edges[j].VerticesIds.Contains(graph.Vertices[i].Id))
                            expr.AddTerm(1.0, x[j]);
                    }
                    solver.AddConstr(expr, GRB.EQUAL, 1, "c0");
                }
                solver.Optimize();
                var res = new List<Edge>();
                for (int j = 0; j < x.Length; j++)
                {
                    if (x[j].X != 0)
                        res.Add(edges[j]);
                }

                newPossibleEdgeWasFound = false;
                foreach (var edge in res)
                {
                    if (!edge.canBeInPerfectMatching)
                    {
                        newPossibleEdgeWasFound = true;
                        edge.canBeInPerfectMatching = true;
                    }
                }
            }
            var canBeEdges = edges.Where(_ => _.canBeInPerfectMatching).ToList();
            //System.IO.File.AppendAllText(@"C:\Users\LFU\Desktop\possibleEdges.txt", Math.Round((0.0 + canBeEdges.Count) / edges.Count * 100, 2).ToString().Replace(",", ".") + ",");
            return canBeEdges;
        }
    }
}