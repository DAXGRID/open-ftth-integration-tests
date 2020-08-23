using MemoryGraph;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using OpenFTTH.Events.Core.Infos;
using OpenFTTH.Events.RouteNetwork.Infos;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFTTH.Test.RouteNetworkDatastore
{
    public class RouteNodeRecord : Node
    {
        public Guid WorkTaskMrid { get; set; }
        public string Username { get; set; }
        public virtual string ApplicationName { get; set; }
        public virtual string ApplicationInfo { get; set; }
        public virtual bool MarkAsDeleted { get; set; }
        public virtual bool DeleteMe { get; set; }
        public LifecycleInfo LifecycleInfo { get; set; }
        public MappingInfo MappingInfo { get; set; }
        public SafetyInfo SafetyInfo { get; set; }
        public RouteNodeInfo RouteNodeInfo { get; set; }
        public NamingInfo NamingInfo { get; set; }

        public byte[] Coord => new WKBWriter().Write(Geometry);
    }
}
