using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Solvers
{
    public class Constructive : IMinimumEdgecoveringSolver
    {
        Random _random = new Random();
        String _name = "";
        public Constructive(String name = "")
        {
            _name = name;
        }

        public override String Name { get => this.GetType().Name + "|" + _name; }


        public override (List<Edge> cover, int iterations) Run(Dictionary<string, double> parameters)
        {
            double maxTime = parameters["maxTime"];
            int t = 0;
            var time = new Stopwatch();
            time.Start();
            var res = new List<Edge>();
            var uncoveredVertices = _graph.Vertices.ToList();
            int iterations = 0;
            _graph.SetVertexAdjEdges();
            var edgeCover = new List<Edge>();
            while (time.ElapsedMilliseconds < maxTime && uncoveredVertices.Count > 0)
            {
                iterations++;
                var plusTreeVertices = new List<Vertex>();
                plusTreeVertices.Add(uncoveredVertices[0]);
                uncoveredVertices.RemoveAt(0);

                while (plusTreeVertices.Count>0)
                {
                    var plusVertex = uncoveredVertices[0];


                    foreach (var adjEdge in plusVertex.AdjEdges)
                    {
                        if (AllOtherVerticesUnmatched(plusVertex, adjEdge) 
                            || (AllOtherVerticesBelongToTheSameEdge(plusVertex, adjEdge) 
                            && (adjEdge.Vertices[0].coveredBy.treeIndex != adjEdge.Vertices[1].coveredBy.treeIndex)))            //attention to the case that activeVertex could stay alone!!
                        { //Augment with single endpoint  <- if all propertys are used corretly this one isnt needed
                          //Augment with edge as endpoint

                            var backTrackingVertex = plusVertex;
                            var oldMatchingEdge = backTrackingVertex.coveredBy;
                            foreach (var vertex in adjEdge.Vertices)
                            {
                                vertex.coveredBy = adjEdge;
                                vertex.IsCovered = true;
                                uncoveredVertices.Remove(vertex);
                            }

                            edgeCover.Add(adjEdge);
                            AugmentUpwards(uncoveredVertices, edgeCover, ref backTrackingVertex, ref oldMatchingEdge);

                        }
                        else if (ExactlyOneVertexLeftToGrow(plusVertex, adjEdge))
                        { //grow with ONE newFreeVertex
                            Edge coveresAll;
                            if (adjEdge.Vertices[0] == plusVertex)
                                coveresAll = adjEdge.Vertices[1].coveredBy;
                            else
                                coveresAll = adjEdge.Vertices[0].coveredBy;
                            foreach (var vertex in coveresAll)
                            {
                                if (!adjEdge.Vertices.Contains(vertex))
                                {
                                    plusTreeVertices.Add(vertex);
                                }
                            }
                            coveresAll.linkingEdge = adjEdge;
                        }
                        else if ()
                        { //contract --------------not implementet yet-------

                        }






                    }


                }

        }
            
            return (edgeCover,iterations);
        }

        private static void AugmentUpwards(List<Vertex> uncoveredVertices, List<Edge> edgeCover, ref Vertex backTrackingVertex, ref Edge oldMatchingEdge)
        {
            while (oldMatchingEdge != null)
            {
                var linkingEdge = oldMatchingEdge.linkingEdge;
                backTrackingVertex = linkingEdge.plusVertex;
                edgeCover.Remove(oldMatchingEdge);
                edgeCover.Add(linkingEdge);
                foreach (var vertex in oldMatchingEdge.Vertices)
                {
                    vertex.treeIndex = -1;
                }
                foreach (var vertex in linkingEdge.Vertices)
                {
                    vertex.coveredBy = linkingEdge;
                }
                oldMatchingEdge.linkingEdge = null;
                oldMatchingEdge = backTrackingVertex.coveredBy;
                backTrackingVertex.coveredBy = linkingEdge;
                backTrackingVertex.IsCovered = true;
                linkingEdge.plusVertex = null;
            }
            uncoveredVertices.Remove(backTrackingVertex);
        }

        private static bool AllOtherVerticesUnmatched(Vertex activeVertex, Edge adjEdge)
        {
            foreach (var vertex in adjEdge.Vertices)
            {
                if (vertex == activeVertex)
                    continue;
                if (vertex.IsCovered)
                    return false;
            }
            return true;
        }
        private static bool AllOtherVerticesBelongToTheSameEdge(Vertex activeVertex, Edge adjEdge)
        {
            Edge coveresAll = null;
            if (adjEdge.Vertices[0] == activeVertex)
                coveresAll = adjEdge.Vertices[1].coveredBy;
            else
                coveresAll = adjEdge.Vertices[0].coveredBy;

            foreach (var vertex in adjEdge.Vertices)
            {
                if (vertex == activeVertex)
                    continue;
                if (vertex.coveredBy != coveresAll)
                    return false;
            }
            return true;
        }
        private static bool ExactlyOneVertexLeftToGrow(Vertex activeVertex, Edge adjEdge)
        {
            Edge coveresAll = null;
            if (adjEdge.Vertices[0] == activeVertex)
                coveresAll = adjEdge.Vertices[1].coveredBy;
            else
                coveresAll = adjEdge.Vertices[0].coveredBy;

            foreach (var vertex in adjEdge.Vertices)
            {
                if (vertex == activeVertex)
                    continue;
                if (vertex.coveredBy != coveresAll)
                    return false;       // the elements (unequal activeVertex) of adjEdge are not all part of another edge in the edgeCovering
            }
            if (coveresAll.Vertices.Count <= adjEdge.Vertices.Count)
                return false;   //there is no newFreeVertex
            return true;
        }
    }
}
