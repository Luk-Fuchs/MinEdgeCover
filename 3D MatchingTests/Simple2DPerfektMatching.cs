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

            Assert.AreEqual(4, matchingInfo.maxMmatching.Count);
        }
        [TestMethod]
        public void ProblemCase()
        {
            var graphString = "12;0->3;0->4;0->7;0->8;1->2;1->5;2->3;2->4;2->5;2->9;3->4;3->10;4->7;6->8;9->10;9->11;0;1;2;3;4;5;6;7;8;9;10;11";
            var graph = Graph.BuildGraphString(graphString);
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(6, matchingInfo.maxMmatching.Count);
        }
        
        [TestMethod]
        public void ProblemCase2()
        {
            var graphString = "5;0->1;0->2;0->3;0->4;1->4;2->3;0;1;2;3;4";
            var graph = Graph.BuildGraphString(graphString);
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(3, matchingInfo.maxMmatching.Count);
        }
        
        [TestMethod]
        public void ProblemCase3()
        {
            var graphString = "6;0->3;0->4;0->5;1->2;1->3;1->4;1->5;2->3;2->4;0;1;2;3;4;5";
            var graph = Graph.BuildGraphString(graphString);
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(3, matchingInfo.maxMmatching.Count);
        }
        
        [TestMethod]
        public void ProblemCase4()
        {
            var graphString = "16;0->4;0->8;1->2;4->6;5->6;5->7;5->10;6->13;7->11;7->15;8->10;9->11;10->14;11->13;13->15;0;1;2;3;4;5;6;7;8;9;10;11;12;13;14;15";
            var graph = Graph.BuildGraphString(graphString);
            graph.InitializeFor2DMatchin();
            var matchingInfo = graph.GetMaximum2DMatching();

            Assert.AreEqual(9, matchingInfo.maxMmatching.Count);
        }
    }
}