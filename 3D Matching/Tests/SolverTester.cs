using _3D_Matching.Solvers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Tests
{
    class SolverTester
    {

        public SolverTester()
        {

        }
        public static String[,] RunSolvers(List<IMinimumEdgecoveringSolver> solvers, Dictionary<String,double> parameter ,String generationType= "readIn", double iterations = 2, int n=100, double p1 = 0.1, double p2 = 0.1, double p3 = 0.1)
        {
            var resData = new String[solvers.Count,(int)TestAttribute.Length];
            var time = new Stopwatch();

            var graphs = new List<Graph>();
            if(generationType == "readIn")
            {
                String path = @"C:\Users\LFU\Documents\GitHub\MinEdgeCover\TestData";
                string[] filePaths = Directory.GetFiles(path);
                graphs = Enumerable.Range(0, Math.Min((int)iterations, filePaths.Length)).Select(_ => (Graph.BuildGraphFromCSV(filePaths[_]))).ToList();
                iterations = graphs.Count();
            }
            else
            {
                graphs = Enumerable.Range(0, (int)iterations).Select(_ => Graph.GenerateRandomGraph(n, p1: p1, p2: p2, p3: p3)).ToList();
            }

            for (int i = 0; i < solvers.Count; i++)
            {
                double totalIterations = 0.0;
                var solver = solvers[i];
                Console.WriteLine(solver.Name);
                int totalEdgeCount = 0;
                time.Restart();
                for (int j = 0; j < iterations; j++)
                {
                    var graph = graphs[j];
                    solver.initialize(graph);
                    (var edgeCover, double used_iterations)= solver.Run(parameter);
                    totalEdgeCount += edgeCover.Count;
                    totalIterations += used_iterations;
                    if (!IsCover(graph, edgeCover).Item1)
                        Console.WriteLine("no real cover:  " + IsCover(graph, edgeCover).Item2);
                        //throw new System.Exception("no real cover  " + IsCover(graph, edgeCover).Item2);
                }
                time.Stop();
                resData[i, (int)TestAttribute.Time] = time.ElapsedMilliseconds / iterations + "";
                resData[i, (int)TestAttribute.Name] = solvers[i].Name;
                resData[i, (int)TestAttribute.Edges] = totalEdgeCount / iterations + "";
                resData[i, (int)TestAttribute.Iterations] = totalIterations / iterations + "";
            }

            return resData;

        }
        

        public static (bool,String) IsCover(Graph graph, List<Edge> cover)
        {
            var covenessArray = new bool[graph.Vertices.Count];
            foreach (var edge in cover)
            {
                //if (!graph.Edges.Contains(edge))        //edge is no real edge
                if(graph.Edges.Where(_=>_.Equals(edge)).ToString().Length==0)
                    return (false, "edge is no real edge");
                foreach(var vertex in edge.Vertices)
                {
                    if (covenessArray[vertex.Id])       //vertex twice gecovered
                        return (false,"vertex twice covered");
                    else
                        covenessArray[vertex.Id] = true;
                }
            }
            var uncoveredVertices = Enumerable.Range(0, covenessArray.Length).Where(_ => !covenessArray[_]).ToList();
            if(uncoveredVertices.Count!=0)
                    return (false, "there are uncovered Vertices " + String.Join("; ", uncoveredVertices));
            return (true,"");
        }
    }
    enum TestAttribute : int
    {
        Name,
        Time,
        Edges,
        Iterations,



        Length, //must be last
    }

}
