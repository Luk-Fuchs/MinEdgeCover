using _3D_Matching.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Solvers
{
    class OnlineSolver : IMinimumPerfectMatchingSolver
    {
        Random _random = new Random();
        //private Func<int, int, int, int, int, double> _func;
        //private String _mode;
        private bool _forceSingleCover;
        public OnlineSolver(bool forceSingleCover = false)
        {
            _forceSingleCover = forceSingleCover;
        }

        public override String Name { get => this.GetType().Name + "|" + _forceSingleCover; }

        public override (List<Edge> cover,int iterations) Run(Dictionary<string, double> parameters)
        {
            double maxTime = parameters["maxTime"];
            var time = new Stopwatch();
            time.Start();
            var res = new List<Edge>();
            int runTrough = 0;
            var resMin = new List<Edge>();
            var resTmp = new List<Edge>();
            var inputLength = _edges.Count;

            var sizes = new List<int>(); 
            while (time.ElapsedMilliseconds < maxTime && runTrough < parameters["maxIter"])
            {
                _graph.SetVertexAdjEdges();
                //var edgesPermutation = _edges.OrderBy(_ => -_.VertexCount * 10 + _random.NextDouble()).ToList();
                var edgesPermutation = _edges.OrderBy(_ => -_.VertexCount*1000 +_random.NextDouble()*20+ _.Vertices.Select(_ => _.AdjEdges.Count).Min()).ToList();
                //var edgesPermutation = _edges.Where(_ => _.VerticesIds.Count != 1 /*&& _random.NextDouble() < 0.3)*/).OrderBy(_ => -_.VerticesIds.Count+ 0.0001 * _random.NextDouble()).Concat(_edges.Where(_ => _.VerticesIds.Count == 1)).ToList();


                //test----------------
                //var vertexCount = _graph.Vertices.Count;
                //var amountOfCoveredVertices = 0;
                //var coverSize = 0;
                //Genetic.CalculateAssociatedMatching(100000, _graph.Vertices.Count, new int[vertexCount], edgesPermutation.Select(_=>_.VerticesIds).ToList(), ref amountOfCoveredVertices, ref coverSize);
                //;
                //---------------

                foreach (var edge in edgesPermutation)
                {
                    if (edge.AllVerticesAreUncovered())
                    {
                        resTmp.Add(edge);
                        foreach (var v in edge.Vertices)
                        {
                            v.IsCovered = true;
                            v.TimesCovered = 1;
                        }
                    }

                }
                if (resTmp.Count < resMin.Count || resMin.Count == 0)
                    resMin = resTmp.ToList();

                //Console.Write("," + resTmp.Count);
                sizes.Add(resTmp.Count);
                foreach (var edge in resTmp)
                    foreach (var vertex in edge.Vertices)
                    {
                        vertex.IsCovered = false;
                        vertex.TimesCovered = 0;
                    }
                resTmp.Clear();
                runTrough++;
            }
            //Plot.CreateFigure(sizes.Select(_=>_+0.0).ToList(), plottype:"f");
            //Plot.AddLine("online = [" + String.Join(",", sizes) + "]");

            return (resMin, runTrough);

            //    while (time.ElapsedMilliseconds < maxTime && runTrough < parameters["maxIter"])
            //{
            //    var edgesInputStream = _edges.OrderBy(_ => _random.Next()).ToList();
            //    var maxWalkThrough = 3;
            //    for (int walkThrough = 0; walkThrough < maxWalkThrough; walkThrough++)
            //    {

            //        var uncoveredVertexCountTotal = _graph.Vertices.Count;
            //        for (int i = 0; i < inputLength; i++)
            //        {
            //            var edge = edgesInputStream[i];
            //            var uncoveredVertexCountEdge = edge.NumberOfNewCoveredVertices();
            //            if (_forceSingleCover)
            //            {
            //                if ((  edge.NumberOfNewCoveredVertices() ==3 //* Math.Pow((averageDegree * 3.0)/edge.Vertices.Select(_ => vertexDegrees[_.Id] /*+(_.IsCovered ?10000:0)*/).Sum(),3)
            //                    || walkThrough > 0 && edge.NumberOfNewCoveredVertices() >= 2 //&& _random.NextDouble() < Math.Pow(1.0 / edge.VerticesIds.Select(_ => vertexDegrees[_]).Min(), 2)
            //                    || walkThrough > 1 && edge.NumberOfNewCoveredVertices() > 0) && edge.NumberOfNewCoveredVertices()==edge.Vertices.Count)  //&& _random.NextDouble() < 1.0 * edge.NumberOfNewCoveredVertices() / edge.VerticesIds.Select(_ => vertexDegrees[_]).Min())
            //                {
            //                    resTmp.Add(edge);
            //                    //Console.WriteLine("new covered:" + uncoveredVertexCountEdge+ "/"+edge.Vertices.Count+ "deg/avgdeg: " + edge.Vertices.Select(_=>vertexDegrees[_.Id]).Sum() +"/"+ averageDegree);
            //                    uncoveredVertexCountTotal -= uncoveredVertexCountEdge;
            //                    foreach (var vertex in edge.Vertices)
            //                    {
            //                        vertex.IsCovered = true;
            //                        vertex.TimesCovered = 1;
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                if (/*_random.NextDouble() */0< (edge.NumberOfNewCoveredVertices() - 2.0) //* Math.Pow((averageDegree * 3.0)/edge.Vertices.Select(_ => vertexDegrees[_.Id] /*+(_.IsCovered ?10000:0)*/).Sum(),3)
            //                    || walkThrough > 0 && edge.NumberOfNewCoveredVertices() >= 2 //&& _random.NextDouble() < Math.Pow(1.0 / edge.VerticesIds.Select(_ => vertexDegrees[_]).Min(), 2)
            //                    || walkThrough > 1 && edge.NumberOfNewCoveredVertices() > 0)   //&& _random.NextDouble() < 1.0 * edge.NumberOfNewCoveredVertices() / edge.VerticesIds.Select(_ => vertexDegrees[_]).Min())
            //                {
            //                    resTmp.Add(edge);
            //                    //Console.WriteLine("new covered:" + uncoveredVertexCountEdge+ "/"+edge.Vertices.Count+ "deg/avgdeg: " + edge.Vertices.Select(_=>vertexDegrees[_.Id]).Sum() +"/"+ averageDegree);
            //                    uncoveredVertexCountTotal -= uncoveredVertexCountEdge;
            //                    foreach (var vertex in edge.Vertices)
            //                    {
            //                        vertex.IsCovered = true;
            //                        vertex.TimesCovered = 1;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    foreach (var edge in _graph.Edges)
            //        if (edge.NumberOfNewCoveredVertices() > 0)
            //        {
            //            resTmp.Add(edge);
            //            foreach (var vertex in edge.Vertices)
            //            {
            //                vertex.IsCovered = true;
            //                vertex.TimesCovered ++;
            //            }
            //        }
            //    if (resTmp.Count < resMin.Count || resMin.Count == 0)
            //        resMin = resTmp.ToList();

            //    foreach (var edge in resTmp)
            //        foreach (var vertex in edge.Vertices)
            //        {
            //            vertex.IsCovered = false;
            //            vertex.TimesCovered = 0;
            //        }
            //    resTmp.Clear();
            //    runTrough++;
            //}
            Console.WriteLine("iterations: "+runTrough);
            return (resMin, runTrough);
        }

        public static double Func1(int i, int maxCount, int newCover, int vertexCount,int uncoveredVertexCountEdge)
        {
            if (i < maxCount * 9 / 10)
                return Math.Pow((newCover / 3), 4) ;
            else
                return Math.Pow((newCover / 2), 1);
        }
        public static Func<int, int, int, int, int, double> GenerateFunc3(double arg1)
        {
            Func<int, int, int, int, int, double> func = (i, maxCount, newCover, vertexCount, uncoveredVertexCountTotal) =>
                                                                   (newCover == 3)
                                                                   ? 1
                                                                   : (double)newCover * newCover * (uncoveredVertexCountTotal) / (maxCount - i);
            return func;
        }
    }
}
