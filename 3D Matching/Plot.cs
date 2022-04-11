using Gurobi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Tests
{
    public class Plot
    {
        static string _currentFile = @"C:\Users\LFU\Desktop\Masterarbeit\Ausarbeitung\images\Data_And_Plots\Plots_Online\size_by_iteration.txt";
        public static void CreateFigure(IEnumerable<double> yValue, IEnumerable<double> xValue = null, String title = "", String xLable = "", String yLable = "", String plottype = "bar", bool show = true, String horizontal = "", String vertical = "")
        {
            var csvDataString = "";
            csvDataString += (xValue == null ? String.Join(",", Enumerable.Range(0, yValue.Count())) : String.Join(",", xValue.Select(_ => _.ToString().Replace(",", ".")))) + ";";
            csvDataString += String.Join(",", yValue.Select(_=>_.ToString().Replace(",",".")));
            System.IO.File.WriteAllText(@"C:\Users\LFU\Desktop\tmp\values.csv", csvDataString);


            var csvParameterString = "";
            csvParameterString += "title," + title + ";";
            csvParameterString += "xLabel," + xLable + ";";
            csvParameterString += "yLabel," + yLable + ";";
            csvParameterString += "show," + show + ";";
            csvParameterString += "horizontal," + horizontal + ";";
            csvParameterString += "vertical," + vertical + ";";
            csvParameterString += "plottype," + plottype;
            System.IO.File.WriteAllText(@"C:\Users\LFU\Desktop\tmp\parameters.csv", csvParameterString);
            RunPythonSkript(@"C:\Users\LFU\Documents\GitHub\MinEdgeCover\PythonPlots\plot.py");
        }
        public static void CreateIntervals(List<Edge> cover, bool reorde = false, List<String> additionalPythonLines = null)
        {
            if (reorde)
                //cover = cover.OrderBy(_ => _.Vertices.Min(x => x.Interval[0])).ToList();
                cover = cover.OrderBy(_ => -_.Vertices.Max(x => x.Interval[1])).ToList();
            var csvString = "[" + String.Join(",", cover.Select(_ => "[" + String.Join(",", _.Vertices.Select(x => "[" + x.Interval[0] + "," + x.Interval[1] + "]")) + "]")) + "]";

            if (additionalPythonLines != null)
            {
                foreach (var line in additionalPythonLines)
                {
                    csvString += "\n " + line;
                }
            }
            System.IO.File.WriteAllText(@"C:\Users\LFU\Desktop\tmp\intervals.csv", csvString);
            RunPythonSkript(@"C:\Users\LFU\Documents\GitHub\MinEdgeCover\PythonPlots\plot_intervals.py");
        }
        public static void ExecuteLines(List<string> lines)
        {
            var linesString = String.Join("\n", lines);
            System.IO.File.WriteAllText(@"C:\Users\LFU\Desktop\tmp\string_to_execute.txt", linesString);
            RunPythonSkript(@"C:\Users\LFU\Documents\GitHub\MinEdgeCover\PythonPlots\execute_string.py");
        }

        public static void AddLine (string line)
        {
            System.IO.File.AppendAllLines(_currentFile,new string[] {line });
        }
        public static void RunCurrentFile()
        {
            ExecuteLines(System.IO.File.ReadAllLines(_currentFile).ToList());
        }

        private static void RunPythonSkript(string fileToExecute)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\Program Files (x86)\Microsoft Visual Studio\Shared\Python37_64\python.exe";//cmd is full path to python.exe
            start.Arguments = fileToExecute;//args is path to .py file and any cmd line args
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }
        }

        public static (List<Edge> outside, List<Edge> inside) InfoOfImprovement(Graph graph, List<Edge> matching, bool plotIntervals = false)
        {
            var edges = graph.Edges;
            GRBEnv env = new GRBEnv(true);
            env.Set("OutputFlag", "0");
            env.Start();

            GRBModel solver = new GRBModel(env);
            var x = edges.Select(_ => solver.AddVar(0.0, 1.0, matching.Contains(_)?0.999:1.0, GRB.BINARY, String.Join(" ", _.Vertices))).ToArray();
            for (int i = 0; i < graph.Vertices.Count; i++)
            {
                GRBLinExpr expr = 0.0;
                for (int j = 0; j < edges.Count; j++)
                {
                    if (edges[j].VerticesIds.Contains(graph.Vertices[i].Id))
                        expr.AddTerm(1.0, x[j]);
                }
                solver.AddConstr(expr, GRB.EQUAL, 1, "c0");
            }

            GRBLinExpr solutionBound = 0.0;
            for (int j = 0; j < edges.Count; j++)
            {
                solutionBound.AddTerm(1.0, x[j]);
            }
            solver.AddConstr(solutionBound, GRB.GREATER_EQUAL, matching.Count - 1, "c0");

            solver.Optimize();
            var res = new List<Edge>();
            for (int j = 0; j < x.Length; j++)
            {
                if (x[j].X != 0)//Variable(j).SolutionValue() != 0)
                    res.Add(edges[j]);
            }

            //graph.ResetVertexAdjEdges();
            var test = solver.ObjVal;
            var test2 = edges.Where(_=>matching.Contains(_)).ToList();
            var test3 = matching.Where(_=>!edges.Contains(_)).ToList();
            var test4 = edges.Where(_ => _.Vertices.Select(x=>x.Id).Contains(286)).ToList();

            if (plotIntervals)
                Plot.CreateIntervals(res, true, new List<string>() { "plt.title(\"Naehstes Optimum mit " + res.Count + " Diensten \")" });

            return (res.Where(_ => !matching.Contains(_)).ToList(), matching.Where(_ => !res.Contains(_)).ToList());


        }
    }
}
