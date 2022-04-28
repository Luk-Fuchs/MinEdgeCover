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
                                                        iterations: 400);

            var maxDegree = graphs.Select(g => g.Vertices.Select(_ => _.AdjEdges.Count).Max()).Max();
            var vertexOfDegree = new int[maxDegree + 1];
            var haveToVertexOfDegree = new int[maxDegree + 1];

            foreach (var graph in graphs)
            {
                var haveToEdges = ComputeuniquePairingEdges(graph);
                var haveToDegrees = haveToEdges.SelectMany(_ => _.Vertices).Select(_ => _.AdjEdges.Count).OrderBy(_ => _).ToList();
                var allDegrees = graph.Vertices.Select(_ => _.AdjEdges.Count).OrderBy(_ => _).ToList();
                foreach (var d in haveToDegrees)
                    haveToVertexOfDegree[d]++;
                foreach (var d in allDegrees)
                    vertexOfDegree[d]++;
                Console.WriteLine(haveToEdges.Count);
            }
            var distributon = new List<double>();
            for (int i = 0; i < maxDegree + 1; i++)
            {
                if (vertexOfDegree[i] == 0)
                    distributon.Add(0);
                else
                    distributon.Add((haveToVertexOfDegree[i] + 0.000001) / vertexOfDegree[i]);
            }
            //Plot.CreateFigure(distributon);
            Plot.CreateFigure(distributon,title:"Anteil eindeutiger Knoten nach Grad (Datensatz 1)", xLable:"Knotengrad",yLable:"relative Anzahl fester Knoten");

            //var graph = graphs[0];
            //graph.InitializeFor2DMatchin();
            //var pathcover = PathCoverSolver.CalculatePathCoverOfNonOverlappingGraph(graph.Vertices);
            //Plot.CreateIntervals(pathcover.Select(_ => new Edge(_)).ToList());

            //var haveToEdges = ComputeuniquePairingEdges(graph);
            //var haveToVertices = haveToEdges.SelectMany(_ => _.Vertices).ToList();

            //var valideEdges = pathcover.Where(_ => _.Count < 4).Select(_ => new Edge(_)).Where(_ => graph.Edges.Contains(_)).ToList();
            //Plot.CreateIntervals(valideEdges);
            ////foreach (var path in pathcover)
            ////{
            ////    foreach (var v in path.ToList())
            ////    {
            ////        if (!haveToVertices.Contains(v))
            ////        {
            ////            path.Remove(v);
            ////        }
            ////    }
            ////}
            ////Console.WriteLine(pathcover.Where(_ => _.Count < 4).Select(_ => new Edge(_)).Where(_ => graph.Edges.Contains(_)).Count());
            ////Plot.CreateIntervals(haveToEdges);

            //var parameters = new Dictionary<String, double>();
            //Console.WriteLine("--------------------------------------");
            //var solver = new MIP();
            //solver.initialize(graph);
            //var res1 = solver.Run(parameters);
            //Console.WriteLine("res1: " + (res1.cover.Count));


            //////valideEdges = valideEdges.Where(_ => _.VertexCount > 2).ToList();

            //////var verticesToDelete = valideEdges.SelectMany(_ => _.Vertices).ToList();
            //////var graph2 = graph.GenerateInducedSubgraph(graph.Vertices.Except(verticesToDelete).ToList());

            ////solver.initialize(graph2);
            ////var res2 = solver.Run(parameters);
            ////Console.WriteLine("res2: " + (res2.cover.Count + valideEdges.Count));


            ////var sizeDistribuion = new int[8];
            ////for(int i = 0; i < sizeDistribuion.Length; i++)
            ////{
            ////    sizeDistribuion[i] = pathcover.
            ////}
            //var test = pathcover.GroupBy(_ => _.Count).Select(_ => (_.Key, _.Count())).OrderBy(_=>_.Key).ToList();
            //;

            //var subEdges = new List<Edge>();
            //foreach(var path in pathcover)
            //{
            //    var edge = DetectValideSubEdge(path, graph);
            //    if ( edge!= null)
            //    {
            //        subEdges.Add(edge);
            //    }
            //    else
            //    {
            //        Console.WriteLine(path.Count);
            //    }

            //}



            //var verticesToDelete = subEdges.SelectMany(_ => _.Vertices).ToList();
            //var graph2 = graph.GenerateInducedSubgraph(graph.Vertices.Except(verticesToDelete).ToList());
            //solver.initialize(graph2);
            //var res2 = solver.Run(parameters);
            //Console.WriteLine("res2: " + (res2.cover.Count + subEdges.Count));



            //foreach (var v in subEdges.SelectMany(_ => _.Vertices))
            //{
            //    foreach(var n in v.NeighboursFor2DMatching)
            //    {
            //        n.NeighboursFor2DMatching.Remove(v);
            //    }
            //    v.NeighboursFor2DMatching.Clear();
            //}

            
            //pathcover = PathCoverSolver.CalculatePathCoverOfNonOverlappingGraph(graph.Vertices);


            //var subEdges2 = new List<Edge>();
            //foreach (var path in pathcover)
            //{
            //    var edge = DetectValideSubEdge(path, graph);
            //    if (edge != null)
            //    {
            //        subEdges2.Add(edge);
            //    }
            //    else
            //    {
            //        //Console.WriteLine(path.Count);
            //    }

            //}

            //var subedes3 = subEdges.Concat(subEdges2).ToList();
            //var verticesToDelete2 = subedes3.SelectMany(_ => _.Vertices).ToList();
            //var graph3 = graph.GenerateInducedSubgraph(graph.Vertices.Except(verticesToDelete2).ToList());
            //solver.initialize(graph2);
            //var res3 = solver.Run(parameters);
            //Console.WriteLine("res3: " + (res3.cover.Count + subedes3.Count));



            //;
            ////graph2.InitializeFor2DMatchin();
            ////var pathcover2 = PathCoverSolver.CalculatePathCoverOfNonOverlappingGraph(graph2.Vertices);
            ////Plot.CreateIntervals(pathcover2.Select(_ => new Edge(_)).ToList());
            ////var valideEdges2 = pathcover2.Where(_ => _.Count < 4).Select(_ => new Edge(_)).Where(_ => graph.Edges.Contains(_)).ToList();




            //Plot.CreateIntervals(pathcover.Select(_ =>  new Edge(_)).ToList());
        }

        private static Edge DetectValideSubEdge (List<Vertex> vertices, Graph graph)
        {
            if (vertices.Count <= 3)
                return null;
            for(int i = 0; i< vertices.Count - 3; i++)
            {
                var tmpEdge = new Edge(vertices.Skip(i).Take(3).ToList());
                if (graph.Edges.Contains(tmpEdge))
                {
                    return tmpEdge;
                }
            }
            return null;
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