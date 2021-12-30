using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Solvers
{
    public class Genetic : IMinimumEdgecoveringSolver
    {
        Random _random = new Random();
        String _type = "";
        public Genetic(String type = "")
        {
            _type = type;
        }

        public override String Name { get => this.GetType().Name + "|" + _type; }

        public override (List<Edge> cover, int iterations) Run(Dictionary<string, double> parameters)
        {
            double maxTime = parameters["maxTime"];
            var time = new Stopwatch();
            time.Start();
            var res = new List<Edge>();

            var uncoveredVertices = _graph.Vertices.ToList();

            var bestResCount = _edges.Count;
            var vertexCount = _graph.Vertices.Count;
            //List<List<int>> bestPermutation = new List<List<int>>();
            var iteration = 0;
            var isCovored = new bool[vertexCount];
            List<List<int>> edges = new List<List<int>>();

            if (_type == "normal")
            {
                edges = _edges.Select(_ => _.VerticesIds).OrderBy(_ => -_.Count).ToList();
            }
            else if (_type =="fast")
            {
                edges = GenerateInitalPermutation();
            }

                while (time.ElapsedMilliseconds < maxTime)
                {
                    iteration++;
                    isCovored = new bool[vertexCount];
                    var index = _random.Next(edges.Count);
                    var amountOfCoveredVertices = 0;
                    var coverSize = 0;

                    //var test = edges.ToList();

                    var tmp = edges[index];
                    edges.RemoveAt(index);
                    edges.Insert(0, tmp);


                    foreach (var edge in edges)
                    {
                        bool skip = false;
                        foreach (var i in edge)
                        {
                            if (isCovored[i])
                            {
                                skip = true;
                                break;

                            }
                        }
                        if (skip)
                            continue;

                        foreach (var i in edge)
                        {
                            isCovored[i] = true;
                            amountOfCoveredVertices++;
                        }

                        coverSize++;
                        if (amountOfCoveredVertices == vertexCount)
                            break;
                    }
                    //Console.WriteLine(coverSize);

                    if (coverSize <= bestResCount)// && amountOfCoveredVertices==vertexCount)
                    {
                        bestResCount = coverSize;
                        //bestPermutation = edges.ToList();
                    }
                    else
                    {
                        tmp = edges[0];
                        edges.RemoveAt(0);
                        edges.Insert(index, tmp);
                    }



                }
            //reconstruct:
            var result = new List<Edge>();
            isCovored = new bool[vertexCount];

            foreach (var edge in edges)
            {
                bool skip = false;
                foreach (var i in edge)
                {
                    if (isCovored[i])
                    {
                        skip = true;
                        break;

                    }
                }
                if (skip)
                    continue;

                foreach (var i in edge)
                {
                    isCovored[i] = true;
                }
                result.Add(new Edge(edge.Select(_ => new Vertex(_)).ToList()));
            }

            return (result, iteration);


        }

        private List<List<int>> GenerateInitalPermutation() //slower that lambda
        {

            //generate structure:
           
            var edges = new List<List<int>>();
            var pointerTwoEdge = 0;

            foreach (var edge in _edges)
            {
                if (edge.Vertices.Count == 3)
                {
                    edges.Insert(0, edge.VerticesIds);
                    pointerTwoEdge++;
                }
                else if (edge.Vertices.Count == 1)
                {
                    edges.Add(edge.VerticesIds);
                }
                else if (edge.Vertices.Count == 2)
                {
                    edges.Insert(pointerTwoEdge, edge.VerticesIds);
                }
                else
                    Console.WriteLine("Edge of size " + edge.Vertices.Count + " cant be handled");
            }

            return edges;
        }


    }

}
