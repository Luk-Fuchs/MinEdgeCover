using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Solvers
{
    class SAS : IMinimumEdgecoveringSolver
    {
        Random _random = new Random();


        public override List<Edge> Run(Dictionary<string, double> parameters)
        {
            double maxTime = parameters["maxTime"];
            int t = 0;
            var time = new Stopwatch();
            time.Start();
            var res = new List<Edge>();
            var decay = 500;
            while (time.ElapsedMilliseconds < maxTime)
            {
                var activeIndex = _random.Next(0, _graph.Edges.Count);

                if (_edges[activeIndex].IsInCover)
                {
                    if (_edges[activeIndex].AllVerticesAreCoveredAtleastTwice())
                    {
                        res.Remove(_edges[activeIndex]);
                        foreach (var vertex in _edges[activeIndex].Vertices)
                        {
                            vertex.TimesCovered--;
                            vertex.IsCovered = vertex.TimesCovered > 0;
                        }
                        _edges[activeIndex].IsInCover = false;
                        t++;
                    }
                }
                else
                {
                    if (_edges[activeIndex].AllVerticesAreUncovered() || _random.NextDouble() < Math.Exp(-t / decay))
                    {
                        t++;
                        res.Add(_edges[activeIndex]);
                        foreach (var vertex in _edges[activeIndex].Vertices)
                        {
                            vertex.IsCovered = true;
                            vertex.TimesCovered++;
                        }
                        _edges[activeIndex].IsInCover = true;
                    }
                }


                //else if (_edges[activeIndex].IsInCover == true && _random.NextDouble() < Math.Exp(-t/500))
                //{
                //    res.Remove(_edges[activeIndex]);
                //    foreach (var vertex in _edges[activeIndex].Vertices)
                //        vertex.IsCovered = false;
                //    _edges[activeIndex].IsInCover = false;
                //    t++;
                //}
            }
            //Console.WriteLine("Rec Count" + res.Count);
            for (int activeIndex = 0; activeIndex < _graph.Edges.Count; activeIndex++)
                if (_edges[activeIndex].IsInCover)
                {
                    if (_edges[activeIndex].AllVerticesAreCoveredAtleastTwice())
                    {
                        res.Remove(_edges[activeIndex]);
                        foreach (var vertex in _edges[activeIndex].Vertices)
                        {
                            vertex.TimesCovered--;
                            vertex.IsCovered = vertex.TimesCovered>0;
                        }
                        _edges[activeIndex].IsInCover = false;
                        t++;
                    }
                }
            //Console.WriteLine("Rec Count" + res.Count);
            foreach (var vertex in _graph.Vertices)
            {
                if (!vertex.IsCovered)
                {
                    res.Add(new Edge(new List<Vertex> {vertex}));
                }
            }
            return res;
        }
    }
}
