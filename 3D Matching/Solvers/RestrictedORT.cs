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
        int additionalNewCalculatedEdges=7;
        int preCalculationTime=10;
        String mode = "normal";
        Random _random = new Random();
        public override String Name { get => this.GetType().Name + "(" + additionalNewCalculatedEdges+"|"+preCalculationTime+"|"+mode+")"; }
        public RORTS(int additionalNewCalculatedEdges, int preCalculationTime = 10, String mode = "normal")
        {
            this.additionalNewCalculatedEdges = additionalNewCalculatedEdges;
            this.preCalculationTime = preCalculationTime;
            this.mode = mode;
        }
        public  List<Edge> Run2(Dictionary<string, double> parameters)
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
            var solver2 = new ORTS();
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
            var edgeCover = new List<Edge>();
            var notFinished = true;
            if (mode == "normal")
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
            } else if (mode == "artificalThinning")
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
            } else if (mode == "minDegree")
            {
                var subgraphEdges = new List<Edge>();
                int minDegree = 3;

                var subgraphDegree = new int[verticesCopy.Count];
                foreach (var vertex in verticesCopy)
                {
                    int i = 0;
                    while (edgesCopy.Count > i)
                    {
                        if (subgraphDegree[vertex.Id] >= minDegree)
                            break;

                        var edge = edgesCopy[i];
                        if (edge.Vertices.Contains(vertex))
                        {
                            edgesCopy.Remove(edge);
                            subgraphEdges.Add(edge);
                            foreach (var vertexId in edge.Vertices.Select(_ => _.Id))
                                subgraphDegree[vertexId]++;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
                edgesCopy = subgraphEdges.Concat(edgesCopy.Where(_ => _.Vertices.Count == 1)).ToList();
            } else if (mode == "partialOptimal")
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

                    var solver2 = new ORTS();
                    solver2.initialize(new Graph(eEdgesOfS,s));
                    var partialRes = solver2.Run(parameters).cover;//).ToList();
                    if(uncoveredVertices.Count() == 0)
                    {
                        edgeCover = edgeCover.Concat(partialRes).ToList();
                        notFinished = false;
                        break;
                    }
                    foreach(var edge in partialRes)
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
            if (notFinished)
            {
            var solver = new ORTS();
            solver.initialize(new Graph(edgesCopy,verticesCopy));
            edgeCover = edgeCover.Concat(solver.Run(parameters).cover).ToList();
            }
            return (edgeCover,-1);
        }
    }
}
