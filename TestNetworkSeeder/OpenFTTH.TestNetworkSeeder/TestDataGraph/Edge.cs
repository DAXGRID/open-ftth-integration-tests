using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGraph
{
    public class Edge : GraphElement
    {
        public Node StartNode { get; set; }
        public Node EndNode { get; set; }
        public double Length { get; set; }
        public LineString Geometry { get; set; }

        public List<Guid> NeighborEdges()
        {
            List<Guid> result = new List<Guid>();

            result.AddRange(StartNode.Edges.Where(l => l != this).Select(l => l.Id));
            result.AddRange(EndNode.Edges.Where(l => l != this).Select(l => l.Id));

            return result;
        }
    }
}
