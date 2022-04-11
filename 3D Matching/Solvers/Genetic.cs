using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _3D_Matching.Tests;

namespace _3D_Matching.Solvers
{

    enum GeneticModi : int
    {
        permuteAllEdges,
        permuteAll2EdgesAndUnion,
        permuteAll3EdgesAndFillOptimal,
        random3EdgesAndFillOptimal,


    }
    class Genetic : IMinimumPerfectMatchingSolver
    {
        Random _random = new Random();
        GeneticModi _mode = 0;
        public Genetic(GeneticModi mode = 0)
        {
            _mode = mode;
        }

        public override String Name { get => this.GetType().Name + "|" + _mode; }

        public override (List<Edge> cover, int iterations) Run(Dictionary<string, double> parameters)
        {
            double maxTime = parameters["maxTime"];
            var time = new Stopwatch();
            var res = new List<Edge>();
            time.Start();

            var bestResCount = int.MaxValue;
            var vertexCount = _graph.Vertices.Count;
            var iteration = 0;
            var isCovoredBySizeOf = new int[vertexCount];
            var notOneSizeEdges = _edges.Where(_ => _.Vertices.Count != 1).Count();
            List<List<int>> edges = new List<List<int>>();

            var sizes = new List<double>();
            if (_mode == GeneticModi.permuteAllEdges)
            {
                var iterationAndSize = new List<(int, int)>();

                    edges = _edges.Select(_ => _.VerticesIds).Where(_ => _.Count != 1 /*&& _random.NextDouble() < 0.3)*/).OrderBy(_=>-_.Count+ 0.0001 * _random.NextDouble()).Concat(_edges.Select(_ => _.VerticesIds).Where(_ => _.Count == 1)).ToList();

                int index = 0;
                while (time.ElapsedMilliseconds < maxTime && parameters["maxIter"] > iteration)
                {
                    iteration++;
                    isCovoredBySizeOf = new int[vertexCount];
                    index = _random.Next(edges.Count);

                    foreach (var i in edges[index])
                        isCovoredBySizeOf[i] = edges[index].Count;
                    var amountOfCoveredVertices = edges[index].Count;
                    var coverSize = 1;

                    CalculateAssociatedMatching(bestResCount, vertexCount, isCovoredBySizeOf, edges, ref amountOfCoveredVertices, ref coverSize);

                    //Console.Write("," + coverSize);
                    sizes.Add(coverSize);

                    if (coverSize <= bestResCount && amountOfCoveredVertices == vertexCount)
                    {
                        //Console.WriteLine(coverSize);

                        if (coverSize < bestResCount && amountOfCoveredVertices == vertexCount)
                        {
                            iterationAndSize.Add((coverSize, iteration));
                        }
                        bestResCount = coverSize;
                        var tmp = edges[index];
                        edges.RemoveAt(index);
                        edges.Insert(0, tmp);

                        //var test = ReconstructResult(vertexCount, out isCovored, edges);
                        //edges = edges.OrderBy(_ => test.Contains(new Edge(_.Select(x => _graph.Vertices[x]).ToList())) ? _.Count==0?1:0 : _random.NextDouble()).ToList();

                    }

                }
                //Console.WriteLine("[" + String.Join(",", iterationAndSize.Select(_ => _.Item1)) + "]");
                //Console.WriteLine("[" + String.Join(",", iterationAndSize.Select(_ => _.Item2)) + "]");
                List<Edge> result = ReconstructResult(vertexCount, out isCovoredBySizeOf, edges);

                //Plot.CreateFigure(sizes, plottype: "f");
                //Plot.AddLine("genetic = [" + String.Join(",",sizes) +"]");
                return (result, iteration);
            }
            else if (_mode == GeneticModi.permuteAll3EdgesAndFillOptimal)
            {
                var iterationAndSize = new List<(int, int)>();
                edges = _edges.Select(_ => _.VerticesIds).Where(_ => _.Count != 1 /*&& _random.NextDouble() < 0.3)*/).OrderBy(_ => -_.Count + 0.0001 * _random.NextDouble()).Concat(_edges.Select(_ => _.VerticesIds).Where(_ => _.Count == 1)).ToList();
                    var edges3 = _graph.Edges.Where(_ => _.VertexCount == 3).Select(_=>_.VerticesIds).ToList();
                int bestMatchingSize = int.MaxValue;
                int index = 0;
                _graph.InitializeFor2DMatchin();
                //_graph.ComputeMaximum2DMatching();
                while (time.ElapsedMilliseconds < maxTime && parameters["maxIter"] > iteration)
                {
                    bool newOptWasFound = false;
                    iteration++;
                    isCovoredBySizeOf = new int[vertexCount];
                    index = _random.Next(edges3.Count);

                    foreach (var i in edges3[index])
                        isCovoredBySizeOf[i] = 3;
                    var amountOfCoveredVertices = 3;
                    var cover3Size = 1;

                    for (int edge3Index = 0; edge3Index < edges3.Count; edge3Index++)
                    {
                        var edge3 = edges3[edge3Index];
                        bool skip = false;
                        for (int i = 0; i < edge3.Count; i++)
                        {
                            var vertexId = edge3[i];
                            if (isCovoredBySizeOf[vertexId] != 0)
                            {
                                skip = true;
                                break;
                            }
                        }
                        if (skip)
                            continue;

                        for (int i = 0; i < edge3.Count; i++)
                        {
                            var vertexId = edge3[i];
                            isCovoredBySizeOf[vertexId] = edge3.Count;
                            amountOfCoveredVertices++;
                        }

                        cover3Size++;
                        //compute best remaining 2D matching

                        for (int i = 0; i < isCovoredBySizeOf.Length; i++)
                        {
                            _graph.Vertices[i].IsContracted = (0 != isCovoredBySizeOf[i]);
                            //_graph.Vertices[i].MatchedVertex = null;
                            if (isCovoredBySizeOf[i] != 0 && _graph.Vertices[i].MatchedVertex != null)
                            {
                                _graph.Vertices[i].MatchedVertex.MatchedVertex = null;
                                _graph.Vertices[i].MatchedVertex = null;
                            }
                        }
                        var tmpRes2 = _graph.GetMaximum2DMatching().maxMmatching;

                        if (tmpRes2.Count + cover3Size <= bestMatchingSize)
                        {
                            if (tmpRes2.Count + cover3Size < bestMatchingSize)
                                iterationAndSize.Add((tmpRes2.Count + cover3Size, iteration));

                            bestMatchingSize = tmpRes2.Count + cover3Size;
                            newOptWasFound = true;
                        }


                        //---------------------------------
                    }
                    //Console.Write("," + coverSize);
                    if (newOptWasFound)
                    {
                        var tmp = edges3[index];
                        edges3.RemoveAt(index);
                        edges3.Insert(0, tmp);
                    }
                }
                //List<Edge> result = ReconstructResult(vertexCount, out isCovored, edges);

                Console.WriteLine("[" + String.Join(",",iterationAndSize.Select(_=>_.Item1)) + "]");
                Console.WriteLine("[" + String.Join(",",iterationAndSize.Select(_=>_.Item2)) + "]");


                isCovoredBySizeOf = new int[vertexCount];
                var coverSize = 0;
                var matching3DEdges = new List<Edge>();
                for (int edge3Index = 0; edge3Index < edges3.Count; edge3Index++)
                {
                    var edge3 = edges3[edge3Index];
                    bool skip = false;
                    for (int i = 0; i < edge3.Count; i++)
                    {
                        var vertexId = edge3[i];
                        if (isCovoredBySizeOf[vertexId] != 0)
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (skip)
                        continue;

                    for (int i = 0; i < edge3.Count; i++)
                    {
                        var vertexId = edge3[i];
                        isCovoredBySizeOf[vertexId] = edge3.Count;
                    }
                    matching3DEdges.Add(new Edge(edge3.Select(_ => _graph.Vertices[_]).ToList()));
                    coverSize++;
                    //compute best remaining 2D matching

                    for (int i = 0; i < isCovoredBySizeOf.Length; i++)
                    {
                        _graph.Vertices[i].IsContracted = (0 != isCovoredBySizeOf[i]);
                        if (isCovoredBySizeOf[i] != 0 && _graph.Vertices[i].MatchedVertex != null)
                        {
                            _graph.Vertices[i].MatchedVertex.MatchedVertex = null;
                            _graph.Vertices[i].MatchedVertex = null;
                        }
                    }
                    var tmpRes2 = _graph.GetMaximum2DMatching().maxMmatching;
                    if (tmpRes2.Count + coverSize <= bestMatchingSize)
                    {
                        matching3DEdges.AddRange(tmpRes2);
                        return (matching3DEdges, iteration);
                    }
                }

                            return (null, iteration);

                //Plot.CreateFigure(sizes, plottype: "f");
                //Plot.AddLine("genetic = [" + String.Join(",",sizes) +"]");
            }
            else if (_mode == GeneticModi.permuteAll2EdgesAndUnion)
            {
                var bestRes = _graph.Edges.ToList();
                var twoDEdges = _graph.TwodEdges().ToList();
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
                    isCovoredBySizeOf = new int[vertexCount];
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
                        if (isCovoredBySizeOf[edge2.vertex1.Id] != 0)
                            continue;

                        if (isCovoredBySizeOf[edge2.vertex0.Id] == 0 && isCovoredBySizeOf[edge2.vertex1.Id] == 0)
                        {
                            isCovoredBySizeOf[edge2.vertex0.Id] = i + 1;
                            isCovoredBySizeOf[edge2.vertex1.Id] = i + 1;
                            res.Add(edge2);
                            continue;
                        }
                        var previous2DEdgeIndex = isCovoredBySizeOf[edge2.vertex0.Id] - 1;
                        if (previous2DEdgeIndex < 0)
                            continue;
                        if (isCovoredBySizeOf[edge2.vertex1.Id] == 0 && edge2.Expandables().Contains(twoDEdges[previous2DEdgeIndex].vertex0))
                        {
                            isCovoredBySizeOf[twoDEdges[previous2DEdgeIndex].vertex0.Id] = -1;
                            isCovoredBySizeOf[edge2.vertex0.Id] = -1;
                            isCovoredBySizeOf[edge2.vertex1.Id] = -1;

                            res.Remove(twoDEdges[previous2DEdgeIndex]);
                            res.Add(new Edge(new List<Vertex>() { twoDEdges[previous2DEdgeIndex].vertex0, edge2.vertex0, edge2.vertex1 }));
                        }
                    }
                    for (int i = 0; i < isCovoredBySizeOf.Length; i++)
                    {
                        if (isCovoredBySizeOf[i] == 0)
                        {
                            res.Add(new Edge(new List<Vertex>() { _graph.Vertices[i] }));
                        }
                    }

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
            //else if (_mode == "partialSolving")
            //{
            //    var allEdges = _graph.Edges.Where(_ => _.VertexCount != 1).Select(_ => _.VerticesIds).ToList();
            //    var partialEdges = allEdges.Where(_ => _random.NextDouble() < 1).ToList();
            //    var inicesOfBestSolution = new List<int>();

            //    bestResCount = int.MaxValue;

            //    var matchedVertices = new bool[vertexCount];
            //    while (time.ElapsedMilliseconds < maxTime && iteration < parameters["maxIter"])
            //    {
            //        bestResCount = int.MaxValue;
            //        Console.WriteLine("-----");
            //        for (int i = 0; i < 1000; i++)
            //        {
            //            iteration++;
            //            matchedVertices = new bool[vertexCount];
            //            int matchingSize = vertexCount;
            //            var newForcedEdgeIndex = _random.Next(partialEdges.Count);

            //            for (int vertexIndex = 0; vertexIndex < partialEdges[newForcedEdgeIndex].Count; vertexIndex++)
            //            {
            //                matchedVertices[partialEdges[newForcedEdgeIndex][vertexIndex]] = true;
            //            }
            //            matchingSize += -partialEdges[newForcedEdgeIndex].Count + 1;



            //            for (int edgeIndex = 0; edgeIndex < partialEdges.Count; edgeIndex++)
            //            {
            //                bool isUncovered = true;
            //                for (int vertexIndex = 0; vertexIndex < partialEdges[edgeIndex].Count; vertexIndex++)
            //                {
            //                    if (matchedVertices[partialEdges[edgeIndex][vertexIndex]])
            //                    {
            //                        isUncovered = false;
            //                        break;
            //                    }
            //                }
            //                if (isUncovered)
            //                {
            //                    for (int vertexIndex = 0; vertexIndex < partialEdges[edgeIndex].Count; vertexIndex++)
            //                    {
            //                        matchedVertices[partialEdges[edgeIndex][vertexIndex]] = true;
            //                    }
            //                    matchingSize += -partialEdges[edgeIndex].Count + 1;
            //                }
            //            }

            //            if (matchingSize <= bestResCount)
            //            {
            //                Console.WriteLine(matchingSize);
            //                var tmp = partialEdges[newForcedEdgeIndex];
            //                partialEdges.RemoveAt(newForcedEdgeIndex);
            //                partialEdges.Insert(0, tmp);
            //                bestResCount = matchingSize;
            //            }


            //        }


            //        inicesOfBestSolution = new List<int>();
            //        var newPartialEdges = new List<List<int>>();
            //        matchedVertices = new bool[vertexCount];

            //        for (int edgeIndex = 0; edgeIndex < partialEdges.Count; edgeIndex++)
            //        {
            //            bool isUncovered = true;
            //            for (int vertexIndex = 0; vertexIndex < partialEdges[edgeIndex].Count; vertexIndex++)
            //            {
            //                if (matchedVertices[partialEdges[edgeIndex][vertexIndex]])
            //                {
            //                    isUncovered = false;
            //                    break;
            //                }
            //            }
            //            if (isUncovered)
            //            {
            //                for (int vertexIndex = 0; vertexIndex < partialEdges[edgeIndex].Count; vertexIndex++)
            //                {
            //                    matchedVertices[partialEdges[edgeIndex][vertexIndex]] = true;
            //                }
            //                inicesOfBestSolution.Add(edgeIndex);
            //            }
            //        }
            //        foreach (var index in inicesOfBestSolution)
            //        {
            //            newPartialEdges.Add(partialEdges[index]);
            //        }
            //        partialEdges = newPartialEdges.Concat(allEdges.Where(_ => _random.NextDouble() < 0.1)).OrderBy(_ => _random.Next()).ToList();
            //    }

            //    res = new List<Edge>();
            //    matchedVertices = new bool[vertexCount];

            //    for (int edgeIndex = 0; edgeIndex < partialEdges.Count; edgeIndex++)
            //    {
            //        bool isUncovered = true;
            //        for (int vertexIndex = 0; vertexIndex < partialEdges[edgeIndex].Count; vertexIndex++)
            //        {
            //            if (matchedVertices[partialEdges[edgeIndex][vertexIndex]])
            //            {
            //                isUncovered = false;
            //                break;
            //            }
            //        }
            //        if (isUncovered)
            //        {
            //            for (int vertexIndex = 0; vertexIndex < partialEdges[edgeIndex].Count; vertexIndex++)
            //            {
            //                matchedVertices[partialEdges[edgeIndex][vertexIndex]] = true;
            //            }
            //            res.Add(new Edge(partialEdges[edgeIndex].Select(x => _graph.Vertices[x]).ToList()));
            //        }
            //    }
            //    for (int i = 0; i < matchedVertices.Length; i++)
            //    {
            //        if (matchedVertices[i] != true)
            //            res.Add(new Edge(new List<Vertex>() { _graph.Vertices[i] }));
            //    }
            //    return (res, iteration);




            //}
            else if (_mode == GeneticModi.random3EdgesAndFillOptimal)
            {
                var matching3 = new List<Edge>();
                var coveredBy3DEdges = new bool[vertexCount];
                
                var all3DEdges = _graph.Edges.Where(_ => _.VertexCount == 3).ToList();      //hier sollten die indices auch reichen

                _graph.ResetVertexAdjEdges();
                var bestMatching3 = new List<Edge>();
                while (time.ElapsedMilliseconds < maxTime && parameters["maxIter"] > iteration)
                {
                    all3DEdges = all3DEdges.OrderBy(_ => _random.Next()).ToList();
                    matching3 = new List<Edge>();
                    coveredBy3DEdges = new bool[vertexCount];
                    isCovoredBySizeOf = new int[vertexCount];
                    for (int edgeIndex = 0; edgeIndex < all3DEdges.Count; edgeIndex++)
                    {

                        var edge3 = all3DEdges[edgeIndex];
                        bool skip = false;
                        for (int i = 0; i < edge3.Vertices.Count; i++)
                        {
                            var vertexId = edge3.VerticesIds[i];
                            if (isCovoredBySizeOf[vertexId] != 0)
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
                            isCovoredBySizeOf[vertexId] = edge3.VerticesIds.Count;
                        }
                        matching3.Add(edge3);
                        if (matching3.Count >  _random.Next(30,50))
                            break;

                    }
                    foreach (var edge3 in matching3)
                        foreach (var id in edge3.VerticesIds)
                            coveredBy3DEdges[id] = true;
                    iteration++;

                    var tmpGraph = _graph.GenerateInducedSubgraph(_graph.Vertices.Where(_ => !coveredBy3DEdges[_.Id]).ToList());
                    tmpGraph.ResetVertexAdjEdges();
                    tmpGraph.InitializeFor2DMatchin();
                    var matching2 = tmpGraph.GetMaximum2DMatching().maxMmatching;

                    Console.WriteLine(matching2.Count +"|"+ matching3.Count + "|" + (int)(matching2.Count + matching3.Count));

                    if (matching2.Count + matching3.Count <= bestResCount)
                    {
                        bestResCount = matching2.Count + matching3.Count;
                        bestMatching3 = matching3.ToList();
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

        public static void CalculateAssociatedMatching(int bestResCount, int vertexCount, int[] isCovored, List<List<int>> edges, ref int amountOfCoveredVertices, ref int coverSize)
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

        public static List<Edge> ReconstructResult(int vertexCount, out int[] isCovored, List<List<int>> edges)
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
