﻿using _3D_Matching.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching
{
    public class Graph
    {
        public Random _random = new Random();
        bool _adjEdgesAreInitialized = false;
        public Graph(List<Edge> edges, List<Vertex> vertices)
        {
            _edges = edges;
            Vertices = vertices;
        }
        private List<Edge> _edges;
        public List<Vertex> Vertices;
        public List<Vertex> ContractedVertices;
        private Edge[] _2DEdges;
        public Edge[] TwodEdges()
        {
            if (_2DEdges == null)
            {
                _2DEdges = Edges.Where(_ => _.Vertices.Count == 2).ToArray();
            }
            return _2DEdges;
        }
        
        public List<Edge> Edges { get => _edges != null ? _edges : GetAndSetEdges(); set=>_edges= value; } 
        private Func<IList<Vertex>, bool> _isEdge;

        public bool IsEdge(IList<Vertex> potentialEdge)
        {
            if (_isEdge != null)
                return _isEdge(potentialEdge);
            foreach (var edge in _edges)
            {
                bool edgesIsNotEqual = false;
                if (edge.Vertices.Count != potentialEdge.Count)
                {
                    edgesIsNotEqual = true;
                    continue;
                }
                for (int i = 0; i < edge.Vertices.Count; i++)
                    if (edge.VerticesIds[i] != potentialEdge[i].Id)
                    {
                        edgesIsNotEqual = true;
                    }
                if (!edgesIsNotEqual)
                    return true;
            }
            return false;
        }
        public List<Edge> GetAndSetEdges() {
            var edges = new List<Edge>();
            var potentialEdge = new List<Vertex>();        //Maybe switch here to the index for faster computation

            for (int i = 0; i < Vertices.Count; i++)
                for (int j = i + 1; j < Vertices.Count; j++)
                    for (int k = j + 1; k < Vertices.Count; k++)
                    {
                        potentialEdge = new List<Vertex>{Vertices[i], Vertices[j], Vertices[k]};
                        if (IsEdge(potentialEdge))
                            edges.Add(new Edge(potentialEdge));
                    }

            for (int i = 0; i < Vertices.Count; i++)
                for (int j = i + 1; j < Vertices.Count; j++)
                {
                    potentialEdge = new List<Vertex> { Vertices[i], Vertices[j]};
                    if (IsEdge(potentialEdge))
                        edges.Add(new Edge(potentialEdge));
                }

            for (int i = 0; i < Vertices.Count; i++)
            {
                potentialEdge = new List<Vertex> { Vertices[i]};
                if (IsEdge(potentialEdge))
                    edges.Add(new Edge(potentialEdge));
            }
            _edges = edges;
            return edges;
        }
        public static Graph GenerateRandomGraph(int n = 200, double p1=0.1, double p2 = 0.1, double p3 = 0.1)
        {
            var random = new Random();
            var vertices = Enumerable.Range(0, n).Select(_ => new Vertex(_)).ToList();
            var edges = new List<Edge>();

            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    for (int k = j + 1; k < n; k++)
                        if (random.NextDouble() < p3)
                            edges.Add(new Edge(new List<Vertex> { vertices[i], vertices[j], vertices[k] }));
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (random.NextDouble() < p2)
                        edges.Add(new Edge(new List<Vertex> { vertices[i], vertices[j] }));
            for (int i = 0; i < n; i++)
                if (random.NextDouble() < p1)
                    edges.Add(new Edge(new List<Vertex> { vertices[i] }));

            var graph = new Graph(edges, vertices);
            return graph;
        }
        public static Graph BuildGraphFromCSV(String path, bool allowAllAsSingle = false, bool forceCompletness = false, bool removeDegreeOne = false, bool addAllPossibleEdges =false)
        { 
            string[] inputLines = System.IO.File.ReadAllLines(path);

            int rowCount = 4;
            var vertices = new List<Vertex>();//[Int32.Parse(inputLines[2].Split(": ")[1])];
            var edges = new List<Edge>();
            for (; rowCount < inputLines.Length; rowCount++)
            {
                var activeLine = inputLines[rowCount];
                if (activeLine[0] == '=')
                    break;
                var vertexId = Int32.Parse(activeLine.Split(";")[0].Split(":")[1]);
                var newVertex = new Vertex(vertexId);
                newVertex.Interval = activeLine.Split(";")[3].Split(" - ").Select(_ => Int32.Parse(_)).ToArray();
                vertices.Add(newVertex);
                if (activeLine.Split(";")[1] == "True")
                    edges.Add(new Edge(new List<Vertex> { newVertex }));
            }
            
            for (rowCount++; rowCount < inputLines.Length; rowCount++)
            {
                var activeLine = inputLines[rowCount];
                if (activeLine.Contains("--"))
                    continue;
                var edgeVertices = activeLine.Split("->").Select(_ => vertices[Int32.Parse(_)]).ToList();
                var newEdge = new Edge(edgeVertices);
                
                edges.Add(newEdge);
            }

            var graph = new Graph(edges, vertices);


            if (removeDegreeOne)
            {
                int removedVertices = 0;
                graph.SetVertexAdjEdges();
                    //Console.WriteLine(graph.Vertices.Count);
                while (vertices.Where(_ => _.AdjEdges.Count <= 1).Count() != 0)
                {
                    int i = 0;
                    while (i < vertices.Count)
                    {
                        if (vertices[i].AdjEdges.Count == 0)
                        {
                            vertices.RemoveAt(i);
                        }
                        else if (vertices[i].AdjEdges.Count == 1 /*&& vertices[i].AdjEdges[0].Vertices.Count == 1*/)
                        {
                            var matchedEdge = vertices[i].AdjEdges[0];
                            foreach (var v in matchedEdge.Vertices)
                            {
                                vertices.Remove(v);
                                removedVertices++;
                                foreach (var edge in v.AdjEdges)
                                {
                                    edges.Remove(edge);
                                }
                            }
                        }
                        else if (vertices[i].AdjEdges.Count == 1)
                        {
                            Console.WriteLine("vertex of degree 1 with edge of size: " + vertices[i].AdjEdges[0].VertexCount);
                            i++;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    foreach (var edge in edges)
                    {
                        edge.VerticesIds = edge.Vertices.Select(_ => _.Id).ToList();
                    }

                    for (int index = 0; index < vertices.Count; index++)
                    {
                        vertices[index].Id = index;
                    }

                    foreach (var e in edges)
                    {
                        e.VerticesIds = e.Vertices.Select(_ => _.Id).ToList();
                    }
                    graph.ResetVertexAdjEdges();
                    //Console.WriteLine(graph.Vertices.Count);
                }
                    //Console.WriteLine("------------");
            }

            if (allowAllAsSingle)
            {
                int i = 0;
                while (i < vertices.Count)
                {
                    var v = vertices[i];
                    if( v.AdjEdges[0].VertexCount != 1)
                    {
                        edges.Add(new Edge(new List<Vertex>() { v }));
                    }
                    i++;
                }
                graph.ResetVertexAdjEdges();
            }

            if (addAllPossibleEdges)
            {
                for (int i = 0; i < graph.Vertices.Count; i++)
                    for (int j = i+1; j < graph.Vertices.Count; j++)
                        if (!graph.Vertices[i].Intersects(graph.Vertices[j]))
                            graph.Edges.Add(new Edge(new List<Vertex>() { graph.Vertices[i], graph.Vertices[j] }));
                for (int i = 0; i < graph.Vertices.Count; i++)
                    for (int j = i + 1; j < graph.Vertices.Count; j++)
                        for (int k = j + 1; k < graph.Vertices.Count; k++)
                            if (!graph.Vertices[i].Intersects(graph.Vertices[j]) && !graph.Vertices[i].Intersects(graph.Vertices[k]) && !graph.Vertices[k].Intersects(graph.Vertices[j]))
                                graph.Edges.Add(new Edge(new List<Vertex>() { graph.Vertices[i], graph.Vertices[j], graph.Vertices[k] }));
            }
            else if (forceCompletness)
            {
                foreach (var edge3 in graph.Edges.Where(_ => _.Vertices.Count == 3).ToList())
                {
                    if (edge3.vertex0.Get2DEdgeBetween(edge3.vertex1) == null)
                        graph.Edges.Add(new Edge(new List<Vertex> {edge3.vertex0,edge3.vertex1 }));
                    if (edge3.vertex2.Get2DEdgeBetween(edge3.vertex1) == null)
                        graph.Edges.Add(new Edge(new List<Vertex> { edge3.vertex1, edge3.vertex2 }));
                    if (edge3.vertex0.Get2DEdgeBetween(edge3.vertex2) == null)
                        graph.Edges.Add(new Edge(new List<Vertex> { edge3.vertex0, edge3.vertex2 }));
                }
            }

            graph.ResetVertexAdjEdges();

            //Console.WriteLine(graph.Edges.Count);
            //foreach (var edge2 in graph.TwodEdges())
            //{
            //    edge2.Expandables();
            //    if (edge2.Expandables().Count != 0)
            //        graph.Edges.Remove(edge2);
            //}
            var subedgeComposition = new int[4];
            int fm = 0;
            int fl = 0;
            int ml = 0;
            foreach(var edge3 in graph.Edges.Where(_ => _.Vertices.Count == 3))
            {
                var subedges = 0;
                if (edge3.vertex0.Get2DEdgeBetween(edge3.vertex1) != null)
                {
                    subedges++;
                    fm++;
                }
                if(edge3.vertex2.Get2DEdgeBetween(edge3.vertex1) != null)
                {
                    subedges++;
                    ml++;
                }
                if(edge3.vertex0.Get2DEdgeBetween(edge3.vertex2) != null)
                {
                    subedges++;
                    fl++;
                }
                subedgeComposition[subedges]++;
            }

            //Console.WriteLine($"fm: {fm} fl: {fl} ml: {ml}");
            //Console.WriteLine("subedgeInfo: " + String.Join("|", subedgeComposition));
            //Console.WriteLine(edges.Where(_ => _.VertexCount == 2).Count());
            //Console.WriteLine(edges.Where(_ => _.VertexCount == 3).Count());
            //Console.WriteLine(graph.Edges.Count);
            //Console.WriteLine(graph.Vertices.Count);

            graph.SetVertexAdjEdges();

            //Console.Write(graph.Edges.Where(_=>_.VertexCount==3).Count()+",");


            //var vs = graph.Vertices.OrderBy(_ => _.AdjEdges.Count).ToList();
            //var bars = vs.Select(_ => _.Adj3Edges.Where(e3 => (_.Adj2Edges.Contains(new Edge(new List<Vertex> { e3.vertex0, e3.vertex1 }))
            //                                                  || _.Adj2Edges.Contains(new Edge(new List<Vertex> { e3.vertex0, e3.vertex2 }))
            //                                                  || _.Adj2Edges.Contains(new Edge(new List<Vertex> { e3.vertex1, e3.vertex2 })))).Count()).ToList();
            ////var bars = vs.Select(_ => _.Adj3Edges.Where(e3 => graph.Edges.Contains(new Edge(e3.Vertices.Where(v=>v!=_).ToList()))).Count()).ToList();
            //var s = "[" + String.Join(",",bars) +"]";
            //Console.WriteLine(s);





            return graph;
        }
        public static Graph BuildGraphString(String vertexCountAndEdges)
        {
            var split = vertexCountAndEdges.Split(";");
            var vertexCount = Int32.Parse(split[0]);
            var vertices = Enumerable.Range(0, vertexCount).Select(_=>new Vertex(_)).ToList();
            var edges = new List<Edge>();
            for(int i = 1; i < split.Length; i++)
            {
                var edgeVertices = split[i].Split("->").Select(_ => Int32.Parse(_)).Select(_=>vertices[_]).ToList();
                edges.Add(new Edge(edgeVertices));
            }
            var graph = new Graph(edges,vertices);
            return graph;
        }
        public override string ToString()
        {
            return Vertices.Count + ";" + String.Join(";", Edges.Select(_ => String.Join("->", _.VerticesIds)));
        }
        public void SetVertexAdjEdges()
        {
            if (_adjEdgesAreInitialized)
                return;
            _adjEdgesAreInitialized = true;
            foreach (var vertex in Vertices)
            {
                vertex.AdjEdges = new List<Edge>();
                vertex.Adj2Edges = new List<Edge>();
                vertex.Adj3Edges = new List<Edge>();
            }
            foreach (var edge in _edges)
                foreach (var vertex in edge.Vertices)
                {
                    vertex.AdjEdges.Add(edge);
                    if (edge.Vertices.Count == 2)
                        vertex.Adj2Edges.Add(edge);
                    else if (edge.Vertices.Count == 3)
                        vertex.Adj3Edges.Add(edge);

                }
        }
        public void ResetVertexAdjEdges()
        {
            _adjEdgesAreInitialized = false;
            SetVertexAdjEdges();
            //foreach (var vertex in Vertices)
            //    vertex.AdjEdges = new List<Edge>();
            //foreach (var edge in _edges)
            //    foreach (var vertex in edge.Vertices)
            //        vertex.AdjEdges.Add(edge);
        }
        public bool IsEdge(Vertex vertex0, Vertex vertex1)
        {
            if (vertex0.Id > vertex1.Id)
            {
                var tmp = vertex0;
                vertex0 = vertex1;
                vertex1 = tmp;
            }

            if (vertex0.Adj2Edges.Count < vertex1.Adj2Edges.Count)
            {
                for (int i = vertex0.Adj2Edges.Count - 1; i >= 0; i--)

                {
                    if (vertex0.Adj2Edges[i].vertex1 == vertex1)
                    {
                        return true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < vertex1.Adj2Edges.Count; i++)
                {
                    if (vertex1.Adj2Edges[i].vertex0 == vertex0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public bool IsEdge(Vertex vertex0, Vertex vertex1, Vertex vertex2)
        {

            if (vertex0.Id > vertex1.Id)
            {
                var tmp = vertex0;
                vertex0 = vertex1;
                vertex1 = tmp;
            }
            if (vertex0.Id > vertex2.Id)
            {
                var tmp = vertex0;
                vertex0 = vertex2;
                vertex2 = tmp;
            }
            if (vertex1.Id > vertex2.Id)
            {
                var tmp = vertex1;
                vertex1 = vertex2;
                vertex2 = tmp;
            }


            var edges3 = vertex0.Adj3Edges;
            if (vertex1.Adj3Edges.Count < edges3.Count)
            {
                edges3 = vertex1.Adj3Edges;
            }
            if (vertex2.Adj3Edges.Count < edges3.Count)
            {
                edges3 = vertex2.Adj3Edges;
            }

            for(int i = 0; i< edges3.Count; i++)
            {
                if(edges3[i].vertex0 == vertex0 && edges3[i].vertex1 == vertex1 && edges3[i].vertex2 == vertex2)
                {
                    return true;
                } 
            }
            return false;
        }
        public (int amount, int time) CalculatePeakTime(List<Vertex> vertices = null)
        {
            if (vertices == null)
                vertices = Vertices;
            var potentialPeakTimes = vertices.SelectMany(_ => _.Interval).Distinct().OrderBy(_ => _).ToList();
            var count = new int[potentialPeakTimes.Count];
            foreach (var vertex in vertices)
            {
                for(int i = 0; i < count.Length; i++)
                {
                    if (potentialPeakTimes[i] < vertex.Interval[0])
                        continue;
                    if (potentialPeakTimes[i] > vertex.Interval[1])
                        break;
                    count[i]++;
                }
            }
            var max = count.Max();
            return (max, potentialPeakTimes[Array.IndexOf(count, max)]);
        }
        public (int amount, int time) CalculateLowTime(List<Vertex> vertices = null)
        {
            if (vertices == null)
                vertices = Vertices;
            var potentialPeakTimes = vertices.SelectMany(_ => _.Interval).Distinct().OrderBy(_ => _).ToList();
            var count = new int[potentialPeakTimes.Count];
            foreach (var vertex in vertices)
            {
                for (int i = 0; i < count.Length; i++)
                {
                    if (potentialPeakTimes[i] < vertex.Interval[0])
                        continue;
                    if (potentialPeakTimes[i] > vertex.Interval[1])
                        break;
                    count[i]++;
                }
            }
            var min = count.Where(_=>_>0).Min();
            return (min, potentialPeakTimes[Array.IndexOf(count, min)]);
        }
        
        public Graph GenerateInducedSubgraph(List<Edge> toOptimizeEdges)
        {
            var tmpVertices = toOptimizeEdges.SelectMany(_ => _.Vertices).Distinct();
            //Maybe remove duplicate
            var tmpEdges = Edges.Where(_ => _.Vertices.Where(x => tmpVertices.Contains(x)).Count() == _.Vertices.Count).ToList();
            var tmpGraph = new Graph(tmpEdges, tmpVertices.ToList());
            return tmpGraph;
        }
        public Graph GenerateSubgraph(List<Edge> subgraphEdges)
        {
            var tmpVertices = subgraphEdges.SelectMany(_ => _.Vertices).Distinct();
            //Maybe remove duplicate
            var tmpGraph = new Graph(subgraphEdges, tmpVertices.ToList());
            return tmpGraph;
        }
        public Graph GenerateInducedSubgraph(List<Vertex> inducedVertices)
        {
            //Maybe remove duplicate
            var tmpEdges = Edges.Where(_ => _.Vertices.Where(x => inducedVertices.Contains(x)).Count() == _.Vertices.Count).ToList();
            var tmpGraph = new Graph(tmpEdges, inducedVertices.ToList());
            return tmpGraph;
        }

        public void InitializeFor2DMatchin(List<(Vertex,Vertex)> initialMatching = null, List<(Vertex,Vertex)> contractingEdges = null, List<(Vertex,int)> initial3DEdges = null)
        {
            SetVertexAdjEdges();
            ContractedVertices = new List<Vertex>(50);

            //initialize Vertices
            SetInitialValuesForVertices(contractingEdges);

            var twoDEdges = TwodEdges();
            for (int i = 0;i< twoDEdges.Length;i++)
            {
                var vertex0 = twoDEdges[i].vertex0;
                var vertex1 = twoDEdges[i].vertex1;
                if (vertex0.IsContracted || vertex1.IsContracted)
                    continue;
                vertex1.NeighboursFor2DMatching.Add(vertex0);
                vertex0.NeighboursFor2DMatching.Add(vertex1);
            }

            SetContractingVerticesNeighberhood(contractingEdges);

            SetInitialMatching(initialMatching);

            if (initial3DEdges != null)
            {
                for(int i = 0; i< initial3DEdges.Count; i++)
                {
                    var vertex = initial3DEdges[i].Item1;
                    var contractedVertex = ContractedVertices[initial3DEdges[i].Item2];

                    vertex.MatchedVertex = contractedVertex;
                    contractedVertex.MatchedVertex = vertex;
                }
            }
        }

        private void SetInitialValuesForVertices(List<(Vertex,Vertex)> contractingEdges)
        {
            //foreach (var vertex in Vertices) //nicht jedesmal resetten sondern weiter benutzen
            for (int j = 0; j < Vertices.Count; j++)
            {
                Vertices[j].IsContracted = false;
            }

            if (contractingEdges != null)
            {
                for (int i = 0; i < contractingEdges.Count; i++)
                {
                    var vertex0 = contractingEdges[i].Item1;
                    var vertex1 = contractingEdges[i].Item2;

                    vertex0.IsContracted = true;
                    vertex1.IsContracted = true;
                }
            }

            for (int j = 0; j < Vertices.Count; j++)
            {
                var vertex = Vertices[j];
                vertex.MatchedVertex = null;
                vertex.IsInTree = false;
                vertex.OddPath = null;
                //vertex.ContractedVertex = null;
                //vertex.ContractedWith = null;
                
                if (vertex.IsContracted)
                    continue;
                if (vertex.NeighboursFor2DMatching != null)
                    vertex.NeighboursFor2DMatching.Clear();
                else
                    vertex.NeighboursFor2DMatching = new List<Vertex>(vertex.Adj2Edges.Count*2);
            }

        }

        private static void SetInitialMatching(List<(Vertex,Vertex)> initialMatching)
        {
            if (initialMatching != null)
            {
                for (int i = 0; i < initialMatching.Count; i++)
                {
                    var edge = initialMatching[i];
                    if (edge.Item1.Id>=0 && edge.Item2.Id>=0)
                    {
                        var vertex0 = edge.Item1;
                        var vertex1 = edge.Item2;

                        vertex0.MatchedVertex = vertex1;
                        vertex1.MatchedVertex = vertex0;
                    }
                }

            }
        }

        private void SetContractingVerticesNeighberhood(List<(Vertex,Vertex)> contractingEdges)
        {
            if (contractingEdges != null)
            {
                for (int i = 0; i < contractingEdges.Count; i++)
                {
                    var contractingEdge = contractingEdges[i];
                    var vertex0 = contractingEdge.Item1;
                    var vertex1 = contractingEdge.Item2;

                    Edge edge = null;
                    for (int j = 0; j < vertex0.Adj2Edges.Count; j++)
                        if (vertex0.Adj2Edges[j].Contains(vertex1))
                        {
                            edge = vertex0.Adj2Edges[j];
                            break;
                        }
                    var expandables = edge.Expandables();
                    vertex0.ContractedWith = vertex1;
                    vertex1.ContractedWith = vertex0;
                    var contractedVertex = new Vertex(-i - 1);
                    contractedVertex.OriginalVertex0 = vertex0;
                    contractedVertex.OriginalVertex1 = vertex1;
                    contractedVertex.NeighboursFor2DMatching = new List<Vertex>(expandables.Count);

                    ContractedVertices.Add(contractedVertex);
                    if (vertex0.Adj2Edges.Count > vertex1.Adj2Edges.Count)
                    {
                        var tmp = vertex0;
                        vertex0 = vertex1;
                        vertex1 = tmp;
                    }

                    for(int j = 0; j < expandables.Count; j++)
                    {
                        var expandableVertex = expandables[j];
                        if (expandableVertex.IsContracted)
                            continue;
                        expandableVertex.NeighboursFor2DMatching.Add(contractedVertex);
                        contractedVertex.NeighboursFor2DMatching.Add(expandableVertex);
                    }
                }
            }
        }

        public (List<Edge> maxMmatching, List<Vertex> uncoveredVertices) GetMaximum2DMatching()
        {
            ComputeMaximum2DMatching();
            return ReconstructMatching();
        }

        public void ComputeMaximum2DMatching()
        {
            if (ContractedVertices == null)
                ContractedVertices = new List<Vertex>(0);

            var activeVertexCounter = 0;
            while (activeVertexCounter< Vertices.Count+ContractedVertices.Count)
            {
                Vertex newRoot;
                if (activeVertexCounter < Vertices.Count)
                {
                    newRoot = Vertices[activeVertexCounter];
                }
                else
                {
                    newRoot = ContractedVertices[activeVertexCounter-Vertices.Count];
                }
                activeVertexCounter++;
                if (newRoot.IsContracted || newRoot.MatchedVertex!=null)
                    continue;

                newRoot.IsInTree = true;
                var plusTreeVertices = new List<Vertex>(20);
                plusTreeVertices.Add(newRoot);


                int blossomCount = 0;

                bool augmentationHasBeenPerformed = TryAndDoAugmentation( newRoot, plusTreeVertices, newRoot);

                var stackIndex = 0;
                while (!augmentationHasBeenPerformed &&  stackIndex< plusTreeVertices.Count )
                {
                    var activeVertex = plusTreeVertices[stackIndex];
                    stackIndex++;

                    for (int neighbourIndex = 0; neighbourIndex < activeVertex.NeighboursFor2DMatching.Count; neighbourIndex++)
                    {
                        var activeNeighbour = activeVertex.NeighboursFor2DMatching[neighbourIndex];
                        if (activeNeighbour.IsContracted)
                            continue;
                        if (activeNeighbour.IsInTree && (activeNeighbour.BlossomIndex != activeVertex.BlossomIndex || activeNeighbour.BlossomIndex + activeVertex.BlossomIndex == 0) && (activeNeighbour.Predecessor == null || activeNeighbour.OddPath != null))
                        { // do blossom building.                         => no blossominternal edge                                                                                            =>is of type plus               => is blossom of tpye plus          
                            var oldStackCount = plusTreeVertices.Count;
                            CalculateAndSetOddPaths(activeVertex, activeNeighbour, plusTreeVertices, ref blossomCount);

                            for (int i = oldStackCount; i < plusTreeVertices.Count; i++)
                            {
                                var v = plusTreeVertices[i];
                                augmentationHasBeenPerformed = TryAndDoAugmentation(newRoot, plusTreeVertices, v);
                                if (augmentationHasBeenPerformed)
                                {
                                    break;
                                }
                            }
                            if (augmentationHasBeenPerformed)
                                break;

                        }
                        else if (activeNeighbour.IsInTree == false)
                        {
                            if (activeNeighbour.MatchedVertex != null)
                            {// grow
                                var newPlusVertex = activeNeighbour.MatchedVertex;
                                activeNeighbour.Predecessor = activeVertex;
                                plusTreeVertices.Add(newPlusVertex);

                                newPlusVertex.IsInTree = true;
                                activeNeighbour.IsInTree = true;

                                augmentationHasBeenPerformed = TryAndDoAugmentation( newRoot, plusTreeVertices, newPlusVertex);
                                if (augmentationHasBeenPerformed)
                                {
                                    break;
                                }
                            }
                        }
                    }

                }
                if (!augmentationHasBeenPerformed)
                {
                    augmentationHasBeenPerformed = PartialAugment( newRoot, plusTreeVertices, augmentationHasBeenPerformed);
                }
            }
        }

        public void DynamicContraction(double maxTime, ref int iteration)
        {
            if (ContractedVertices != null && ContractedVertices.Count != 0)
                throw new NotImplementedException();

            var time = new Stopwatch();
            time.Start();
            var potentialRootesLinkedList = new LinkedList<Vertex>(Vertices);
            while (time.ElapsedMilliseconds<maxTime)
            {
                iteration++;
                //Console.WriteLine(ContractedVertices.Count);
                if (potentialRootesLinkedList.Count == 0)
                {
                    //if (_random.NextDouble() < 1)
                    //{
                    var contractedUnmatchedVertices = ContractedVertices.Where(_ => _.MatchedVertex == null).ToList();
                    //    if (contractedUnmatchedVertices.Count != 0)
                    //{
                    //        var contractedVertex = contractedUnmatchedVertices[_random.Next(contractedUnmatchedVertices.Count)];

                    //undo contraction of rhis vertex
                    foreach (var contractedVertex in contractedUnmatchedVertices)
                    {

                        var vertex0 = contractedVertex.OriginalVertex0;
                        var vertex1 = contractedVertex.OriginalVertex1;

                        vertex0.MatchedVertex = null;
                        vertex1.MatchedVertex = null;
                        vertex0.IsContracted = false;
                        vertex1.IsContracted = false;
                        vertex0.OddPath = null;
                        vertex1.OddPath = null;
                        vertex0.Predecessor = null;
                        vertex1.Predecessor = null;
                        vertex0.NeighboursFor2DMatching.Clear();
                        vertex1.NeighboursFor2DMatching.Clear();

                        for (int i = 0; i < contractedVertex.NeighboursFor2DMatching.Count; i++)
                        {
                            var neighbour = contractedVertex.NeighboursFor2DMatching[i];
                            neighbour.NeighboursFor2DMatching.Remove(contractedVertex);
                        }
                        for (int i = 0; i < vertex0.Adj2Edges.Count; i++)
                        {
                            var a = vertex0.Adj2Edges[i].vertex0;
                            var b = vertex0.Adj2Edges[i].vertex1;
                            if (a.IsContracted || b.IsContracted)
                                continue;
                            a.NeighboursFor2DMatching.Add(b);
                            b.NeighboursFor2DMatching.Add(a);
                        }
                        for (int i = 0; i < vertex1.Adj2Edges.Count; i++)
                        {
                            var a = vertex1.Adj2Edges[i].vertex0;
                            var b = vertex1.Adj2Edges[i].vertex1;
                            if (a.IsContracted || b.IsContracted)
                                continue;
                            a.NeighboursFor2DMatching.Add(b);
                            b.NeighboursFor2DMatching.Add(a);
                        }
                        for (int i = 0; i < vertex0.Adj3Edges.Count; i++)
                        {
                            var edge3 = vertex0.Adj3Edges[i];
                            Vertex a, b = null;
                            if (edge3.vertex0 == vertex0)
                            {
                                a = edge3.vertex1;
                                b = edge3.vertex2;
                            }
                            else if (edge3.vertex1 == vertex0)
                            {
                                a = edge3.vertex0;
                                b = edge3.vertex2;
                            }
                            else
                            {
                                a = edge3.vertex0;
                                b = edge3.vertex1;
                            }
                            if (a.IsContracted && a.ContractedVertex == b.ContractedVertex)
                            {
                                a.ContractedVertex.NeighboursFor2DMatching.Add(vertex0);
                                vertex0.NeighboursFor2DMatching.Add(a.ContractedVertex);
                            }
                        }
                        for (int i = 0; i < vertex1.Adj3Edges.Count; i++)
                        {
                            var edge3 = vertex1.Adj3Edges[i];
                            Vertex a, b = null;
                            if (edge3.vertex0 == vertex1)
                            {
                                a = edge3.vertex1;
                                b = edge3.vertex2;
                            }
                            else if (edge3.vertex1 == vertex1)
                            {
                                a = edge3.vertex0;
                                b = edge3.vertex2;
                            }
                            else
                            {
                                a = edge3.vertex0;
                                b = edge3.vertex1;
                            }
                            if (a.IsContracted && a.ContractedVertex == b.ContractedVertex)
                            {
                                a.ContractedVertex.NeighboursFor2DMatching.Add(vertex1);
                                vertex1.NeighboursFor2DMatching.Add(a.ContractedVertex);
                            }
                        }
                        potentialRootesLinkedList.Remove(contractedVertex);  //sicherstellen dass nicht benötigt
                        ContractedVertices.Remove(contractedVertex);
                        potentialRootesLinkedList.AddLast(vertex1);
                        potentialRootesLinkedList.AddLast(vertex0);
                        continue;
                    }
                //}

                    //contract random matched vertices
                    for (int contractionCount = 0; contractionCount < 10; contractionCount++)
                    {
                        var matchedVertex0 = Vertices[_random.Next(Vertices.Count)];
                        int continueLimit = 0;
                        while (matchedVertex0.MatchedVertex == null || matchedVertex0.MatchedVertex.Id < 0 || matchedVertex0.IsContracted || matchedVertex0.MatchedVertex.IsContracted)
                        {
                            matchedVertex0 = Vertices[_random.Next(Vertices.Count)];
                            continueLimit++;
                            if (continueLimit > 300)
                                break;
                        }
                        if (continueLimit > 100)
                            continue;
                        var matchedVertex1 = matchedVertex0.MatchedVertex;
                        var newContractedVertex = new Vertex(-1);        //nicht mehr eindeutig aber auch egal
                        newContractedVertex.NeighboursFor2DMatching = new List<Vertex>();
                        newContractedVertex.OriginalVertex0 = matchedVertex0;
                        newContractedVertex.OriginalVertex1 = matchedVertex1;
                        matchedVertex0.IsContracted = true;
                        matchedVertex1.IsContracted = true;
                        matchedVertex0.ContractedVertex = newContractedVertex;
                        matchedVertex1.ContractedVertex = newContractedVertex;

                        for (int i = 0; i < matchedVertex0.NeighboursFor2DMatching.Count; i++)
                        {
                            var neighbour = matchedVertex0.NeighboursFor2DMatching[i];
                            neighbour.NeighboursFor2DMatching.Remove(matchedVertex0);
                        }
                        for (int i = 0; i < matchedVertex1.NeighboursFor2DMatching.Count; i++)
                        {
                            var neighbour = matchedVertex1.NeighboursFor2DMatching[i];
                            neighbour.NeighboursFor2DMatching.Remove(matchedVertex1);
                        }

                        var matchedEdge = matchedVertex0.Get2DEdgeBetween(matchedVertex1);
                        var expandables = matchedEdge.Expandables();

                        for (int i = 0; i < expandables.Count; i++)
                        {
                            expandables[i].NeighboursFor2DMatching.Add(newContractedVertex);
                            newContractedVertex.NeighboursFor2DMatching.Add(expandables[i]);
                        }




                        potentialRootesLinkedList.Remove(matchedVertex0);
                        potentialRootesLinkedList.Remove(matchedVertex1);
                        potentialRootesLinkedList.AddLast(newContractedVertex);
                        ContractedVertices.Add(newContractedVertex);
                    }


                    foreach(var vertex in Vertices)
                    {
                        if (vertex.MatchedVertex != null && vertex.Id>=0 && vertex.MatchedVertex.Id>=0 && vertex.IsContracted==false && vertex.MatchedVertex.IsContracted==false && _random.NextDouble()<0.65)
                        {
                            potentialRootesLinkedList.AddLast(vertex);
                            potentialRootesLinkedList.AddLast(vertex.MatchedVertex);
                            vertex.MatchedVertex.MatchedVertex = null;
                            vertex.MatchedVertex = null;
                        }
                    }
                }
                Vertex newRoot = potentialRootesLinkedList.First.Value;
                potentialRootesLinkedList.RemoveFirst();
                if (newRoot.IsContracted || newRoot.MatchedVertex != null)
                    continue;

                newRoot.IsInTree = true;
                var plusTreeVertices = new List<Vertex>(20);
                plusTreeVertices.Add(newRoot);


                int blossomCount = 0;

                bool augmentationHasBeenPerformed = TryAndDoAugmentation(newRoot, plusTreeVertices, newRoot);

                var stackIndex = 0;
                while (!augmentationHasBeenPerformed && stackIndex < plusTreeVertices.Count)
                {
                    var activeVertex = plusTreeVertices[stackIndex];
                    stackIndex++;

                    for (int neighbourIndex = 0; neighbourIndex < activeVertex.NeighboursFor2DMatching.Count; neighbourIndex++)
                    {
                        var activeNeighbour = activeVertex.NeighboursFor2DMatching[neighbourIndex];
                        if (activeNeighbour.IsContracted)
                            continue;
                        if (activeNeighbour.IsInTree && (activeNeighbour.BlossomIndex != activeVertex.BlossomIndex || activeNeighbour.BlossomIndex + activeVertex.BlossomIndex == 0) && (activeNeighbour.Predecessor == null || activeNeighbour.OddPath != null))
                        { // do blossom building.                         => no blossominternal edge                                                                                            =>is of type plus               => is blossom of tpye plus          
                            var oldStackCount = plusTreeVertices.Count;
                            CalculateAndSetOddPaths(activeVertex, activeNeighbour, plusTreeVertices, ref blossomCount);

                            for (int i = oldStackCount; i < plusTreeVertices.Count; i++)
                            {
                                var v = plusTreeVertices[i];
                                augmentationHasBeenPerformed = TryAndDoAugmentation(newRoot, plusTreeVertices, v);
                                if (augmentationHasBeenPerformed)
                                {
                                    break;
                                }
                            }
                            if (augmentationHasBeenPerformed)
                                break;

                        }
                        else if (activeNeighbour.IsInTree == false)
                        {
                            if (activeNeighbour.MatchedVertex != null)
                            {// grow
                                var newPlusVertex = activeNeighbour.MatchedVertex;
                                activeNeighbour.Predecessor = activeVertex;
                                plusTreeVertices.Add(newPlusVertex);

                                newPlusVertex.IsInTree = true;
                                activeNeighbour.IsInTree = true;

                                augmentationHasBeenPerformed = TryAndDoAugmentation(newRoot, plusTreeVertices, newPlusVertex);
                                if (augmentationHasBeenPerformed)
                                {
                                    break;
                                }
                            }
                        }
                    }

                }
                if (!augmentationHasBeenPerformed)
                {
                    augmentationHasBeenPerformed = PartialAugment(newRoot, plusTreeVertices, augmentationHasBeenPerformed, prefereContractedVerticesAsSingle:true);
                }

            }
            var contractedUnmatchedVertices2 = ContractedVertices.Where(_ => _.MatchedVertex == null).ToList();
            //    if (contractedUnmatchedVertices.Count != 0)
            //{
            //        var contractedVertex = contractedUnmatchedVertices[_random.Next(contractedUnmatchedVertices.Count)];

            //undo contraction of rhis vertex
            foreach (var contractedVertex in contractedUnmatchedVertices2)
            {

                var vertex0 = contractedVertex.OriginalVertex0;
                var vertex1 = contractedVertex.OriginalVertex1;

                vertex0.MatchedVertex = null;
                vertex1.MatchedVertex = null;
                vertex0.IsContracted = false;
                vertex1.IsContracted = false;
                vertex0.OddPath = null;
                vertex1.OddPath = null;
                vertex0.Predecessor = null;
                vertex1.Predecessor = null;
                vertex0.NeighboursFor2DMatching.Clear();
                vertex1.NeighboursFor2DMatching.Clear();

                for (int i = 0; i < contractedVertex.NeighboursFor2DMatching.Count; i++)
                {
                    var neighbour = contractedVertex.NeighboursFor2DMatching[i];
                    neighbour.NeighboursFor2DMatching.Remove(contractedVertex);
                }
                for (int i = 0; i < vertex0.Adj2Edges.Count; i++)
                {
                    var a = vertex0.Adj2Edges[i].vertex0;
                    var b = vertex0.Adj2Edges[i].vertex1;
                    if (a.IsContracted || b.IsContracted)
                        continue;
                    a.NeighboursFor2DMatching.Add(b);
                    b.NeighboursFor2DMatching.Add(a);
                }
                for (int i = 0; i < vertex1.Adj2Edges.Count; i++)
                {
                    var a = vertex1.Adj2Edges[i].vertex0;
                    var b = vertex1.Adj2Edges[i].vertex1;
                    if (a.IsContracted || b.IsContracted)
                        continue;
                    a.NeighboursFor2DMatching.Add(b);
                    b.NeighboursFor2DMatching.Add(a);
                }
                for (int i = 0; i < vertex0.Adj3Edges.Count; i++)
                {
                    var edge3 = vertex0.Adj3Edges[i];
                    Vertex a, b = null;
                    if (edge3.vertex0 == vertex0)
                    {
                        a = edge3.vertex1;
                        b = edge3.vertex2;
                    }
                    else if (edge3.vertex1 == vertex0)
                    {
                        a = edge3.vertex0;
                        b = edge3.vertex2;
                    }
                    else
                    {
                        a = edge3.vertex0;
                        b = edge3.vertex1;
                    }
                    if (a.IsContracted && a.ContractedVertex == b.ContractedVertex)
                    {
                        a.ContractedVertex.NeighboursFor2DMatching.Add(vertex0);
                        vertex0.NeighboursFor2DMatching.Add(a.ContractedVertex);
                    }
                }
                for (int i = 0; i < vertex1.Adj3Edges.Count; i++)
                {
                    var edge3 = vertex1.Adj3Edges[i];
                    Vertex a, b = null;
                    if (edge3.vertex0 == vertex1)
                    {
                        a = edge3.vertex1;
                        b = edge3.vertex2;
                    }
                    else if (edge3.vertex1 == vertex1)
                    {
                        a = edge3.vertex0;
                        b = edge3.vertex2;
                    }
                    else
                    {
                        a = edge3.vertex0;
                        b = edge3.vertex1;
                    }
                    if (a.IsContracted && a.ContractedVertex == b.ContractedVertex)
                    {
                        a.ContractedVertex.NeighboursFor2DMatching.Add(vertex1);
                        vertex1.NeighboursFor2DMatching.Add(a.ContractedVertex);
                    }
                }
                potentialRootesLinkedList.Remove(contractedVertex);  //sicherstellen dass nicht benötigt
                ContractedVertices.Remove(contractedVertex);
                potentialRootesLinkedList.AddLast(vertex1);
                potentialRootesLinkedList.AddLast(vertex0);
                continue;
            }
        }

        private bool TryAndDoAugmentation(Vertex newRoot, List<Vertex> plusTreeVertices, Vertex activeVertex)
        {
            bool augmentationHasBeenPerformed = false;
            var neighbouhood = activeVertex.NeighboursFor2DMatching;
            for (int i = 0; i < neighbouhood.Count; i++)
            {
                var neighbour = neighbouhood[i];
                if (neighbour.IsContracted)
                    continue;
                if (neighbour.MatchedVertex == null && neighbour != newRoot)
                {
                    Augment(plusTreeVertices, activeVertex, neighbour);
                    augmentationHasBeenPerformed = true;
                    break;
                }
            }

            return augmentationHasBeenPerformed;
        }

        private (List<Edge> matching, List<Vertex> unmatchedVertices) ReconstructMatching()
        {
            var resMatching = new List<Edge>(Vertices.Count / 2);
            var uncoveredVertices = new List<Vertex>();
            for (int i = 0; i < Vertices.Count; i++)
            {
                var vertex = Vertices[i];
                if (vertex.IsContracted)
                    continue;
                if (vertex.MatchedVertex != null)
                {
                    if (vertex.Id < vertex.MatchedVertex.Id)
                    {
                        resMatching.Add(new Edge(new List<Vertex> { vertex, vertex.MatchedVertex }));
                    }
                }
                else
                {
                    uncoveredVertices.Add(vertex);
                    resMatching.Add(new Edge(new List<Vertex> { vertex }));

                }
            }
            if (ContractedVertices != null)
            {
                for (int i = 0; i < ContractedVertices.Count; i++)
                {
                    var contractedVertex = ContractedVertices[i];
                    if (contractedVertex.MatchedVertex != null)
                    {
                        resMatching.Add(new Edge(new List<Vertex> { contractedVertex.OriginalVertex0, contractedVertex.OriginalVertex1, contractedVertex.MatchedVertex }));
                    }
                    else
                    {
                        resMatching.Add(new Edge(new List<Vertex> { contractedVertex.OriginalVertex0, contractedVertex.OriginalVertex1 }));
                    }
                }
            }
            return (resMatching, uncoveredVertices);
        }

        private bool PartialAugment(Vertex newRoot, List<Vertex> plusTreeVertices, bool augmentationHasBeenPerformed, bool prefereContractedVerticesAsSingle = false)
        {
            var x = plusTreeVertices[_random.Next(Math.Min(8, plusTreeVertices.Count / 5))];       //sorgt dafür, dass die augmenting paths nicht zu lange werden
            //var x = plusTreeVertices[0];
            if (prefereContractedVerticesAsSingle)
            {
                int i = 0;
                while (x.Predecessor != null && i<15 && x.Id>=0 )
                {
                    x = plusTreeVertices[_random.Next(plusTreeVertices.Count)];
                }
            }
                while (x.Predecessor != null)
                {
                    x = plusTreeVertices[_random.Next(plusTreeVertices.Count)];
                }

            
            if (x == newRoot)
            {
                ResetTree(plusTreeVertices);
                return true;
            }
            Vertex activeNeighbour;
            Vertex activeVertex;
            activeNeighbour = x.MatchedVertex;
            activeVertex = activeNeighbour.Predecessor;

            x.MatchedVertex = null;
            x.OddPath = null;
            x.IsInTree = false;
            x.Predecessor = null;
            x.BlossomIndex = 0;
            plusTreeVertices.Remove(x);
            Augment(plusTreeVertices, activeVertex, activeNeighbour);
            return true;
        }


        private void Augment(List<Vertex> plusTreeVertices, Vertex activeVertex, Vertex activeNeighbour)
        {
            activeNeighbour.Predecessor = activeVertex;
            (Vertex, List<Vertex>) upwardsInfo = (activeNeighbour, null);
            while (upwardsInfo != (null, null))
            {
                upwardsInfo = DoNextAugmentationStep(upwardsInfo);
            }

            ResetTree(plusTreeVertices);
        }
        public static List<Vertex> CalculateEvenPathToRoot(Vertex activeVertex, Vertex root)
        {
            var evenPath = new List<Vertex>();
            (Vertex, List<Vertex>) upwardsInfo = (activeVertex, activeVertex.OddPath);
            while (upwardsInfo.Item1!= root)
            {
                upwardsInfo = GoOneStepUpwards(upwardsInfo, evenPath);
            }
            evenPath.Add(root);

            return evenPath;
        }

        private static (Vertex, List<Vertex>) GoOneStepUpwards((Vertex matchedVertex, List<Vertex> oddPath) upwardsInfo, List<Vertex> evenPath)
        {
            (Vertex matchedVertex, List<Vertex> oddPath) newUpwardsInfo = (null, null);
            if (upwardsInfo.oddPath != null)
            {

                newUpwardsInfo.matchedVertex = upwardsInfo.oddPath.Last();
                newUpwardsInfo.oddPath = upwardsInfo.oddPath.Last().OddPath;
                evenPath.Add(upwardsInfo.matchedVertex);
                evenPath.AddRange(upwardsInfo.oddPath);
                evenPath.RemoveAt(evenPath.Count - 1);//last gets active in next step and therefore added next step
            }
            else
            {
                var lowerVertex = upwardsInfo.matchedVertex;
                var higherVertex = upwardsInfo.matchedVertex.MatchedVertex;

                newUpwardsInfo.matchedVertex = higherVertex.Predecessor;
                newUpwardsInfo.oddPath = higherVertex.Predecessor.OddPath;

                evenPath.Add(lowerVertex);
                evenPath.Add(higherVertex);
            }
            return newUpwardsInfo;
        }
        private static void CalculateAndSetOddPaths(Vertex aVertex, Vertex bVertex, List<Vertex> plusTreeVertices, ref int blossomCount)
        {
            //calculate common Vertex
            var root = plusTreeVertices[0];
            var rootPahtA = CalculateEvenPathToRoot(aVertex, root);
            var rootPahtB = CalculateEvenPathToRoot(bVertex, root);

            int indexA;
            int indexB;

            (indexA, indexB) = ComputeIndicesOfRootPaths(rootPahtA, rootPahtB);

            

            //calculate path Upwards to firstCommonVertex;
            var pathAUpwards = new List<Vertex>(indexA + 1);
            for (int i = 0; i < indexA + 1; i++)
            {
                pathAUpwards.Add(rootPahtA[i]);
            }
            var pathBUpwards = new List<Vertex>(indexB + 1);
            for (int i = 0; i < indexB + 1; i++)
            {
                pathBUpwards.Add(rootPahtB[i]);
            }

            for (int i = pathAUpwards.Count - 1; i >= 0; i--)
            {
                var x = pathAUpwards[i];
                if (x.Predecessor == null || x.OddPath != null)
                    continue;

                if(x.OddPath==null && x.Predecessor != null)
                {
                    plusTreeVertices.Add(x);
                }
                x.OddPath = new List<Vertex>(pathAUpwards.Count - i - 1 + pathBUpwards.Count);
                for (int j = i - 1; j >= 0; j--)
                {
                    x.OddPath.Add(pathAUpwards[j]);
                }
                for (int j = 0; j < pathBUpwards.Count; j++)
                {
                    x.OddPath.Add(pathBUpwards[j]);
                }

            }

            for (int i = pathBUpwards.Count-1; i>=0; i--)
            {
                var x = pathBUpwards[i];
                if (x.Predecessor == null || x.OddPath != null)
                    continue;
                if (x.OddPath == null && x.Predecessor != null)
                {
                    plusTreeVertices.Add(x);
                }
                x.OddPath = new List<Vertex>(pathBUpwards.Count - i - 1 + pathAUpwards.Count);
                for (int j = i -1; j >= 0; j--)
                {
                    x.OddPath.Add(pathBUpwards[j]);
                }
                for (int j = 0; j < pathAUpwards.Count; j++)
                {
                    x.OddPath.Add(pathAUpwards[j]);
                }

            }


            var blossomIndizesToReplace = new List<int>();
            for(int i = 0; i < pathAUpwards.Count; i++)
            {
                if (pathAUpwards[i].BlossomIndex != 0)
                    blossomIndizesToReplace.Add(pathAUpwards[i].BlossomIndex);
            }
            for (int i = 0; i < pathBUpwards.Count; i++)
            {
                if (pathBUpwards[i].BlossomIndex != 0)
                    blossomIndizesToReplace.Add(pathBUpwards[i].BlossomIndex);
            }
            blossomCount++;
            for(int i = 0; i<plusTreeVertices.Count;i++)
            {
                var plusVertex = plusTreeVertices[i];
                for (int j = 0; j < blossomIndizesToReplace.Count; j++)
                {
                    if (blossomIndizesToReplace[j]==plusVertex.BlossomIndex)
                    {
                        plusVertex.BlossomIndex = blossomCount;
                        if (plusVertex.MatchedVertex != null)
                            plusVertex.MatchedVertex.BlossomIndex = blossomCount;
                        break;
                    }
                }
            }
            for(int i = 0; i<pathAUpwards.Count;i++)
            {
                pathAUpwards[i].BlossomIndex = blossomCount;
            }

            for(int i = 0; i<pathBUpwards.Count;i++)
            {
                pathBUpwards[i].BlossomIndex = blossomCount;
            }
        }

        private static (int indexA,int indexB) ComputeIndicesOfRootPaths(List<Vertex> rootPahtA, List<Vertex> rootPahtB)
        {
            for (int i = 0; i < rootPahtA.Count; i++)
            {
                for (int j = 0; j < rootPahtB.Count; j++)
                {
                    if (rootPahtA[i] == rootPahtB[j])
                    {
                        if (rootPahtA[i].BlossomIndex == 0)
                        {
                            return (i, j);
                        }
                        var blossomIndex = rootPahtA[i].BlossomIndex;
                        while (i>0 && rootPahtA[i-1].BlossomIndex==blossomIndex)
                        {
                            i--;
                        }
                        while (j > 0 && rootPahtB[j - 1].BlossomIndex == blossomIndex)
                        {
                            j--;
                        }
                        return (i, j);
                    }
                }
            }
            return (-1, -1);
        }

        public void ResetTree(List<Vertex> plusTreeVertices)
        {
            //foreach (var plusVertex in plusTreeVertices)
            for(int i = 0; i< plusTreeVertices.Count;i++)
            {
                var plusVertex = plusTreeVertices[i];
                plusVertex.IsInTree = false;
                plusVertex.OddPath = null;
                plusVertex.Predecessor = null;
                plusVertex.BlossomIndex = 0;
                if (plusVertex.MatchedVertex == null)
                    continue;
                plusVertex.MatchedVertex.IsInTree = false;
                plusVertex.MatchedVertex.OddPath = null;
                plusVertex.MatchedVertex.Predecessor = null;
                plusVertex.MatchedVertex.BlossomIndex = 0;
            }
        }

        private (Vertex, List<Vertex>) DoNextAugmentationStep((Vertex matchedVertex, List<Vertex> oddPath) upwardsInfo)
        {
            (Vertex matchedVertex, List<Vertex> oddPath) newUpwardsInfo = (null, null);
            if (upwardsInfo.oddPath != null)
            {
                var lastVertexOfOddPAth = upwardsInfo.oddPath[upwardsInfo.oddPath.Count - 1];
                newUpwardsInfo.matchedVertex = lastVertexOfOddPAth.MatchedVertex;
                newUpwardsInfo.oddPath = lastVertexOfOddPAth.OddPath;

                for (int i = 0; i < upwardsInfo.oddPath.Count; i += 2)
                {
                    var vertex0 = upwardsInfo.oddPath[i];
                    var vertex1 = upwardsInfo.oddPath[i + 1];

                    vertex0.MatchedVertex = vertex1;
                    vertex1.MatchedVertex = vertex0;
                }
            }
            else
            {
                var lowerVertex = upwardsInfo.matchedVertex;
                var higherVertex = upwardsInfo.matchedVertex.Predecessor;

                newUpwardsInfo.matchedVertex = higherVertex.MatchedVertex;
                newUpwardsInfo.oddPath = higherVertex.OddPath;

                lowerVertex.MatchedVertex = higherVertex;
                higherVertex.MatchedVertex = lowerVertex;
            }
            return newUpwardsInfo;
        }
    }

    public class Edge
    {
        public List<Vertex> Vertices;
        public List<int> VerticesIds;
        public bool IsInCover = false;
        public double property1;
        public Vertex plusVertex;
        public Edge linkingEdge;
        public int treeIndex;
        private List<Vertex> _expandables;

        public int inedex;

        public bool canBeInPerfectMatching = false;

        public List<Vertex> Expandables()
        {
            if (this.Vertices.Count != 2)
                throw new Exception("only two size edges are expandable");
            var res = new List<Vertex>();
            if (_expandables == null)
            {
                if (vertex0.Adj3Edges.Count < vertex1.Adj3Edges.Count)//determine which neighberhood should be searched 
                {
                    for(int i = 0; i<vertex0.Adj3Edges.Count; i++)
                    {
                        var edge3 = vertex0.Adj3Edges[i];
                        if(edge3.vertex1 == vertex1)
                        {
                            res.Add(edge3.vertex2);
                        }
                        else if (edge3.vertex2 == vertex1)
                        {
                            if (edge3.vertex0 == vertex0)
                            {
                                res.Add(edge3.vertex1);
                            }
                            else
                            {
                                res.Add(edge3.vertex0);
                            }
                        }

                    }
                }
                else
                {
                    for (int i = 0; i < vertex1.Adj3Edges.Count; i++)
                    {
                        var edge3 = vertex1.Adj3Edges[i];
                        if (edge3.vertex1 == vertex0)
                        {
                            res.Add(edge3.vertex0);
                        }
                        else if (edge3.vertex0 == vertex0)
                        {
                            if (edge3.vertex1 == vertex1)
                            {
                                res.Add(edge3.vertex2);
                            }
                            else
                            {
                                res.Add(edge3.vertex1);
                            }
                        }

                    }
                }
                _expandables = res;
            }

            return _expandables;
        }

        public Vertex vertex0;
        public Vertex vertex1;
        public Vertex vertex2;
        public int VertexCount;


        public Edge(List<Vertex> vertices)
        {
            Vertices = vertices;
            VerticesIds = vertices.Select(_ => _.Id).ToList();
            VertexCount = Vertices.Count;
            vertex0 = Vertices[0];
            if (VertexCount >= 2)
                vertex1 = Vertices[1];
            if (VertexCount >= 3)
                vertex2 = Vertices[2];
        }
        public override string ToString()
        {
            return String.Join(" ", Vertices.Select(_=>_.Id));
        }
        public bool Contains(Vertex vertex)
        {
            if (vertex0 == vertex)
                return true;
            if (vertex1 == vertex)
                return true;
            if (vertex2 == vertex)
                return true;
            return false;
        }
        public bool Contains(Edge edge)
        {
            if(edge.VertexCount == 1)
            {
                if (this.Contains(edge.vertex0))
                    return true;
            }
            else if(edge.VertexCount == 2)
            {
                if (this.Contains(edge.vertex0) && this.Contains(edge.vertex1))
                    return true;
            }
            else
            {
                if (vertex0 == edge.vertex0 && vertex1 == edge.vertex1 && vertex2==edge.vertex2)    //funktioniert nur wenn sortiert
                    return true;
            }
            return false;    

        }
        public override bool Equals(Object otherEdge)          //immer Knoten in Kante aufsteigend sortieren, sonst fehleranfällig!
        {
            var vert1 = (otherEdge as Edge).VerticesIds;
            var vert2 = VerticesIds;
            if (VerticesIds.Count != vert1.Count)
                return false;
            for (int i = 0; i < Vertices.Count; i++)
                if (!VerticesIds.Contains(vert1[i]))
                    return false;
            return true;
        }

        public bool AllVerticesAreUncovered()
        {
            //foreach (var vertex in Vertices)
            for(int i = 0;i< Vertices.Count;i++)
            {
                var vertex = Vertices[i];
                if (vertex.IsCovered)
                    return false;
            }
            return true;
        }

        public bool AllVerticesAreCoveredAtleastTwice()
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                var vertex = Vertices[i];
                if (vertex.TimesCovered<2)
                    return false;
            }
            return true;
        }
        public int NumberOfNewCoveredVertices()
        {
            int i = 0;
            foreach (var vertex in Vertices)
                if (!vertex.IsCovered)
                    i++;
            return i;
        }
        public int NumberOfAlreadyCoveredVertices()
        {
            int i = 0;
            foreach (var vertex in Vertices)
                if (vertex.IsCovered)
                    i++;
            return i;
        }

        public bool ContainsTime(int time)
        {
            foreach(var vertex in Vertices)
            {
                if (vertex.ContainsTime(time))
                    return true;
            }
            return false;
        }
    }
    public class Vertex
    {
        public List<Vertex> NeighboursFor2DMatching;
        public Vertex Predecessor;
        public bool IsInTree;
        public List<Vertex> OddPath;
        public Vertex MatchedVertex;
        public bool IsContracted;
        public int BlossomIndex;
        public List<Edge> Adj2Edges;
        public List<Edge> Adj3Edges;
        public Vertex ContractedWith;
        public Vertex ContractedVertex;

        public int intersectionNumber;

        public List<Vertex> ForbiddenSuccessessors;
        public bool IsCovered = false;
        public int TimesCovered = 0;
        public int[] Interval;
        public double hardness = 0.0;  //high if several attempts, matching this vertex failtured
        public Edge coveredBy;
        public Dictionary<int, int> NeighbourhoodAndMultiplicity;
        public List<Edge> AdjEdges = null;
        public int treeIndex = -1;
        public Vertex(int id)
        {
            this.Id = id;
        }
        public int Id;
        internal Vertex OriginalVertex0;
        internal Vertex OriginalVertex1;

        public Edge Get2DEdgeBetween(Vertex oppositeVertex)
        {
            if (Adj2Edges.Count > oppositeVertex.Adj2Edges.Count)
                return oppositeVertex.Get2DEdgeBetween(this);
            if (oppositeVertex.Id < Id)
            {
                for (int i = 0; i < Adj2Edges.Count; i++)
                {
                    if (Adj2Edges[i].vertex0 == oppositeVertex)
                        return Adj2Edges[i];
                }
            }
            else
            {
                for (int i = 0; i < Adj2Edges.Count; i++)
                {
                    if (Adj2Edges[i].vertex1 == oppositeVertex)
                        return Adj2Edges[i];
                }
            }
            return null;
        }

        public bool Equals(Vertex otherObject)
        {
            return Id == otherObject.Id;
        }
        public override string ToString()
        {
            return Id + "";
        }

        internal bool ContainsTime(int time)
        {
            return Interval[0] <= time && Interval[1] >= time;
        }
        public bool Intersects(Vertex otherVertex)
        {
            if(Interval[0]<=otherVertex.Interval[0] && Interval[1] >= otherVertex.Interval[0])
            {
                return true;
            }
            if (otherVertex.Interval[0] <= Interval[0] && otherVertex.Interval[1] >= Interval[0])
            {
                return true;
            }
            return false;

        }
    }
}
