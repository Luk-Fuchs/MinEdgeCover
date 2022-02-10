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
        public static String[,] RunSolvers(List<IMinimumEdgecoveringSolver> solvers, Dictionary<String,double> parameter ,String generationType= "readIn", int skip = 0,bool allowAllAsSingle = false,bool removeDegreeOne = true, double iterations = 2, int n=100, double p1 = 0.1, double p2 = 0.1, double p3 = 0.1)
        {
            var resData = new String[solvers.Count,(int)TestAttribute.Length];
            var time = new Stopwatch();
            iterations += skip;
            var graphs = new List<Graph>();
            if(generationType == "readIn")
            {
                String path = @"C:\Users\LFU\Documents\GitHub\MinEdgeCover\LFU-Tmp_Run1";
                //String path = @"C:\Users\LFU\Documents\GitHub\MinEdgeCover\TestData";
                //String path = @"C:\Users\LFU\Documents\GitHub\MinEdgeCover\LFU-Tmp_Run2";
                //String path = @"C:\Users\LFU\Documents\GitHub\MinEdgeCover\TestDaten2";
                //String path = @"C:\Users\LFU\Desktop\Masterarbeit\UnitTestDaten";
                string[] filePaths = Directory.GetFiles(path);
                graphs = Enumerable.Range(0, Math.Min((int)iterations, filePaths.Length)).Select(_ => (Graph.BuildGraphFromCSV(filePaths[_],allowAllAsSingle: allowAllAsSingle, removeDegreeOne: removeDegreeOne))).ToList();
                iterations = graphs.Count();
            }
            else
            {
                graphs = Enumerable.Range(0, (int)iterations).Select(_ => Graph.GenerateRandomGraph(n, p1: p1, p2: p2, p3: p3)).ToList();
            }

            graphs = graphs.Skip(skip).ToList();

            for (int i = 0; i < solvers.Count; i++)
            {
                double totalIterations = 0.0;
                double multipleTimesCoveredVertices = 0;
                var solver = solvers[i];
                Console.WriteLine(solver.Name);
                int totalEdgeCount = 0;
                time.Reset();
                var edgeSizes = new int[3];
                for (int j = 0; j < iterations; j++)
                {
                    var graph = graphs[j];

                    time.Start();
                    solver.initialize(graph);
                    (var edgeCover, double used_iterations)= solver.Run(parameter);
                    time.Stop();

                    totalEdgeCount += edgeCover.Count;
                    totalIterations += used_iterations;
                    if (!IsCover(graph, edgeCover).Item1)
                        Console.WriteLine(IsCover(graph, edgeCover).Item2 + "multipleCovers");
                    for(int size = 1; size < 4; size++)
                    {
                        edgeSizes[size - 1] += edgeCover.Where(_ => _.Vertices.Count == size).Count();
                    }
                    //throw new System.Exception("no real cover  " + IsCover(graph, edgeCover).Item2);
                    multipleTimesCoveredVertices += IsCover(graph, edgeCover).Item2;
                }
                time.Stop();
                resData[i, (int)TestAttribute.Time] = Math.Round(time.ElapsedMilliseconds / iterations ,0) + "";
                resData[i, (int)TestAttribute.Name] = solvers[i].Name;
                resData[i, (int)TestAttribute.Edges] = Math.Round(totalEdgeCount / iterations,4) + "";
                resData[i, (int)TestAttribute.Iter] = totalIterations / iterations + "";
                resData[i, (int)TestAttribute.MultCov] = multipleTimesCoveredVertices / iterations + "";
                resData[i, (int)TestAttribute.CoverComp] = (int)(edgeSizes[0] / iterations) + "|"+ (int)(edgeSizes[1] / iterations) + "|"+ (int)(edgeSizes[2] / iterations) + "|";
            }

            return resData;

        }
        

        public static (bool,double) IsCover(Graph graph, List<Edge> cover)
        {
            var covenessArray = new bool[graph.Vertices.Count];
            int multipleCoverCounter = 0;
            foreach (var edge in cover)
            {
                //if (!graph.Edges.Contains(edge))        //edge is no real edge
                if (graph.Edges.Where(_ => _.Equals(edge)).ToString().Length == 0)
                    throw new Exception("edge is no real edge");
                    //return (false, "edge is no real edge");
                foreach(var vertex in edge.Vertices)
                {
                    if (covenessArray[vertex.Id])       //vertex twice gecovered
                        multipleCoverCounter++;
                    else
                        covenessArray[vertex.Id] = true;
                }
            }
            var uncoveredVertices = Enumerable.Range(0, covenessArray.Length).Where(_ => !covenessArray[_]).ToList();
            if(uncoveredVertices.Count!=0)
                    throw new Exception ("there are uncovered Vertices " + String.Join("; ", uncoveredVertices));
            return (true,multipleCoverCounter);
        }
    }
    enum TestAttribute : int
    {
        Name,
        Time,
        Edges,
        Iter,
        MultCov,
        CoverComp,


        Length, //must be last
    }

}
