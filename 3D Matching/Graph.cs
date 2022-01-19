using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching
{
    public class Graph
    {
        bool _adjEdgesAreInitialized = false;
        public Graph(List<Edge> edges, List<Vertex> vertices)
        {
            _edges = edges;
            Vertices = vertices;
        }
        private List<Edge> _edges;
        public List<Vertex> Vertices;
        public List<Vertex> ContractedVertices;
        public List<Edge> Edges { get => _edges!=null? _edges:GetAndSetEdges(); } 
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

        public static Graph BuildGraphFromCSV(String path, bool allowAllAsSingle = false, bool removeDegreeOne = false)
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
                if (activeLine.Split(";")[1] == "True" || allowAllAsSingle)
                    edges.Add(new Edge(new List<Vertex> { newVertex }));
            }
            
            for (rowCount++; rowCount < inputLines.Length; rowCount++)
            {
                var activeLine = inputLines[rowCount];
                if (activeLine.Contains("--"))
                    continue;
                var newEdge = new Edge(activeLine.Split("->").Select(_ => vertices[Int32.Parse(_)]).ToList());
                edges.Add(newEdge);
            }


            //foreach (var vertex in vertices)        //wichtig falls kein matching existiert
            //{
            //    edges.Add(new Edge(new List<Vertex> { vertex }));
            //}

            var graph = new Graph(edges, vertices);

            if (removeDegreeOne)
            {
                graph.SetVertexAdjEdges();
                int i = 0;
                while (i < vertices.Count)
                {
                    if (vertices[i].AdjEdges.Count == 1 && vertices[i].AdjEdges[0].Vertices.Count==1)
                    {
                        foreach(var vertex in vertices)
                        {
                            if(vertex.Id> vertices[i].Id)
                            {
                                vertex.Id--;
                            }
                        }
                        edges.Remove(vertices[i].AdjEdges[0]);
                        vertices.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
                foreach(var edge in edges)
                {
                    edge.VerticesIds = edge.Vertices.Select(_ => _.Id).ToList();
                }
            }
            return graph;
        }

        public void SetVertexAdjEdges()
        {
            if (_adjEdgesAreInitialized)
                return;
            _adjEdgesAreInitialized = true;
            foreach (var vertex in Vertices)
                vertex.AdjEdges = new List<Edge>();
            foreach (var edge in _edges)
                foreach (var vertex in edge.Vertices)
                    vertex.AdjEdges.Add(edge);
        }
        public void ResetVertexAdjEdges()
        {
            _adjEdgesAreInitialized = true;
            foreach (var vertex in Vertices)
                vertex.AdjEdges = new List<Edge>();
            foreach (var edge in _edges)
                foreach (var vertex in edge.Vertices)
                    vertex.AdjEdges.Add(edge);
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

        public Graph GenerateInducedSubgraph(List<Edge> toOptimizeEdges)
        {
            var tmpVertices = toOptimizeEdges.SelectMany(_ => _.Vertices);
            //Maybe remove duplicate
            var tmpEdges = Edges.Where(_ => _.Vertices.Where(x => tmpVertices.Contains(x)).Count() == _.Vertices.Count).ToList();
            var tmpGraph = new Graph(tmpEdges, tmpVertices.ToList());
            return tmpGraph;
        }
        public Graph GenerateInducedSubgraph(List<Vertex> inducedVertices)
        {
            //Maybe remove duplicate
            var tmpEdges = Edges.Where(_ => _.Vertices.Where(x => inducedVertices.Contains(x)).Count() == _.Vertices.Count).ToList();
            var tmpGraph = new Graph(tmpEdges, inducedVertices.ToList());
            return tmpGraph;
        }
        public void InitializeFor2DMatchin(List<Edge> initialMatching = null, List<Edge> contractingEdges = null)
        {
            SetVertexAdjEdges();
            ContractedVertices = new List<Vertex>();
            

            foreach (var vertex in Vertices)
            {
                vertex.NeighboursFor2DMatching = new List<Vertex>();
                foreach (var edge in vertex.AdjEdges)
                {
                    if (edge.Vertices.Count != 2)
                        continue;
                    vertex.NeighboursFor2DMatching.Add(edge.Vertices[0] == vertex ? edge.Vertices[1] : edge.Vertices[0]);
                }
            }

            if (contractingEdges != null)
            {
                for (int i = 0; i < contractingEdges.Count; i++)
                {
                    var contractingEdge = contractingEdges[i];

                    var contractedVertex = new Vertex(-i - 1);
                    contractedVertex.OriginalVertex0 = contractingEdge.Vertices[0];
                    contractedVertex.OriginalVertex1 = contractingEdge.Vertices[0];
                    
                    ContractedVertices.Add(contractedVertex);

                    var vertex0 = contractingEdge.Vertices[0];
                    var vertex1 = contractingEdge.Vertices[1];

                    vertex0.IsContracted = true;
                    vertex1.IsContracted = true;

                    //vertex.VerticesFor3DEdge = new List<Vertex>();
                    foreach (var edge in vertex0.AdjEdges) //speed: nicht alle müssen durchlaufen werden unfd forschleife
                    {
                        if (edge.Vertices.Count == 1 || edge == contractingEdge)
                            continue;
                        if (edge.Vertices.Count == 2)
                        {
                            edge.Vertices[0].NeighboursFor2DMatching.Remove(edge.Vertices[1]);
                            edge.Vertices[1].NeighboursFor2DMatching.Remove(edge.Vertices[0]);
                        }
                        if (edge.Vertices.Count == 3)
                        {
                            if ((edge.Vertices[0] == vertex0 && edge.Vertices[1] == vertex1) || (edge.Vertices[1] == vertex0 && edge.Vertices[0] == vertex1))
                            {
                                edge.Vertices[2].NeighboursFor2DMatching.Add(contractedVertex);
                                contractedVertex.NeighboursFor2DMatching.Add(edge.Vertices[2]);
                            }
                            else if ((edge.Vertices[2] == vertex0 && edge.Vertices[1] == vertex1) || (edge.Vertices[1] == vertex0 && edge.Vertices[2] == vertex1))
                            {
                                edge.Vertices[0].NeighboursFor2DMatching.Add(contractedVertex);
                                contractedVertex.NeighboursFor2DMatching.Add(edge.Vertices[0]);
                            }
                            else if ((edge.Vertices[2] == vertex0 && edge.Vertices[0] == vertex1) || (edge.Vertices[0] == vertex0 && edge.Vertices[2] == vertex1))
                            {
                                edge.Vertices[1].NeighboursFor2DMatching.Add(contractedVertex);
                                contractedVertex.NeighboursFor2DMatching.Add(edge.Vertices[1]);
                            }
                        }
                    }
                    foreach (var edge in vertex0.AdjEdges)
                    {
                        if (edge.Vertices.Count == 1 || edge == contractingEdge)
                            continue;
                        if (edge.Vertices.Count == 2)
                        {
                            edge.Vertices[0].NeighboursFor2DMatching.Remove(edge.Vertices[1]);
                            edge.Vertices[1].NeighboursFor2DMatching.Remove(edge.Vertices[0]);
                        }
                    }
                }
            }

            if (initialMatching != null)
            {
                for (int i = 0; i < initialMatching.Count; i++)
                {
                    var edge = initialMatching[i];
                    if (edge.Vertices.Count == 2)
                    {
                        var vertex0 = edge.Vertices[0];
                        var vertex1 = edge.Vertices[1];

                        vertex0.MatchedVertex = vertex1;
                        vertex1.MatchedVertex = vertex0;
                    }
                }

            }
        }
        public (List<Edge> maxMmatching, List<Vertex> uncoveredVertices) GetMaximum2DMatching()
        {
            var potentialRoots = Vertices.Concat(ContractedVertices).Where(_=>_.MatchedVertex==null).ToList();
            while (potentialRoots.Count > 0)
            {
                var newRoot = potentialRoots.First();
                newRoot.IsInTree = true;
                var plusTreeVertices = new List<Vertex>();
                plusTreeVertices.Add(newRoot);
                potentialRoots.RemoveAt(0);
                var stack = new List<Vertex>();
                stack.Add(newRoot);

                var augmentationHasBeenPerformed = false;
                int blossomCount = 0;

                while (!augmentationHasBeenPerformed && stack.Count > 0)
                {
                    var activeVertex = stack.First();
                    stack.Remove(activeVertex);

                    for (int neighbourIndex = 0; neighbourIndex < activeVertex.NeighboursFor2DMatching.Count; neighbourIndex++)
                    {
                        var activeNeighbour = activeVertex.NeighboursFor2DMatching[neighbourIndex];
                        if (activeNeighbour.IsInTree && (activeNeighbour.BlossomIndex != activeVertex.BlossomIndex || activeNeighbour.BlossomIndex + activeVertex.BlossomIndex==0) && (activeNeighbour.Predecessor == null || activeNeighbour.OddPath != null))
                        { // do blossom building.                         => no blossominternal edge                                                                                            =>is of type plus               => is blossom of tpye plus          
                            
                            CalculateAndSetOddPaths(activeVertex, activeNeighbour, stack,plusTreeVertices, ref blossomCount);

                        }
                        else if (activeNeighbour.IsInTree == false)
                        {
                            if (activeNeighbour.MatchedVertex == null)
                            {// Augment

                                activeNeighbour.Predecessor = activeVertex;
                                potentialRoots.Remove(activeNeighbour);
                                (Vertex, List<Vertex>) upwardsInfo = (activeNeighbour, null);
                                while (upwardsInfo != (null, null))
                                {
                                    upwardsInfo = DoNextAugmentationStep(upwardsInfo);
                                }

                                ResetTree(plusTreeVertices);

                                augmentationHasBeenPerformed = true;
                                break;
                            }
                            else
                            {// grow
                                var newPlusVertex = activeNeighbour.MatchedVertex;
                                stack.Add(newPlusVertex);
                                activeNeighbour.Predecessor = activeVertex;
                                plusTreeVertices.Add(newPlusVertex);

                                newPlusVertex.IsInTree = true;
                                activeNeighbour.IsInTree = true;
                            }
                        }
                    }

                }
            }
            var resMatching = new List<Edge>();
            var uncoveredVertices = new List<Vertex>();
            foreach(var vertex in Vertices.Concat(ContractedVertices))
            {
                if (vertex.MatchedVertex != null)
                {
                    if (vertex.Id < vertex.MatchedVertex.Id)
                    {
                        if(vertex.Id<0)//contracted Vertex
                        {
                            resMatching.Add(new Edge(new List<Vertex> { vertex.OriginalVertex0, vertex.OriginalVertex1, vertex.MatchedVertex }));
                        }
                        else //normal vertex
                        {
                            resMatching.Add(new Edge(new List<Vertex> { vertex, vertex.MatchedVertex }));
                        }

                    }
                }
                else
                {
                    if (vertex.Id < 0)
                    {
                        resMatching.Add(new Edge(new List<Vertex> { vertex.OriginalVertex0, vertex.OriginalVertex1}));
                    }
                    else
                    {
                        uncoveredVertices.Add(vertex);
                    }
                }
            }
            return (resMatching, uncoveredVertices);
        }

        private static void CalculateAndSetOddPaths(Vertex activeVertex, Vertex activeNeighbour, List<Vertex> stack, List<Vertex> plusTreeVertices, ref int blossomCount)
        {
            //search commom vertex
            var hashSet = new HashSet<Vertex>();
            Vertex x = activeVertex;
            while (x != null)
            {
                hashSet.Add(x);
                x = x.MatchedVertex;
                if (x == null)
                    break;
                x = x.Predecessor;
            }
            x = activeNeighbour;
            while (!hashSet.Contains(x))
            {
                x = x.MatchedVertex;
                x = x.Predecessor;
            }
            var commonVertex = x;


            //compute BlossomPath
            var leftPathUpwards = new List<Vertex>();
            x = activeVertex;
            while (x != commonVertex)
            {
                leftPathUpwards.Add(x);
                x = x.MatchedVertex;
                leftPathUpwards.Add(x);
                x = x.Predecessor;
            }
            leftPathUpwards.Add(x);

            var rightPathUpwards = new List<Vertex>();
            x = activeNeighbour;
            while (x != commonVertex)
            {
                rightPathUpwards.Add(x);
                x = x.MatchedVertex;
                rightPathUpwards.Add(x);
                x = x.Predecessor;
            }
            rightPathUpwards.Add(x);
            var leftPathDownwards = leftPathUpwards.ToList();
            leftPathDownwards.Reverse();
            var rightPathDownwards = rightPathUpwards.ToList();
            rightPathDownwards.Reverse();


            //set oddPaths
            for(int i = 1; i< leftPathDownwards.Count; i += 2)
            {
                x = leftPathDownwards[i];
                x.OddPath = leftPathDownwards.Skip(i + 1).Concat(rightPathUpwards).ToList();
                stack.Add(x);
            }
            for (int i = 1; i < rightPathDownwards.Count; i += 2)
            {
                x = rightPathDownwards[i];
                x.OddPath = rightPathDownwards.Skip(i + 1).Concat(leftPathUpwards).ToList();
                stack.Add(x);
            }

            var blossomIndizesToReplace = leftPathDownwards.Concat(rightPathDownwards).Select(_ => _.BlossomIndex).Distinct().ToHashSet();
            blossomCount++;

            foreach(var plusVertex in plusTreeVertices)
            {
                if (blossomIndizesToReplace.Contains(plusVertex.BlossomIndex))
                    plusVertex.BlossomIndex = blossomCount;
            }
            foreach (var plusVertex in leftPathDownwards)
            {
                plusVertex.BlossomIndex = blossomCount;
            }
            foreach (var plusVertex in rightPathDownwards)
            {
                plusVertex.BlossomIndex = blossomCount;
            }
        }

        private static void ResetTree(List<Vertex> plusTreeVertices)
        {
            foreach (var plusVertex in plusTreeVertices)
            {
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
                newUpwardsInfo.matchedVertex = upwardsInfo.oddPath.Last().MatchedVertex;
                newUpwardsInfo.oddPath = upwardsInfo.oddPath.Last().OddPath;

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
        public Edge(List<Vertex> vertices)
        {
            Vertices = vertices;
            VerticesIds = vertices.Select(_ => _.Id).ToList();
        }
        public override string ToString()
        {
            return String.Join(" ", Vertices.Select(_=>_.Id));
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
            foreach (var vertex in Vertices)
            {
                if (vertex.IsCovered)
                    return false;
            }
            return true;
        }
        public bool AllVerticesAreCoveredAtleastTwice()
        {
            foreach (var vertex in Vertices)
            {
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

        internal bool ContainsTime(int time)
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

        public bool Equals(Vertex otherObject)
        {
            return Id== otherObject.Id;
        }
        public override string ToString()
        {
            return Id +"";
        }

        internal bool ContainsTime(int time)
        {
            return Interval[0] <= time && Interval[1] >= time;
        }
    }
}
