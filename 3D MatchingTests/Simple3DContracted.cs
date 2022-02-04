using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;
using _3D_Matching;
using System.Collections.Generic;

namespace _3D_MatchingTests
{
    [TestClass]
    public class Simple3DContracted
    {
        [TestMethod]
        public void Use3DEdge()
        {
            var graphString = "5;1->2;1->2->4;0->1;2->3;0->3";
            var graph = Graph.BuildGraphString(graphString);
            var contractingEdges = new List<Edge> {graph.Edges[0] };
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(2, matchingInfo.maxMmatching.Count);
        }
        [TestMethod]
        public void DontUse3DEdge()
        {
            var graphString = "6;1->2;1->2->4;0->1;2->3;0->3;4->5";
            var graph = Graph.BuildGraphString(graphString);
            var contractingEdges = new List<Edge> { graph.Edges[0] };
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(3, matchingInfo.maxMmatching.Count);
        }

    }
}