using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _3D_Matching.Tests;

namespace _3D_Matching.Solvers
{
    public class Genetic : IMinimumEdgecoveringSolver
    {
        Random _random = new Random();
        String _type = "";
        private bool _precalc;
        public Genetic(String type = "", bool precalc = false)
        {
            _type = type;
            _precalc = precalc;
        }

        public override String Name { get => this.GetType().Name + "|" + _type; }

        public override (List<Edge> cover, int iterations) Run(Dictionary<string, double> parameters)
        {
            double maxTime = parameters["maxTime"];
            var time = new Stopwatch();
            var res = new List<Edge>();
            time.Start();

            var bestResCount = int.MaxValue;
            var vertexCount = _graph.Vertices.Count;
            var iteration = 0;
            var isCovored = new int[vertexCount];
            var notOneSizeEdges = _edges.Where(_ => _.Vertices.Count != 1).Count();
            List<List<int>> edges = new List<List<int>>();

            if (_type == "")
            {
                if (_precalc)
                {
                    var _precalcSolver = new Greedy();
                    _precalcSolver.initialize(_graph);
                    var tmpParams = new Dictionary<String, double>();
                    tmpParams.Add("maxIter", 1000);
                    tmpParams.Add("maxTime", 80);
                    var preRes = _precalcSolver.Run(tmpParams);
                    edges = preRes.cover.Where(_ => _.Vertices.Count != 1).Concat(_edges.Where(_ => !preRes.cover.Contains(_) && _.Vertices.Count != 1)).Concat(_edges.Where(_ => _.Vertices.Count == 1)).Select(_ => _.VerticesIds).ToList();
                    bestResCount = preRes.cover.Count;
                    Console.WriteLine(bestResCount);
                }
                else
                {
                    //edges = _edges.Select(_ => _.VerticesIds).OrderBy(_ => _.Count == 1 ? 1 : 0).ToList();
                    //edges = _edges.Select(_ => _.VerticesIds).OrderBy(_ => -_.Count).ToList();
                    edges = _edges.Select(_ => _.VerticesIds).Where(_ => _.Count != 1 /*&& _random.NextDouble() < 0.3)*/).Concat(_edges.Select(_ => _.VerticesIds).Where(_ => _.Count == 1)).ToList();
                }
                //notOneSizeEdges = edges.Where(_ => _.Count != 1).Count();

                int index = 0;
                while (time.ElapsedMilliseconds < maxTime && parameters["maxIter"] > iteration)
                {
                    iteration++;
                    isCovored = new int[vertexCount];
                    index = _random.Next(notOneSizeEdges - 10);

                    foreach (var i in edges[index])
                        isCovored[i] = edges[index].Count;
                    var amountOfCoveredVertices = edges[index].Count;
                    var coverSize = 1;

                    CalculateAssociatedMatching(bestResCount, vertexCount, isCovored, edges, ref amountOfCoveredVertices, ref coverSize);

                    if (coverSize <= bestResCount && amountOfCoveredVertices == vertexCount)
                    {
                        //Console.WriteLine(coverSize);
                        bestResCount = coverSize;
                        var tmp = edges[index];
                        edges.RemoveAt(index);
                        edges.Insert(0, tmp);

                        //var test = ReconstructResult(vertexCount, out isCovored, edges);
                        //edges = edges.OrderBy(_ => test.Contains(new Edge(_.Select(x => _graph.Vertices[x]).ToList())) ? _.Count==0?1:0 : _random.NextDouble()).ToList();

                    }
                }
                List<Edge> result = ReconstructResult(vertexCount, out isCovored, edges);

                return (result, iteration);
            }
            else if (_type == "expandFrom2D")
            {
                var bestRes = _graph.Edges.ToList();
                var twoDEdges = _graph.TwodEdges();
                while (time.ElapsedMilliseconds < maxTime)
                {
                    iteration++;
                    var oldPermutation = twoDEdges.ToList();
                    res = new List<Edge>();



                    //var a = _random.Next(vertexCount);
                    //var b = 0;
                    //if (bestRes.Count != _graph.Edges.Count)
                    //{

                    //    for (int i = _random.Next(isCovored.Length / 2); i < isCovored.Length; i++)
                    //    {
                    //        if (isCovored[i] > 0)
                    //        {
                    //            b = isCovored[i] - 1;
                    //            break;
                    //        }
                    //    }
                    //}
                    isCovored = new int[vertexCount];
                    //var tmp = twoDEdges[a];
                    //twoDEdges[a] = twoDEdges[b];
                    //twoDEdges[b] = tmp;

                    var tmp = twoDEdges[_random.Next(twoDEdges.Count)];
                    twoDEdges.Remove(tmp);
                    twoDEdges.Insert(0, tmp);

                    int a, b = 0;
                    for (int i = 0; i < 30; i++)
                    {
                        a = _random.Next(vertexCount);
                        b = _random.Next(vertexCount);
                        tmp = twoDEdges[a];
                        twoDEdges[a] = twoDEdges[b];
                        twoDEdges[b] = tmp;
                    }
                    //twoDEdges = twoDEdges.OrderBy(_ => _random.Next()).ToList();




                    for (int i = 0; i < twoDEdges.Count; i++)
                    {
                        var edge2 = twoDEdges[i];
                        if (isCovored[edge2.vertex1.Id] != 0)
                            continue;

                        if (isCovored[edge2.vertex0.Id] == 0 && isCovored[edge2.vertex1.Id] == 0)
                        {
                            isCovored[edge2.vertex0.Id] = i + 1;
                            isCovored[edge2.vertex1.Id] = i + 1;
                            res.Add(edge2);
                            continue;
                        }
                        var previous2DEdgeIndex = isCovored[edge2.vertex0.Id] - 1;
                        if (previous2DEdgeIndex < 0)
                            continue;
                        if (isCovored[edge2.vertex1.Id] == 0 && edge2.Expandables().Contains(twoDEdges[previous2DEdgeIndex].vertex0))
                        {
                            isCovored[twoDEdges[previous2DEdgeIndex].vertex0.Id] = -1;
                            isCovored[edge2.vertex0.Id] = -1;
                            isCovored[edge2.vertex1.Id] = -1;

                            res.Remove(twoDEdges[previous2DEdgeIndex]);
                            res.Add(new Edge(new List<Vertex>() { twoDEdges[previous2DEdgeIndex].vertex0, edge2.vertex0, edge2.vertex1 }));
                        }
                    }
                    for (int i = 0; i < isCovored.Length; i++)
                    {
                        if (isCovored[i] == 0)
                        {
                            res.Add(new Edge(new List<Vertex>() { _graph.Vertices[i] }));
                        }
                    }
                    //try { 
                    //SolverTester.IsCover(_graph, res);
                    //}
                    //catch
                    //{
                    //    ;
                    //}
                    if (res.Count <= bestRes.Count)
                    {
                        bestRes = res.ToList();
                        //Console.WriteLine(res.Count);
                    }
                    else
                    {
                        twoDEdges = oldPermutation.ToList();
                    }

                }
                return (bestRes, iteration);
            }
            else if (_type == "partialSolving")
            {
                var allEdges = _graph.Edges.Where(_ => _.VertexCount != 1).Select(_ => _.VerticesIds).ToList();
                var partialEdges = allEdges.Where(_ => _random.NextDouble() < 1).ToList();
                var inicesOfBestSolution = new List<int>();

                bestResCount = int.MaxValue;

                var matchedVertices = new bool[vertexCount];
                while (time.ElapsedMilliseconds < maxTime && iteration < parameters["maxIter"])
                {
                    bestResCount = int.MaxValue;
                    Console.WriteLine("-----");
                    for (int i = 0; i < 1000; i++)
                    {
                        iteration++;
                        matchedVertices = new bool[vertexCount];
                        int matchingSize = vertexCount;
                        var newForcedEdgeIndex = _random.Next(partialEdges.Count);

                        for (int vertexIndex = 0; vertexIndex < partialEdges[newForcedEdgeIndex].Count; vertexIndex++)
                        {
                            matchedVertices[partialEdges[newForcedEdgeIndex][vertexIndex]] = true;
                        }
                        matchingSize += -partialEdges[newForcedEdgeIndex].Count + 1;



                        for (int edgeIndex = 0; edgeIndex < partialEdges.Count; edgeIndex++)
                        {
                            bool isUncovered = true;
                            for (int vertexIndex = 0; vertexIndex < partialEdges[edgeIndex].Count; vertexIndex++)
                            {
                                if (matchedVertices[partialEdges[edgeIndex][vertexIndex]])
                                {
                                    isUncovered = false;
                                    break;
                                }
                            }
                            if (isUncovered)
                            {
                                for (int vertexIndex = 0; vertexIndex < partialEdges[edgeIndex].Count; vertexIndex++)
                                {
                                    matchedVertices[partialEdges[edgeIndex][vertexIndex]] = true;
                                }
                                matchingSize += -partialEdges[edgeIndex].Count + 1;
                            }
                        }

                        if (matchingSize <= bestResCount)
                        {
                            Console.WriteLine(matchingSize);
                            var tmp = partialEdges[newForcedEdgeIndex];
                            partialEdges.RemoveAt(newForcedEdgeIndex);
                            partialEdges.Insert(0, tmp);
                            bestResCount = matchingSize;
                        }


                    }


                    inicesOfBestSolution = new List<int>();
                    var newPartialEdges = new List<List<int>>();
                    matchedVertices = new bool[vertexCount];

                    for (int edgeIndex = 0; edgeIndex < partialEdges.Count; edgeIndex++)
                    {
                        bool isUncovered = true;
                        for (int vertexIndex = 0; vertexIndex < partialEdges[edgeIndex].Count; vertexIndex++)
                        {
                            if (matchedVertices[partialEdges[edgeIndex][vertexIndex]])
                            {
                                isUncovered = false;
                                break;
                            }
                        }
                        if (isUncovered)
                        {
                            for (int vertexIndex = 0; vertexIndex < partialEdges[edgeIndex].Count; vertexIndex++)
                            {
                                matchedVertices[partialEdges[edgeIndex][vertexIndex]] = true;
                            }
                            inicesOfBestSolution.Add(edgeIndex);
                        }
                    }
                    foreach (var index in inicesOfBestSolution)
                    {
                        newPartialEdges.Add(partialEdges[index]);
                    }
                    partialEdges = newPartialEdges.Concat(allEdges.Where(_ => _random.NextDouble() < 0.1)).OrderBy(_ => _random.Next()).ToList();
                }

                res = new List<Edge>();
                matchedVertices = new bool[vertexCount];

                for (int edgeIndex = 0; edgeIndex < partialEdges.Count; edgeIndex++)
                {
                    bool isUncovered = true;
                    for (int vertexIndex = 0; vertexIndex < partialEdges[edgeIndex].Count; vertexIndex++)
                    {
                        if (matchedVertices[partialEdges[edgeIndex][vertexIndex]])
                        {
                            isUncovered = false;
                            break;
                        }
                    }
                    if (isUncovered)
                    {
                        for (int vertexIndex = 0; vertexIndex < partialEdges[edgeIndex].Count; vertexIndex++)
                        {
                            matchedVertices[partialEdges[edgeIndex][vertexIndex]] = true;
                        }
                        res.Add(new Edge(partialEdges[edgeIndex].Select(x => _graph.Vertices[x]).ToList()));
                    }
                }
                for (int i = 0; i < matchedVertices.Length; i++)
                {
                    if (matchedVertices[i] != true)
                        res.Add(new Edge(new List<Vertex>() { _graph.Vertices[i] }));
                }
                return (res, iteration);




            }
            else if (_type == "3DRandomAnd2DBlossom")
            {
                var matching3 = new List<Edge>();
                var coveredBy3DEdges = new bool[vertexCount];
                
                var all3DEdges = _graph.Edges.Where(_ => _.VertexCount == 3).ToList();      //hier sollten die indices auch reichen

                //var tmpSolver = new ORTS();
                //tmpSolver.initialize(_graph);
                //var ortRes = tmpSolver.Run(parameters).cover;

                _graph.ResetVertexAdjEdges();
                var bestMatching3 = new List<Edge>();
                while (time.ElapsedMilliseconds < maxTime && parameters["maxIter"] > iteration)
                {
                    //all3DEdges = all3DEdges.OrderBy(_ => ortRes.Contains(_)?0:1).ToList();
                    all3DEdges = all3DEdges.OrderBy(_ => /*_.Vertices.Select(v=>v.AdjEdges.Count).Sum()*/ _random.Next()).ToList();
                    ;
                    matching3 = new List<Edge>();
                    coveredBy3DEdges = new bool[vertexCount];
                    isCovored = new int[vertexCount];
                    for (int edgeIndex = 0; edgeIndex < all3DEdges.Count; edgeIndex++)
                    {

                        var edge3 = all3DEdges[edgeIndex];
                        bool skip = false;
                        for (int i = 0; i < edge3.Vertices.Count; i++)
                        {
                            var vertexId = edge3.VerticesIds[i];
                            if (isCovored[vertexId] != 0)
                            {
                                skip = true;
                                break;
                            }
                        }

                        if (skip)
                            continue;

                        for (int i = 0; i < edge3.VerticesIds.Count; i++)
                        {
                            var vertexId = edge3.VerticesIds[i];
                            isCovored[vertexId] = edge3.VerticesIds.Count;
                        }
                        matching3.Add(edge3);
                        if (matching3.Count >  _random.Next(30,50))
                            break;

                    }
                    foreach (var edge3 in matching3)
                        foreach (var id in edge3.VerticesIds)
                            coveredBy3DEdges[id] = true;
                    iteration++;
                    var coverSize = 0;

                    //var removed3DEdge = matching3[_random.Next(matching3.Count)];
                    //matching3.Remove(removed3DEdge);
                    //var added3DEdge = all3DEdges.OrderBy(_ => _random.Next()).FirstOrDefault(_ => _.VerticesIds.All(x => !coveredBy3DEdges[x]));
                    //foreach (var id in removed3DEdge.VerticesIds)
                    //    coveredBy3DEdges[id] = false;
                    //if (added3DEdge != null)
                    //{
                    //    matching3.Add(added3DEdge);
                    //    foreach (var id in added3DEdge.VerticesIds)
                    //        coveredBy3DEdges[id] = true;
                    //}


                    var tmpGraph = _graph.GenerateInducedSubgraph(_graph.Vertices.Where(_ => !coveredBy3DEdges[_.Id]).ToList());
                    tmpGraph.ResetVertexAdjEdges();
                    tmpGraph.InitializeFor2DMatchin();
                    var matching2 = tmpGraph.GetMaximum2DMatching().maxMmatching;

                    //tmpSolver.initialize(tmpGraph);
                    //var ortRes2 = tmpSolver.Run(parameters).cover;

                    var test = new bool[_graph.Vertices.Count];
                    foreach (var edge in matching2.Concat(matching3))
                    {
                        foreach (var id in edge.VerticesIds)
                        {
                            if (test[id])
                                ;
                            test[id] = true;
                        }
                    }
                    int z = 0;
                    while (z < test.Length && test[z])
                    {
                        z++;
                    }
                        Console.WriteLine(matching2.Count +"|"+ matching3.Count + "|" + (int)(matching2.Count + matching3.Count));
                    if (matching2.Count + matching3.Count <= bestResCount)
                    {
                        bestResCount = matching2.Count + matching3.Count;
                        bestMatching3 = matching3.ToList();
                    }
                    else
                    {
                        //foreach (var id in added3DEdge.VerticesIds)
                        //    coveredBy3DEdges[id] = false;
                        //foreach (var id in removed3DEdge.VerticesIds)
                        //    coveredBy3DEdges[id] = true;
                        //matching3.Remove(added3DEdge);
                        //matching3.Add(removed3DEdge);
                    }
                }


                coveredBy3DEdges = new bool[vertexCount];
                foreach (var edge3 in bestMatching3)
                    foreach (var id in edge3.VerticesIds)
                        coveredBy3DEdges[id] = true;

                var lastGraph = _graph.GenerateInducedSubgraph(_graph.Vertices.Where(_ => !coveredBy3DEdges[_.Id]).ToList());
                lastGraph.InitializeFor2DMatchin();
                var lastMatching2 = lastGraph.GetMaximum2DMatching().maxMmatching;
                List<Edge> result = bestMatching3.Concat(lastMatching2).ToList();

                return (result, iteration);
            }


            return (null, -1);

        }

