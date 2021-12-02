using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Matching.Solvers
{
    abstract class IMinimumEdgecoveringSolver
    {
        public virtual String Name { get => this.GetType().Name; }
        public Graph _graph;
        public List<Edge> _edges;
        public void initialize(Graph graph)
        {
            _graph = graph;
            _edges = graph.Edges;
            foreach ( var vertex in graph.Vertices)
            {
                vertex.IsCovered = false;
                vertex.TimesCovered = 0;
            }
        }
        public abstract List<Edge> Run(Dictionary<String,double> parameters);
    }
}
