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
        private List<Edge> _2DEdges;
        public List<Edge> TwodEdges()
        {
            if (_2DEdges == null)
            {
                _2DEdges = Edges.Where(_ => _.Vertices.Count == 2).ToList();
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

            //initialize Vertices
            SetInitialValuesForVertices(contractingEdges);

            foreach (var edge in TwodEdges())
            {
                if (edge.Vertices[0].IsContracted || edge.Vertices[1].IsContracted)
                    continue;
                edge.Vertices[1].NeighboursFor2DMatching.Add(edge.Vertices[0]);
                edge.Vertices[0].NeighboursFor2DMatching.Add(edge.Vertices[1]);
            }

            SetContractingVerticesNeighberhood(contractingEdges);

            SetInitialMatching(initialMatching);
        }

        private void SetInitialValuesForVertices(List<Edge> contractingEdges)
        {
            foreach (var vertex in Vertices) //nicht jedesmal resetten sondern weiter benutzen
            {
                vertex.IsContracted = false;
                vertex.MatchedVertex = null;
                vertex.IsInTree = false;
                vertex.OddPath = null;
                vertex.NeighboursFor2DMatching = new List<Vertex>();
            }

            if (contractingEdges != null)
            {
                for (int i = 0; i < contractingEdges.Count; i++)
                {
                    var contractingEdge = contractingEdges[i];
                    var vertex0 = contractingEdge.Vertices[0];
                    var vertex1 = contractingEdge.Vertices[1];

                    vertex0.IsContracted = true;
                    vertex1.IsContracted = true;
                }
            }
        }

        private static void SetInitialMatching(List<Edge> initialMatching)
        {
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

        private void SetContractingVerticesNeighberhood(List<Edge> contractingEdges)
        {
            if (contractingEdges != null)
            {
                for (int i = 0; i < contractingEdges.Count; i++)
                {
                    var contractingEdge = contractingEdges[i];
                    var vertex0 = contractingEdge.Vertices[0];
                    var vertex1 = contractingEdge.Vertices[1];

                    var contractedVertex = new Vertex(-i - 1);
                    contractedVertex.OriginalVertex0 = vertex0;
                    contractedVertex.OriginalVertex1 = vertex1;
                    contractedVertex.NeighboursFor2DMatching = new List<Vertex>();

                    ContractedVertices.Add(contractedVertex);
                    if (vertex0.AdjEdges.Count > vertex1.AdjEdges.Count)
                    {
                        var tmp = vertex0;
                        vertex0 = vertex1;
                        vertex1 = tmp;
                    }
                    for(int k = 0; k<vertex0.AdjEdges.Count;k++)
                    {
                        var edge = vertex0.AdjEdges[k];
                        if (edge.Vertices.Count != 3)
                            continue;

                        if (((edge.Vertices[0] == vertex0 && edge.Vertices[1] == vertex1) || (edge.Vertices[1] == vertex0 && edge.Vertices[0] == vertex1)) && edge.Vertices[2].IsContracted == false)
                        {
                            edge.Vertices[2].NeighboursFor2DMatching.Add(contractedVertex);
                            contractedVertex.NeighboursFor2DMatching.Add(edge.Vertices[2]);
                        }
                        else if (((edge.Vertices[2] == vertex0 && edge.Vertices[1] == vertex1) || (edge.Vertices[1] == vertex0 && edge.Vertices[2] == vertex1)) && edge.Vertices[0].IsContracted == false)
                        {
                            edge.Vertices[0].NeighboursFor2DMatching.Add(contractedVertex);
                            contractedVertex.NeighboursFor2DMatching.Add(edge.Vertices[0]);
                        }
                        else if (((edge.Vertices[2] == vertex0 && edge.Vertices[0] == vertex1) || (edge.Vertices[0] == vertex0 && edge.Vertices[2] == vertex1)) && edge.Vertices[1].IsContracted == false)
                        {
                            edge.Vertices[1].NeighboursFor2DMatching.Add(contractedVertex);
                            contractedVertex.NeighboursFor2DMatching.Add(edge.Vertices[1]);
                        }

                    }
                }
            }
        }

        public (List<Edge> maxMmatching, List<Vertex> uncoveredVertices) GetMaximum2DMatching()
        {
            List<Vertex> potentialRoots;
            if (ContractedVertices == null)
                potentialRoots = Vertices.Where(_ => _.MatchedVertex == null).ToList();
            else
                potentialRoots = Vertices.Concat(ContractedVertices).Where(_ => _.MatchedVertex == null && !_.IsContracted)/*.OrderBy(_=>_random.Next())*/.ToList();
            while (potentialRoots.Count > 0)
            {
                var newRoot = potentialRoots.First();
                newRoot.Label = 1;
                newRoot.IsInTree = true;
                var plusTreeVertices = new List<Vertex>();
                plusTreeVertices.Add(newRoot);
                potentialRoots.RemoveAt(0);
                var stack = new List<Vertex>();
                stack.Add(newRoot);

                var augmentationHasBeenPerformed = false;
                int blossomCount = 0;

                augmentationHasBeenPerformed = TryAndDoAugmentation(potentialRoots, newRoot, plusTreeVertices, newRoot);

                while (!augmentationHasBeenPerformed && stack.Count > 0)
                {
                    var activeVertex = stack.First();
                    stack.Remove(activeVertex);

                    for (int neighbourIndex = 0; neighbourIndex < activeVertex.NeighboursFor2DMatching.Count; neighbourIndex++)
                    {
                        var activeNeighbour = activeVertex.NeighboursFor2DMatching[neighbourIndex];
                        if (activeNeighbour.IsInTree && (activeNeighbour.BlossomIndex != activeVertex.BlossomIndex || activeNeighbour.BlossomIndex + activeVertex.BlossomIndex == 0) && (activeNeighbour.Predecessor == null || activeNeighbour.OddPath != null))
                        { // do blossom building.                         => no blossominternal edge                                                                                            =>is of type plus               => is blossom of tpye plus          
                            var oldStackCount = stack.Count;
                            CalculateAndSetOddPaths(activeVertex, activeNeighbour, stack, plusTreeVertices, ref blossomCount);
                            
                            for (int i = oldStackCount; i < stack.Count; i++)
                            {
                                var v = stack[i];
                                augmentationHasBeenPerformed = TryAndDoAugmentation(potentialRoots, newRoot, plusTreeVertices, v);
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
                                activeNeighbour.Label = activeVertex.Label * -1;
                                newPlusVertex.Label = activeVertex.Label;
                                stack.Add(newPlusVertex);
                                activeNeighbour.Predecessor = activeVertex;
                                plusTreeVertices.Add(newPlusVertex);

                                newPlusVertex.IsInTree = true;
                                activeNeighbour.IsInTree = true;

                                augmentationHasBeenPerformed = TryAndDoAugmentation(potentialRoots, newRoot, plusTreeVertices, newPlusVertex);
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
                    augmentationHasBeenPerformed = PartialAugment(potentialRoots, newRoot, plusTreeVertices, augmentationHasBeenPerformed);
                }
            }
            List<Edge> resMatching;
            List<Vertex> uncoveredVertices;
            ReconstructMatching(out resMatching, out uncoveredVertices);
            return (resMatching, uncoveredVertices);
        }

        public void ComputeMaximum2DMatching()
        {
            List<Vertex> potentialRoots;
            if (ContractedVertices == null)
                potentialRoots = Vertices.Where(_ => _.MatchedVertex == null).ToList();
            else
                potentialRoots = Vertices.Concat(ContractedVertices).Where(_ => _.MatchedVertex == null && !_.IsContracted)/*.OrderBy(_=>_random.Next())*/.ToList();

            while (potentialRoots.Count > 0)
            {
                var newRoot = potentialRoots.First();
                newRoot.Label = 1;
                newRoot.IsInTree = true;
                var plusTreeVertices = new List<Vertex>();
                plusTreeVertices.Add(newRoot);
                potentialRoots.RemoveAt(0);
                var stack = new List<Vertex>();
                stack.Add(newRoot);

                var augmentationHasBeenPerformed = false;
                int blossomCount = 0;

                augmentationHasBeenPerformed = TryAndDoAugmentation(potentialRoots, newRoot, plusTreeVertices, newRoot);

                while (!augmentationHasBeenPerformed && stack.Count > 0)
                {
                    var activeVertex = stack.First();
                    stack.Remove(activeVertex);

                    for (int neighbourIndex = 0; neighbourIndex < activeVertex.NeighboursFor2DMatching.Count; neighbourIndex++)
                    {
                        var activeNeighbour = activeVertex.NeighboursFor2DMatching[neighbourIndex];
                        if (activeNeighbour.IsInTree && (activeNeighbour.BlossomIndex != activeVertex.BlossomIndex || activeNeighbour.BlossomIndex + activeVertex.BlossomIndex == 0) && (activeNeighbour.Predecessor == null || activeNeighbour.OddPath != null))
                        { // do blossom building.                         => no blossominternal edge                                                                                            =>is of type plus               => is blossom of tpye plus          
                            var oldStackCount = stack.Count;
                            CalculateAndSetOddPaths(activeVertex, activeNeighbour, stack, plusTreeVertices, ref blossomCount);

                            for (int i = oldStackCount; i < stack.Count; i++)
                            {
                                var v = stack[i];
                                augmentationHasBeenPerformed = TryAndDoAugmentation(potentialRoots, newRoot, plusTreeVertices, v);
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
                                activeNeighbour.Label = activeVertex.Label * -1;
                                newPlusVertex.Label = activeVertex.Label;
                                stack.Add(newPlusVertex);
                                activeNeighbour.Predecessor = activeVertex;
                                plusTreeVertices.Add(newPlusVertex);

                                newPlusVertex.IsInTree = true;
                                activeNeighbour.IsInTree = true;

                                augmentationHasBeenPerformed = TryAndDoAugmentation(potentialRoots, newRoot, plusTreeVertices, newPlusVertex);
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
                    augmentationHasBeenPerformed = PartialAugment(potentialRoots, newRoot, plusTreeVertices, augmentationHasBeenPerformed);
                }
            }
        }

        private bool TryAndDoAugmentation(List<Vertex> potentialRoots, Vertex newRoot, List<Vertex> plusTreeVertices, Vertex activeVertex)
        {
            bool augmentationHasBeenPerformed = false;
            for (int i = 0; i < activeVertex.NeighboursFor2DMatching.Count; i++)
            {
                var neighbour = activeVertex.NeighboursFor2DMatching[i];
                if (neighbour.MatchedVertex == null && neighbour != newRoot)
                {
                    //if (neighbour.IsContracted)
                    //    ;
                    Augment(potentialRoots, plusTreeVertices, activeVertex, neighbour);
                    augmentationHasBeenPerformed = true;
                    break;
                }
            }

            return augmentationHasBeenPerformed;
        }

        private void ReconstructMatching(out List<Edge> resMatching, out List<Vertex> uncoveredVertices)
        {
            resMatching = new List<Edge>();
            uncoveredVertices = new List<Vertex>();
            foreach (var vertex in Vertices.Concat(ContractedVertices).Where(_ => !_.IsContracted))
            {
                if (vertex.MatchedVertex != null)
                {
                    if (vertex.Id < vertex.MatchedVertex.Id)
                    {
                        if (vertex.Id < 0)//contracted Vertex
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
                        resMatching.Add(new Edge(new List<Vertex> { vertex.OriginalVertex0, vertex.OriginalVertex1 }));
                    }
                    else
                    {
                        uncoveredVertices.Add(vertex);
                        resMatching.Add(new Edge(new List<Vertex> { vertex }));
                    }
                }
            }
        }

        private bool PartialAugment(List<Vertex> potentialRoots, Vertex newRoot, List<Vertex> plusTreeVertices, bool augmentationHasBeenPerformed)
        {
            foreach (var x in plusTreeVertices)     //save as bool at vertex if allowed as singleton
            {
                if (x.Id < 0 || x.AdjEdges.Contains(new Edge(new List<Vertex> { x })))
                {
                    if (x == newRoot)
                        break;
                    Vertex activeNeighbour;
                    Vertex activeVertex;
                    if (x.OddPath != null)
                    {
                        activeVertex = x.OddPath[1];
                        activeNeighbour = x.OddPath[0];
                    }
                    else
                    {
                        activeNeighbour = x.MatchedVertex;
                        activeVertex = activeNeighbour.Predecessor;
                    }

                    x.MatchedVertex = null;
                    Augment(potentialRoots, plusTreeVertices, activeVertex, activeNeighbour);
                    augmentationHasBeenPerformed = true;
                    break;
                }
            }
            ResetTree(plusTreeVertices);
            return augmentationHasBeenPerformed;
        }

        private void Augment(List<Vertex> potentialRoots, List<Vertex> plusTreeVertices, Vertex activeVertex, Vertex activeNeighbour)
        {
            activeNeighbour.Predecessor = activeVertex;
            potentialRoots.Remove(activeNeighbour);
            (Vertex, List<Vertex>) upwardsInfo = (activeNeighbour, null);
            while (upwardsInfo != (null, null))
            {
                upwardsInfo = DoNextAugmentationStep(upwardsInfo);
            }

            ResetTree(plusTreeVertices);
        }
        public static List<Vertex> CalculateEvenPathToRoot(Vertex activeVertex, Vertex root)
        { //only unse on vertices which are in plusTreeVertices
            var evenPath = new List<Vertex>();
            (Vertex, List<Vertex>) upwardsInfo = (activeVertex, activeVertex.OddPath);
            while (upwardsInfo.Item1!= root)//upwardsInfo != (null, null))
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
    

        private static void CalculateAndSetOddPaths1(Vertex aVertex, Vertex bVertex, List<Vertex> stack, List<Vertex> plusTreeVertices, ref int blossomCount)
        {
            //search commom vertex
            var hashSetVertex = new HashSet<Vertex>();
            var hashSetBlossomIndex = new HashSet<int>();
            Vertex x = aVertex;
            if (x.Predecessor!=null)
            {
                x = x.Predecessor;
            }
            while (x != null)
            {
                hashSetVertex.Add(x);
                if (x.BlossomIndex != 0)
                    hashSetBlossomIndex.Add(x.BlossomIndex);
                x = x.MatchedVertex;
                if (x == null)
                    break;
                x = x.Predecessor;
            }
            x = bVertex;
            if (x.Predecessor != null)
            {
                x = x.Predecessor;
            }
            while (!hashSetVertex.Contains(x) && !hashSetBlossomIndex.Contains(x.BlossomIndex))
            {
                x = x.MatchedVertex;
                x = x.Predecessor;
            }
            var commonB = x;
            Vertex commonA;
            if (hashSetVertex.Contains(x))
                commonA = x;
            else
            {
                x = aVertex;
                if (x.Predecessor != null)
                {
                    x = x.Predecessor;
                }
                while (x.BlossomIndex != commonB.BlossomIndex)
                {
                    x = x.MatchedVertex;
                    x = x.Predecessor;
                }
                commonA = x;
            }


            //compute BlossomPath
            var aPathUpwards = new List<Vertex>();
            x = aVertex;
            if (x.Predecessor != null)
            {
                aPathUpwards.Add(x);
                aPathUpwards.AddRange(x.OddPath);
                x = aPathUpwards.Last();
                aPathUpwards.Remove(aPathUpwards.Last());
            }
            while (x != commonA)
            {
                aPathUpwards.Add(x);
                x = x.MatchedVertex;
                aPathUpwards.Add(x);
                x = x.Predecessor;
            }
            aPathUpwards.Add(x);

            var bPathUpwards = new List<Vertex>();
            x = bVertex;
            if (x.Predecessor != null)
            {
                bPathUpwards.Add(x);
                bPathUpwards.AddRange(x.OddPath);
                x = bPathUpwards.Last();
                bPathUpwards.Remove(bPathUpwards.Last());
            }
            while (x != commonB)
            {
                bPathUpwards.Add(x);
                x = x.MatchedVertex;
                bPathUpwards.Add(x);
                x = x.Predecessor;
            }
            bPathUpwards.Add(x);
            var aPathDownwards = aPathUpwards.ToList();
            aPathDownwards.Reverse();
            var bPathDownwards = bPathUpwards.ToList();
            bPathDownwards.Reverse();


            //set oddPaths
            for(int i = 1; i< aPathDownwards.Count; i += 2)
            {
                x = aPathDownwards[i];
                x.OddPath = aPathDownwards.Skip(i + 1).Concat(bPathUpwards).ToList();
                stack.Add(x);
                plusTreeVertices.Add(x);
            }
            for (int i = 1; i < bPathDownwards.Count; i += 2)
            {
                x = bPathDownwards[i];
                x.OddPath = bPathDownwards.Skip(i + 1).Concat(aPathUpwards).ToList();
                stack.Add(x);
                plusTreeVertices.Add(x);
            }

            var blossomIndizesToReplace = aPathDownwards.Concat(bPathDownwards).Where(_=>_.BlossomIndex!=0).Select(_ => _.BlossomIndex).Distinct().ToHashSet();
            blossomCount++;

            foreach(var plusVertex in plusTreeVertices)
            {
                if (blossomIndizesToReplace.Contains(plusVertex.BlossomIndex))
                {
                    plusVertex.BlossomIndex = blossomCount;
                    if (plusVertex.MatchedVertex != null)
                        plusVertex.MatchedVertex.BlossomIndex = blossomCount;
                }
            }
            foreach (var plusVertex in aPathDownwards)
            {
                plusVertex.BlossomIndex = blossomCount;
            }
            foreach (var plusVertex in bPathDownwards)
            {
                plusVertex.BlossomIndex = blossomCount;
            }
        }
        private static void CalculateAndSetOddPaths2(Vertex aVertex, Vertex bVertex, List<Vertex> stack, List<Vertex> plusTreeVertices, ref int blossomCount)
        {
            //search commom vertex
            var hashSetVertex = new HashSet<Vertex>();
            var hashSetBlossomIndex = new HashSet<int>();
            Vertex x = aVertex;
            if (x.Predecessor != null)
            {
                hashSetVertex.Add(x);
                x = x.Predecessor;
            }
            while (x != null)
            {
                hashSetVertex.Add(x);
                if (x.MatchedVertex == null)
                    break;
                x = x.MatchedVertex;
                hashSetVertex.Add(x);
                x = x.Predecessor;
            }
            x = bVertex;
            while (!hashSetVertex.Contains(x))
            {
                if (x.Predecessor != null)
                    x = x.Predecessor;
                else
                    x = x.MatchedVertex;
            }
            var firstRealCommonVertex = x;
            Vertex commonA;
            Vertex commonB;
            //if (firstRealCommonVertex.BlossomIndex == 0)    
            //{
            if (firstRealCommonVertex.Predecessor == null)
            {
                commonA = firstRealCommonVertex;
                commonB = firstRealCommonVertex;
            }
            else
            {
                commonA = firstRealCommonVertex.Predecessor;//SCHWACHSINN
                commonB = firstRealCommonVertex.Predecessor;
            }

            //}
            //else
            //{ //if firstRealCommonVertex is in a Blossom, the oddPath only has to be computet to this point

            //}


            //if (hashSetVertex.Contains(x))
            //    commonA = x;
            //else
            //{
            //    x = aVertex;
            //    while (x.BlossomIndex != commonB.BlossomIndex)
            //    {
            //        x = x.MatchedVertex;
            //        x = x.Predecessor;
            //    }
            //    commonA = x;
            //}


            //compute BlossomPath
            var aPathUpwards = new List<Vertex>();
            x = aVertex;
            if (x.Predecessor != null && x != commonA)
            {
                aPathUpwards.Add(x);
                foreach ( var o in x.OddPath)
                {
                    if (o != commonA)
                        aPathUpwards.Add(o);
                    else
                        break;
                }
                //aPathUpwards.AddRange(x.OddPath);
                x = x.OddPath.Last();
                aPathUpwards.Remove(x);
            }
            while (x != commonA)
            {
                aPathUpwards.Add(x);
                x = x.MatchedVertex;
                aPathUpwards.Add(x);
                x = x.Predecessor;
            }
            aPathUpwards.Add(x);

            var bPathUpwards = new List<Vertex>();
            x = bVertex;
            if (x.Predecessor != null && x != commonB)
            {
                bPathUpwards.Add(x);
                foreach (var o in x.OddPath)
                {
                    if (o != commonB)
                        bPathUpwards.Add(o);
                    else
                        break;
                }
                //aPathUpwards.AddRange(x.OddPath);
                x = x.OddPath.Last();
                bPathUpwards.Remove(x);
            }
            while (x != commonB)
            {
                bPathUpwards.Add(x);
                x = x.MatchedVertex;
                bPathUpwards.Add(x);
                x = x.Predecessor;
            }
            bPathUpwards.Add(x);
            var aPathDownwards = aPathUpwards.ToList();
            aPathDownwards.Reverse();
            var bPathDownwards = bPathUpwards.ToList();
            bPathDownwards.Reverse();


            //set oddPaths
            for (int i = 0; i < aPathDownwards.Count; i ++)
            {
                x = aPathDownwards[i];
                if (x.Predecessor == null)
                    continue;
                x.OddPath = aPathDownwards.Skip(i + 1).Concat(bPathUpwards).ToList();
                stack.Add(x);
                plusTreeVertices.Add(x);
            }
            for (int i = 0; i < bPathDownwards.Count; i ++)
            {
                x = bPathDownwards[i];
                if (x.Predecessor == null)
                    continue;
                x.OddPath = bPathDownwards.Skip(i + 1).Concat(aPathUpwards).ToList();
                stack.Add(x);
                plusTreeVertices.Add(x);
            }

            var blossomIndizesToReplace = aPathDownwards.Concat(bPathDownwards).Where(_ => _.BlossomIndex != 0).Select(_ => _.BlossomIndex).Distinct().ToHashSet();
            blossomCount++;

            foreach (var plusVertex in plusTreeVertices)
            {
                if (blossomIndizesToReplace.Contains(plusVertex.BlossomIndex))
                {
                    plusVertex.BlossomIndex = blossomCount;
                    if (plusVertex.MatchedVertex != null)
                        plusVertex.MatchedVertex.BlossomIndex = blossomCount;
                }
            }
            foreach (var plusVertex in aPathDownwards)
            {
                plusVertex.BlossomIndex = blossomCount;
            }
            foreach (var plusVertex in bPathDownwards)
            {
                plusVertex.BlossomIndex = blossomCount;
            }
        }
        private static void CalculateAndSetOddPaths(Vertex aVertex, Vertex bVertex, List<Vertex> stack, List<Vertex> plusTreeVertices, ref int blossomCount)
        {
            //calculate common Vertex
            var root = plusTreeVertices[0];
            var rootPahtA = CalculateEvenPathToRoot(aVertex, root);
            var rootPahtB = CalculateEvenPathToRoot(bVertex, root);
            var firstCommonVertex = rootPahtA.First(_ => rootPahtB.Contains(_));

            //if both paths enter the Blossom of firstCommonVertex before firstCommonVertex, this becoms their privat enpoint of pathABUpwards
            int indexA;
            int indexB;
            
            if (firstCommonVertex.BlossomIndex != 0)
            {
                indexA = rootPahtA.Select(_ => _.BlossomIndex).ToList().IndexOf(firstCommonVertex.BlossomIndex); 
                indexB = rootPahtB.Select(_ => _.BlossomIndex).ToList().IndexOf(firstCommonVertex.BlossomIndex); 
            }
            else
            {
            indexA = rootPahtA.IndexOf(firstCommonVertex);
            indexB = rootPahtB.IndexOf(firstCommonVertex);

            }

            //calculate path Upwards to firstCommonVertex;
            var pathAUpwards = new List<Vertex>(indexA+1);
            for(int i = 0; i< indexA+1; i++)
            {
                pathAUpwards.Add(rootPahtA[i]);
            }
            var pathBUpwards = new List<Vertex>(indexB+1);
            for (int i = 0; i < indexB+1; i++)
            {
                pathBUpwards.Add(rootPahtB[i]);
            }

            var pathADownwards = pathAUpwards.ToList();
            pathADownwards.Reverse();
            var pathBDownwards = pathBUpwards.ToList();
            pathBDownwards.Reverse();


            //set oddPaths
            for(int i = 1; i< pathADownwards.Count; i++)
            {
                var x = pathADownwards[i];
                if (x.Predecessor == null || x.OddPath!=null)
                    continue;
                x.OddPath = pathADownwards.Skip(i + 1).Concat(pathBUpwards).ToList();
                if (!plusTreeVertices.Contains(x))
                {
                plusTreeVertices.Add(x);
                stack.Add(x);
                }
            }

            for (int i = 1; i < pathBDownwards.Count; i++)
            {
                var x = pathBDownwards[i];
                if (x.Predecessor == null || x.OddPath != null)
                    continue;
                x.OddPath = pathBDownwards.Skip(i + 1).Concat(pathAUpwards).ToList();
                if (!plusTreeVertices.Contains(x))
                {
                    plusTreeVertices.Add(x);
                    stack.Add(x);
                }
            }


            var blossomIndizesToReplace = pathADownwards.Concat(pathBDownwards).Where(_ => _.BlossomIndex != 0).Select(_ => _.BlossomIndex).Distinct().ToHashSet();
            blossomCount++;

            foreach (var plusVertex in plusTreeVertices)
            {
                if (blossomIndizesToReplace.Contains(plusVertex.BlossomIndex))
                {
                    plusVertex.BlossomIndex = blossomCount;
                    if (plusVertex.MatchedVertex != null)
                        plusVertex.MatchedVertex.BlossomIndex = blossomCount;
                }
            }
            foreach (var plusVertex in pathADownwards)
            {
                plusVertex.BlossomIndex = blossomCount;
            }
            foreach (var plusVertex in pathBDownwards)
            {
                plusVertex.BlossomIndex = blossomCount;
            }
        }

        public void ResetTree(List<Vertex> plusTreeVertices)
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
        public int Label;
        public List<Edge> Adj2Edges;
        public List<Edge> Adj3Edges;
        public Vertex ContractedWith;
        public Vertex ContractedVertex;


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
    }
}
