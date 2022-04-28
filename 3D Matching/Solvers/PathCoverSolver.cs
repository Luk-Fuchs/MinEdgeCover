using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Solvers
{
    public class PathCoverSolver
    {

        public static List<List<Vertex>> CalculatePathCoverOfNonOverlappingGraph(List<Vertex> vertices)
        {
            var leftNewVertices = vertices.Select(_ => new Vertex(_.Id)).ToList();
            var rightNewVertices = vertices.Select(_ => new Vertex(10_000+_.Id)).ToList();
            var bipartEdges = new List<Edge>();


            for(int l = 0; l<leftNewVertices.Count;l++)
            {
            for(int r = 0; r<rightNewVertices.Count;r++)
                {
                    //if (!vertices[l].Intersects(vertices[r]) && vertices[l].Interval[0]< vertices[r].Interval[0])
                    //{
                    //    bipartEdges.Add(new Edge(new List<Vertex>() { leftNewVertices[l], rightNewVertices[r] }));
                    //}
                    if (vertices[l].NeighboursFor2DMatching.Contains(vertices[r]) && vertices[l].Interval[0] < vertices[r].Interval[0])
                    {
                        bipartEdges.Add(new Edge(new List<Vertex>() { leftNewVertices[l], rightNewVertices[r] }));
                    }
                }
            }
            var helpGraph = new Graph(bipartEdges, leftNewVertices.Concat(rightNewVertices).ToList());
            helpGraph.ResetVertexAdjEdges();
            helpGraph.InitializeFor2DMatchin();
            var helpMatching= helpGraph.GetMaximum2DMatching();

            var pathcover = new List<List<Vertex>>();

            var unconsideredVertices = vertices.ToList();
            //works only if ordered
            while (unconsideredVertices.Count != 0)
            {
                var activeVertex = unconsideredVertices[0];
                unconsideredVertices.Remove(activeVertex);
                pathcover.Add(new List<Vertex>() { activeVertex });
                while (leftNewVertices[activeVertex.Id].MatchedVertex != null)
                {
                    activeVertex = vertices[leftNewVertices[activeVertex.Id].MatchedVertex.Id % 10_000];
                    unconsideredVertices.Remove(activeVertex);
                    pathcover.Last().Add(activeVertex);
                }
            }

            var allVerticesInPathcover = pathcover.SelectMany(_ => _).ToList();
            pathcover.AddRange(vertices.Where(_ => !allVerticesInPathcover.Contains(_)).Select(_ => new List<Vertex>() { _ }));
            return pathcover;
        }
    }
}
