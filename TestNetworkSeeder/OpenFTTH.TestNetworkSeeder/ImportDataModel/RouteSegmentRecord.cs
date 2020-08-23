using MemoryGraph;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFTTH.TestNetworkSeeder.Datastores
{
    public class RouteSegmentRecord : Edge
    {
        public Guid WorkTaskMrid { get; set; }
        public string Username { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationInfo { get; set; }
        public virtual bool MarkAsDeleted { get; set; }
        public virtual bool DeleteMe { get; set; }
        public LifecycleInfo LifecycleInfo { get; set; }
        public MappingInfo MappingInfo { get; set; }
        public SafetyInfo SafetyInfo { get; set; }
        public RouteSegmentInfo RouteSegmentInfo { get; set; }
        public NamingInfo NamingInfo { get; set; }
        public byte[] Coord => new WKBWriter().Write(Geometry);
    }
}
