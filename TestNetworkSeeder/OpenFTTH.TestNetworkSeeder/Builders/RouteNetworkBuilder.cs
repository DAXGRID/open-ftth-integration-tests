using MemoryGraph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenFTTH.Events.RouteNetwork.Infos;
using OpenFTTH.TestNetworkSeeder.Util;
using OpenFTTH.TestNetworkSeeder.Datastores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenFTTH.Events.Core.Infos;
using System.Linq;

namespace DemoDataBuilder.Builders
{
    public class RouteNetworkBuilder
    {
        private Graph _routeGraph = new Graph();
        private Dictionary<Guid, string[]> _nodeBuildCodes = new Dictionary<Guid, string[]>();
        private Dictionary<Guid, string[]> _segmentBuildCodes = new Dictionary<Guid, string[]>();

        private string _routeNodeFilename;
        private string _routeSegmentFilename;

        private RouteNetworkDatastore _routeNetworkDatestore;

        private Guid _startMarker;
        private Guid _endMarker;

        private string _nodeIdPrefix = "0b2168f2-d9be-455c-a4de-e9169f";
        private string _segmentIdPrefix = "b95000fb-425d-4cd3-9f45-66e8c5";

        private string _applicationName = "TestNetworkSeeder";
        private string _userName = Environment.UserName;
        private Guid _workTaskId = Guid.Parse("22f110e2-e132-4301-a7df-2f1cb85167e3");

        public RouteNetworkBuilder Run(string routeNodeFilename, string routeSegmentFilename, RouteNetworkDatastore routeNetworkDatestore, Guid startMarker, Guid endMarker)
        {
            _routeNodeFilename = routeNodeFilename;
            _routeSegmentFilename = routeSegmentFilename;
            _routeNetworkDatestore = routeNetworkDatestore;
            _startMarker = startMarker;
            _endMarker = endMarker;

            // Create route nodes and segments
            var graphBuilder = new Wgs84GraphBuilder(_routeGraph);

            ImportRouteNodes(graphBuilder);
            ImportRouteSegments(graphBuilder);

            return this;
        }

        public Graph RouteGraph => _routeGraph;

