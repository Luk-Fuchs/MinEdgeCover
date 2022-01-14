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


            if (_precalc)
            {
                var _precalcSolver = new Greedy();
                _precalcSolver.initialize(_graph);
                var tmpParams = new Dictionary<String, double>();
                tmpParams.Add("maxIter", 1000);
                tmpParams.Add("maxTime", 80);
                var preRes = _precalcSolver.Run(tmpParams);
                edges = preRes.cover.Where(_=>_.Vertices.Count!=1).Concat(_edges.Where(_ => !preRes.cover.Contains(_) && _.Vertices.Count!=1)).Concat(_edges.Where(_=>_.Vertices.Count==1)).Select(_ => _.VerticesIds).ToList();
                bestResCount = preRes.cover.Count;
            }
            else
            {
                //edges = _edges.Select(_ => _.VerticesIds).OrderBy(_ => _.Count == 1 ? 1 : 0).ToList();
                //edges = _edges.Select(_ => _.VerticesIds).OrderBy(_ => -_.Count).ToList();
                edges = _edges.Select(_ => _.VerticesIds).Where(_ => _.Count != 1).Concat(_edges.Select(_ => _.VerticesIds).Where(_ => _.Count == 1)).ToList();
            }


            int index = 0;
            while (time.ElapsedMilliseconds < maxTime)
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
                    bestResCount = coverSize;
                    var tmp = edges[index];
                    edges.RemoveAt(index);
                    edges.Insert(0, tmp);
                }
            }
            List<Edge> result = ReconstructResult(vertexCount, out isCovored, edges);

            return (result, iteration);


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
