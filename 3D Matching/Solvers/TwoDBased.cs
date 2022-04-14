﻿using _3D_Matching.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Solvers
{
    public class TwoDBased : IMinimumPerfectMatchingSolver
    {
        Random _random = new Random();
        String _type = "";
        int _additionalContractedEdges = 10;
        private bool _randomContract;
        public TwoDBased(String type = "2DContractWithBlossom", bool randomContract = true, int additionalContractedEdges = 40)
        {
            _type = type;
            _randomContract = randomContract;
            _additionalContractedEdges = additionalContractedEdges;
        }

        public override String Name { get => this.GetType().Name + "|" + _type +"|" + _additionalContractedEdges; }

        public override (List<Edge> cover, int iterations) Run(Dictionary<string, double> parameters)
        {
            double maxTime = parameters["maxTime"];
            double maxIter = parameters["maxIter"];
            var time = new Stopwatch();
            time.Start();
            _graph.ResetVertexAdjEdges(); //set genügt auch, aber macht probleme, falls auf Graph verschiedene Löser benutzt werden
            var iteration = 0;

            _graph.InitializeFor2DMatchin();
            List<Edge> res = new List<Edge>();
            var valuePerIteration = new List<double>();
            if (_type == "splitAndAugment")
            {

                res = _graph.GetMaximum2DMatching().maxMmatching;
                foreach (var toSplitEdge in res.Where(_ => _.Vertices.Count == 2))
                {
                    var vertex1 = toSplitEdge.Vertices[0];
                    var vertex2 = toSplitEdge.Vertices[1];
                    var possibleNewMatchingEdges1 = new List<Edge>();
                    var possibleNewMatchingEdges2 = new List<Edge>();
                    foreach (var possNewMatchingEdge1 in vertex1.AdjEdges.OrderBy(_ => -_.Vertices.Count))
                    {
                        if (possNewMatchingEdge1.Vertices.Count == 1)
                            continue;
                        if (possNewMatchingEdge1.Vertices.All(x => x == vertex1 || x.coveredBy.Vertices.All(_ => possNewMatchingEdge1.Vertices.Contains(_))))
                        {

                            possibleNewMatchingEdges1.Add(possNewMatchingEdge1);
                        }
                    }
                    foreach (var possNewMatchingEdge in vertex2.AdjEdges.OrderBy(_ => -_.Vertices.Count))
                    {
                        if (possNewMatchingEdge.Vertices.Count == 1)
                            continue;
                        if (possNewMatchingEdge.Vertices.All(x => x == vertex1 || x.coveredBy.Vertices.All(_ => possNewMatchingEdge.Vertices.Contains(_))))
                        {
                            possibleNewMatchingEdges2.Add(possNewMatchingEdge);
                        }
                    }

                    //here a sort after degrees or similar could be usefull
                    var augmentationHasBeenPerformed = false;
                    foreach (var edge1 in possibleNewMatchingEdges1)
                    {
                        foreach (var edge2 in possibleNewMatchingEdges2)
                        {
                            if (edge1.Vertices.All(_ => !edge2.Vertices.Contains(_)))
                            {
                                var oldMatchingEdges = edge1.Vertices.Select(_ => _.coveredBy).Concat(edge2.Vertices.Select(_ => _.coveredBy)).Distinct().ToList();
                                foreach (var edge in oldMatchingEdges)
                                    res.Remove(edge);
                                res.Add(edge1);
                                res.Add(edge2);
                                foreach (var v in edge1.Vertices)
                                    v.coveredBy = edge1;
                                foreach (var v in edge2.Vertices)
                                    v.coveredBy = edge2;
                                break;
                            }
                        }
                        if (augmentationHasBeenPerformed)
                            break;
                    }
                }
            }
            else if (_type == "randomSplitAndAugmentOnceByOnce")
            {
                res = _graph.GetMaximum2DMatching().maxMmatching;
                var initialRes = res.ToList();
                var bestRes = res.ToList();
                //Console.WriteLine(res.Count + "------------");

                while (time.ElapsedMilliseconds < maxTime)
                {
                    iteration++;
                    var verticesToGrow = new List<Vertex>();
                    for (int i = 0; i < 20; i++)
                    {
                        var edge = res[_random.Next(res.Count)];
                        res.Remove(edge);
                        foreach (var vertex in edge.Vertices)
                        {
                            verticesToGrow.Add(vertex);
                            vertex.coveredBy = null;
                        }
                    }
                    //foreach(var vertex in verticesToGrow)
                    //{
                    //    res.Add(new Edge(new List<Vertex> { vertex }));
                    //}


                    int j = 0;
                    while (j < verticesToGrow.Count)
                    {
                        var vertex = verticesToGrow[j];
                        foreach (var possNewMatchingEdge in vertex.AdjEdges.OrderBy(_ => -_.Vertices.Count))
                        {
                            if (possNewMatchingEdge.Vertices.Count == 1)
                                continue;
                            if (possNewMatchingEdge.Vertices.All(x => x == vertex || x.coveredBy == null || x.coveredBy.Vertices.All(_ => possNewMatchingEdge.Vertices.Contains(_))))
                            {
                                var oldMatchingEdges = possNewMatchingEdge.Vertices.Where(_ => _ != null).Select(_ => _.coveredBy).Distinct().ToList();
                                foreach (var edge in oldMatchingEdges)
                                    res.Remove(edge);
                                res.Add(possNewMatchingEdge);
                                foreach (var v in possNewMatchingEdge.Vertices)
                                    v.coveredBy = possNewMatchingEdge;
                                break;
                            }
                        }
                        if (vertex.coveredBy != null)
                            verticesToGrow.Remove(vertex);
                        else
                        {
                            j++;

                        }
                    }

                    Console.WriteLine(res.Count);
                    if (res.Count < bestRes.Count)
                    {
                        bestRes = res.ToList();
                    }
                    else
                    {
                        res = initialRes.ToList();
                    }
                }

                res = bestRes;
            }
            else if (_type == "2DContractWithBlossom")
            {
                _graph.InitializeFor2DMatchin();
                _graph.ComputeMaximum2DMatching();

                while (time.ElapsedMilliseconds < maxTime && iteration < maxIter)
                {
                    iteration++;

                    var matchingTupel = new List<(Vertex, Vertex)>(_graph.Vertices.Count / 2);

                    for (int i = 0; i < _graph.Vertices.Count; i++)
                    {
                        var activeVertex = _graph.Vertices[i];
                        if (activeVertex.MatchedVertex != null && activeVertex.MatchedVertex.Id < activeVertex.Id)
                        {
                            matchingTupel.Add((activeVertex.MatchedVertex, activeVertex));
                        }
                    }

                    var contractedEdges = new List<(Vertex, Vertex)>(matchingTupel.Count);
                    var initialMatching = new List<(Vertex, Vertex)>(matchingTupel.Count);

                    for (int i = 0; i < matchingTupel.Count; i++)
                    {
                        var tupleEdge = matchingTupel[i];
                        if (tupleEdge.Item1.Id < 0)
                            continue;
                        if (tupleEdge.Item1.Get2DEdgeBetween(tupleEdge.Item2).Expandables().Count > 0 && contractedEdges.Count < _graph.Vertices.Count / 5)
                        {
                            contractedEdges.Add(tupleEdge);
                        }
                        else if (_random.NextDouble() < 0.75)
                        {
                            initialMatching.Add(tupleEdge);
                        }
                    }


                    for (int i = 0; i < matchingTupel.Count; i++)
                    {
                        var edge3Strich = matchingTupel[i];
                        if (edge3Strich.Item1.Id >= 0)
                            continue;
                        if (_random.NextDouble() < 0.45)
                        {
                            contractedEdges.Add((edge3Strich.Item1.OriginalVertex0, edge3Strich.Item1.OriginalVertex1));
                            continue;
                        }
                        var edge3 = new Vertex[] { edge3Strich.Item1.OriginalVertex0, edge3Strich.Item1.OriginalVertex1, edge3Strich.Item2 };
                        int a = 0;
                        int b = 1;
                        int c = 2;
                        if (_randomContract)
                        {
                            a = _random.Next(3);
                            b = (_random.Next(1, 3) + a) % 3;
                            c = 3 - a - b;
                        }
                        if (edge3[a].Get2DEdgeBetween(edge3[b]) != null)
                        {
                            contractedEdges.Add((edge3[a], edge3[b]));
                            continue;
                        }
                        if (edge3[a].Get2DEdgeBetween(edge3[c]) != null)
                        {
                            contractedEdges.Add((edge3[a], edge3[c]));
                            continue;
                        }
                        if (edge3[c].Get2DEdgeBetween(edge3[b]) != null)
                        {
                            contractedEdges.Add((edge3[c], edge3[b]));
                            continue;
                        }

                    }

                    _graph.InitializeFor2DMatchin(initialMatching: initialMatching, contractingEdges: contractedEdges);
                    _graph.ComputeMaximum2DMatching();

                    //res = _graph.GetMaximum2DMatching().maxMmatching;
                    //valuePerIteration.Add(res.Count + 0.0);


                }
                res = _graph.GetMaximum2DMatching().maxMmatching;

                //Console.WriteLine(edgeIsUsed.Sum() / ThreeDEdges.Count);
                //var MIP = new ORTS();
                //MIP.initialize(_graph);
                //var bound = MIP.Run(parameters).cover.Count;
                //Plot.CreateFigure(valuePerIteration, plottype: "f", xLable: "Iterations", yLable: "Matching Size", title: "2DBased history of size ( ca 300 iterations/second", horizontal: "" + bound, show: false);
                //Plot.CreateFigure(edgeIsUsed, xLable: "Iterations", yLable: "Matching Size", title: "2DBased history of size ( ca 300 iterations/second", show: false);
            }
            else if (_type == "2DContractWithBlossomV2")
            {
                _graph.InitializeFor2DMatchin();
                _graph.ComputeMaximum2DMatching();

                while (time.ElapsedMilliseconds < maxTime && iteration < maxIter)
                {
                    iteration++;

                    //Console.WriteLine(_graph.ContractedVertices.Where(_ => _.MatchedVertex != null).Count());


                    for (int i = 0; i < _graph.ContractedVertices.Count; i++)
                    {
                        var contractedVertex = _graph.ContractedVertices[i];
                        if (contractedVertex.MatchedVertex == null)
                        {//Expand contracted Vertices, which are not Matched
                            //if (_random.NextDouble() < 0.2)
                            //    continue;

                            DoExpand(contractedVertex);
                        }
                        else
                        {//switch contracted edge of 3D edge
                            if (_random.NextDouble() > 0.2)
                            {
                                contractedVertex.MatchedVertex.MatchedVertex = null;
                                contractedVertex.MatchedVertex = null;
                                continue;
                            }
                            if (contractedVertex.MatchedVertex.Adj2Edges.Contains(new Edge(new List<Vertex> { contractedVertex.OriginalVertex0, contractedVertex.MatchedVertex })) && _random.NextDouble() > 0.5)
                            {
                                var newNotContracted = contractedVertex.OriginalVertex1;
                                var newContracted = contractedVertex.MatchedVertex;

                                DoExpand(contractedVertex);
                                DoContract(contractedVertex.OriginalVertex0, contractedVertex.MatchedVertex);

                                newNotContracted.MatchedVertex = newContracted.ContractedVertex;
                                newContracted.ContractedVertex.MatchedVertex = newNotContracted;

                            }
                            else if (contractedVertex.MatchedVertex.Adj2Edges.Contains(new Edge(new List<Vertex> { contractedVertex.OriginalVertex1, contractedVertex.MatchedVertex })))
                            {
                                var newNotContracted = contractedVertex.OriginalVertex0;
                                var newContracted = contractedVertex.MatchedVertex;

                                DoExpand(contractedVertex);
                                DoContract(contractedVertex.OriginalVertex1, contractedVertex.MatchedVertex);

                                newNotContracted.MatchedVertex = newContracted.ContractedVertex;
                                newContracted.ContractedVertex.MatchedVertex = newNotContracted;
                            }
                            else if (contractedVertex.MatchedVertex.Adj2Edges.Contains(new Edge(new List<Vertex> { contractedVertex.OriginalVertex0, contractedVertex.MatchedVertex })))
                            {
                                var newNotContracted = contractedVertex.OriginalVertex1;
                                var newContracted = contractedVertex.MatchedVertex;

                                DoExpand(contractedVertex);
                                DoContract(contractedVertex.OriginalVertex0, contractedVertex.MatchedVertex);

                                newNotContracted.MatchedVertex = newContracted.ContractedVertex;
                                newContracted.ContractedVertex.MatchedVertex = newNotContracted;

                            }
                            contractedVertex.MatchedVertex.MatchedVertex = null;
                            contractedVertex.MatchedVertex = null;
                        }
                    }

                    //contract random ...% of the remaining vertices
                    int newContractedCount = 0;
                    for (int i = 0; i < _graph.Vertices.Count; i++)
                    {
                        var vertex = _graph.Vertices[i];
                        if (vertex.IsContracted || vertex.MatchedVertex == null || vertex.MatchedVertex.Id < vertex.Id)
                            continue;
                        if (_random.NextDouble() < 0.9)//1.0/_additionalContractedEdges)
                            continue;
                        newContractedCount++;
                        DoContract(vertex, vertex.MatchedVertex);
                        if (newContractedCount > 30)
                            break;
                    }
                    if (iteration % 100 == 0)
                        Console.WriteLine(_graph.ContractedVertices.Count);
                    //var test = _graph.GetMaximum2DMatching();
                    //var coveredVertices = new bool[_graph.Vertices.Count];

                    //foreach (var vertex in _graph.Vertices)
                    //{
                    //    vertex.NeighboursFor2DMatching = vertex.NeighboursFor2DMatching.OrderBy(_ => _.Id ).ToList();
                    //    if (vertex.MatchedVertex == null)
                    //        continue;
                    //    //vertex.MatchedVertex.MatchedVertex = null;
                    //    //vertex.MatchedVertex = null;
                    //}
                    //if (iteration == 100)
                    //{
                    //    foreach (var v in _graph.Vertices)
                    //    {
                    //        if (v.NeighboursFor2DMatching.Count <= 3)
                    //            continue;
                    //        Console.WriteLine(String.Join(" ", v.NeighboursFor2DMatching.Select(_ => _.Id)));
                    //    }
                    //}


                    _graph.ComputeMaximum2DMatching();
                }

                res = _graph.GetMaximum2DMatching().maxMmatching;
                //Console.WriteLine(edgeIsUsed.Sum() / ThreeDEdges.Count);
                //var MIP = new ORTS();
                //MIP.initialize(_graph);
                //var bound = MIP.Run(parameters).cover.Count;
                //Plot.CreateFigure(valuePerIteration, plottype: "f", xLable: "Iterations", yLable: "Matching Size", title: "2DBased history of size ( ca 300 iterations/second", horizontal: "" + bound, show: false);
                //Plot.CreateFigure(edgeIsUsed, xLable: "Iterations", yLable: "Matching Size", title: "2DBased history of size ( ca 300 iterations/second", show: false);
            }
            else if (_type == "2DContractHyprid")
            {
                _graph.SetVertexAdjEdges();
                var potentialContracionEdges = _graph.Edges.Where(_ => _.Vertices.Count == 2).ToList();
                foreach (var edge in potentialContracionEdges)
                {
                    edge.Expandables();

                }

                potentialContracionEdges = potentialContracionEdges.OrderBy(_ => -_.Expandables().Count + 10 * _random.NextDouble()).Where(_ => _.Expandables().Count > 1).ToList();
                potentialContracionEdges.Shuffle();
                //potentialContracionEdges = potentialContracionEdges.OrderBy(_ => _random.NextDouble()).ToList();
                var alreadyContractdVertices = new int[_graph.Vertices.Count];
                var contractionEdges = new List<(Vertex, Vertex)>();
                for (int i = 0; i < potentialContracionEdges.Count; i++)
                {
                    var edge = potentialContracionEdges[i];
                    if (alreadyContractdVertices[edge.vertex0.Id] > 2 || alreadyContractdVertices[edge.vertex1.Id] > 2)
                        continue;
                    if (_random.NextDouble() < 0.5)
                        continue;
                    alreadyContractdVertices[edge.vertex0.Id] = 10;
                    alreadyContractdVertices[edge.vertex1.Id] = 10;
                    foreach (var v in edge.Expandables())
                    {
                        alreadyContractdVertices[v.Id]++;
                    }

                    contractionEdges.Add((edge.vertex0, edge.vertex1));
                    if (contractionEdges.Count > 40)
                        break;
                }
                _graph.InitializeFor2DMatchin(contractingEdges: contractionEdges);
                res = _graph.GetMaximum2DMatching().maxMmatching;
                Console.WriteLine(res.Count);

                while (time.ElapsedMilliseconds < maxTime && iteration < maxIter)
                {
                    iteration++;

                    var matchingTupel = new List<(Vertex, Vertex)>(_graph.Vertices.Count / 2);

                    for (int i = 0; i < _graph.Vertices.Count; i++)
                    {
                        var activeVertex = _graph.Vertices[i];
                        if (activeVertex.MatchedVertex != null && activeVertex.MatchedVertex.Id < activeVertex.Id)
                        {
                            matchingTupel.Add((activeVertex.MatchedVertex, activeVertex));
                        }
                    }

                    var contractedEdges = new List<(Vertex, Vertex)>(matchingTupel.Count);
                    var initialMatching = new List<(Vertex, Vertex)>(matchingTupel.Count);

                    for (int i = 0; i < matchingTupel.Count; i++)
                    {
                        var tupleEdge = matchingTupel[i];
                        if (tupleEdge.Item1.Id < 0)
                            continue;
                        if (tupleEdge.Item1.Get2DEdgeBetween(tupleEdge.Item2).Expandables().Count > 0 && contractedEdges.Count < _graph.Vertices.Count / 5)
                        {
                            contractedEdges.Add(tupleEdge);
                        }
                        else if (_random.NextDouble() < 0.75)
                        {
                            initialMatching.Add(tupleEdge);
                        }
                    }


                    for (int i = 0; i < matchingTupel.Count; i++)
                    {
                        var edge3Strich = matchingTupel[i];
                        if (edge3Strich.Item1.Id >= 0)
                            continue;
                        if (_random.NextDouble() < 0.45)
                        {
                            contractedEdges.Add((edge3Strich.Item1.OriginalVertex0, edge3Strich.Item1.OriginalVertex1));
                            continue;
                        }
                        var edge3 = new Vertex[] { edge3Strich.Item1.OriginalVertex0, edge3Strich.Item1.OriginalVertex1, edge3Strich.Item2 };
                        int a = 0;
                        int b = 1;
                        int c = 2;
                        if (_randomContract)
                        {
                            a = _random.Next(3);
                            b = (_random.Next(1, 3) + a) % 3;
                            c = 3 - a - b;
                        }
                        if (edge3[a].Get2DEdgeBetween(edge3[b]) != null)
                        {
                            contractedEdges.Add((edge3[a], edge3[b]));
                            continue;
                        }
                        if (edge3[a].Get2DEdgeBetween(edge3[c]) != null)
                        {
                            contractedEdges.Add((edge3[a], edge3[c]));
                            continue;
                        }
                        if (edge3[c].Get2DEdgeBetween(edge3[b]) != null)
                        {
                            contractedEdges.Add((edge3[c], edge3[b]));
                            continue;
                        }

                    }

                    _graph.InitializeFor2DMatchin(initialMatching: initialMatching, contractingEdges: contractedEdges);
                    _graph.ComputeMaximum2DMatching();

                    //res = _graph.GetMaximum2DMatching().maxMmatching;
                    //valuePerIteration.Add(res.Count + 0.0);


                }


                res = _graph.GetMaximum2DMatching().maxMmatching;


            }
            return (res, iteration);
        }

        private void DoContract(Vertex vertexA, Vertex vertexB)
        {
            //reset:
            vertexA.MatchedVertex = null;
            vertexB.MatchedVertex = null;
            vertexA.ContractedWith = vertexB;
            vertexB.ContractedWith = vertexA;
            vertexA.IsContracted = true;
            vertexB.IsContracted = true;


            var newContractedVertex = new Vertex(_graph.ContractedVertices.Count==0?-1:_graph.ContractedVertices[_graph.ContractedVertices.Count - 1].Id - 1);
            if (_random.NextDouble() < 0.5)
            {
                newContractedVertex.OriginalVertex0 = vertexB;
                newContractedVertex.OriginalVertex1 = vertexA;
            }
            else
            {
                newContractedVertex.OriginalVertex0 = vertexA;
                newContractedVertex.OriginalVertex1 = vertexB;
            }

            newContractedVertex.NeighboursFor2DMatching = new List<Vertex>();
            _graph.ContractedVertices.Add(newContractedVertex);
            vertexA.ContractedVertex= newContractedVertex;
            vertexB.ContractedVertex= newContractedVertex;

            //foreach(var edge2 in vertexA.Adj2Edges)
            //{
            //    edge2.Vertices[0].NeighboursFor2DMatching.Remove(edge2.Vertices[1]);
            //    edge2.Vertices[1].NeighboursFor2DMatching.Remove(edge2.Vertices[0]);
            //}
            //foreach (var edge2 in vertexB.Adj2Edges)
            //{
            //    edge2.Vertices[0].NeighboursFor2DMatching.Remove(edge2.Vertices[1]);
            //    edge2.Vertices[1].NeighboursFor2DMatching.Remove(edge2.Vertices[0]);
            //}
            while(vertexA.NeighboursFor2DMatching.Count>0)
            {
                var neighbour = vertexA.NeighboursFor2DMatching[0];
                vertexA.NeighboursFor2DMatching.Remove(neighbour);
                neighbour.NeighboursFor2DMatching.Remove(vertexA);
            }
            while (vertexB.NeighboursFor2DMatching.Count > 0)
            {
                var neighbour = vertexB.NeighboursFor2DMatching[0];
                vertexB.NeighboursFor2DMatching.Remove(neighbour);
                neighbour.NeighboursFor2DMatching.Remove(vertexB);
            }

            if (vertexB.Adj3Edges.Count > vertexA.Adj3Edges.Count)
            {
                var tmp = vertexB;
                vertexB = vertexA;
                vertexA = tmp;
            }
            for (int k = 0; k < vertexB.Adj3Edges.Count; k++)
            {
                var edge = vertexB.Adj3Edges[k];
                if (edge.Vertices[0].ContractedWith == edge.Vertices[1] && edge.Vertices[2].IsContracted == false)
                {
                    //edge.Vertices[2].NeighboursFor2DMatching.Add(newContractedVertex);
                    //newContractedVertex.NeighboursFor2DMatching.Add(edge.Vertices[2]);
                    edge.Vertices[2].NeighboursFor2DMatching.Insert(0,newContractedVertex);
                    newContractedVertex.NeighboursFor2DMatching.Insert(0,edge.Vertices[2]);
                }
                else if (edge.Vertices[2].ContractedWith == edge.Vertices[1] && edge.Vertices[0].IsContracted == false)
                {
                    //edge.Vertices[0].NeighboursFor2DMatching.Add(newContractedVertex);
                    //newContractedVertex.NeighboursFor2DMatching.Add(edge.Vertices[0]);
                    edge.Vertices[0].NeighboursFor2DMatching.Insert(0,newContractedVertex);
                    newContractedVertex.NeighboursFor2DMatching.Insert(0,edge.Vertices[0]);
                }
                else if (edge.Vertices[0].ContractedWith == edge.Vertices[2] && edge.Vertices[1].IsContracted == false)
                {
                    //edge.Vertices[1].NeighboursFor2DMatching.Add(newContractedVertex);
                    //newContractedVertex.NeighboursFor2DMatching.Add(edge.Vertices[1]);
                    edge.Vertices[1].NeighboursFor2DMatching.Insert(0,newContractedVertex);
                    newContractedVertex.NeighboursFor2DMatching.Insert(0,edge.Vertices[1]);
                }

            }
        }

        private void DoExpand(Vertex contractedVertex)
        {
            var vertex0 = contractedVertex.OriginalVertex0;
            var vertex1 = contractedVertex.OriginalVertex1;
            vertex0.MatchedVertex = null;
            vertex1.MatchedVertex = null;
            vertex0.IsContracted = false;
            vertex1.IsContracted = false;
            vertex0.ContractedWith = null;
            vertex1.ContractedWith = null;
            vertex0.ContractedVertex = null;
            vertex1.ContractedVertex = null;

            _graph.ContractedVertices.Remove(contractedVertex);

            foreach (var neighbour in contractedVertex.NeighboursFor2DMatching)
            {
                neighbour.NeighboursFor2DMatching.Remove(contractedVertex);
            }
            foreach (var edge in vertex0.Adj2Edges) //beide Knoten von contractedVertex haben sich 2 mal als nachbarn sollte aber kein Problem geben
            {
                if (edge.Vertices[0].IsContracted || edge.Vertices[1].IsContracted)
                    continue;
                edge.Vertices[0].NeighboursFor2DMatching.Add(edge.Vertices[1]);
                edge.Vertices[1].NeighboursFor2DMatching.Add(edge.Vertices[0]);
            }
            foreach (var edge in vertex1.Adj2Edges)
            {
                if (edge.Vertices[0].IsContracted || edge.Vertices[1].IsContracted)
                    continue;
                edge.Vertices[0].NeighboursFor2DMatching.Add(edge.Vertices[1]);
                edge.Vertices[1].NeighboursFor2DMatching.Add(edge.Vertices[0]);
            }
            foreach (var edge3 in vertex0.Adj3Edges)
            {
                var edgeVertices = edge3.Vertices;
                if (edgeVertices[0] == vertex0 && edgeVertices[1].ContractedWith == edgeVertices[2])
                {
                    edgeVertices[1].ContractedVertex.NeighboursFor2DMatching.Add(vertex0);
                    vertex0.NeighboursFor2DMatching.Add(edgeVertices[1].ContractedVertex);
                }
                if (edgeVertices[1] == vertex0 && edgeVertices[0].ContractedWith == edgeVertices[2])
                {
                    edgeVertices[0].ContractedVertex.NeighboursFor2DMatching.Add(vertex0);
                    vertex0.NeighboursFor2DMatching.Add(edgeVertices[0].ContractedVertex);
                }
                if (edgeVertices[2] == vertex0 && edgeVertices[1].ContractedWith == edgeVertices[0])
                {
                    edgeVertices[1].ContractedVertex.NeighboursFor2DMatching.Add(vertex0);
                    vertex0.NeighboursFor2DMatching.Add(edgeVertices[1].ContractedVertex);
                }
            }
            foreach (var edge3 in vertex1.Adj3Edges)
            {
                var edgeVertices = edge3.Vertices;
                if (edgeVertices[0] == vertex1 && edgeVertices[1].ContractedWith == edgeVertices[2])
                {
                    edgeVertices[1].ContractedVertex.NeighboursFor2DMatching.Add(vertex1);
                    vertex1.NeighboursFor2DMatching.Add(edgeVertices[1].ContractedVertex);
                }
                if (edgeVertices[1] == vertex1 && edgeVertices[0].ContractedWith == edgeVertices[2])
                {
                    edgeVertices[0].ContractedVertex.NeighboursFor2DMatching.Add(vertex1);
                    vertex1.NeighboursFor2DMatching.Add(edgeVertices[0].ContractedVertex);
                }
                if (edgeVertices[2] == vertex1 && edgeVertices[1].ContractedWith == edgeVertices[0])
                {
                    edgeVertices[1].ContractedVertex.NeighboursFor2DMatching.Add(vertex1);
                    vertex1.NeighboursFor2DMatching.Add(edgeVertices[1].ContractedVertex);
                }
            }
        }
    }
}
