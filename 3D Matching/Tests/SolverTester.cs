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
        public static String[,] RunSolvers(List<IMinimumEdgecoveringSolver> solvers, Dictionary<String,double> parameter, double iterations = 2, int n=100, int m = 200)
        {
            var resData = new String[solvers.Count,(int)TestAttribute.Length];
            var time = new Stopwatch();
            //var graphs = Enumerable.Range(0, (int)iterations).Select(_=>Graph.GenerateRandomGraph(n,m)).ToArray();


            String path = @"C:\Users\LFU\Documents\GitHub\MinEdgeCover\TestData";
            string[] filePaths = Directory.GetFiles(path);
            var graphs = Enumerable.Range(0, Math.Min((int)iterations,filePaths.Length)).Select(_=>(Graph.BuildGraphFromCSV(filePaths[_]))).ToArray();
            iterations = graphs.Count();

            for (int i = 0; i < solvers.Count; i++)
            {
                var solver = solvers[i];
                Console.WriteLine(solver.Name);
                int totalEdgeCount = 0;
                time.Restart();
                for (int j = 0; j < iterations; j++)
                {
                    var graph = graphs[j];
                    solver.initialize(graph);
                    var edgeCover = solver.Run(parameter);
                    totalEdgeCount += edgeCover.Count;
                    if (!IsCover(graph, edgeCover).Item1)
                        throw new System.Exception("no real cover  " + IsCover(graph, edgeCover).Item2);
                }
                time.Stop();
                resData[i, (int)TestAttribute.Time] = time.ElapsedMilliseconds / iterations + "";
                resData[i, (int)TestAttribute.Name] = solvers[i].Name;
                resData[i, (int)TestAttribute.Edges] = totalEdgeCount / iterations + "";
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
            return (true,"");
        }
    }
    enum TestAttribute : int
    {
        Name,
        Time,
        Edges,



        Length, //must be last
    }

}
