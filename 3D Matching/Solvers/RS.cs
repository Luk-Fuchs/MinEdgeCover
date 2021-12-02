using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Solvers
{
    class RS : IMinimumEdgecoveringSolver
    {
        Random random = new Random();
        String _mode = "normal";

        public RS(String mode = "normal")
        {
            _mode = mode;
        }

        public override List<Edge> Run(Dictionary<string, double> parameters)
        {
            double maxTime = parameters["maxTime"];
            var time = new Stopwatch();
            time.Start();
            Random _random = new Random();
            var resMin = new List<Edge>();
            var resTmp = new List<Edge>();
            int runTrough = 0;

            while (time.ElapsedMilliseconds < maxTime)
            {
                if (_mode == "normal")
                {
                    foreach (var edge in _edges.OrderBy(_ => -_.Vertices.Count + _random.NextDouble()))
                    {
                        if (edge.AllVerticesAreUncovered())
                        {
                            resTmp.Add(edge);
                            foreach (var vertex in edge.Vertices)
                            {
                                vertex.IsCovered = true;
                                vertex.TimesCovered = 1;
                            }
                        }
                    }
                }
                else if (_mode == "byDegree")
                {
                    var edgesCopy = _edges.ToList();
                    var vertexDegrees = _graph.Vertices.Select(_ => _edges.Where(x => x.Vertices.Contains(_)).ToList().Count).ToArray();
                    foreach (var edge in _edges.OrderBy(_ => -_.Vertices.Count * 100 + _.Vertices.Select(x => vertexDegrees[x.Id]).Sum() + _random.NextDouble()))
                    {
                        if (edge.AllVerticesAreUncovered())
                        {
                            resTmp.Add(edge);
                            foreach (var vertex in edge.Vertices)
                            {
                                vertex.IsCovered = true;
                                vertex.TimesCovered = 1;
                            }
                        }
                    }

                }
                else if (_mode == "byDegreeUpdating")
                {
                    var edgesCopy = _edges.ToList();
                    var vertexDegrees = _graph.Vertices.Select(_ => _edges.Where(x => x.Vertices.Contains(_)).Count()).ToArray();
                    int amountOfCoveredVertices = 0;
                    while (amountOfCoveredVertices < _graph.Vertices.Count)
                    {
                        var edge = edgesCopy.OrderBy(_ => -_.Vertices.Count * 100 + _.Vertices.Select(z => vertexDegrees[z.Id]).Sum() + runTrough * _random.NextDouble()).First(); //.Where(_ => _.Vertices.Where(y => !y.IsCovered).Count() ==_.Vertices.Count)
                        //Where(_ => _.Vertices.Where(y => y.IsCovered).Count() == 0).
                        foreach (var vertex in edge.Vertices)
                        {
                            vertex.IsCovered = true;
                            vertex.TimesCovered = 1;
                        }

                        foreach (var x in edgesCopy.Where(_ => _.Vertices.Where(y => y.IsCovered).Count() > 0).ToList())
                        {
                            edgesCopy.Remove(x);
                            foreach (var vertex in x.Vertices)
                                vertexDegrees[vertex.Id]--;
                        }

                        resTmp.Add(edge);
                        amountOfCoveredVertices += edge.Vertices.Count;

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
            Console.WriteLine(runTrough);
            return resMin;
        }
    }
}
