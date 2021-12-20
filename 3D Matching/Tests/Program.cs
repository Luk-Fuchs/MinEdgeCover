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
            var parameters = new Dictionary<String, double>();
            parameters.Add("maxTime",300);
            int n = 300;        //320
            double p3 = 0.01;
            double p2 = 0.1;
            double p1 = 0.5;
            int iterations = 1;

            var solvers = new List<IMinimumEdgecoveringSolver> {
                //new RS("1"),
                new RS(mode: "byDegree"),
                new RS(mode: "byDegreeUpdating"),
                new RS(mode: "byDegreeUpdatingAndRegret"),  //scheint bis jetzt nicht sinnvoll
                new RS(mode: "byDegreeUpdating_V3"),
                //new SAS(500),
                //new RORTS(0,preCalculationTime:20),
                //new RORTS(20,preCalculationTime:20),
                //new RORTS(40,preCalculationTime:20,mode:"partialOptimal"),
                //new RORTS(20,preCalculationTime:20, mode:"minDegree"),
                //new RORTS(20,preCalculationTime:20,mode:"artificalThinning"),
                //new ORTS(),
                new OnlineSolver(OnlineSolver.GenerateFunc3(120),"test"),
        };

            //for (int j = 1; j < 15; j++)
            //    solvers.Add(new SAS(j*0.2, "" + j + ""));


            var data = SolverTester.RunSolvers(solvers, parameters,iterations:iterations,n:n,p1:p1,p2:p2,p3:p3);

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