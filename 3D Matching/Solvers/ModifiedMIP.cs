using _3D_Matching.Tests;
using Gurobi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ORT = Google.OrTools.LinearSolver;
using Coin = Sonnet;
using COIN;


namespace _3D_Matching.Solvers
{

    enum MMIPModi : int
    {
        relaxation,
        inducedSubgraphs,
    }
    class MMIP : IMinimumEdgecoveringSolver
    {
        public static int[] auslastung = new int[9500];
        int additionalNewCalculatedEdges = 7;
        int preCalculationTime = 10;
        MMIPModi _mode = 0;
        Random _random = new Random();
        public override String Name { get => this.GetType().Name + "(" + additionalNewCalculatedEdges + "|" + preCalculationTime + "|" + _mode + ")"; }
        public MMIP(MMIPModi mode = 0, int additionalNewCalculatedEdges = 0, int preCalculationTime = 10)
        {
            this.additionalNewCalculatedEdges = additionalNewCalculatedEdges;
            this.preCalculationTime = preCalculationTime;
            _mode = mode;
        }

        public override (List<Edge> cover, int iterations) Run(Dictionary<string, double> parameters)
        {



            if (_mode == MMIPModi.relaxation)
            {
                GRBEnv env = new GRBEnv(true);
                env.Set("OutputFlag", "0");
                env.Start();
                GRBModel solver = new GRBModel(env);
                var x = _edges.Select(_ => solver.AddVar(0.0, 1.0, 1.0, GRB.CONTINUOUS, String.Join(" ", _.Vertices))).ToArray();
                for (int i = 0; i < _graph.Vertices.Count; i++)
                {
                    GRBLinExpr expr = 0.0;
                    for (int j = 0; j < _edges.Count; j++)
                    {
                        if (_edges[j].VerticesIds.Contains(_graph.Vertices[i].Id))
                            expr.AddTerm(1.0, x[j]);
                    }
                    solver.AddConstr(expr, GRB.EQUAL, 1, "c0");
                }
                solver.Optimize();
                var res = new List<Edge>();
                for (int j = 0; j < x.Length; j++)
                {
                    if (x[j].X == 1)
                        res.Add(_edges[j]);
                }

                foreach(var e in res)
                {
                    foreach(var v in e.Vertices)
                    {
                        v.IsCovered = true;
                    }
                }
                //var haveToEdges = ComputeuniquePairingEdges(_graph);

                //Console.WriteLine((0.0 + haveToEdges.Intersect(res).Count()) / haveToEdges.Count);

                var remainingEdges = _edges.Where(_ => _.AllVerticesAreUncovered()).ToList();
                var remainingVertices = _graph.Vertices.Where(_ => !_.IsCovered).ToList();

                env = new GRBEnv(true);
                env.Set("OutputFlag", "0");
                env.Start();
                solver = new GRBModel(env);
                x = remainingEdges.Select(_ => solver.AddVar(0.0, 1.0, 1.0, GRB.INTEGER, String.Join(" ", _.Vertices))).ToArray();
                for (int i = 0; i < remainingVertices.Count; i++)
                {
                    GRBLinExpr expr = 0.0;
                    for (int j = 0; j < remainingEdges.Count; j++)
                    {
                        if (remainingEdges[j].VerticesIds.Contains(remainingVertices[i].Id))
                            expr.AddTerm(1.0, x[j]);
                    }
                    solver.AddConstr(expr, GRB.EQUAL, 1, "c0");
                }
                solver.Optimize();
                for (int j = 0; j < x.Length; j++)
                {
                    if (x[j].X == 1)
                        res.Add(remainingEdges[j]);
                }

                return (res, 2);
            }
            if (_mode == MMIPModi.inducedSubgraphs)
            {
                var res = new List<Edge>();
                int sizeOfS = additionalNewCalculatedEdges;     //determines the size of the subgraphs
                var uncoveredVertices = _graph.Vertices.ToList();
                uncoveredVertices.Shuffle();
                int subGraphAmount = 0;
                var s = new List<Vertex>();
                while (uncoveredVertices.Count() > 0)
                {
                    subGraphAmount++;
                    s = uncoveredVertices.Take(sizeOfS).ToList();
                    foreach (var vertex in s)
                        uncoveredVertices.Remove(vertex);
                    var eEdgesOfS = s.SelectMany(_ => _.AdjEdges).Where(_ => _.Vertices.Where(x => !s.Contains(x)).Count() == 0).ToList();

                    var solver2 = new MIP();
                    solver2.initialize(new Graph(eEdgesOfS, s));
                    var partialRes = solver2.Run(parameters).cover;

                    res = res.Concat(partialRes).ToList();
                }
                return (res, subGraphAmount);
            }




            //Console.WriteLine("Usable Edges:" + _edges.Where(_ => _.canBeInPerfectMatching).Count());


            //{

            //var time = new Stopwatch();
            //time.Start();
            //var edges = _graph.Edges;

            //GRBEnv env = new GRBEnv(true);
            //env.Set("OutputFlag", "0");
            //env.Start();

            //GRBModel solver = new GRBModel(env);
            //var x = edges.Select(_ => solver.AddVar(0.0, 1.0, 1.0, GRB.CONTINUOUS, String.Join(" ", _.Vertices))).ToArray();
            //for (int i = 0; i < _graph.Vertices.Count; i++)
            //{
            //    //var constraint = solver.MakeConstraint(1, 1, "");
            //    GRBLinExpr expr = 0.0;
            //    for (int j = 0; j < edges.Count; j++)
            //    {
            //        if (edges[j].VerticesIds.Contains(_graph.Vertices[i].Id))
            //            //constraint.SetCoefficient(x[j], 1);
            //            expr.AddTerm(1.0, x[j]);
            //    }
            //    solver.AddConstr(expr, GRB.EQUAL, 1, "c0");
            //}
            //solver.Optimize();
            //var res = new List<Edge>();
            //var vertexCopy = _graph.Vertices.ToList();
            //for (int j = 0; j < x.Length; j++)
            //{
            //    if (x[j].X == 1)
            //    {
            //        res.Add(edges[j]);
            //        foreach (var v in edges[j].Vertices)
            //            vertexCopy.Remove(v);
            //    }
            //}

            //    Console.WriteLine("time: " + time.ElapsedMilliseconds);
            //    //var sorted = x.OrderBy(_ =>- _.X);
            //    //Console.WriteLine(String.Join(" ",sorted.Take(10).Select(_=>_.VarName)));
            //    //Console.WriteLine(String.Join(" ",sorted.Take(300).Select(_=>_.X)));

            //    //Console.WriteLine("Usable Edges:" + _edges.Where(_ => _.canBeInPerfectMatching).Count());
            //    Console.WriteLine(vertexCopy.Count);
            //    var subgraph = _graph.GenerateInducedSubgraph(vertexCopy);
            //    var subSolver = new TwoDBased();
            //    subgraph.ResetVertexAdjEdges();
            //    foreach (var v in subgraph.Vertices)
            //    {
            //        v.MatchedVertex = null;
            //        v.IsContracted = false;
            //        v.Predecessor = null;
            //        v.OddPath = null;
            //        v.IsInTree = false;
            //        v.BlossomIndex = 0;
            //        v.ContractedWith = null;
            //        v.ContractedVertex = null;
            //    }
            //    subSolver.initialize(subgraph);
            //    var subRes = subSolver.Run(parameters);

            //    Console.WriteLine("kombined res size: " + (res.Count + subRes.cover.Count));
            //    return (res.Concat(subRes.cover).ToList(), -1);
            //}

            //_graph.ResetVertexAdjEdges();










            //Console.WriteLine("-----------");
            //{
            //    for (int i = 0; i < _edges.Count; i++)
            //    {
            //        _edges[i].inedex = i;
            //    }

            //    var solver3 = new MIP();
            //    solver3.initialize(_graph);
            //    var tmpRes = solver3.Run(parameters);
            //    var optIndices = tmpRes.cover.Select(_ => _.inedex);


            //    var x = _edges.Select(_ => 0.0/*_random.NextDouble()*/).ToArray();

            //    //foreach(var indes in optIndices)
            //    //{
            //    //    x[indes] = 1;
            //    //}


            //    var error2 = x.Sum();

            //    foreach (var v in _graph.Vertices)
            //    {
            //        error2 += (1 - v.AdjEdges.Select(_ => x[_.inedex]).Sum()) * (1 - v.AdjEdges.Select(_ => x[_.inedex]).Sum());
            //    }
            //    Console.WriteLine(error2);



            //    var plotData = new List<double>();
            //    var C = 100000000000000;
            //    for (int iter = 0; iter < 5000; iter++)
            //    {
            //        //if (iter % 1000 == 0)
            //        //    Console.WriteLine(iter);
            //        var derivative = _edges.Select(edge => ((x[edge.inedex]<0?-1:1) + C*edge.Vertices.Select(vertex_in_edge => -2 * (1 - vertex_in_edge.AdjEdges.Select(_ => x[_.inedex]).Sum())).Sum())).ToArray();

            //        for (int i = 0; i < x.Length; i++)
            //        {
            //            if (iter < 2000)
            //                x[i] = x[i] - 0.0001 / C * derivative[i];
            //            else
            //                x[i] = x[i] - 0.001 / C * derivative[i];
            //            //    x[i] = x[i] - (131/2 * 1/derivative.Select(_=>_*_).Sum())* derivative[i];

            //        }

            //        //if(iter %100 == 0)
            //        //{
            //        //    for (int k = 0; k < x.Length; k++)
            //        //    {
            //        //        if (x[k] < 0.005)
            //        //            x[k] = 0;
            //        //        else
            //        //            x[k] = 1;
            //        //    }
            //        //}
            //        //for (int k = 0; k < x.Length; k++)
            //        //{
            //        //    if (x[k] < 0)
            //        //        x[k] = 0;
            //        //}

            //        //if(iter >= 2000)
            //        //{

            //        //var error = 0.0; ;
            //        //foreach (var v in _graph.Vertices)
            //        //{
            //        //    error += (1 - v.AdjEdges.Select(_ => x[_.inedex]).Sum()) * (1 - v.AdjEdges.Select(_ => x[_.inedex]).Sum());
            //        //}

            //        //plotData.Add(error);
            //        //Console.WriteLine((error + x.Sum())+"     " + x.Sum()+ "     " +error);
            //        //}
            //    }

            //    //Plot.CreateFigure(plotData);  

            //; Plot.CreateFigure(x.ToList(),title:"x-Values");
            //    var res = new List<Edge>();
            //    var orderedValues = x.OrderBy(_ => -_).ToList();
            //    for(int i = 0; i< 100; i++)
            //    {
            //        res = _edges.Where(_ => x[_.inedex] >= orderedValues[i]).ToList();

            //        var vWithduplicates = res.SelectMany(_ => _.VerticesIds).ToList();
            //        var vWithoutduplicates = vWithduplicates.Distinct().ToList();

            //        if (vWithduplicates.Count != vWithoutduplicates.Count)
            //            Console.WriteLine(i);

            //    }



            //    var verticesOfCover = res.SelectMany(_ => _.Vertices);
            //    foreach(var v in _graph.Vertices)
            //    {
            //        if (!verticesOfCover.Contains(v))
            //        {
            //            res.Add(new Edge(new List<Vertex>() { v }));
            //        }
            //    }
            //    Console.WriteLine(String.Join("|", x.OrderBy(_ => _)));


            //    var lowDegreeVertíces = _graph.Vertices.OrderBy(_ => _.AdjEdges.Count).ToList();




            //return (res, -1);
            //}
            //Console.WriteLine("sortiert nach größe");


            //;


            //if (_mode == "normal")
            //{
            //    edgesCopy = _edges.OrderBy(_ => -_.Vertices.Count *100000+_.Vertices.Select(x=>x.AdjEdges.Count).Sum()+ _random.NextDouble()).ToList();
            //    while (edgesCopy.Count > 3000)
            //    {
            //        var edge = edgesCopy[0];
            //        edgeCover.Add(edge);
            //        foreach (var vertex in edge.Vertices)
            //        {
            //            verticesCopy.Remove(vertex);
            //            int j = 0;
            //            while (j < edgesCopy.Count)
            //            {
            //                if (edgesCopy[j].Vertices.Contains(vertex))
            //                    edgesCopy.RemoveAt(j);
            //                else
            //                    j++;
            //            }
            //        }
            //    }
            //}
            //else if (_mode == "artificalThinning")
            //{
            //    while (edgesCopy.Count > 300)
            //    {
            //        var edge = edgesCopy[0];
            //        edgeCover.Add(edge);
            //        foreach (var vertex in edge.Vertices)
            //        {
            //            verticesCopy.Remove(vertex);
            //            int j = 0;
            //            while (j < edgesCopy.Count)
            //            {
            //                if (edgesCopy[j].Vertices.Contains(vertex))
            //                    edgesCopy.RemoveAt(j);
            //                else
            //                    j++;
            //            }
            //        }
            //    }
            //    edgesCopy = edgesCopy.OrderBy(_ => _.Vertices.Count + _random.NextDouble()).ToList();
            //    int i = 0;
            //    while (edgesCopy.Count > 280)
            //    {
            //        if (edgesCopy[i].Vertices.Count == 1)
            //            i++;
            //        else
            //            edgesCopy.RemoveAt(i);
            //    }
            //}
            //else if (_mode == "matchHighDegreeAfterWards")
            //{
            //    var highDegreeVertices = _graph.Vertices.OrderBy(_=>-_.AdjEdges.Count).Take(additionalNewCalculatedEdges).ToList();
            //    var restrictedEdges = _graph.Edges.Where(_ => _.Vertices.All(x => !highDegreeVertices.Contains(x))).ToList();

            //    var solver = new MIP(solver: "ORT");
            //    solver.initialize(new Graph(restrictedEdges, _graph.Vertices.Where(_=>!highDegreeVertices.Contains(_)).ToList()));
            //    var time = new Stopwatch();
            //    time.Start();
            //    edgeCover = solver.Run(parameters).cover.ToList();
            //    time.Stop();
            //    //Console.WriteLine("partial time: " + time.ElapsedMilliseconds);

            //    var tmpGraphEdges = edgeCover.ToList(); ;
            //    foreach(var edge in _graph.Edges)
            //    {
            //        foreach (var vertex in edge.Vertices)
            //        {
            //            if (highDegreeVertices.Contains(vertex)&& (edge.VertexCount==1|| edgeCover.Contains(new Edge(edge.Vertices.Where(_ => _ != vertex).ToList()))))
            //                tmpGraphEdges.Add(edge);
            //        }
            //        if (edge.VertexCount == 2 && highDegreeVertices.Contains(edge.vertex0) && highDegreeVertices.Contains(edge.vertex1))
            //            tmpGraphEdges.Add(edge);
            //    }
            //    var tmpGraph = new Graph(tmpGraphEdges, _graph.Vertices);
            //    tmpGraph.ResetVertexAdjEdges();
            //    solver = new MIP();
            //    solver.initialize(tmpGraph);


            //    var edgeCover2 = solver.Run(parameters).cover.ToList();
            //    ;
            //    return (edgeCover2, -1);
            //}
            //else if (_mode == "minDegree")
            //{
            //    var subgraphEdges = new List<Edge>();
            //    int minDegree = 3;
            //    var degree = new int[verticesCopy.Count];

            //    edgesCopy = _graph.Edges;
            //    edgesCopy.Shuffle();
            //    var newEdges = new List<Edge>();

            //    for(int i = 0; i< edgesCopy.Count; i++)
            //    {
            //        var edge = edgesCopy[i];
            //        if ((degree[edge.Vertices[0].Id] < additionalNewCalculatedEdges)
            //            || (edge.Vertices.Count > 1 && degree[edge.Vertices[1].Id] < minDegree)
            //            || (edge.Vertices.Count > 2 && degree[edge.Vertices[2].Id] < minDegree))
            //        {
            //            for(int j = 0; j < edge.Vertices.Count; j++)
            //            {
            //                degree[edge.VerticesIds[j]]++;
            //            }
            //            newEdges.Add(edge);
            //        }
            //    }

            //    edgesCopy = newEdges.Concat(edgesCopy.Where(_=>_.Vertices.Count==1)).ToList();
            //}

            //else if (_mode=="initalSolution")
            //{

            //    var solver2 = new TwoDBased();
            //    solver2.initialize(_graph);
            //    var twoDRes = solver2.Run(parameters).cover;

            //    //_graph.InitializeFor2DMatchin();
            //    //var twoDRes = _graph.GetMaximum2DMatching();
            //    edges = _graph.Edges;
            //    GRBEnv env = new GRBEnv(true);
            //    env.Set("OutputFlag", "0");
            //    env.Start();

            //    GRBModel solver = new GRBModel(env);
            //    var x = edges.Select(_ => solver.AddVar(0.0, 1.0, 1.0, GRB.BINARY, String.Join(" ", _.Vertices))).ToArray();
            //    for (int i = 0; i < _graph.Vertices.Count; i++)
            //    {
            //        //var constraint = solver.MakeConstraint(1, 1, "");
            //        GRBLinExpr expr = 0.0;
            //        for (int j = 0; j < edges.Count; j++)
            //        {
            //            if (edges[j].VerticesIds.Contains(_graph.Vertices[i].Id))
            //                //constraint.SetCoefficient(x[j], 1);
            //                expr.AddTerm(1.0, x[j]);
            //        }
            //        solver.AddConstr(expr, GRB.EQUAL, 1, "c0");
            //    }

            //    for(int i = 0; i< edges.Count; i++)
            //    {
            //        if (twoDRes.Contains(edges[i]))
            //        {
            //            x[i].Start = 1;
            //        }
            //        else
            //        {
            //            x[i].Start = 0;

            //        }
            //    }


            //    solver.Optimize();
            //    var res = new List<Edge>();
            //    for (int j = 0; j < x.Length; j++)
            //    {
            //        if (x[j].X != 0)//Variable(j).SolutionValue() != 0)
            //            res.Add(edges[j]);
            //    }
            //    return (res, 1);
            //}
            //else if (_mode == "precalc")
            //{
            //    var graphCopy = new Graph(_graph.Edges.ToList(), _graph.Vertices.ToList());
            //    graphCopy.SetVertexAdjEdges();
            //    var activeVertex = graphCopy.Vertices.FirstOrDefault(_=>_.AdjEdges.Count<3);
            //    var precalcEdges = new List<Edge>();
            //    while (activeVertex!= null)
            //    {
            //        var matchingEdge = activeVertex.AdjEdges.Last();
            //        graphCopy.Edges = graphCopy.Edges.Where(_ => _.Vertices.Intersect(matchingEdge.Vertices).Count() == 0).ToList();
            //        graphCopy.Vertices = graphCopy.Vertices.Where(_ => !matchingEdge.Contains(_)).ToList();
            //        precalcEdges.Add(matchingEdge);
            //        graphCopy.ResetVertexAdjEdges();
            //        activeVertex = graphCopy.Vertices.FirstOrDefault(_=>_.AdjEdges.Count<3);
            //    }
            //    ;
            //    var time = new Stopwatch();
            //    time.Start();
            //    var solver = new MIP();
            //    solver.initialize(graphCopy);
            //    edgeCover = solver.Run(parameters).cover;
            //    edgeCover.AddRange(precalcEdges);
            //    notFinished = false;
            //    Console.WriteLine("needed time: " + time.ElapsedMilliseconds);
            //    Console.WriteLine("edges reduced by "+ (_graph.Edges.Count-graphCopy.Edges.Count));
            //}



            //if (notFinished)
            //{
            //    var time = new Stopwatch();
            //    var solver = new MIP();
            //    solver.initialize(new Graph(edgesCopy, verticesCopy));
            //    time.Start();
            //    edgeCover = edgeCover.Concat(solver.Run(parameters).cover).ToList();
            //time.Stop();
            ////Console.WriteLine("partial time: " + time.ElapsedMilliseconds);
            //}

            //return (edgeCover, -1);
            return (null, -1);
        }

