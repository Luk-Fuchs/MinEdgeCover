using _3D_Matching.Tests;
using Gurobi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Solvers
{

    enum GreedyModi : int
    {
        byTime,
        byTimeReverse,
        bySizeAndDegreeUpdating,
        bySizeAndNeighbourhoodSize,
        byIntegral,
        byPeak,
        bySize,
        withExplizitBadPairing,
        variable,
        variableHybrid,
    }
    class Greedy : IMinimumEdgecoveringSolver
    {

        Random _random = new Random();
        GreedyModi _mode;
        Func<Edge,double> _costFunc;

        public Greedy(GreedyModi mode = 0, Func<Edge, double> costFunc=null)
        {
            _mode = mode;
            _costFunc = costFunc;
        }

        public override String Name { get => this.GetType().Name + "|" + _mode; }

        public override (List<Edge> cover, int iterations) Run(Dictionary<string, double> parameters)
        {
            double maxTime = parameters["maxTime"];
            var time = new Stopwatch();
            time.Start();
            var resMin = new List<Edge>();
            var resTmp = new List<Edge>();
            int runTrough = 0;


            if (_mode == GreedyModi.byTime)
            {
                var res = new List<Edge>();
                var verticesByStart = _graph.Vertices.OrderBy(_ => _.Interval[0]);
                var chains = verticesByStart.Select(_ => new List<Vertex>() { _ }).ToList();
                var chainEndpoints = verticesByStart.Select(_ => _.Interval[1]).ToList();
                for (int j = 0; j < _graph.Vertices.Count; j++)
                {
                    _graph.Vertices[j].Predecessor = null;
                }
                int i = -1;
                while (chains.Count != 0)
                {
                    var minEndpoint = chainEndpoints.Min();
                    var activeChainIndex = chainEndpoints.IndexOf(minEndpoint);
                    var activeChain = chains[activeChainIndex];
                    for (i = activeChainIndex + 1; i < chains.Count; i++)
                    {

                        if ((activeChain.Count == 2 && chains[i].Count == 1 && _graph.IsEdge(activeChain[0], activeChain[1], chains[i][0]))
                            || (activeChain.Count == 1 && chains[i].Count == 1 && _graph.IsEdge(activeChain[0], chains[i][0]))
                            || (activeChain.Count == 1 && chains[i].Count == 2 && _graph.IsEdge(activeChain[0], chains[i][0], chains[i][1])))
                        {
                            chains[i][0].Predecessor = activeChain.Last();
                            activeChain.AddRange(chains[i]);
                            chains.RemoveAt(i);
                            chainEndpoints.RemoveAt(i);
                            chainEndpoints[activeChainIndex] = activeChain.Last().Interval[1];
                            i = -1;
                            break;
                        }
                    }
                    if (i != -1)
                    {
                        chains.RemoveAt(activeChainIndex);
                        chainEndpoints.RemoveAt(activeChainIndex);
                        res.Add(new Edge(activeChain));
                    }
                }

                //Console.WriteLine(res.Count);
                Plot.CreateIntervals(res, true, new List<string>() { "plt.title(\"Standard mit " + res.Count + " entstandenen Diensten \")" });
                //var info = Plot.InfoOfImprovement(_graph, res, true);
                return (res, 1);
            }
            if (_mode == GreedyModi.withExplizitBadPairing)     //initallösung mit schlecht zu matchenden
            {
                var bestRes = new List<Edge>();
                var verticesByStart = _graph.Vertices.OrderBy(_ => _.Interval[0]);
                var chains = verticesByStart.Select(_ => new List<Vertex>() { _ }).ToList();
                var chainEndpoints = verticesByStart.Select(_ => _.Interval[1]).ToList();
                var res = new List<Edge>();
                for (int iteration = 0; iteration < 2; iteration++)
                {

                    res = new List<Edge>();
                    int i = -1;
                    while (chains.Count != 0)
                    {
                        var minEndpoint = chainEndpoints.Min();
                        var activeChainIndex = chainEndpoints.IndexOf(minEndpoint);
                        var activeChain = chains[activeChainIndex];
                        for (i = activeChainIndex + 1; i < chains.Count; i++)
                        {

                            if ((activeChain.Count == 2 && chains[i].Count == 1 && _graph.IsEdge(activeChain[0], activeChain[1], chains[i][0]))
                                || (activeChain.Count == 1 && chains[i].Count == 1 && _graph.IsEdge(activeChain[0], chains[i][0]))
                                || (activeChain.Count == 1 && chains[i].Count == 2 && _graph.IsEdge(activeChain[0], chains[i][0], chains[i][1])))
                            {
                                activeChain.AddRange(chains[i]);
                                chains.RemoveAt(i);
                                chainEndpoints.RemoveAt(i);
                                chainEndpoints[activeChainIndex] = activeChain.Last().Interval[1];
                                i = -1;
                                break;
                            }
                        }

                        if (i != -1)
                        {
                            chains.RemoveAt(activeChainIndex);
                            chainEndpoints.RemoveAt(activeChainIndex);
                            res.Add(new Edge(activeChain));
                        }
                    }

                    chains = verticesByStart.Select(_ => new List<Vertex>() { _ }).ToList();
                    chainEndpoints = chains.Select(_ => _.Last().Interval[1]).ToList();
                    var badEdges = res.Where(_ => _.VertexCount == 1).ToList();

                    while (badEdges.Count != 0)
                    {
                        var badEdge = badEdges[_random.Next(badEdges.Count)];
                        var neighbouredge = badEdge.vertex0.Adj2Edges[_random.Next(badEdge.vertex0.Adj2Edges.Count)];
                        var neighbourVertex = neighbouredge.vertex0 == badEdge.vertex0 ? neighbouredge.vertex1 : neighbouredge.vertex0;

                        var tryCount = 0;
                        while (!chains.Where(_ => _.Count == 1).Select(_ => _[0]).Contains(neighbourVertex) && tryCount < 20)
                        {
                            neighbouredge = badEdge.vertex0.Adj2Edges[_random.Next(badEdge.vertex0.Adj2Edges.Count)];
                            neighbourVertex = neighbouredge.vertex0 == badEdge.vertex0 ? neighbouredge.vertex1 : neighbouredge.vertex0;
                            tryCount++;
                        }
                        if (tryCount == 20)
                        {
                            badEdges.Remove(badEdge);
                            continue;
                        }

                        var chainIndex0 = chains.Select(_ => _[0]).ToList().IndexOf(badEdge.vertex0);
                        var chainIndex1 = chains.Select(_ => _[0]).ToList().IndexOf(neighbourVertex);

                        if (chainIndex1 > chainIndex0)
                        {
                            var tmp = chainIndex0;
                            chainIndex0 = chainIndex1;
                            chainIndex1 = tmp;
                        }

                        chains[chainIndex0].Add(chains[chainIndex1][0]);
                        chains.RemoveAt(chainIndex1);
                        chainEndpoints[chainIndex0] = chainEndpoints[chainIndex1];
                        chainEndpoints.RemoveAt(chainIndex1);
                        badEdges.Remove(badEdge);
                        badEdges.Remove(new Edge(new List<Vertex>() { neighbourVertex }));
                    }
                }
                Plot.CreateIntervals(res, true, new List<string>() { "plt.title(\"Explizites Paaren schlechter Knoten mit " + res.Count + " entstandenen Diensten \")" });
                return (res, 2);
            }
            if (_mode == GreedyModi.byIntegral)
            {

                var res = new List<Edge>();
                var remainingEdges = _edges.ToList();


                int i = 0;
                while (remainingEdges.Count > 0)
                {
                    Console.WriteLine(remainingEdges.Count);
                    (var xValues, var yValues) = ComputeOverlapGraph(remainingEdges);
                    var orderedRemainingEdges = remainingEdges.OrderBy(_ => -ComputeIntergral(xValues.ToList(), yValues, _)).ToList();
                    var newMatchingEdge = orderedRemainingEdges[0];
                    res.Add(newMatchingEdge);
                    var coveredVertices = newMatchingEdge.Vertices;

                    remainingEdges = remainingEdges.Where(_ => _.Vertices.Intersect(coveredVertices).Count() == 0).ToList();
                }

                Plot.CreateIntervals(res, false, new List<string>() { "plt.title(\"Priorisierung mittels Integration mit " + res.Count + " entstandenen Diensten \")" });
                //Plot.CreateIntervals(res);
                return (res, -1);
            }
            if (_mode == GreedyModi.bySize)
            {
                foreach (var v in _graph.Vertices)
                {
                    v.IsCovered = false;
                }
                var res = new List<Edge>();
                foreach (var edge in _edges.OrderBy(_ => -_.Vertices.Count -_.Vertices.Select(_=>_.AdjEdges.Count).Sum()))
                {
                    if (edge.AllVerticesAreUncovered())
                    {
                        res.Add(edge);
                        foreach (var vertex in edge.Vertices)
                        {
                            vertex.IsCovered = true;
                        }
                    }
                }
                Plot.CreateIntervals(res, false, new List<string>() { "plt.title(r\" f(e) = -| e |  mit " + res.Count + " resultierenden Diensten\" )" });

                return (res, 1);
            }
            if (_mode == GreedyModi.byTimeReverse)
            {
                var verticesByStart = _graph.Vertices.OrderBy(_ => _.Interval[0]);
                var chains = verticesByStart.Select(_ => new List<Vertex>() { _ }).ToList();
                var chainEndpoints = verticesByStart.Select(_ => _.Interval[1]).ToList();

                var res = new List<Edge>();
                int i = -1;

                //from start
                var growFrowStartCounter = 0;
                while (chains.Count != 0)
                {
                    if (growFrowStartCounter > -1)//_graph.Vertices.Count / 5)
                        break;
                    growFrowStartCounter++;
                    var minEndpoint = chainEndpoints.Min();
                    var activeChainIndex = chainEndpoints.IndexOf(minEndpoint);
                    var activeChain = chains[activeChainIndex];
                    for (i = activeChainIndex + 1; i < chains.Count; i++)
                    {

                        //if ((activeChain.Count == 2 && chains[i].Count == 1 && _graph.IsEdge(activeChain[0], activeChain[1], chains[i][0]))
                        //    || (activeChain.Count == 1 && chains[i].Count == 1 && _graph.IsEdge(activeChain[0], chains[i][0]))
                        //    || (activeChain.Count == 1 && chains[i].Count == 2 && _graph.IsEdge(activeChain[0], chains[i][0], chains[i][1])))
                        if (_graph.IsEdge(activeChain.Concat(chains[i]).OrderBy(_ => _.Id).ToList()))
                        {
                            chains[i][0].Predecessor = activeChain.Last();
                            activeChain.AddRange(chains[i]);
                            chains.RemoveAt(i);
                            chainEndpoints.RemoveAt(i);
                            chainEndpoints[activeChainIndex] = activeChain.Last().Interval[1];
                            i = -1;
                            break;
                        }
                    }

                    if (i != -1)
                    {
                        chains.RemoveAt(activeChainIndex);
                        chainEndpoints.RemoveAt(activeChainIndex);
                        res.Add(new Edge(activeChain));
                    }
                }

                //from end
                chains = chains.OrderBy(_ => -_.Last().Interval[1]).ToList();

                while (chains.Count != 0)
                {
                    var chainStartPoints = chains.Select(_ => _[0].Interval[0]).ToList();
                    var maxStart = chainStartPoints.Max();
                    var activeChainIndex = chainStartPoints.IndexOf(maxStart);
                    var activeChain = chains[activeChainIndex];
                    for (i = 0; i < chains.Count; i++)
                    {
                        if (_graph.IsEdge(activeChain.Concat(chains[i]).OrderBy(_ => _.Id).ToList()))
                        {

                            //sortiereung der Knoten in chain beibehaltne:
                            var help = activeChain.Concat(chains[i]).OrderBy(_ => _.Interval[0]).ToList();
                            activeChain.Clear();
                            foreach (var v in help)
                            {
                                activeChain.Add(v);
                            }
                            chainStartPoints[activeChainIndex] = activeChain[0].Interval[0];
                            chains.RemoveAt(i);
                            chainStartPoints.RemoveAt(i);
                            i = -1;
                            break;
                        }

                    }

                    if (i != -1)
                    {
                        chains.RemoveAt(activeChainIndex);
                        chainStartPoints.RemoveAt(activeChainIndex);
                        res.Add(new Edge(activeChain));
                    }
                }

                //Plot.CreateIntervals(res, true, new List<string>() { "plt.title(\"Beidseitiges Paaren mit " + res.Count + " entstandenen Diensten \")" });
                Plot.CreateIntervals(res, true);//, new List<string>() { "plt.title(r\"Tagesende $\rightarrow$ mit Tagesbegin ->" + res.Count + " entstandenen Diensten \")" });
                return (res, 1);
            }
            if (_mode == GreedyModi.variable || _mode == GreedyModi.variableHybrid)
            {
                var res = new List<Edge>();
                var bestRes = new List<Edge>();
                var edgesCopy = _edges.ToList();
                var tmpGraph = _graph.GenerateInducedSubgraph(edgesCopy);
                while (edgesCopy.Count != 0)
                {
                    tmpGraph.ResetVertexAdjEdges();
                    edgesCopy = tmpGraph.Edges.OrderBy(_ => _costFunc(_)).ToList();

                    for (int i = 0; i < edgesCopy.Count; i++)
                    {
                        var edge = edgesCopy[i];
                        if (edge.AllVerticesAreUncovered())
                        {
                            res.Add(edge);
                            foreach (var vertex in edge.Vertices)
                            {
                                vertex.IsCovered = true;
                            }
                            break;
                        }
                    }
                    edgesCopy = edgesCopy.Where(_ => _.AllVerticesAreUncovered()).ToList();
                    tmpGraph.Edges = edgesCopy;

                    if (_mode == GreedyModi.variableHybrid)
                    {
                        var tmpSolver2 = new MIP();
                        var subgraph2 = _graph.GenerateSubgraph(edgesCopy.Where(_ => _.VertexCount != 3).ToList());
                        tmpSolver2.initialize(subgraph2);
                        var tmpRes2 = tmpSolver2.Run(parameters);

                        if (bestRes.Count == 0 || (res.Count + tmpRes2.cover.Count) < bestRes.Count)
                        {
                            bestRes = res.Concat(tmpRes2.cover).ToList();
                        }
                    }
                }
                if (_mode == GreedyModi.variableHybrid)
                {
                    res = bestRes;
                    Plot.CreateIntervals(res, false, new List<string>() { "plt.title(r\"$f(e) = -10 ^{ 10 } " + @"\cdot | e | + |\bigcup _{v \in e} N(v)| (hybrid)" + "$ mit " + res.Count + " resultierenden Diensten\" )" });
                }
                else
                {
                    Plot.CreateIntervals(res, false, new List<string>() { "plt.title(r\"$f(e) = -10 ^{ 10 } " + @"\cdot | e | + |\bigcup _{v \in e} N(v)| " + "$ mit " + res.Count + " resultierenden Diensten\" )" });
                }

                //Plot.CreateIntervals(res, false, new List<string>() { "plt.title(r\"$f(e) = -10 ^{ 10 } " + @"\cdot | e | + |\bigcup _{v \in e} N(v)| " + "$ mit " + res.Count + " resultierenden Diensten\" )" });
                //Plot.CreateIntervals(res, false, new List<string>() { "plt.title(r\" f(e) = -| e |  mit " + res.Count + " resultierenden Diensten\" )" });
                //Plot.CreateFigure(edgesSize, plottype: "f", show: true, xLable: "Iteration", yLable: "Anzahl verbleibender Kanten |E(G)|");
                //Console.WriteLine("[" + String.Join(",", edgesSize) + "]");
                //Plot.CreateIntervals(res, false);

                return (res, 1);
            }
            if (_mode == GreedyModi.byPeak) //nicht effizient, nur für plots
            {

                var xValues = _graph.Vertices.SelectMany(_ => _.Interval).ToList();
                xValues = xValues.OrderBy(_ => _).ToList();
                Console.WriteLine(String.Join(",", xValues));
                Console.WriteLine("----------------");
                var res = new List<Edge>();
                var firtstTime = true;
                var edgesCopy = _edges.ToList();
                var tmpGraph = _graph.GenerateInducedSubgraph(edgesCopy);
                var edgesSize = new List<double>();
                int round = 0;
                while (edgesCopy.Count != 0)
                {
                    tmpGraph.ResetVertexAdjEdges();
                    var peakTime = tmpGraph.CalculatePeakTime(_graph.Vertices.Where(_ => _.IsCovered == false).ToList()).time;
                    var lowTime = _graph.CalculateLowTime(_graph.Vertices.Where(_ => _.IsCovered == false).ToList()).time;
                    edgesCopy = tmpGraph.Edges.OrderBy(_ => (_.ContainsTime(peakTime) ? -100000000000 : +10000000000) - _.VertexCount * 10000 + _.Vertices.Select(_ => _.AdjEdges.Count).Sum()).ToList();
                    //edgesCopy = tmpGraph.Edges.OrderBy(_ => (_.ContainsTime(lowTime) ? -100000000000 : +10000000000) - _.VertexCount + 10000 * _.Vertices.Select(_ => _.AdjEdges.Count).Min()).ToList();
                    //edgesCopy = tmpGraph.Edges.OrderBy(_ => -_.VertexCount *10000- 100000000*_.Vertices.Select(v=>v.hardness).Max() +_.Vertices.Select(_=>_.AdjEdges.Count).Sum()).ToList();
                    edgesSize.Add(edgesCopy.Count);
                    for (int i = 0; i < edgesCopy.Count; i++)
                    {
                        var edge = edgesCopy[i];
                        if (edge.AllVerticesAreUncovered())
                        {
                            res.Add(edge);
                            foreach (var vertex in edge.Vertices)
                            {
                                vertex.IsCovered = true;
                            }
                            break;
                        }
                    }
                    edgesCopy = edgesCopy.Where(_ => _.AllVerticesAreUncovered()).ToList();
                    //tmpGraph.Edges = edgesCopy;
                    //tmpGraph = tmpGraph.GenerateInducedSubgraph(edgesCopy);
                    Console.WriteLine(tmpGraph.Vertices.Count);

                }
                Plot.CreateIntervals(res, false, new List<string>() { $"plt.title(\"Priorisierung von Kanten im Peak mit {res.Count} resultierenden Diensten\")" });
                //Console.WriteLine("[" + String.Join(",", res.Select(_ => "[" + String.Join(",", _.Vertices.Select(x => "[" + x.Interval[0] + "," + x.Interval[1] + "]")) + "]")) + "]");
                return (res, 1);
            }
            if (_mode == GreedyModi.bySizeAndDegreeUpdating)
            {
                var vertexDegrees = new int[_graph.Vertices.Count];
                for (int i = 0; i < _edges.Count; i++)
                {
                    foreach (var vertexId in _edges[i].VerticesIds)
                        vertexDegrees[vertexId]++;
                }
                initializeDegrees(_random, runTrough, vertexDegrees);
                int amountOfCoveredVertices = 0;
                var edgesCopy = new LinkedList<Edge>(_edges.OrderBy(_ => _.property1));
                var res = new List<Edge>();
                while (amountOfCoveredVertices < _graph.Vertices.Count)
                {
                    if (edgesCopy.Count == 0)
                        break;

                    var edge = edgesCopy.First.Value;
                    var degreeModifikation = new int[_graph.Vertices.Count];
                    foreach (var vertex in edge.Vertices)
                    {
                        vertex.IsCovered = true;
                        vertex.TimesCovered = 1;
                    }
                    var linkedEdge = edgesCopy.First;
                    while (linkedEdge != null)
                    {
                        var tmp = linkedEdge.Next;
                        foreach (var vertexId in edge.VerticesIds)
                            if (linkedEdge.Value.VerticesIds.Contains(vertexId))
                            {
                                foreach (var secondVertexId in linkedEdge.Value.VerticesIds)
                                    degreeModifikation[secondVertexId]++;
                                edgesCopy.Remove(linkedEdge);
                                break;
                            }
                        linkedEdge = tmp;
                    }

                    foreach (var tmpEdge in edgesCopy)     //update vertex degrees
                    {
                        foreach (var vertexId in tmpEdge.VerticesIds)
                        {
                            tmpEdge.property1 -= degreeModifikation[vertexId];
                        }
                    }
                    var activeNode = edgesCopy.First;
                    while (activeNode != null)
                    {
                        var tmp = activeNode.Next;
                        var offsetNode = activeNode;
                        while (offsetNode.Previous != null && offsetNode.Value.property1 < activeNode.Value.property1)
                            offsetNode = offsetNode.Previous;

                        if (offsetNode != activeNode)
                        {
                            edgesCopy.Remove(activeNode);
                            edgesCopy.AddAfter(offsetNode, activeNode);
                        }
                        activeNode = tmp;
                    }

                    res.Add(edge);
                    amountOfCoveredVertices += edge.Vertices.Count;

                }
                Plot.CreateIntervals(res, false, new List<string>() { "plt.title(r\"$f(e) = -10 ^{ 10 } " + @"\cdot | e | + \sum _{v \in e} d(v) " + "$ mit " + res.Count + " resultierenden Diensten\" )" });
                return (res, 1);
            }
            if (_mode == GreedyModi.bySizeAndNeighbourhoodSize)
            {
                var res = new List<Edge>();
                var edgesCopy = _edges.ToList();

                //initialize neighbourhoods
                foreach (var vertex in _graph.Vertices)
                    vertex.NeighbourhoodAndMultiplicity = new Dictionary<int, int>();

                for (int i = 0; i < _edges.Count; i++)
                {
                    foreach (var vertex in _edges[i].Vertices)
                        foreach (var neighbourID in _edges[i].VerticesIds)
                        {
                            if (vertex.NeighbourhoodAndMultiplicity.ContainsKey(neighbourID))
                                vertex.NeighbourhoodAndMultiplicity[neighbourID]++;
                            else
                                vertex.NeighbourhoodAndMultiplicity.Add(neighbourID, 1);
                        }
                }
                foreach (var edge in _edges.OrderBy(_ => -_.Vertices.Count * 1000000 + _.Vertices.SelectMany(v => v.NeighbourhoodAndMultiplicity.Keys).Distinct().Count() + _random.NextDouble()))
                {
                    if (edge.AllVerticesAreUncovered())
                    {
                        res.Add(edge);
                        foreach (var vertex in edge.Vertices)
                            vertex.IsCovered = true;
                    }
                }
                //Plot.CreateIntervals(res, false, new List<string>() { "plt.title(r\"$f(e) = -10 ^{ 10 } " + @"\cdot | e | + |\bigcup _{v \in e} N(v)| " + "$ mit " + res.Count + " resultierenden Diensten\" )" });

                return (res, 1);
            }


            //if (_mode == "remaining2DMatching")
            //{
            //    var iterationAndSize = new List<(int, int)>();
            //    var i = 0;
            //    var res = new List<Edge>();
            //    var bestResCount = int.MaxValue;
            //    var edgesCopy = _graph.Edges.ToList();
            //    edgesCopy = edgesCopy.OrderBy(_ => -_.VertexCount*100000+_.Vertices.Select(_=>_.AdjEdges.Count).Sum()).ToList();
            //    if(_graph.Vertices.Count % 2 == 1)
            //    {
            //        var edge = edgesCopy.First();
            //        res.Add(edge);
            //        edgesCopy.Remove(edge);
            //    }

            //    //for(int i = 0; i<4;i++)
            //    //{
            //    //    var e = edgesCopy[0];
            //    //    res.Add(e);
            //    //    edgesCopy = edgesCopy.Where(_ => _.Vertices.Intersect(e.Vertices).Count() == 0).ToList();
            //    //}



            //    while (time.ElapsedMilliseconds<maxTime)
            //    {
            //        var edge3Count = edgesCopy.Where(_ => _.VertexCount == 3).Count();
            //        if (edge3Count <= 1)
            //        {
            //            break;
            //        }
            //        Edge edge1 = null;
            //        Edge edge2 = null;
            //        var newCoveredVertices = new List<Vertex>();

            //        int trys = 0;
            //        while (res.Count<42)
            //        {
            //            edge1 = edgesCopy[_random.Next(edge3Count)];
            //            edge2 = edgesCopy[_random.Next(edge3Count)];
            //            if(edge1.VerticesIds.Intersect(edge2.VerticesIds).Count()!=0)
            //                continue;
            //            i++;
            //            trys++;
            //            newCoveredVertices = edge1.Vertices.Concat(edge2.Vertices).ToList();

            //            var tmpSolver2 = new MIP();
            //            var subgraph2 = _graph.GenerateSubgraph(edgesCopy.Where(_ => _.Vertices.Intersect(newCoveredVertices).Count() == 0).Where(_ => _.VertexCount != 3).ToList());
            //            tmpSolver2.initialize(subgraph2);
            //            var tmpRes2 = tmpSolver2.Run(parameters);

            //            Console.WriteLine((res.Count + tmpRes2.cover.Count+2) + "  " + trys + " " + edge3Count+ " "+ res.Count);

            //            if (res.Count + tmpRes2.cover.Count < bestResCount || (trys > 30 && res.Count + tmpRes2.cover.Count <= bestResCount))
            //            {
            //                bestResCount = res.Count + tmpRes2.cover.Count;
            //                iterationAndSize.Add((i,bestResCount+2));
            //                break;
            //            }
            //        }

            //        edgesCopy = edgesCopy.Where(_ => _.Vertices.Intersect(newCoveredVertices).Count() == 0).ToList();
            //        res.Add(edge1);
            //        res.Add(edge2);


            //    }

            //    Console.WriteLine("[" + String.Join(",", iterationAndSize.Select(_ => _.Item1)) + "]");
            //    Console.WriteLine("[" + String.Join(",", iterationAndSize.Select(_ => _.Item2)) + "]");

            //    if (true)
            //    {
            //        Console.WriteLine(res.Count);
            //        var tmpSolver2 = new MIP();
            //        var subgraph2 = _graph.GenerateSubgraph(edgesCopy.Where(_ => _.VertexCount != 3).ToList());
            //        tmpSolver2.initialize(subgraph2);
            //        var tmpRes2 = tmpSolver2.Run(parameters);

            //        return (res.Concat(tmpRes2.cover).ToList(), -1);
            //    }





            //}
            return (null, -1);
        }

        private (int[] xValues, int[] yValues) ComputeOverlapGraph(List<Edge> edges)
        {
            var vertices = edges.SelectMany(e => e.Vertices).Distinct().ToList();
            var xValues = vertices.SelectMany(_ => _.Interval).Distinct().OrderBy(_ => _).ToList();
            var yValues = new int[xValues.Count];
            foreach (var vertex in vertices)
            {
                for (int i = 0; i < yValues.Length; i++)
                {
                    if (xValues[i] < vertex.Interval[0])
                        continue;
                    if (xValues[i] >= vertex.Interval[1])
                        break;
                    yValues[i]++;
                }
            }
            return (xValues.ToArray(), yValues);
        }

        private static int ComputeIntergral(List<int> xValues, int[] yValues, Edge edge)
        {
            int integralE = 0;
            foreach (var v in edge.Vertices)
            {
                var intergralV = 0;
                for (int i = xValues.IndexOf(v.Interval[0]); i < xValues.Count; i++)
                {
                    if (xValues[i] >= v.Interval[1])
                        break;
                    intergralV += (xValues[i + 1] - xValues[i]) * yValues[i];
                }
                integralE += intergralV;
            }

            return integralE;
        }

        private void initializeDegrees(Random _random, int runTrough, int[] vertexDegrees)
        {
            for (int i = 0; i < _edges.Count; i++)
            {
                _edges[i].property1 = -_edges[i].Vertices.Count * 100000 + runTrough * _random.NextDouble();
                foreach (var vertex in _edges[i].Vertices)
                {
                    _edges[i].property1 += vertexDegrees[vertex.Id];
                    _edges[i].property1 += -vertex.hardness;
                }
            }
        }
    }
}
