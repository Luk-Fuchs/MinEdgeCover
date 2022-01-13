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
        IMinimumEdgecoveringSolver _precalcSolver;
        private bool _precalc;
        public Genetic(String type = "", bool precalc = true, IMinimumEdgecoveringSolver precalcSolver= null)
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
            var res = new List<Edge>();

            var uncoveredVertices = _graph.Vertices.ToList();

            var bestResCount = int.MaxValue;
            var vertexCount = _graph.Vertices.Count;
            //List<List<int>> bestPermutation = new List<List<int>>();
            var iteration = 0;
            var isCovored = new bool[vertexCount];
            List<List<int>> edges = new List<List<int>>();

            if (_type =="fast")
            {
                edges = GenerateInitalPermutation();
            }
            else if (_type =="test")
            {
                edges = GenerateInitalPermutationBySeveralLists();
            }else if (_type == "precalc")
            {
                //var solver = new Greedy(mode: "byDegreeUpdatingAndMinSumSorting", sizeWeight: 4, variation: 0);
                _precalcSolver.initialize(_graph);
                var tmpParams = new Dictionary<String, double>();
                tmpParams.Add("maxIter", 10);
                tmpParams.Add("maxTime", 25);
                var preRes = _precalcSolver.Run(tmpParams);
                edges = preRes.cover.Concat(_edges.Where(_ => !preRes.cover.Contains(_))).Select(_=>_.VerticesIds).ToList();
                bestResCount = preRes.cover.Count;
            }
            else 
            {
                edges = _edges.Select(_ => _.VerticesIds).OrderBy(_ => -_.Count).ToList();
            }

            if (_type == "normal" || _type == "fast" || _type == "test" || _type == "precalc")
            {
                int index = 0;
                var tmp = new List<int>();
                while (time.ElapsedMilliseconds < maxTime)
                {
                    isCovored = new bool[vertexCount];
                    index = _random.Next(edges.Count);

                    tmp = edges[index];
                    edges.RemoveAt(index);
                    edges.Insert(0, tmp);


                    var amountOfCoveredVertices = 0;
                    var coverSize = 0;
                    iteration++;

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
                        if (amountOfCoveredVertices == vertexCount || coverSize > bestResCount)
                            break;
                    }
                    //Console.WriteLine(coverSize +"  " + (amountOfCoveredVertices == vertexCount));

                    if (coverSize <= bestResCount && amountOfCoveredVertices == vertexCount)// && amountOfCoveredVertices==vertexCount)
                    {
                        bestResCount = coverSize;
                        //bestPermutation = edges.ToList();
                    }
                    else if (bestResCount != int.MaxValue)
                    {
                        tmp = edges[0];
                        edges.RemoveAt(0);
                        edges.Insert(index, tmp);
                    }

                }

            }
            else if (_type == "linkedList")
            {
                var linkedEdges = new LinkedList<List<int>>(GenerateInitalPermutationBySeveralLists());

                var swapItem = linkedEdges.First;
                for (int i = 0; i < _edges.Count / 2; i++)
                {
                    swapItem = swapItem.Next;
                }

                var index = _edges.Count / 2;
                while (time.ElapsedMilliseconds < maxTime)
                {
                    iteration++;
                    isCovored = new bool[vertexCount];
                    var offset = _random.Next(-5, 5);
                    var amountOfCoveredVertices = 0;
                    var coverSize = 0;

                    //var test = edges.ToList();
                    if (offset > 0)
                    {
                        offset = Math.Min(_edges.Count - index - 1, 5);
                        for (int i = 0; i < offset; i++)
                            swapItem = swapItem.Next;

                    }
                    if (offset < 0)
                    {
                        offset = Math.Max(-index + 1, -5);

                        for (int i = 0; i > offset; i--)
                            swapItem = swapItem.Previous;
                    }
                    index += offset;

                    var toInsertFirst = swapItem.Previous;
                    linkedEdges.Remove(toInsertFirst);
                    linkedEdges.AddFirst(toInsertFirst);


                    foreach (var edge in linkedEdges)
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

                    if (coverSize <= bestResCount)
                    {
                        bestResCount = coverSize;
                    }
                    else
                    {
                        linkedEdges.Remove(toInsertFirst);
                        linkedEdges.AddBefore(swapItem, toInsertFirst);
                    }
                }
                edges = linkedEdges.ToList();
            }
            else if (_type == "onlyPermuteLargeEdges")
            {
                int index = 0;
                var tmp = new List<int>();
                var edges1 = edges.Where(_ => _.Count == 1).ToList();
                edges = edges.Where(_ => _.Count != 1).ToList();
                var testCounter = 0;
                while (time.ElapsedMilliseconds < maxTime)
                {
                    isCovored = new bool[vertexCount];
                    index = _random.Next(edges.Count);

                    tmp = edges[index];
                    edges.RemoveAt(index);
                    edges.Insert(0, tmp);


                    var amountOfCoveredVertices = 0;
                    var coverSize = 0;
                    iteration++;

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
                        if (amountOfCoveredVertices == vertexCount || coverSize > bestResCount)
                            break;
                    }
                    foreach (var edge in edges1)
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
                        if (amountOfCoveredVertices == vertexCount || coverSize > bestResCount)
                            break;
                    }
                    //Console.WriteLine(coverSize +"  " + (amountOfCoveredVertices == vertexCount));


                    if (coverSize <= bestResCount && amountOfCoveredVertices == vertexCount)// && amountOfCoveredVertices==vertexCount)
                    {
                        bestResCount = coverSize;
                        testCounter++;
                        //bestPermutation = edges.ToList();
                    }
                    else/* if (bestResCount != int.MaxValue /*&& coverSize != bestResCount*/
                    {
                        tmp = edges[0];
                        edges.RemoveAt(0);
                        edges.Insert(index, tmp);
                    }



                }
                Console.WriteLine((double)iteration/testCounter);
                edges = edges.Concat(edges1).ToList();
            }
            else if (_type == "prefereBadCovered")
            {
                int index = 0;
                var tmp = new List<int>();
                var edges1 = edges.Where(_ => _.Count == 1).ToList();
                edges = edges.Where(_ => _.Count != 1).ToList();
                var isCovoredByEdgeSize = new int[vertexCount];
                while (time.ElapsedMilliseconds < maxTime)
                {
                    isCovored = new bool[vertexCount];


                    index = _random.Next(1,edges.Count -1);

                    foreach( var i in edges[index+1])
                        if (isCovoredByEdgeSize[i] == 1)
                        {
                            index = index +1;
                            break;
                        }
                    //foreach (var i in edges[index - 1])
                    //    if (isCovoredByEdgeSize[i] == 1)
                    //    {
                    //        index--;
                    //        break;
                    //    }

                    if (edges[index].Select(_=>isCovoredByEdgeSize[_]).Min() > edges[index+1].Select(_ => isCovoredByEdgeSize[_]).Min())
                        index++;

                    tmp = edges[index];
                    edges.RemoveAt(index);
                    edges.Insert(0, tmp);




                    isCovoredByEdgeSize = new int[vertexCount];

                    var amountOfCoveredVertices = 0;
                    var coverSize = 0;
                    iteration++;

                    foreach (var edge in edges)
                    {
                        bool skip = false;
                        foreach (var i in edge)
                        {
                            if (isCovoredByEdgeSize[i] != 0)
                            {
                                skip = true;
                                break;

                            }
                        }
                        if (skip)
                            continue;

                        foreach (var i in edge)
                        {
                            isCovoredByEdgeSize[i] = edge.Count;
                            amountOfCoveredVertices++;
                        }

                        coverSize++;
                        if (amountOfCoveredVertices == vertexCount || coverSize > bestResCount)
                            break;
                    }
                    foreach (var edge in edges1)
                    {
                        bool skip = false;
                        foreach (var i in edge)
                        {
                            if (isCovoredByEdgeSize[i] != 0)
                            {
                                skip = true;
                                break;

                            }
                        }
                        if (skip)
                            continue;

                        foreach (var i in edge)
                        {
                            isCovoredByEdgeSize[i] = edge.Count;
                            amountOfCoveredVertices++;
                        }

                        coverSize++;
                        if (amountOfCoveredVertices == vertexCount || coverSize > bestResCount)
                            break;
                    }
                    //Console.WriteLine(coverSize + "  " + (amountOfCoveredVertices == vertexCount));

                    if (coverSize <= bestResCount && amountOfCoveredVertices == vertexCount)// && amountOfCoveredVertices==vertexCount)
                    {
                        bestResCount = coverSize;
                        //bestPermutation = edges.ToList();
                    }
                    else if (bestResCount != int.MaxValue)
                    {
                        tmp = edges[0];
                        edges.RemoveAt(0);
                        edges.Insert(index, tmp);
                    }



                }
                edges = edges.Concat(edges1).ToList();
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
        
        private List<List<int>> GenerateInitalPermutationBySeveralLists() //slower that lambda
        {

            //generate structure:
           
            var edges = new List<List<int>>();
            var edges1 = new List<List<int>>();
            var edges2 = new List<List<int>>();
            var edges3 = new List<List<int>>();
            var pointerTwoEdge = 0;

            foreach (var edge in _edges)
            {
                if (edge.Vertices.Count == 3)
                {
                    edges3.Add(edge.VerticesIds);
                }
                else if (edge.Vertices.Count == 1)
                {
                    edges1.Add(edge.VerticesIds);
                }
                else if (edge.Vertices.Count == 2)
                {
                    edges2.Add(edge.VerticesIds);
                }
                else
                    Console.WriteLine("Edge of size " + edge.Vertices.Count + " cant be handled");
            }

            return edges3.Concat(edges2).Concat(edges1).ToList();
        }


    }

}