        private static List<Edge> ComputeuniquePairingEdges(Graph graph)
        {
            var edges = graph.Edges;
            {
                var haveToEdges = new List<Edge>();
                var lastObjective = double.MaxValue;
                int p = 0;
                var res = new List<Edge>();
                foreach (var edge in edges)
                {
                    edge.canBeInPerfectMatching = false;
                }
                while (true)
                {

                    edges = graph.Edges;

                    GRBEnv env = new GRBEnv(true);
                    env.Set("OutputFlag", "0");
                    env.Start();

                    GRBModel solver = new GRBModel(env);
                    var x = edges.Select(_ => solver.AddVar(0.0, 1.0, _.canBeInPerfectMatching ? 1 : 0.999, GRB.BINARY, String.Join(" ", _.Vertices))).ToArray();
                    for (int i = 0; i < graph.Vertices.Count; i++)
                    {
                        //var constraint = solver.MakeConstraint(1, 1, "");
                        GRBLinExpr expr = 0.0;
                        for (int j = 0; j < edges.Count; j++)
                        {
                            if (edges[j].VerticesIds.Contains(graph.Vertices[i].Id))
                                //constraint.SetCoefficient(x[j], 1);
                                expr.AddTerm(1.0, x[j]);
                        }
                        solver.AddConstr(expr, GRB.EQUAL, 1, "c0");
                    }
                    solver.Optimize();
                    res = new List<Edge>();
                    for (int j = 0; j < x.Length; j++)
                    {
                        if (x[j].X != 0)
                            res.Add(edges[j]);
                    }

                    haveToEdges = new List<Edge>();
                    if (lastObjective != double.MaxValue)
                    {
                        foreach (var edge in edges)
                        {
                            if (edge.canBeInPerfectMatching && res.Contains(edge))
                            {
                                edge.canBeInPerfectMatching = true;
                                continue;
                            }
                            edge.canBeInPerfectMatching = false;
                        }
                    }
                    else
                    {
                        foreach (var edge in res)
                        {
                            edge.canBeInPerfectMatching = true;
                        }
                    }
                    if (lastObjective == solver.ObjVal)
                    {
                        //Console.WriteLine(p);
                        break;
                    }
                    lastObjective = solver.ObjVal;
                }

                foreach (var edge in res)
                {
                    if (edge.canBeInPerfectMatching)
                    {
                        haveToEdges.Add(edge);
                    }
                }
                graph.ResetVertexAdjEdges();
                //Console.Write("[" + String.Join(",", haveToEdges.Select(e => "[" + String.Join(",", e.Vertices.Select(v => "[" + v.Interval[0] + "," + v.Interval[1] + "]")) + "]")) + "],");
                //Console.WriteLine(String.Join(",", haveToEdges.SelectMany(_ => _.Vertices).Select(_ => _.AdjEdges.Count).OrderBy(_ => _)));
                return haveToEdges;
            }
        }
    }
}