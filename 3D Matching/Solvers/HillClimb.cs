using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Solvers
{
    class HC : IMinimumEdgecoveringSolver
    {
        int preCalculationTime=10;
        int maxEdgeSwapSize;
        private String climbMode;
        private String precalculationMode;
        Random _random = new Random();

        public override String Name { get => this.GetType().Name + "(" + precalculationMode + "|"+climbMode + "|" + maxEdgeSwapSize + ")"; }
        public HC(int preCalculationTime = 10, String precalculationMode = "normal", String climbMode = "normal", int maxEdgeSwapSize =10)
        {
            this.maxEdgeSwapSize = maxEdgeSwapSize;
            this.preCalculationTime = preCalculationTime;
            this.climbMode = climbMode;
            this.precalculationMode = precalculationMode;
        }
        public override (List<Edge> cover, int iterations) Run(Dictionary<string, double> parameters)
        {
            double maxTime = parameters["maxTime"];

            var time = new Stopwatch();
            time.Start();


            IMinimumEdgecoveringSolver solver;
            if (precalculationMode == "normal")
                solver = new OnlineSolver(true);// Greedy();
            else
                solver = new Greedy();

            var modifiedParameters = parameters.ToDictionary(entry => entry.Key,
                                                             entry => entry.Value);

            // modification for parameters for precalculation
            modifiedParameters["maxTime"] = 50;//(double)parameters["maxTime"] / 2;

            Console.WriteLine("MAxTime:" + modifiedParameters["maxTime"]);
            solver.initialize(_graph);
            var edgeCover = solver.Run(modifiedParameters).cover;
            int runThrough = 0;


            if (climbMode == "allNotOptimal")
            {
                while (time.ElapsedMilliseconds < maxTime)
                {

                    if (maxEdgeSwapSize > edgeCover.Count - 5)
                    {
                        solver = new ORTS();
                        solver.initialize(_graph);
                        return solver.Run(parameters);
                    }
                    runThrough++;
                    //determine edges to optimize
                    var toOptimizeEdges = edgeCover.Where(_ => _.Vertices.Count < 3).ToList();
                    Console.Write("Not optimal Edges: " + toOptimizeEdges.Count);
                    if (toOptimizeEdges.Count == 0)
                        break;
                    for (int i = 0; i < maxEdgeSwapSize - 1; i++)
                    {
                        var index = _random.Next(edgeCover.Count);
                        if (toOptimizeEdges.Contains(edgeCover[index]))
                        {
                            i--;
                            continue;
                        }
                        toOptimizeEdges.Add(edgeCover[index]);
                    }
                    Console.Write("Not optimal Edges: " + toOptimizeEdges.Count);

                    Graph tmpGraph = GenerateInducedSubgraph(toOptimizeEdges);
                    var solver2 = new ORTS();
                    solver2.initialize(tmpGraph);
                    var optimizedEdges = solver2.Run(parameters).cover;
                    Console.WriteLine("Optimized from " + toOptimizeEdges.Count + "to" + optimizedEdges.Count);
                    if (optimizedEdges.Count < toOptimizeEdges.Count)
                    {
                        foreach (var edge in toOptimizeEdges)
                            edgeCover.Remove(edge);
                        foreach (var edge in optimizedEdges)
                            edgeCover.Add(edge);
                    }

                }
            }
            if (climbMode == "normal")
            {
                while (time.ElapsedMilliseconds < maxTime)
                {

                    if (maxEdgeSwapSize > edgeCover.Count - 5)
                    {
                        solver = new ORTS();
                        solver.initialize(_graph);
                        return solver.Run(parameters);
                    }
                    runThrough++;
                    //determine edgesmto optimize
                    var toOptimizeEdges = new List<Edge>();
                    var notOptimal = edgeCover.Where(_ => _.Vertices.Count < 3).ToList();
                    if (notOptimal.Count == 0)
                        break;
                    else
                        toOptimizeEdges.Add(notOptimal[_random.Next(notOptimal.Count)]);
                    for (int i = 0; i < maxEdgeSwapSize - 1; i++)
                    {
                        var index = _random.Next(edgeCover.Count);
                        if (toOptimizeEdges.Contains(edgeCover[index]))
                        {
                            i--;
                            continue;
                        }
                        toOptimizeEdges.Add(edgeCover[index]);
                    }

                    var tmpVertices = toOptimizeEdges.SelectMany(_ => _.Vertices);
                    //Maybe remove duplicate
                    var tmpEdges = _graph.Edges.Where(_ => _.Vertices.Where(_ => tmpVertices.Contains(_)).Count() == _.Vertices.Count).ToList();
                    var tmpGraph = new Graph(tmpEdges, tmpVertices.ToList());
                    var solver2 = new ORTS();
                    solver2.initialize(tmpGraph);
                    var optimizedEdges = solver2.Run(parameters).cover;
                    Console.WriteLine("Optimized from " + toOptimizeEdges.Count + "to" + optimizedEdges.Count);
                    if (optimizedEdges.Count < toOptimizeEdges.Count)
                    {
                        foreach (var edge in toOptimizeEdges)
                            edgeCover.Remove(edge);
                        foreach (var edge in optimizedEdges)
                            edgeCover.Add(edge);
                    }

                }
            }
            if (climbMode == "alternatingSmallImprovements")
            {
                var tmpParameters = parameters.ToDictionary(entry => entry.Key,
                                                            entry => entry.Value);
                tmpParameters["maxTime"] = 50;
                var bestRes = edgeCover.ToList();
                while (time.ElapsedMilliseconds < maxTime)
                {
                    runThrough++;
                    solver = new Greedy();
                    solver.initialize(_graph);
                    var tmpRes = solver.Run(tmpParameters).cover;

                    for (int i = 0; i< 3; i++)
                    {
                        var toOptimizeEdges = tmpRes.Where(_ => _.Vertices.Count < 3).ToList();
                        if (toOptimizeEdges.Count == 0)
                            break;
                        for (int j = 0; j < Math.Min(maxEdgeSwapSize - 1,(double)(tmpRes.Count-toOptimizeEdges.Count)/2); j++)
                        {
                            var index = _random.Next(tmpRes.Count);
                            if (toOptimizeEdges.Contains(tmpRes[index]))        //time inefficent if toOptimize is large
                            {
                                j--;
                                continue;
                            }

                            toOptimizeEdges.Add(tmpRes[index]);
                        }
                        Graph tmpGraph = GenerateInducedSubgraph(toOptimizeEdges);
                        var solver2 = new ORTS();
                        solver2.initialize(tmpGraph);
                        var optimizedEdges = solver2.Run(parameters).cover;
                        if (optimizedEdges.Count < toOptimizeEdges.Count)
                        {
                            foreach (var edge in toOptimizeEdges)
                                tmpRes.Remove(edge);
                            foreach (var edge in optimizedEdges)
                                tmpRes.Add(edge);
                        }
                    }
                    if(tmpRes.Count< edgeCover.Count)
                    {
                        edgeCover = tmpRes.ToList();
                    }

                }
            }


            return (edgeCover, runThrough);
        }

        private Graph GenerateInducedSubgraph(List<Edge> toOptimizeEdges)
        {
            var tmpVertices = toOptimizeEdges.SelectMany(_ => _.Vertices);
            //Maybe remove duplicate
            var tmpEdges = _graph.Edges.Where(_ => _.Vertices.Where(_ => tmpVertices.Contains(_)).Count() == _.Vertices.Count).ToList();
            var tmpGraph = new Graph(tmpEdges, tmpVertices.ToList());
            return tmpGraph;
        }
    }
}
