using _3D_Matching.Solvers;
using _3D_Matching.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace _3D_Matching
{
    class Program
    {
        static void Main(string[] args)
        {

            var parameters = new Dictionary<String, double>();
            parameters.Add("maxTime", 700);
            parameters.Add("maxIter", 300);
            int n = 300;        //320
            double p3 = 0.01;
            double p2 = 0.1;
            double p1 = 0.5;
            int iterations = 10;
            int skip = 0;





            var solvers = new List<IMinimumEdgecoveringSolver> {

                //-----------------TEST SECTION------------------







                //--------------------WORKS OK----------------
                //new Genetic(precalc: true),
                //new Genetic(precalc: false),
                //new Greedy(mode: "byDegreeUpdating_V3"),
                //new RORTS(30 , mode: "withoutHighDegreeVertices"),


                //-------------------WORKS BAD (TIME)--------------------
                //new TwoDBased(type: "2DContractTest", newCalc:20, randomContract: false),
                //new TwoDBased(type: "2DContractTest", newCalc:20, randomContract: true),
                //new HC(climbMode: "allNotOptimal",maxEdgeSwapSize:7),
                //new HC(maxEdgeSwapSize:10),
                //new HC(climbMode: "alternatingSmallImprovements",maxEdgeSwapSize:5),
                //new HC(maxEdgeSwapSize:20),
                //new HC(maxEdgeSwapSize:25),
                //new HC(maxEdgeSwapSize:30),
                //new Greedy(mode: "byNeighbourhoodSize"),
                //new Greedy(mode: "byDegreeUpdating"),

                //-------------------WORKS BAD (SIZE)--------------------
                //new BAB(),
                //new Greedy("byDegree"),
                //new Greedy(mode: "byDegreeUpdatingAndMinSumSorting", sizeWeight: 4, variation: 2),


                //-----------DOESNT WORK YET (NO REAL MATCHING)----------
                //new TwoDBased(type: "noAugmentation"),
                //new TwoDBased(type: "randomSplitAndAugmentOnceByOnce"),
                //new PeakPairing(),
                //new Constructive(),
                //new RORTS(0,preCalculationTime:20),
                //new RORTS(10,preCalculationTime:20),
                //new RORTS(5,preCalculationTime:20,mode:"partialOptimal"),
                //new RORTS(20,preCalculationTime:20,mode:"artificalThinning"),
                //new OnlineSolver(true),
                //new OnlineSolver(false),
                //new OnlineSolver(true,criterion: "firstTimePeak"),
                //new OnlineSolver(true,criterion: "firstTimeByDeg"),
                //new HC(maxEdgeSwapSize:15),
                //new HC(climbMode: "alternatingPaths"),
                //new SAS(500),
                //new RORTS(5,preCalculationTime:20, mode:"minDegree"),







                new ORTS(),
            };

            //for (int i = 0; i < 80; i++)
            //    solvers.Add(new TwoDBased(type: "2DContractTest", newCalc: i));
            //for (int i = 5; i < 350; i++)
            //    solvers.Add(new RORTS(0, mode: "partialOptimalOnInducesSubgraphs", subgaphSize: i));

            var data = SolverTester.RunSolvers(solvers,
                                                parameters,
                                                generationType: "readIn",            //"readIn", "random"
                                                skip: skip,
                                                allowAllAsSingle: true,
                                                removeDegreeOne: true,
                                                iterations: iterations,
                                                n: n, p1: p1, p2: p2, p3: p3);

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


            Console.WriteLine("Sollten die Daten gespeichert werden?   y/n");
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