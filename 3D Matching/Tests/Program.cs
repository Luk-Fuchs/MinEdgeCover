using _3D_Matching.Solvers;
using _3D_Matching.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace _3D_Matching
{
    class Program
    {
        static void Main(string[] args)
        {

            //var time = new Stopwatch();
            //time.Start();
            //var list = new List<int>(Enumerable.Range(0, 300));
            //for (int i = 0; i < 10000000; i++)
            //{
            //    list.RemoveAt(1);
            //    list.Insert(1,0);
            //}
            //time.Stop();
            //Console.WriteLine(time.ElapsedMilliseconds);
            //var link = new LinkedList<int>(Enumerable.Range(0, 300));
            //for (int i = 0; i < 10000000; i++)
            //{
            //    var node = link.First;
            //    node = node.Next;
            //    link.Remove(node);
            //    link.AddBefore(link.First, node);
            //}
            //time.Stop();
            //Console.WriteLine(time.ElapsedMilliseconds);

            //time.Restart();
            //return;





            var parameters = new Dictionary<String, double>();
            parameters.Add("maxTime",30);
            int n = 320;        //320
            int m = 350;
            int iterations = 10;

            var solvers = new List<IMinimumEdgecoveringSolver> {
                new RS(),
                new RS(mode: "byDegree"),
                new RS(mode: "byDegreeUpdating"),
                new RS(mode: "byDegreeUpdating2"),
                new RS(mode: "byDegreeUpdating3"),
                //new SAS(),
                //new RORTS(0,preCalculationTime:20),
                //new RORTS(20,preCalculationTime:20),
                //new RORTS(40,preCalculationTime:20,mode:"partialOptimal"),
                //new RORTS(20,preCalculationTime:20, mode:"minDegree"),
                //new RORTS(20,preCalculationTime:20,mode:"artificalThinning"),
                //new ORTS(),
            };

            //for (int i = 1; i < n/9-1; i++)
            //{
            //    solvers.Add(new HillClimbSolver(i * 3));
            //}
            //var solverTester = new SolverTester();
            var data = SolverTester.RunSolvers(solvers, parameters,iterations:iterations,n:n,m:m);

            PrintData(data);

        }

        public static void PrintData(String[,] data)
        {
            String csv = String.Join(";", Enumerable.Range(0, (int)TestAttribute.Length).Select(_ => (TestAttribute)_)) + "\n";
            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    csv += data[i, j];
                    if (j != data.GetLength(1) - 1)
                        csv += ";";
                }
                csv += "\n";
            }
            //Console.WriteLine(csv);
            csv = csv.Replace(",", ".");
            //Console.WriteLine(csv);
            var format = "";
            for (int i = 0; i < data.GetLength(1); i++)
                if(i==0)
                    format += "{" + i + "," + Enumerable.Range(0, data.GetLength(0)).Select(_ => data[_, i].Length).Max() + "}";
                else
                    format += "{" + i + "," + Enumerable.Range(0, data.GetLength(0)).Select(_ => data[_, i].Length + 5).Max() + "}";

            foreach (var line in csv.Split("\n"))
            {
                if (line == "")
                    continue;
                Console.WriteLine(format, line.Split(";"));
            }


            Console.WriteLine("Sollten die Daten gespeichert werden?");
            Console.WriteLine("Pfad:  " + @"C:\Users\LFU\Desktop\Masterarbeit\Algorithm_data\test.csv");
            var shouldSave = Console.Read();
            if (shouldSave == 121)
            {
                System.IO.File.WriteAllText(@"C:\Users\LFU\Desktop\Masterarbeit\Algorithm_data\test.csv", csv);
                Console.WriteLine("Data saved!");
            }

        }
    }
}