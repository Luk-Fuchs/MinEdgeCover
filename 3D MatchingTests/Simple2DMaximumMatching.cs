using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;
using _3D_Matching;
using System.Collections.Generic;

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

            Assert.AreEqual(3, matchingInfo.maxMmatching.Count);
        }
        [TestMethod]
        public void TreeGetsResettetCorrectly()
        {
            var graphString = "5;0->1;0->2;0->3;0->4;1->2";
            var graph = Graph.BuildGraphString(graphString);
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(3, matchingInfo.maxMmatching.Count);
        }
        [TestMethod]
        public void UnmatchedLeave()
        {
            var graphString = "3;0->1;1->2;0";
            var graph = Graph.BuildGraphString(graphString);
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(2, matchingInfo.maxMmatching.Count);
            Assert.AreEqual(1, matchingInfo.uncoveredVertices.Count);
            Assert.AreEqual(graph.Vertices[0], matchingInfo.uncoveredVertices[0]);
        }
        [TestMethod]
        public void UnmatchedRoot()
        {
            var graphString = "3;0->1;1->2;2";
            var graph = Graph.BuildGraphString(graphString);
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(2, matchingInfo.maxMmatching.Count);
            Assert.AreEqual(1, matchingInfo.uncoveredVertices.Count);
            Assert.AreEqual(graph.Vertices[2], matchingInfo.uncoveredVertices[0]);
        }
        [TestMethod]
        public void UnmatchedInteriorVertexInFlatTree()
        {
            var graphString = "5;0->1;1->2;2->3;3->4;2";
            var graph = Graph.BuildGraphString(graphString);
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(3, matchingInfo.maxMmatching.Count);
            Assert.AreEqual(1, matchingInfo.uncoveredVertices.Count);
            Assert.AreEqual(graph.Vertices[2], matchingInfo.uncoveredVertices[0]);
        }
        [TestMethod]
        public void UnmatchedMinusVertexInBlossom()
        {
            var graphString = "3;0->1;0->2;1->2;0";
            var graph = Graph.BuildGraphString(graphString);
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(2, matchingInfo.maxMmatching.Count);
            Assert.AreEqual(1, matchingInfo.uncoveredVertices.Count);
            Assert.AreEqual(graph.Vertices[0], matchingInfo.uncoveredVertices[0]);
        }
    }
}