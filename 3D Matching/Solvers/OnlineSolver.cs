using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Solvers
{
    class OnlineSolver : IMinimumEdgecoveringSolver
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

            while (time.ElapsedMilliseconds < maxTime && runTrough < parameters["maxIter"])
            {
                var edgesInputStream = _edges.OrderBy(_ => _random.Next()).ToList();
                var maxWalkThrough = 3;
                for (int walkThrough = 0; walkThrough < maxWalkThrough; walkThrough++)
                {
                    //Console.WriteLine("walkThrough: " + walkThrough);

                    var uncoveredVertexCountTotal = _graph.Vertices.Count;
                    //var vertexDegrees = new int[_graph.Vertices.Count];
                    //for (int j = 0; j < _edges.Count; j++)
                    //{
                    //    foreach (var vertexId in _edges[j].VerticesIds)
                    //        vertexDegrees[vertexId]++;
                    //}
                    for (int i = 0; i < inputLength; i++)
                    {
                        var edge = edgesInputStream[i];
                        var uncoveredVertexCountEdge = edge.NumberOfNewCoveredVertices();
                        //if (_random.NextDouble() < _func(i, inputLength, uncoveredVertexCountEdge, _graph.Vertices.Count, uncoveredVertexCountTotal))// edge.NumberOfAlreadyCoveredVertices())

                        //int averageDegree = vertexDegrees.Sum() / vertexDegrees.Length;
                        //var zähler = (uncoveredVertexCountEdge * uncoveredVertexCountEdge * uncoveredVertexCountEdge * i);
                        //var nenner = (30 * inputLength * (edge.VerticesIds.Select(_ => vertexDegrees[_]).Sum()));
                        //double exponent = (double)zähler / nenner;
                        //var prob = Math.Exp(exponent) / 3;
                        //Console.WriteLine(zähler +"/" +nenner+"    " +exponent +"  "+prob);
                        //if (_random.NextDouble() < prob)
                        //Console.WriteLine(1.0 / edge.VerticesIds.Select(_ => vertexDegrees[_]).Min());
                        //if (_random.NextDouble() < Math.Pow(1.0/edge.VerticesIds.Select(_ => vertexDegrees[_]).Min() * edge.NumberOfNewCoveredVertices(), (8 - walkThrough)) )
                        //Console.WriteLine((edge.NumberOfNewCoveredVertices() - 2.0) * Math.Pow((averageDegree * 3.0) / edge.Vertices.Select(_ => vertexDegrees[_.Id] /*+(_.IsCovered ?10000:0)*/).Sum(), 10));
                        if (_forceSingleCover)
                        {
                            if ((  edge.NumberOfNewCoveredVertices() ==3 //* Math.Pow((averageDegree * 3.0)/edge.Vertices.Select(_ => vertexDegrees[_.Id] /*+(_.IsCovered ?10000:0)*/).Sum(),3)
                                || walkThrough > 0 && edge.NumberOfNewCoveredVertices() >= 2 //&& _random.NextDouble() < Math.Pow(1.0 / edge.VerticesIds.Select(_ => vertexDegrees[_]).Min(), 2)
                                || walkThrough > 1 && edge.NumberOfNewCoveredVertices() > 0) && edge.NumberOfNewCoveredVertices()==edge.Vertices.Count)  //&& _random.NextDouble() < 1.0 * edge.NumberOfNewCoveredVertices() / edge.VerticesIds.Select(_ => vertexDegrees[_]).Min())
                            {
                                resTmp.Add(edge);
                                //Console.WriteLine("new covered:" + uncoveredVertexCountEdge+ "/"+edge.Vertices.Count+ "deg/avgdeg: " + edge.Vertices.Select(_=>vertexDegrees[_.Id]).Sum() +"/"+ averageDegree);
                                uncoveredVertexCountTotal -= uncoveredVertexCountEdge;
                                foreach (var vertex in edge.Vertices)
                                {
                                    vertex.IsCovered = true;
                                    vertex.TimesCovered = 1;
                                }
                            }
                        }
                        else
                        {
                            if (/*_random.NextDouble() */0< (edge.NumberOfNewCoveredVertices() - 2.0) //* Math.Pow((averageDegree * 3.0)/edge.Vertices.Select(_ => vertexDegrees[_.Id] /*+(_.IsCovered ?10000:0)*/).Sum(),3)
                                || walkThrough > 0 && edge.NumberOfNewCoveredVertices() >= 2 //&& _random.NextDouble() < Math.Pow(1.0 / edge.VerticesIds.Select(_ => vertexDegrees[_]).Min(), 2)
                                || walkThrough > 1 && edge.NumberOfNewCoveredVertices() > 0)   //&& _random.NextDouble() < 1.0 * edge.NumberOfNewCoveredVertices() / edge.VerticesIds.Select(_ => vertexDegrees[_]).Min())
                            {
                                resTmp.Add(edge);
                                //Console.WriteLine("new covered:" + uncoveredVertexCountEdge+ "/"+edge.Vertices.Count+ "deg/avgdeg: " + edge.Vertices.Select(_=>vertexDegrees[_.Id]).Sum() +"/"+ averageDegree);
                                uncoveredVertexCountTotal -= uncoveredVertexCountEdge;
                                foreach (var vertex in edge.Vertices)
                                {
                                    vertex.IsCovered = true;
                                    vertex.TimesCovered = 1;
                                }
                            }
                        }
                    }
                }
                //Console.WriteLine("uncovered:" + _graph.Vertices.Where(_ => !_.IsCovered).Count());
                //if (_graph.Vertices.Where(_ => !_.IsCovered).Count() > 0)
                //    resTmp = _graph.Edges.ToList();
                foreach (var edge in _graph.Edges)
                    if (edge.NumberOfNewCoveredVertices() > 0)
                    {
                        resTmp.Add(edge);
                        foreach (var vertex in edge.Vertices)
                        {
                            vertex.IsCovered = true;
                            vertex.TimesCovered ++;
                        }
                    }
                if (resTmp.Count < resMin.Count || resMin.Count == 0)
                    resMin = resTmp.ToList();

                foreach (var edge in resTmp)
                    foreach (var vertex in edge.Vertices)
                    {
                        vertex.IsCovered = false;
                        vertex.TimesCovered = 0;
                    }
                resTmp.Clear();
                runTrough++;
            }
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
