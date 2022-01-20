using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;
using _3D_Matching;

namespace _3D_MatchingTests
{
    [TestClass]
    public class Simple2DPerfektMatching
    {
        [TestMethod]
        public void EvenPath()
        {
            var graphString = "6;0->1;1->2;2->3;3->4;4->5";
            var graph = Graph.BuildGraphString(graphString);
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(3, matchingInfo.maxMmatching.Count);
        }
        [TestMethod]
        public void TwoTriangles()
        {
            var graphString = "6;0->1;0->2;1->2;1->3;1->4;3->4;4->5";
            var graph = Graph.BuildGraphString(graphString);
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(3, matchingInfo.maxMmatching.Count);
        }
        [TestMethod]
        public void TwoInterlacedBlossoms()
        {
            var graphString = "7;0->1;1->6;6->2;2->5;5->0;5->3;3->4;4->6";
            //var graphString = "10;0->1;0->2;0->5;0->8;1->2;3->7";
            var graph = Graph.BuildGraphString(graphString);
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(3, matchingInfo.maxMmatching.Count);
        }
    }
}