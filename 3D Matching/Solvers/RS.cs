using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Solvers
{
    class RS : IMinimumEdgecoveringSolver
    {
        Random random = new Random();
        String _mode = "normal";

        public RS(String mode = "normal")
        {
            _mode = mode;
        }

        public override String Name { get => this.GetType().Name + "|" + _mode; }

        public override List<Edge> Run(Dictionary<string, double> parameters)
        {
            double maxTime = parameters["maxTime"];
            var time = new Stopwatch();
            time.Start();
            Random _random = new Random();
            var resMin = new List<Edge>();
            var resTmp = new List<Edge>();
            int runTrough = 0;

            while (time.ElapsedMilliseconds < maxTime)
            {
                if (_mode == "byDegree")
                {
                    var edgesCopy = _edges.ToList();

                    var vertexDegrees = new int[_graph.Vertices.Count];
                    for (int i = 0; i < edgesCopy.Count; i++)
                    {
                        foreach (var vertexId in edgesCopy[i].VerticesIds)
                            vertexDegrees[vertexId]++;
                    }


                    foreach (var edge in _edges.OrderBy(_ => -_.Vertices.Count * 1000000 + _.Vertices.Select(x => vertexDegrees[x.Id]).Sum() + _random.NextDouble()))
                    {
                        if (edge.AllVerticesAreUncovered())
                        {
                            resTmp.Add(edge);
                            foreach (var vertex in edge.Vertices)
                            {
                                vertex.IsCovered = true;
                                vertex.TimesCovered = 1;
                            }
                        }
                    }

                }
                else if (_mode == "byDegreeUpdating")
                {
                    var edgesCopy = _edges.ToList();
                    //var vertexDegrees = _graph.Vertices.Select(_ => _edges.Where(x => x.Vertices.Contains(_)).Count()).ToList();
                    var vertexDegrees = new int[_graph.Vertices.Count];
                    for (int i = 0; i< edgesCopy.Count; i++)
                    {
                        foreach (var vertexId in edgesCopy[i].VerticesIds)
                            vertexDegrees[vertexId]++;
                    }
                    int amountOfCoveredVertices = 0;

                    //edgesCopy = edgesCopy.OrderBy(_ => -_.Vertices.Count * 100 + _.Vertices.Select(z => vertexDegrees[z.Id]).Sum() + runTrough * _random.NextDouble()).ToList();

                    while (amountOfCoveredVertices < _graph.Vertices.Count)
                    {
                        if (edgesCopy.Count == 0)
                        {
                            resTmp.AddRange(_graph.Edges);
                            break;
                        }
                        //Edge = edgesCopy.First();
                        var edge = edgesCopy.OrderBy(_ => -_.Vertices.Count * 100 + _.Vertices.Select(z => vertexDegrees[z.Id]).Sum() + runTrough * _random.NextDouble()).First(); //.Where(_ => _.Vertices.Where(y => !y.IsCovered).Count() ==_.Vertices.Count)
                        //Where(_ => _.Vertices.Where(y => y.IsCovered).Count() == 0).
                        var degreeModifikation = new int[_graph.Vertices.Count];
                        foreach (var vertex in edge.Vertices)
                        {
                            vertex.IsCovered = true;
                            vertex.TimesCovered = 1;
                        }

                        //foreach (var x in edgesCopy.Where(_ => _.Vertices.Where(y => y.IsCovered).Count() > 0).ToList())
                        //{
                        //    edgesCopy.Remove(x);
                        //    foreach (var vertex in x.Vertices)
                        //        vertexDegrees[vertex.Id]--;
                        //}
                        int i = 0;
                        var edgeHasBeenRemoved = false;
                        while (i < edgesCopy.Count)
                        {
                            foreach (var vertexId in edge.VerticesIds)
                                if (edgesCopy[i].VerticesIds.Contains(vertexId))
                                {
                                    edgesCopy.RemoveAt(i);
                                    edgeHasBeenRemoved = true;
                                    break;
                                }
                            if (!edgeHasBeenRemoved)
                                i++;
                            edgeHasBeenRemoved = false;
                        }

                        resTmp.Add(edge);
                        amountOfCoveredVertices += edge.Vertices.Count;

                    }
                }
                else if (_mode == "byDegreeUpdatingAndRegret")
                {
                    var vertexDegrees = new int[_graph.Vertices.Count];
                    for (int i = 0; i < _edges.Count; i++)
                    {
                        foreach (var vertexId in _edges[i].VerticesIds)
                            vertexDegrees[vertexId]++;
                    }
                    initializeDegrees(_random, runTrough, vertexDegrees);
                    int amountOfCoveredVertices = 0;
                    var edgesCopy = new LinkedList<Edge>(_edges.OrderBy(_ => _.property1));

                    while (amountOfCoveredVertices < _graph.Vertices.Count)
                    {
                        if (edgesCopy.Count == 0)
                        {
                            resTmp.AddRange(_graph.Edges);
                            break;
                        }
                        var edge = edgesCopy.First.Value;
                        var degreeModifikation = new int[_graph.Vertices.Count];
                        foreach (var vertex in edge.Vertices)
                        {
                            vertex.hardness += 1.0/(edge.Vertices.Count*edge.Vertices.Count);
                            vertex.IsCovered = true;
                            vertex.TimesCovered = 1;
                        }
                        int i = 0;
                        var linkedEdge = edgesCopy.First;
                        while (linkedEdge != null)
                        {
                            var tmp = linkedEdge.Next;
                            foreach (var vertexId in edge.VerticesIds)
                                if (linkedEdge.Value.VerticesIds.Contains(vertexId))
                                {
                                    foreach (var secondVertexId in linkedEdge.Value.VerticesIds)
                                        degreeModifikation[secondVertexId]++;
                                    edgesCopy.Remove(linkedEdge);
                                    break;
                                }
                            linkedEdge = tmp;
                        }

                        foreach (var tmpEdge in edgesCopy)     //update vertex degrees
                        {
                            foreach (var vertexId in tmpEdge.VerticesIds)
                            {
                                tmpEdge.property1 -= degreeModifikation[vertexId];
                            }
                        }
                        var activeNode = edgesCopy.First;
                        while (activeNode != null)
                        {
                            var tmp = activeNode.Next;
                            var offsetNode = activeNode;
                            while (offsetNode.Previous != null && offsetNode.Value.property1 < activeNode.Value.property1)
                            {
                                offsetNode = offsetNode.Previous;
                            }
                            if (offsetNode != activeNode)
                            {
                                edgesCopy.Remove(activeNode);
                                edgesCopy.AddAfter(offsetNode, activeNode);
                            }
                            activeNode = tmp;
                        }


                        resTmp.Add(edge);
                        amountOfCoveredVertices += edge.Vertices.Count;

                    }
                }
                else if (_mode == "byDegreeUpdating_V3")
                {
                    var vertexDegrees = new int[_graph.Vertices.Count];
                    for (int i = 0; i < _edges.Count; i++)
                    {
                        foreach (var vertexId in _edges[i].VerticesIds)
                            vertexDegrees[vertexId]++;
                    }
                    initializeDegrees(_random, runTrough, vertexDegrees);
                    int amountOfCoveredVertices = 0;
                    var edgesCopy = new LinkedList<Edge>(_edges.OrderBy(_ => _.property1));

                    while (amountOfCoveredVertices < _graph.Vertices.Count)
                    {
                        if (edgesCopy.Count == 0)
                        {
                            resTmp.AddRange(_graph.Edges);
                            break;
                        }
                        var edge = edgesCopy.First.Value;
                        var degreeModifikation = new int[_graph.Vertices.Count];
                        foreach (var vertex in edge.Vertices)
                        {
                            vertex.IsCovered = true;
                            vertex.TimesCovered = 1;
                        }
                        int i = 0;
                        var linkedEdge = edgesCopy.First;
                        while (linkedEdge != null)
                        {
                            var tmp = linkedEdge.Next;
                            foreach (var vertexId in edge.VerticesIds)
                                if (linkedEdge.Value.VerticesIds.Contains(vertexId))
                                {
                                    foreach (var secondVertexId in linkedEdge.Value.VerticesIds)
                                        degreeModifikation[secondVertexId]++;
                                    edgesCopy.Remove(linkedEdge);
                                    break;
                                }
                            linkedEdge = tmp;
                        }

                        foreach (var tmpEdge in edgesCopy)     //update vertex degrees
                        {
                            foreach (var vertexId in tmpEdge.VerticesIds)
                            {
                                tmpEdge.property1 -= degreeModifikation[vertexId];
                            }
                        }
                        //update edge ordering
                        //int offset;
                        //for (i = 0; i < edgesCopy.Count; i++)
                        //{
                        //    for (offset = 1; offset <= i; offset++)
                        //    {
                        //        if (edgesCopy[i - offset].property1 <= edgesCopy[i].property1)
                        //            break;
                        //    }
                        //    if (offset != 1)
                        //    {
                        //        var tmp = edgesCopy[i];
                        //        edgesCopy.RemoveAt(i);
                        //        edgesCopy.Insert(i - offset + 1, tmp);
                        //    }
                        //}
                        var activeNode = edgesCopy.First;
                        while (activeNode != null)
                        {
                            int debug = 0;
                            var tmp = activeNode.Next;
                            var offsetNode = activeNode;
                            while (offsetNode.Previous != null && offsetNode.Value.property1 < activeNode.Value.property1)
                            {
                                offsetNode = offsetNode.Previous;
                                debug++;
                            }
                            if (offsetNode != activeNode)
                            {
                                edgesCopy.Remove(activeNode);
                                edgesCopy.AddAfter(offsetNode, activeNode);
                                Console.WriteLine(debug);
                            }
                            activeNode = tmp;
                        }



                        //Debug!
                        //var test2 = edgesCopy.Select(_ => _.property1).ToList();
                        //for (i = 0; i < edgesCopy.Count - 1; i++)
                        //{
                        //    if (edgesCopy[i + 1].property1 < edgesCopy[i].property1 && i!=0)
                        //        ;
                        //}


                        resTmp.Add(edge);
                        amountOfCoveredVertices += edge.Vertices.Count;

                    }
                }
                else //(_mode == "normal")
                {
                    foreach (var edge in _edges.OrderBy(_ => -_.Vertices.Count + _random.NextDouble()))
                    {
                        if (edge.AllVerticesAreUncovered())
                        {
                            resTmp.Add(edge);
                            foreach (var vertex in edge.Vertices)
                            {
                                vertex.IsCovered = true;
                                vertex.TimesCovered = 1;
                            }
                        }
                    }
                }

                //Console.Write(resTmp.Count +",");
                if (resTmp.Count < resMin.Count || resMin.Count == 0)
                    resMin = resTmp.ToList();

                foreach (var edge in resTmp)
                    foreach (var vertex in edge.Vertices)
                    {
                        vertex.IsCovered = false;
                        vertex.TimesCovered = 0;
                    }
                resTmp.Clear();
                runTrough++;
            }
            Console.WriteLine(runTrough);
            return resMin;
        }


        private void initializeDegrees(Random _random, int runTrough, int[] vertexDegrees)
        {
            for (int i = 0; i < _edges.Count; i++)
            {
                _edges[i].property1 = -_edges[i].Vertices.Count * 100000 + runTrough * _random.NextDouble();
                foreach (var vertex in _edges[i].Vertices)
                {
                    _edges[i].property1 += vertexDegrees[vertex.Id];
                    _edges[i].property1 += -vertex.hardness;
                }
            }
        }
    }
}
