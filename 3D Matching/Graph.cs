using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching
{
    class Graph
    {
        public Graph(List<Edge> edges, List<Vertex> vertices)
        {
            _edges = edges;
            Vertices = vertices;
        }
        private List<Edge> _edges;
        public List<Vertex> Vertices;
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


        public static Graph BuildGraphFromCSV(String path)
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
                if(activeLine.Split(";")[1]=="True")
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
            return graph;
        }


    }

    class Edge
    {
        public List<Vertex> Vertices;
        public List<int> VerticesIds;
        public bool IsInCover = false;
        public double property1;
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
    }
    class Vertex 
    {
        public bool IsCovered = false;
        public int TimesCovered = 0;
        public int[] Interval;
        public double hardness = 0.0;  //high if several attempts, matching this vertex failtured
        public Dictionary<int, int> NeighbourhoodAndMultiplicity;
        public Vertex(int id)
        {
            this.Id = id;
        }
        public int Id;

        public bool Equals(Vertex otherObject)
        {
            return Id== otherObject.Id;
        }
        public override string ToString()
        {
            return Id +"";
        }
    }
}
