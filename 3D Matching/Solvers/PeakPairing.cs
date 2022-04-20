using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Solvers
{
    public class PeakPairing : IMinimumPerfectMatchingSolver
    {
        Random _random = new Random();
        String _type = "";
        IMinimumPerfectMatchingSolver _precalcSolver;
        private bool _precalc;
        public PeakPairing(String type = "", bool precalc = true, IMinimumPerfectMatchingSolver precalcSolver= null)
        {
            _type = type;
            _precalc = precalc;
            _precalcSolver = precalcSolver;
        }

        public override String Name { get => this.GetType().Name + "|" + _type; }

        public override (List<Edge> cover, int iterations) Run(Dictionary<string, double> parameters)
        {
            double maxTime = parameters["maxTime"];
            var time = new Stopwatch();
            time.Start();
            //var res = new List<Edge>();
            _graph.SetVertexAdjEdges();
            var iteration = 0;
            var peakInfo = _graph.CalculatePeakTime();
            var firstPeakVertices = _graph.Vertices.Where(_=>_.ContainsTime(peakInfo.time)).ToList();

            var remainingVertices = _graph.Vertices.Where(_ => ! _.ContainsTime(peakInfo.time)).ToList();
            var secondPeakInfo = _graph.CalculatePeakTime(remainingVertices);
            var secondPeakVertices = remainingVertices.Where(_ => _.ContainsTime(secondPeakInfo.time)).ToList();
            remainingVertices = remainingVertices.Where(_ => !_.ContainsTime(secondPeakInfo.time)).ToList();

            var output = _graph.Vertices.OrderBy(_ => _.Interval[0]).Select(_ => "[" + _.Interval[0] + "," + _.Interval[1] + "]");
            Console.WriteLine(String.Join(",", output));
            var twoDEdgesbetween = _graph.Edges.Where(_ => _.VertexCount == 2).Where(_ => _.ContainsTime(secondPeakInfo.time) && _.ContainsTime(peakInfo.time)).ToList();
            var threeDEdgesbetween = _graph.Edges.Where(_ => _.VertexCount == 3).Where(_ => _.ContainsTime(secondPeakInfo.time) && _.ContainsTime(peakInfo.time)).ToList();

            var peak1VerticesWithEdges = twoDEdgesbetween.Select(_ => _.vertex1).Distinct().ToList();

            Console.WriteLine(String.Join(",", peak1VerticesWithEdges.Select(_ => _.Interval[0])));
            var peak2VerticesWithEdges = twoDEdgesbetween.Select(_ => _.vertex0).Distinct().ToList();
            Console.Write(String.Join(",", peak2VerticesWithEdges.Select(_ => _.Interval[0])));
            var test2 = _graph.GenerateSubgraph(twoDEdgesbetween.Concat(firstPeakVertices.Select(_ => _.AdjEdges[0])).Concat(secondPeakVertices.Select(_ => _.AdjEdges[0])).ToList());
            test2.InitializeFor2DMatchin();
            var t = test2.GetMaximum2DMatching();
            var matchingSize = t.maxMmatching.Where(_ => _.VertexCount == 2).Count();
            var contractedEdges = t.maxMmatching.Where(_ => _.VertexCount == 2).Select(_ => (_.vertex0, _.vertex1)).ToList();
            _graph.ResetVertexAdjEdges();
            _graph.InitializeFor2DMatchin(initialMatching: null, contractingEdges: contractedEdges);
            var res = _graph.GetMaximum2DMatching();

            return (res.maxMmatching, 1);






            //var erweitereKanten = _edges.Concat(_edges.Where(_ => _.Vertices.Count == 3).Select(_ => new Edge(new List<Vertex> { _.Vertices[0], _.Vertices[1] })))
            //                            .Concat(_edges.Where(_ => _.Vertices.Count == 3).Select(_ => new Edge(new List<Vertex> { _.Vertices[0], _.Vertices[2] })))
            //                            .Concat(_edges.Where(_ => _.Vertices.Count == 3).Select(_ => new Edge(new List<Vertex> { _.Vertices[1], _.Vertices[2] })));
            //var helpGraph = new Graph(/*_edges*/erweitereKanten.Where(_=>_.Vertices.Count==2  && ((firstPeakVertices.Contains(_.Vertices[0]) && secondPeakVertices.Contains(_.Vertices[1]))
            //                                                                  || (firstPeakVertices.Contains(_.Vertices[1]) && secondPeakVertices.Contains(_.Vertices[0]))))
            //                                                                   .Concat(firstPeakVertices.Select(_=>new Edge(new List<Vertex> {_})))
            //                                                                   .Concat(secondPeakVertices.Select(_=>new Edge(new List<Vertex> {_})))
            //                                                                   .ToList(),
            //                          firstPeakVertices.Concat(secondPeakVertices).ToList());
            //var ortSolver = new MIP();
            //ortSolver.initialize(helpGraph);
            //var peakMatching= ortSolver.Run(parameters);
            //var verticesOfPeakMatching = peakMatching.cover.SelectMany(_ => _.Vertices).ToList();


            //var tmpVertices = _graph.Vertices.Where(_ => !verticesOfPeakMatching.Contains(_));
            //var tmpEdges = _edges.Where(_ => _.Vertices.Count == 2 && tmpVertices.Contains(_.Vertices[0]) && tmpVertices.Contains(_.Vertices[1]))
            //                    .Concat(_edges.Where(_=>peakMatching.cover.Contains(_)))
            //                    .Concat(_edges.Where(_ => _.Vertices.Count == 1 && tmpVertices.Contains(_.Vertices[0])))
            //                    .Concat(_edges.Where(_=>_.Vertices.Count==3 
            //                    && ((peakMatching.cover.Contains(new Edge(new List<Vertex> { _.Vertices[0] , _.Vertices[1]})) && tmpVertices.Contains(_.Vertices[2]))
            //                       || (peakMatching.cover.Contains(new Edge(new List<Vertex> { _.Vertices[1], _.Vertices[2] })) && tmpVertices.Contains(_.Vertices[0]))
            //                       || (peakMatching.cover.Contains(new Edge(new List<Vertex> { _.Vertices[2], _.Vertices[0] })) && tmpVertices.Contains(_.Vertices[1])))))
            //                    .Concat(_edges.Where(_=>_.Vertices.Count == 1)).ToList();

            //var test = tmpEdges.OrderBy(_ => -_.Vertices.Count).ToList();
            //var stringA = String.Join("\n", peakMatching.cover);
            //var tmpGraph = new Graph(tmpEdges, _graph.Vertices);
            //var tmpSolver = new MIP();

            //var testIsttmpEdgesVollständig = new List<Edge>();
            //foreach(var edge2 in /*helpGraph.Edges.Where(_=>_.Vertices.Count==2))*/peakMatching.cover.Where(_=>_.Vertices.Count==2))
            //    foreach (var edge3 in _edges)
            //    {
            //        if (edge3.Vertices.Count != 3)
            //            continue;
            //        if (edge3.Vertices.Contains(edge2.Vertices[0]) && edge3.Vertices.Contains(edge2.Vertices[1]))
            //            testIsttmpEdgesVollständig.Add(edge3);
            //    }


            //tmpSolver.initialize(tmpGraph);
            //var res = tmpSolver.Run(parameters);


            //return (res.cover, iteration);


        }




    }

}
