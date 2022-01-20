using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;
using _3D_Matching;

namespace _3D_MatchingTests
{
    [TestClass]
    public class Simple2DMaximumMatching
    {
        [TestMethod]
        public void OddPath()
        {
            var graphString = "5;0->1;1->2;2->3;3->4";
            var graph = Graph.BuildGraphString(graphString);
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(2, matchingInfo.maxMmatching.Count);
        }
        [TestMethod]
        public void TreeGetsResettetCorrectly()
        {
            var graphString = "5;0->1;0->2;0->3;0->4;1->2";
            var graph = Graph.BuildGraphString(graphString);
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(2, matchingInfo.maxMmatching.Count);
        }
    }
}