using _3D_Matching.Solvers;
using _3D_Matching.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Diagnostics;



namespace _3D_Matching
{
    class Program
    {
        static void Main(string[] args)
        {

            

            var parameters = new Dictionary<String, double>();
            parameters.Add("maxTime", 5000);
            parameters.Add("maxIter", 1_00000);
            int n = 300;        //320
            double p3 = 0.01;
            double p2 = 0.1;
            double p1 = 0.5;
            int iterations = 1;
            int skip = 0;

            var random = new Random();





            var solvers = new List<IMinimumPerfectMatchingSolver> {

                //---------------- GREEDY SECTION -----------------
                //new Greedy(GreedyModi.byTime),
                //new Greedy(GreedyModi.withExplizitBadPairing),
                //new Greedy(GreedyModi.byTimeReverse),
                //new Greedy(GreedyModi.bySize),
                //new Greedy(GreedyModi.bySizeAndDegreeUpdating),
                //new Greedy(GreedyModi.bySizeAndNeighbourhoodSize),
                //new Greedy(GreedyModi.byPeak),
                new Greedy(GreedyModi.byIntegral),
                //new Greedy(GreedyModi.variable,new Func<Edge, double>(_=>-_.VertexCount*100000 +_.Vertices.SelectMany(v=>v.AdjEdges.SelectMany(e=>e.Vertices)).Distinct().Count())),

                ////hybride Greedy Ansätze mit verscheidenen Kostfunktionen:
                //    //Size (hybrid)
                //new Greedy(GreedyModi.variableHybrid,new Func<Edge, double>(_=>-_.VertexCount*100000 -random.NextDouble())),
                //    //Size and Degree
                //new Greedy(GreedyModi.variableHybrid,new Func<Edge, double>(_=>-_.VertexCount*100000 - _.Vertices.Select(v=>v.AdjEdges.Count).Sum())),
                //    //Size and short dutys
                //new Greedy(GreedyModi.variableHybrid,new Func<Edge, double>(_=>-_.Vertices.Last().Interval[1]+_.Vertices[0].Interval[0])),
                //    //Size and 2-Degree
                //new Greedy(GreedyModi.variableHybrid,new Func<Edge, double>(_=>-_.VertexCount*100000 + _.Vertices.Select(v=>v.Adj2Edges.Count).Sum())),
                //    //Size and NeighbourhoodSize
                //new Greedy(GreedyModi.variableHybrid,new Func<Edge, double>(_=>-_.VertexCount*100000 +_.Vertices.SelectMany(v=>v.AdjEdges.SelectMany(e=>e.Vertices)).Distinct().Count())),


                //---------------- Genetic SECTION -----------------
                //new Genetic(GeneticModi.permuteAllEdges),
                //new Genetic(GeneticModi.permuteAll2EdgesAndUnion),
                //new Genetic(GeneticModi.permuteAll3EdgesAndFillOptimal),
                //new Genetic(GeneticModi.random3EdgesAndFillOptimal),


                //---------------- ModifiedMIP SECTION -----------------
                new MMIP(MMIPModi.inducedSubgraphs),
                new MMIP(MMIPModi.relaxation),
                
                //---------------- MIP SECTION -----------------
                new MIP(MIPModi.GUROBI),
                new MIP(MIPModi.COINOR),
                new MIP(MIPModi.ORT),



                //-----------------TEST SECTION------------------
                //new TwoDBased(),
                //new TwoDBased(),
                //new HC(climbMode: "boundedDepth"),
                //new PeakPairing(),
                //new TwoDBased(),
                //new PeakPairing(),
                //new TwoDBased("DynamicContraction"),
                //new OnlineSolver(),
                //new TwoDBased(),
                //new TwoDBased("2DContractHyprid"),




                //new TwoDBased(type: "2DContractTest", newCalc:20, randomContract: false),
                //new TwoDBased(type: "2DContractTest", newCalc:20, randomContract: true),
                //new HC(climbMode: "allNotOptimal",maxEdgeSwapSize:7),
                //new HC(maxEdgeSwapSize:10),
                //new HC(climbMode: "alternatingSmallImprovements",maxEdgeSwapSize:5),
                //new HC(maxEdgeSwapSize:20),
                //new HC(maxEdgeSwapSize:25),
                //new HC(maxEdgeSwapSize:30),

            };




            //for (int i = 0; i < 11; i++)
            //    solvers.Add(new MIP(usePerc3D: 1 - 0.1 * i));


            var data = SolverTester.RunSolvers(solvers,
                                                parameters,
                                                generationType: "readIn",            //"readIn", "random"
                                                skip: skip,
                                                allowAllAsSingle: true,
                                                removeDegreeOne: true,
                                                forceCompletness: false,
                                                addAllPossibleEdges: false,
                                                iterations: iterations,
                                                n: n, p1: p1, p2: p2, p3: p3);


            //Console.WriteLine(String.Join(",", TwoDBased.valuePerIteration.Select(_=>((_+0.0)/20).ToString().Replace(",","."))));
            //Console.WriteLine(String.Join(",", MMIP.auslastung));

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