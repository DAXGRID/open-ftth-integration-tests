using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGraph
{
    public class Node : GraphElement
    {
        public Point Geometry { get; set; }
        public List<Edge> Edges = new List<Edge>();
        public bool IsAutoCreated = false;
    }
}
