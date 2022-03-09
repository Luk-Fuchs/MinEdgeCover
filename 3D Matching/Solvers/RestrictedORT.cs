using Gurobi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Solvers
{
    class RORTS : IMinimumEdgecoveringSolver
    {
        int additionalNewCalculatedEdges = 7;
        int preCalculationTime = 10;
        String mode = "normal";
        Random _random = new Random();
        public override String Name { get => this.GetType().Name + "(" + additionalNewCalculatedEdges + "|" + preCalculationTime + "|" + mode + ")"; }
        public RORTS(int additionalNewCalculatedEdges, int preCalculationTime = 10, String mode = "normal")
        {
            this.additionalNewCalculatedEdges = additionalNewCalculatedEdges;
            this.preCalculationTime = preCalculationTime;
            this.mode = mode;
        }
        public List<Edge> Run2(Dictionary<string, double> parameters)
        {

            //Es könnte für die geschwindigkeit des MIP solvers zuträglich sein, knoten mit hohem grad zuerst weg zu picken und nicht für die neuberechnung über zu lassen
            //var solver = new RandomSolver();
            var parameters2 = new Dictionary<String, double>();
            parameters2.Add("maxTime", preCalculationTime);

            double maxTime = parameters["maxTime"];
            var time = new Stopwatch();
            time.Start();
            Random _random = new Random();
            var bestRes = new List<Edge>();
            var solver = new Greedy();
            solver.initialize(_graph);
            var edgeCover = solver.Run(parameters2).cover;
            var twoSizeEdges = edgeCover.Where(_ => _.Vertices.Count == 2).ToList();


            //-------------test-------------------
            var notOptimalEdges = edgeCover.Where(_ => _.Vertices.Count != 3).ToList();
            var vertices2 = new List<Vertex>();//.Select(_=>_.Vertices)
            foreach (var edge in notOptimalEdges)
            {
                foreach (var vertex in edge.Vertices)
                {
                    if (!vertices2.Contains(vertex))
                        vertices2.Add(vertex);
                }
            }
            var edges3 = edgeCover.Where(_ => _.Vertices.Count == 3).ToList();

            for (int i = 0; i < additionalNewCalculatedEdges; i++)
            {
                if (edges3.Count == 0)
                    break;
                foreach (var vertex in edges3[0].Vertices) //index out of bounds
                    vertices2.Add(vertex);
                edges3.RemoveAt(0);
            }

            var edges2 = _graph.Edges.Where(_ => _.Vertices.Where(_ => vertices2.Contains(_)).Count() == _.Vertices.Count).ToList();
            var graph2 = new Graph(edges2, vertices2);
            var solver2 = new MIP();
            solver2.initialize(graph2);
            var edgeCover2 = solver2.Run(parameters).cover;
            var res = edges3.Concat(edgeCover2).ToList();


            return res;

            /*
            for (int i = 0; i < 100000; i++)
            {
                if (twoSizeEdges.Count < 3)
                    break;
                var a = twoSizeEdges[_random.Next(twoSizeEdges.Count)];
                var b = twoSizeEdges[_random.Next(twoSizeEdges.Count)];
                var c = twoSizeEdges[_random.Next(twoSizeEdges.Count)];

                if (a == b || b == c || a == c)
                    continue;

                bool edgeACanBeSplittet = false;
                if (_graph.IsEdge(new List<Vertex>() { a.Vertices[0], b.Vertices[0], b.Vertices[1] }) && _graph.IsEdge(new List<Vertex>() { a.Vertices[1], c.Vertices[0], c.Vertices[1] }))
                    edgeACanBeSplittet = true;
                else if (_graph.IsEdge(new List<Vertex>() { a.Vertices[1], b.Vertices[0], b.Vertices[1] }) && _graph.IsEdge(new List<Vertex>() { a.Vertices[0], c.Vertices[0], c.Vertices[1] }))
                {
                    Console.WriteLine("choice found");
                    edgeACanBeSplittet = true;
                    var tmp = a.Vertices[1];
                    a.Vertices[1] = a.Vertices[0];
                    a.Vertices[0] = tmp;
                }
                if (edgeACanBeSplittet)
                {
                twoSizeEdges.Remove(a);
                twoSizeEdges.Remove(b);
                twoSizeEdges.Remove(c);

                edgeCover.Remove(a);
                b.Vertices.Add(b.Vertices[0]);
                b.Vertices.Add(c.Vertices[1]);
                }
            }
            */

            //Console.WriteLine(edgeCover.Count);
            //return edgeCover;
        }

        public override (List<Edge> cover, int iterations) Run(Dictionary<string, double> parameters)
        {
            var verticesCopy = _graph.Vertices.ToList();
            var edgesCopy = _edges.OrderBy(_ => -_.Vertices.Count + _random.NextDouble()).ToList();
            _graph.SetVertexAdjEdges();
            var edgeCover = new List<Edge>();
            var notFinished = true;
            if (mode == "normal")
            {
                edgesCopy = _edges.OrderBy(_ => -_.Vertices.Count *100000+_.Vertices.Select(x=>x.AdjEdges.Count).Sum()+ _random.NextDouble()).ToList();
                while (edgesCopy.Count > 3000)
                {
                    var edge = edgesCopy[0];
                    edgeCover.Add(edge);
                    foreach (var vertex in edge.Vertices)
                    {
                        verticesCopy.Remove(vertex);
                        int j = 0;
                        while (j < edgesCopy.Count)
                        {
                            if (edgesCopy[j].Vertices.Contains(vertex))
                                edgesCopy.RemoveAt(j);
                            else
                                j++;
                        }
                    }
                }
            }
            else if (mode == "artificalThinning")
            {
                while (edgesCopy.Count > 300)
                {
                    var edge = edgesCopy[0];
                    edgeCover.Add(edge);
                    foreach (var vertex in edge.Vertices)
                    {
                        verticesCopy.Remove(vertex);
                        int j = 0;
                        while (j < edgesCopy.Count)
                        {
                            if (edgesCopy[j].Vertices.Contains(vertex))
                                edgesCopy.RemoveAt(j);
                            else
                                j++;
                        }
                    }
                }
                edgesCopy = edgesCopy.OrderBy(_ => _.Vertices.Count + _random.NextDouble()).ToList();
                int i = 0;
                while (edgesCopy.Count > 280)
                {
                    if (edgesCopy[i].Vertices.Count == 1)
                        i++;
                    else
                        edgesCopy.RemoveAt(i);
                }
            }
            else if (mode == "matchHighDegreeAfterWards")
            {
                var highDegreeVertices = _graph.Vertices.OrderBy(_=>-_.AdjEdges.Count).Take(additionalNewCalculatedEdges).ToList();
                var restrictedEdges = _graph.Edges.Where(_ => _.Vertices.All(x => !highDegreeVertices.Contains(x))).ToList();

                var solver = new MIP(solver: "ORT");
                solver.initialize(new Graph(restrictedEdges, _graph.Vertices.Where(_=>!highDegreeVertices.Contains(_)).ToList()));
                var time = new Stopwatch();
                time.Start();
                edgeCover = solver.Run(parameters).cover.ToList();
                time.Stop();
                Console.WriteLine("partial time: " + time.ElapsedMilliseconds);

                var tmpGraphEdges = edgeCover.ToList(); ;
                foreach(var edge in _graph.Edges)
                {
                    foreach (var vertex in edge.Vertices)
                    {
                        if (highDegreeVertices.Contains(vertex)&& (edge.VertexCount==1|| edgeCover.Contains(new Edge(edge.Vertices.Where(_ => _ != vertex).ToList()))))
                            tmpGraphEdges.Add(edge);
                    }
                    if (edge.VertexCount == 2 && highDegreeVertices.Contains(edge.vertex0) && highDegreeVertices.Contains(edge.vertex1))
                        tmpGraphEdges.Add(edge);
                }
                var tmpGraph = new Graph(tmpGraphEdges, _graph.Vertices);
                tmpGraph.ResetVertexAdjEdges();
                solver = new MIP();
                solver.initialize(tmpGraph);


                var edgeCover2 = solver.Run(parameters).cover.ToList();
                ;
                return (edgeCover2, -1);
            }
            else if (mode == "minDegree")
            {
                var subgraphEdges = new List<Edge>();
                int minDegree = 3;
                var degree = new int[verticesCopy.Count];

                edgesCopy = _graph.Edges;
                edgesCopy.Shuffle();
                var newEdges = new List<Edge>();

                for(int i = 0; i< edgesCopy.Count; i++)
                {
                    var edge = edgesCopy[i];
                    if ((degree[edge.Vertices[0].Id] < additionalNewCalculatedEdges)
                        || (edge.Vertices.Count > 1 && degree[edge.Vertices[1].Id] < minDegree)
                        || (edge.Vertices.Count > 2 && degree[edge.Vertices[2].Id] < minDegree))
                    {
                        for(int j = 0; j < edge.Vertices.Count; j++)
                        {
                            degree[edge.VerticesIds[j]]++;
                        }
                        newEdges.Add(edge);
                    }
                }

                edgesCopy = newEdges.Concat(edgesCopy.Where(_=>_.Vertices.Count==1)).ToList();
            }
            else if (mode == "partialOptimal")
            {
                int sizeOfS = 120;
                var uncoveredVertices = _graph.Vertices.OrderBy(x => _random.Next()).ToList();
                var s = new List<Vertex>();
                while (uncoveredVertices.Count() > 0)
                {
                    s = s.Concat(uncoveredVertices.Take(sizeOfS)).ToList();
                    foreach (var vertex in s)
                        uncoveredVertices.Remove(vertex);
                    var eEdgesOfS = _edges.Where(_ => _.Vertices.Where(x => !s.Contains(x)).Count() == 0).ToList();

                    var solver2 = new MIP();
                    solver2.initialize(new Graph(eEdgesOfS, s));
                    var partialRes = solver2.Run(parameters).cover;//).ToList();
                    if (uncoveredVertices.Count() == 0)
                    {
                        edgeCover = edgeCover.Concat(partialRes).ToList();
                        notFinished = false;
                        break;
                    }
                    foreach (var edge in partialRes)
                    {
                        if (edge.Vertices.Count == 3)
                        {
                            edgeCover.Add(edge);
                            foreach (var vertex in edge.Vertices)
                                s.Remove(vertex);
                        }
                    }
                }
            }
            else if (mode=="initalSolution")
            {

                var solver2 = new TwoDBased();
                solver2.initialize(_graph);
                var twoDRes = solver2.Run(parameters).cover;

                //_graph.InitializeFor2DMatchin();
                //var twoDRes = _graph.GetMaximum2DMatching();
                var edges = _graph.Edges;
                GRBEnv env = new GRBEnv(true);
                env.Set("OutputFlag", "0");
                env.Start();

                GRBModel solver = new GRBModel(env);
                var x = edges.Select(_ => solver.AddVar(0.0, 1.0, 1.0, GRB.BINARY, String.Join(" ", _.Vertices))).ToArray();
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

                for(int i = 0; i< edges.Count; i++)
                {
                    if (twoDRes.Contains(edges[i]))
                    {
                        x[i].Start = 1;
                    }
                    else
                    {
                        x[i].Start = 0;

                    }
                }


                solver.Optimize();
                var res = new List<Edge>();
                for (int j = 0; j < x.Length; j++)
                {
                    if (x[j].X != 0)//Variable(j).SolutionValue() != 0)
                        res.Add(edges[j]);
                }
                return (res, 1);
            }


            if (notFinished)
            {
                var time = new Stopwatch();
                var solver = new MIP();
                solver.initialize(new Graph(edgesCopy, verticesCopy));
                time.Start();
                edgeCover = edgeCover.Concat(solver.Run(parameters).cover).ToList();
            time.Stop();
            Console.WriteLine("partial time: " + time.ElapsedMilliseconds);
            }

            return (edgeCover, -1);
        }
    }
}
