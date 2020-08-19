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

        private string _nodeIdPrefix = "0b2168f2-d9be-455c-a4de-e9169f";
        private string _segmentIdPrefix = "b95000fb-425d-4cd3-9f45-66e8c5";

        public void Run(string routeNodeFilename, string routeSegmentFilename, RouteNetworkDatastore routeNetworkDatestore)
        {
            _routeNodeFilename = routeNodeFilename;
            _routeSegmentFilename = routeSegmentFilename;
            _routeNetworkDatestore = routeNetworkDatestore;

            // Create route nodes and segments
            var graphBuilder = new Wgs84GraphBuilder(_routeGraph);

            ImportRouteNodes(graphBuilder);
            ImportRouteSegments(graphBuilder);
        }

        public Graph RouteGraph => RouteGraph;

        private void ImportRouteNodes(Wgs84GraphBuilder graphBuilder)
        {
            // Import node objects to database
            var nodesJsonText = File.ReadAllText(_routeNodeFilename);

            var nodesJson = JsonConvert.DeserializeObject(nodesJsonText) as JObject;

            var features = nodesJson["features"];

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

                RouteNodeRecord routeNode = new RouteNodeRecord()
                {
                    Id = nodeId,
                    Geometry = GeographicToProjectedCoordinateConverter.ConvertPoint(GeoJsonConversionHelper.ConvertFromPointGeoJson(geometryCoordinates)),
                    Kind = nodeKind,
                    Function = nodeFunctionKind
                };

                graphBuilder.AddNodeToGraph(routeNode, (double)x, (double)y);
                _routeNetworkDatestore.InsertNode(routeNode);
            }
        }

        private void ImportRouteSegments(Wgs84GraphBuilder graphBuilder)
        {
            // Import node objects to database
            var segmentJsonText = File.ReadAllText(_routeSegmentFilename);

            var segmentsJson = JsonConvert.DeserializeObject(segmentJsonText) as JObject;

            var features = segmentsJson["features"];

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

                RouteSegmentRecord routeSegment = new RouteSegmentRecord()
                {
                    Id = segmentId,
                    Geometry = GeographicToProjectedCoordinateConverter.ConvertLineString(GeoJsonConversionHelper.ConvertFromLineGeoJson(geometryCoordinates)),
                    Kind = segmentKindCode.Value
                };

                graphBuilder.AddEdgeToGraph(routeSegment, (double)startX, (double)startY, (double)endX, (double)endY);
                _routeNetworkDatestore.InsertSegment(routeSegment);
            }
        }
    }
}