        private static void CalculateAssociatedMatching(int bestResCount, int vertexCount, int[] isCovored, List<List<int>> edges, ref int amountOfCoveredVertices, ref int coverSize)
        {
            for (int edgeIndex = 0; edgeIndex < edges.Count; edgeIndex++)
            {

                var edge = edges[edgeIndex];
                bool skip = false;
                for (int i = 0; i < edge.Count; i++)
                {
                    var vertexId = edge[i];
                    if (isCovored[vertexId] != 0)
                    {
                        skip = true;
                        break;
                    }
                }
                if (skip)
                    continue;

                for (int i = 0; i < edge.Count; i++)
                {
                    var vertexId = edge[i];
                    isCovored[vertexId] = edge.Count;
                    amountOfCoveredVertices++;
                }

                coverSize++;
            }
        }

        private static List<Edge> ReconstructResult(int vertexCount, out int[] isCovored, List<List<int>> edges)
        {
            var result = new List<Edge>();
            isCovored = new int[vertexCount];

            for (int edgeIndex = 0; edgeIndex < edges.Count; edgeIndex++)
            {
                var edge = edges[edgeIndex];
                bool skip = false;
                foreach (var i in edge)
                {
                    if (isCovored[i] != 0)
                    {
                        skip = true;
                        break;

                    }
                }
                if (skip)
                    continue;

                foreach (var i in edge)
                {
                    isCovored[i] = edge.Count;
                }
                result.Add(new Edge(edge.Select(_ => new Vertex(_)).ToList()));
            }
            return result;
        }

    }

}
