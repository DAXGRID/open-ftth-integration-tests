using MemoryGraph;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using OpenFTTH.Events.RouteNetwork.Infos;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFTTH.TestNetworkSeeder.Datastores
{
    public class RouteNodeRecord : Node
    {
        public string Name { get; set; }
        public RouteNodeKindEnum? Kind { get; set; }
        public RouteNodeFunctionEnum? Function { get; set; }
        public byte[] Coord => new WKBWriter().Write(Geometry);
    }
}