        private void ImportRouteNodes(Wgs84GraphBuilder graphBuilder)
        {
            // Import node objects to database
            var nodesJsonText = File.ReadAllText(_routeNodeFilename);

            var nodesJson = JsonConvert.DeserializeObject(nodesJsonText) as JObject;

            var features = nodesJson["features"];

            bool firstNode = true;

            foreach (var feature in features)
            {
                var properties = feature["properties"] as JObject;

                var geometry = feature["geometry"];

                var geometryType = geometry["type"].ToString();
                var geometryCoordinates = geometry["coordinates"].ToString().Replace("\r\n", "").Replace(" ", "");

                var nodeId = Guid.Parse(_nodeIdPrefix + properties["Id"].ToString().PadLeft(6, '0'));
                var nodeType = properties["NodeType"].ToString();
                var nodeName = properties["NodeName"].ToString();
                var assetStatus = properties["Status"].ToString();

                if (properties["BuildTestData"].ToString() != "")
                {
                    var buildCodes = properties["BuildTestData"].ToString().Split(';');
                    _nodeBuildCodes.Add(nodeId, buildCodes);
                }


                // Add node to graph
                var x = ((JArray)geometry["coordinates"])[0];
                var y = ((JArray)geometry["coordinates"])[1];
                

                // Derive node and function kind
                RouteNodeKindEnum? nodeKind = null;
                RouteNodeFunctionEnum? nodeFunctionKind = null;

                if (nodeType == "CO")
                {
                    nodeKind = RouteNodeKindEnum.CentralOfficeSmall;
                    nodeFunctionKind = RouteNodeFunctionEnum.SecondaryNode;
                }
                else if (nodeType == "HH")
                {
                    nodeKind = RouteNodeKindEnum.HandHole;
                    nodeFunctionKind = RouteNodeFunctionEnum.AccessibleConduitClosure;
                }
                else if (nodeType == "CC")
                {
                    nodeKind = RouteNodeKindEnum.ConduitClosure;
                    nodeFunctionKind = RouteNodeFunctionEnum.NonAccessibleConduitClosure;
                }
                else if (nodeType == "CE")
                {
                    nodeKind = RouteNodeKindEnum.ConduitEnd;
                    nodeFunctionKind = RouteNodeFunctionEnum.NonAccessibleConduitClosure;
                }
                else if (nodeType == "SJ")
                {
                    nodeKind = RouteNodeKindEnum.ConduitSimpleJunction;
                    nodeFunctionKind = RouteNodeFunctionEnum.NonAccessibleConduitClosure;
                }
                else if (nodeType == "FP")
                {
                    nodeKind = RouteNodeKindEnum.CabinetBig;
                    nodeFunctionKind = RouteNodeFunctionEnum.FlexPoint;
                }
                else if (nodeType == "SP")
                {
                    nodeKind = RouteNodeKindEnum.CabinetSmall;
                    nodeFunctionKind = RouteNodeFunctionEnum.SplicePoint;
                }
                else if (nodeType == "A")
                {
                    nodeKind = RouteNodeKindEnum.BuildingAccessPoint;
                    nodeFunctionKind = RouteNodeFunctionEnum.SplicePoint;
                }
                else if (nodeType == "MDU")
                {
                    nodeKind = RouteNodeKindEnum.MultiDwellingUnit;
                    nodeFunctionKind = RouteNodeFunctionEnum.CustomerPremisesPoint;
                }
                else if (nodeType == "SDU")
                {
                    nodeKind = RouteNodeKindEnum.SingleDwellingUnit;
                    nodeFunctionKind = RouteNodeFunctionEnum.CustomerPremisesPoint;
                }

                // On the first node, we set the start marker into applicaion info, and insert data into all the other propeties as well to test if every information is captured into the generated events
                if (firstNode)
                {
                    RouteNodeRecord routeNode = new RouteNodeRecord()
                    {
                        Id = nodeId,
                        WorkTaskMrid = _startMarker,
                        ApplicationName = _applicationName,
                        ApplicationInfo = _applicationName,
                        DeleteMe = false,
                        MarkAsDeleted = false,
                        Username = _userName,
                        Geometry = GeographicToProjectedCoordinateConverter.ConvertPoint(GeoJsonConversionHelper.ConvertFromPointGeoJson(geometryCoordinates)),
                        RouteNodeInfo = new RouteNodeInfo(nodeKind, nodeFunctionKind),
                        LifecycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Now, DateTime.Now),
                        MappingInfo = new MappingInfo(MappingMethodEnum.LandSurveying, "10 cm", "20 cm", DateTime.Now, "Surveyed with GPS"),
                        NamingInfo = new NamingInfo(nodeName, "Route node"),
                        SafetyInfo = new SafetyInfo("no danger", "might contain rats"),
                        IsAutoCreated = false
                    };

                    graphBuilder.AddNodeToGraph(routeNode, (double)x, (double)y);
                    _routeNetworkDatestore.InsertRouteNode(routeNode);
                }
                else
                {
                    RouteNodeRecord routeNode = new RouteNodeRecord()
                    {
                        Id = nodeId,
                        ApplicationName = _applicationName,
                        ApplicationInfo = _applicationName,
                        DeleteMe = false,
                        MarkAsDeleted = false,
                        Username = _userName,
                        Geometry = GeographicToProjectedCoordinateConverter.ConvertPoint(GeoJsonConversionHelper.ConvertFromPointGeoJson(geometryCoordinates)),
                        RouteNodeInfo = new RouteNodeInfo(nodeKind, nodeFunctionKind),
                        NamingInfo = new NamingInfo(nodeName, "Route node"),
                        IsAutoCreated = false
                    };

                    graphBuilder.AddNodeToGraph(routeNode, (double)x, (double)y);
                    _routeNetworkDatestore.InsertRouteNode(routeNode);
                }
               

                firstNode = false;
            }
        }

