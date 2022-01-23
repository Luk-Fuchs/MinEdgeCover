﻿using _3D_Matching.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Solvers
{
    public class TwoDBased : IMinimumEdgecoveringSolver
    {
        Random _random = new Random();
        String _type = "";
        int _newCalc = 10;
        private bool _randomContract;
        public TwoDBased(String type = "splitAndAugment", bool randomContract = true, int newCalc = 10)
        {
            _type = type;
            _randomContract = randomContract;
            _newCalc = newCalc;
        }

        public override String Name { get => this.GetType().Name + "|" + _type +"|" + _newCalc; }

        public override (List<Edge> cover, int iterations) Run(Dictionary<string, double> parameters)
        {
            double maxTime = parameters["maxTime"];
            double maxIter = parameters["maxIter"];
            var time = new Stopwatch();
            time.Start();
            _graph.SetVertexAdjEdges();
            var iteration = 0;
            //var tmpSolver = new ORTS();

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

                while(time.ElapsedMilliseconds< maxTime)
                {
                    iteration++;
                    var verticesToGrow = new List<Vertex>();
                    for(int i = 0; i< 20; i++)
                    {
                        var edge = res[_random.Next(res.Count)];
                        res.Remove(edge);
                        foreach(var vertex in edge.Vertices)
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
                    while(j< verticesToGrow.Count)
                    {
                        var vertex = verticesToGrow[j];
                        foreach (var possNewMatchingEdge in vertex.AdjEdges.OrderBy(_ => -_.Vertices.Count))
                        {
                            if (possNewMatchingEdge.Vertices.Count == 1)
                                continue;
                            if (possNewMatchingEdge.Vertices.All(x => x == vertex || x.coveredBy==null || x.coveredBy.Vertices.All(_ => possNewMatchingEdge.Vertices.Contains(_))))
                            {
                                var oldMatchingEdges = possNewMatchingEdge.Vertices.Where(_=>_!=null).Select(_ => _.coveredBy).Distinct().ToList();
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
                res = _graph.GetMaximum2DMatching().maxMmatching;
                var bestRes = res.ToList();

                while (time.ElapsedMilliseconds < maxTime && iteration < maxIter)
                {
                    iteration++;

                    var contractedEdges = new List<Edge>();
                    if (iteration == 0) //maybe use here a better massure for the number of 3D edges, the 2Deges lie in
                    {
                        var sortedRes = res.OrderBy(_ => -_.Vertices.Min(x => x.AdjEdges.Count)).ToList();
                        for (int i = 0; i < _newCalc; i++)
                        {
                            var edge = sortedRes[i];
                            if (edge.Vertices.Count != 2)
                                continue;
                            contractedEdges.Add(edge);
                            res.Remove(edge);
                            i--;
                            if (contractedEdges.Count == _newCalc)
                                break;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _newCalc; i++)
                        {
                            var edge = res[_random.Next(res.Count)];
                            if (edge.Vertices.Count != 2)
                                continue;
                            res.Remove(edge);
                            contractedEdges.Add(edge);
                        }
                    }

                    foreach (var edge3 in res.Where(_ => _.Vertices.Count == 3))
                    {
                        int a = 0;
                        int b = 1;
                        int c = 2;
                        if (_randomContract)
                        {
                            a = _random.Next(3);
                            b = (_random.Next(1, 3) + a) % 3;
                            c = 3 - a - b;
                        }

                        if (edge3.Vertices[0].AdjEdges.Contains(new Edge(new List<Vertex> { edge3.Vertices[a], edge3.Vertices[b] })))
                        {
                            contractedEdges.Add(new Edge(new List<Vertex> { edge3.Vertices[a], edge3.Vertices[b] }));
                            continue;
                        }
                        if (edge3.Vertices[0].AdjEdges.Contains(new Edge(new List<Vertex> { edge3.Vertices[a], edge3.Vertices[c] })))
                        {
                            contractedEdges.Add(new Edge(new List<Vertex> { edge3.Vertices[a], edge3.Vertices[c] }));
                            continue;
                        }
                        if (edge3.Vertices[0].AdjEdges.Contains(new Edge(new List<Vertex> { edge3.Vertices[c], edge3.Vertices[b] })))
                        {
                            contractedEdges.Add(new Edge(new List<Vertex> { edge3.Vertices[c], edge3.Vertices[b] }));
                            continue;
                        }

                    }
                    _graph.InitializeFor2DMatchin(initialMatching: res.Where(_=>_.Vertices.Count==2).ToList(),contractingEdges: contractedEdges);
                    res = _graph.GetMaximum2DMatching().maxMmatching;

                    valuePerIteration.Add(res.Count + 0.0);



                    if (res.Count <= bestRes.Count)
                    {
                        bestRes = res.ToList();
                    }
                    else
                    {
                        res = bestRes.ToList();
                    }
                }
                res = bestRes;

                //var MIP = new ORTS();
                //MIP.initialize(_graph);
                //var bound = MIP.Run(parameters).cover.Count;
                //Plot.CreateFigure(valuePerIteration, plottype: "f", xLable: "Iterations", yLable: "Matching Size", title: "2DBased history of size ( ca 300 iterations/second", horizontal: "" + bound, show: false);
            }



            return (res, iteration);


        }

        


    }

}
