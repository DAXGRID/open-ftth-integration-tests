using MemoryGraph;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using OpenFTTH.Events.RouteNetwork.Infos;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFTTH.TestNetworkSeeder.Datastores
{
    public class RouteSegmentRecord : Edge
    {
        public RouteSegmentKindEnum Kind { get; set; }
        public byte[] Coord => new WKBWriter().Write(Geometry);
    }
}