        private void ImportRouteSegments(Wgs84GraphBuilder graphBuilder)
        {
            // Import node objects to database
            var segmentJsonText = File.ReadAllText(_routeSegmentFilename);

            var segmentsJson = JsonConvert.DeserializeObject(segmentJsonText) as JObject;

            var features = segmentsJson["features"];

            bool firstSegment = true;
            bool lastSegment = false;

            var numberOfSegmentFeatures = features.Count();

            var segmentCounter = 1;

            foreach (var feature in features)
            {
                var properties = feature["properties"] as JObject;

                var geometry = feature["geometry"];

                var geometryType = geometry["type"].ToString();
                var geometryCoordinates = geometry["coordinates"].ToString().Replace("\r\n", "").Replace(" ", "");

                var segmentId = Guid.Parse(_segmentIdPrefix + properties["Id"].ToString().PadLeft(6, '0'));
                var segmentKind = properties["RouteSegmentKind"].ToString();
                var assetStatus = properties["Status"].ToString();

                if (properties["BuildTestData"].ToString() != "")
                {
                    var buildCodes = properties["BuildTestData"].ToString().Split(';');
                    _segmentBuildCodes.Add(segmentId, buildCodes);
                }


                // Add link to graph
                var coordinates = geometry["coordinates"] as JArray;
                var startX = coordinates.First[0];
                var startY = coordinates.First[1];

                var endX = coordinates.Last[0];
                var endY = coordinates.Last[1];

                // Derive node and function kind
                RouteSegmentKindEnum? segmentKindCode = null;

                if (segmentKind == "buried")
                {
                    segmentKindCode = RouteSegmentKindEnum.Underground;
                }

                // On the first node, we set the start marker into applicaion info, and insert data into all the other propeties as well to test if every information is captured into the generated events
                if (firstSegment)
                {
                    RouteSegmentRecord routeSegment = new RouteSegmentRecord()
                    {
                        Id = segmentId,
                        ApplicationName = _applicationName,
                        ApplicationInfo = _applicationName,
                        DeleteMe = false,
                        MarkAsDeleted = false,
                        Username = _userName,
                        Geometry = GeographicToProjectedCoordinateConverter.ConvertLineString(GeoJsonConversionHelper.ConvertFromLineGeoJson(geometryCoordinates)),
                        RouteSegmentInfo = new RouteSegmentInfo(RouteSegmentKindEnum.Underground, "50 cm", "90 cm"),
                        LifecycleInfo = new LifecycleInfo(DeploymentStateEnum.InService, DateTime.Now, DateTime.Now),
                        MappingInfo = new MappingInfo(MappingMethodEnum.LandSurveying, "10 cm", "20 cm", DateTime.Now, "Surveyed with GPS"),
                        NamingInfo = new NamingInfo("Route segment", "I'm an underground route segment"),
                        SafetyInfo = new SafetyInfo("no danger", "might contain gophers"),
                    };

                    graphBuilder.AddEdgeToGraph(routeSegment, (double)startX, (double)startY, (double)endX, (double)endY);
                    _routeNetworkDatestore.InsertRouteSegment(routeSegment);
                }
                else
                {
                    RouteSegmentRecord routeSegment = new RouteSegmentRecord()
                    {
                        Id = segmentId,
                        ApplicationName = _applicationName,
                        ApplicationInfo = _applicationName,
                        DeleteMe = false,
                        MarkAsDeleted = false,
                        Username = _userName,
                        Geometry = GeographicToProjectedCoordinateConverter.ConvertLineString(GeoJsonConversionHelper.ConvertFromLineGeoJson(geometryCoordinates)),
                        RouteSegmentInfo = new RouteSegmentInfo(RouteSegmentKindEnum.Underground, "50 cm", "90 cm"),
                    };

                    // Mark last segment
                    if (segmentCounter == numberOfSegmentFeatures)
                        routeSegment.WorkTaskMrid = _endMarker;

                    graphBuilder.AddEdgeToGraph(routeSegment, (double)startX, (double)startY, (double)endX, (double)endY);
                    _routeNetworkDatestore.InsertRouteSegment(routeSegment);
                }


                firstSegment = false;
                segmentCounter++;
            }
        }
    }
}
