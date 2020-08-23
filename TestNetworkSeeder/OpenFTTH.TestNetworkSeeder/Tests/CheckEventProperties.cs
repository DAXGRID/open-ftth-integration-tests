using MemoryGraph;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.TestNetworkSeeder.Datastores;
using Serilog;
using System;
using System.Collections.Generic;

namespace OpenFTTH.TestNetworkSeeder.Tests
{
    public class CheckEventProperties
    {
        private HashSet<Guid> _ids = new HashSet<Guid>();

        public bool Run(Graph importedData, List<RouteNetworkEvent> resultingEvents)
        {
            int eventListIndex = 0;

            var testFailed = false;

            // Check nodes
            foreach (var node in importedData.Nodes.Values)
            {
                var routeNetworkEvent = resultingEvents[eventListIndex];

                if (!(routeNetworkEvent is RouteNodeAdded))
                    Log.Error($"The event at seq no: {routeNetworkEvent.EventSequenceNumber} was expected to be a RouteNodeAdded event, but got a {routeNetworkEvent.GetType().Name} event instead.");
                else
                {
                    var routeNodeAddedEvent = routeNetworkEvent as RouteNodeAdded;

                    if (!CheckNodeAddedEventProperties(routeNodeAddedEvent, (RouteNodeRecord)node))
                        testFailed = true;
                }

                eventListIndex++;
            }

            // Check segments
            foreach (var segment in importedData.Edges.Values)
            {
                var routeNetworkEvent = resultingEvents[eventListIndex];

                if (!(routeNetworkEvent is RouteSegmentAdded))
                    Log.Error($"The event at seq no: {routeNetworkEvent.EventSequenceNumber} was expected to be a RouteSegmentAdded event, but got a {routeNetworkEvent.GetType().Name} event instead.");
                else
                {
                    var routeNodeAddedEvent = routeNetworkEvent as RouteSegmentAdded;

                    // Check properties
                    if (!CheckSegmentAddedEventProperties(routeNodeAddedEvent, (RouteSegmentRecord)segment))
                        testFailed = true;






                }



                eventListIndex++;
            }


            if (!testFailed)
                return true;
            else
                return false;
        }


        private bool CheckNodeAddedEventProperties(RouteNodeAdded routeNodeAddedEvent, RouteNodeRecord sourceNode)
        {
            var allTestsOk = true;

            if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.Id, routeNodeAddedEvent.NodeId, "NodeId"))
                allTestsOk = false;

            // Check route node info
            if (!TestPropertyValueNoEquals(routeNodeAddedEvent, sourceNode.RouteNodeInfo, routeNodeAddedEvent.RouteNodeInfo, "RouteNodeInfo"))
            {
                allTestsOk = false;
            }
            else if (routeNodeAddedEvent.RouteNodeInfo != null)
            {
                if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.RouteNodeInfo.Function, routeNodeAddedEvent.RouteNodeInfo.Function, "RouteNodeInfo.Function"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.RouteNodeInfo.Kind, routeNodeAddedEvent.RouteNodeInfo.Kind, "RouteNodeInfo.Kind"))
                    allTestsOk = false;
            }

            // general

            // Check event type
            if (!TestPropertyValue(routeNodeAddedEvent, "RouteNodeAdded", routeNodeAddedEvent.EventType, "EventType"))
                allTestsOk = false;

            // Check command type
            if (!TestPropertyValue(routeNodeAddedEvent, "NewRouteNodeDigitized", routeNodeAddedEvent.CmdType, "CmdType"))
                allTestsOk = false;

            // Check IsLastEventInCmd
            if (!TestPropertyValue(routeNodeAddedEvent, true, routeNodeAddedEvent.IsLastEventInCmd, "IsLastEventInCmd"))
                allTestsOk = false;

            // Check event id uniqueness
            if (!TestIfIdNotAlreadyUsed(routeNodeAddedEvent, routeNodeAddedEvent.EventId, _ids, "EventId"))
                allTestsOk = false;

            // Check cmd id uniqueness
            if (!TestIfIdNotAlreadyUsed(routeNodeAddedEvent, routeNodeAddedEvent.CmdId, _ids, "CmdId"))
                allTestsOk = false;

            if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.ApplicationName, routeNodeAddedEvent.ApplicationName, "ApplicationName"))
                allTestsOk = false;

            if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.ApplicationInfo, routeNodeAddedEvent.ApplicationInfo, "ApplicationInfo"))
                allTestsOk = false;

            if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.Username, routeNodeAddedEvent.UserName, "UserName"))
                allTestsOk = false;

            if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.WorkTaskMrid, routeNodeAddedEvent.WorkTaskMrid, "WorkTaskMrid"))
                allTestsOk = false;


            // Check naming info
            if (!TestPropertyValueNoEquals(routeNodeAddedEvent, sourceNode.NamingInfo, routeNodeAddedEvent.NamingInfo, "NamingInfo"))
            {
                allTestsOk = false;
            }
            else if (routeNodeAddedEvent.NamingInfo != null)
            {
                if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.NamingInfo.Name, routeNodeAddedEvent.NamingInfo.Name, "NamingInfo.Name"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.NamingInfo.Description, routeNodeAddedEvent.NamingInfo.Description, "NamingInfo.Description"))
                    allTestsOk = false;
            }


            // Check mapping info
            if (!TestPropertyValueNoEquals(routeNodeAddedEvent, sourceNode.MappingInfo, routeNodeAddedEvent.MappingInfo, "MappingInfo"))
            {
                allTestsOk = false;
            }
            else if (routeNodeAddedEvent.MappingInfo != null)
            {
                if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.MappingInfo.Method, routeNodeAddedEvent.MappingInfo.Method, "MappingInfo.Method"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.MappingInfo.HorizontalAccuracy, routeNodeAddedEvent.MappingInfo.HorizontalAccuracy, "MappingInfo.HorizontalAccuracy"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.MappingInfo.VerticalAccuracy, routeNodeAddedEvent.MappingInfo.VerticalAccuracy, "MappingInfo.VerticalAccuracy"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.MappingInfo.SurveyDate, routeNodeAddedEvent.MappingInfo.SurveyDate, "MappingInfo.SurveyDate"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.MappingInfo.SourceInfo, routeNodeAddedEvent.MappingInfo.SourceInfo, "MappingInfo.SourceInfo"))
                    allTestsOk = false;
            }

            // Check lifecycle info
            if (!TestPropertyValueNoEquals(routeNodeAddedEvent, sourceNode.LifecycleInfo, routeNodeAddedEvent.LifecyleInfo, "LifecycleInfo"))
            {
                allTestsOk = false;
            }
            else if (routeNodeAddedEvent.LifecyleInfo != null)
            {
                if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.LifecycleInfo.DeploymentState, routeNodeAddedEvent.LifecyleInfo.DeploymentState, "LifecycleInfo.DeploymentState"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.LifecycleInfo.InstallationDate, routeNodeAddedEvent.LifecyleInfo.InstallationDate, "LifecycleInfo.InstallationDate"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.LifecycleInfo.RemovalDate, routeNodeAddedEvent.LifecyleInfo.RemovalDate, "LifecycleInfo.RemovalDate"))
                    allTestsOk = false;
            }

            // Check safety info
            if (!TestPropertyValueNoEquals(routeNodeAddedEvent, sourceNode.SafetyInfo, routeNodeAddedEvent.SafetyInfo, "SafetyInfo"))
            {
                allTestsOk = false;
            }
            else if (routeNodeAddedEvent.SafetyInfo != null)
            {
                if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.SafetyInfo.Classification, routeNodeAddedEvent.SafetyInfo.Classification, "SafetyInfo.Classification"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeNodeAddedEvent, sourceNode.SafetyInfo.Remark, routeNodeAddedEvent.SafetyInfo.Remark, "SafetyInfo.Remark"))
                    allTestsOk = false;
            }

            return allTestsOk;
        }

        private bool CheckSegmentAddedEventProperties(RouteSegmentAdded routeSegmentAddedEvent, RouteSegmentRecord sourceSegment)
        {
            var allTestsOk = true;

            if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.Id, routeSegmentAddedEvent.SegmentId, "SegmentId"))
                allTestsOk = false;

            // Check route segment info
            if (!TestPropertyValueNoEquals(routeSegmentAddedEvent, sourceSegment.RouteSegmentInfo, routeSegmentAddedEvent.RouteSegmentInfo, "RouteSegmentInfo"))
            {
                allTestsOk = false;
            }
            else if (routeSegmentAddedEvent.RouteSegmentInfo != null)
            {
                if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.RouteSegmentInfo.Kind, routeSegmentAddedEvent.RouteSegmentInfo.Kind, "RouteSegmentInfo.Kind"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.RouteSegmentInfo.Width, routeSegmentAddedEvent.RouteSegmentInfo.Width, "RouteSegmentInfo.Width"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.RouteSegmentInfo.Height, routeSegmentAddedEvent.RouteSegmentInfo.Height, "RouteSegmentInfo.Height"))
                    allTestsOk = false;
            }

            // Check from node id
            if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.StartNode.Id, routeSegmentAddedEvent.FromNodeId, "FromNodeId"))
                allTestsOk = false;

            // Check to node id
            if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.EndNode.Id, routeSegmentAddedEvent.ToNodeId, "ToNodeId"))
                allTestsOk = false;


            // general

            // Check event type
            if (!TestPropertyValue(routeSegmentAddedEvent, "RouteSegmentAdded", routeSegmentAddedEvent.EventType, "EventType"))
                allTestsOk = false;

            // Check command type
            if (!TestPropertyValue(routeSegmentAddedEvent, "NewRouteSegmentDigitized", routeSegmentAddedEvent.CmdType, "CmdType"))
                allTestsOk = false;

            // Check IsLastEventInCmd
            if (!TestPropertyValue(routeSegmentAddedEvent, true, routeSegmentAddedEvent.IsLastEventInCmd, "IsLastEventInCmd"))
                allTestsOk = false;

            // Check event id uniqueness
            if (!TestIfIdNotAlreadyUsed(routeSegmentAddedEvent, routeSegmentAddedEvent.EventId, _ids, "EventId"))
                allTestsOk = false;

            // Check cmd id uniqueness
            if (!TestIfIdNotAlreadyUsed(routeSegmentAddedEvent, routeSegmentAddedEvent.CmdId, _ids, "CmdId"))
                allTestsOk = false;

            if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.ApplicationName, routeSegmentAddedEvent.ApplicationName, "ApplicationName"))
                allTestsOk = false;

            if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.ApplicationInfo, routeSegmentAddedEvent.ApplicationInfo, "ApplicationInfo"))
                allTestsOk = false;

            if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.Username, routeSegmentAddedEvent.UserName, "UserName"))
                allTestsOk = false;

            if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.WorkTaskMrid, routeSegmentAddedEvent.WorkTaskMrid, "WorkTaskMrid"))
                allTestsOk = false;


            // Check naming info
            if (!TestPropertyValueNoEquals(routeSegmentAddedEvent, sourceSegment.NamingInfo, routeSegmentAddedEvent.NamingInfo, "NamingInfo"))
            {
                allTestsOk = false;
            }
            else if (routeSegmentAddedEvent.NamingInfo != null)
            {
                if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.NamingInfo.Name, routeSegmentAddedEvent.NamingInfo.Name, "NamingInfo.Name"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.NamingInfo.Description, routeSegmentAddedEvent.NamingInfo.Description, "NamingInfo.Description"))
                    allTestsOk = false;
            }


            // Check mapping info
            if (!TestPropertyValueNoEquals(routeSegmentAddedEvent, sourceSegment.MappingInfo, routeSegmentAddedEvent.MappingInfo, "MappingInfo"))
            {
                allTestsOk = false;
            }
            else if (routeSegmentAddedEvent.MappingInfo != null)
            {
                if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.MappingInfo.Method, routeSegmentAddedEvent.MappingInfo.Method, "MappingInfo.Method"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.MappingInfo.HorizontalAccuracy, routeSegmentAddedEvent.MappingInfo.HorizontalAccuracy, "MappingInfo.HorizontalAccuracy"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.MappingInfo.VerticalAccuracy, routeSegmentAddedEvent.MappingInfo.VerticalAccuracy, "MappingInfo.VerticalAccuracy"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.MappingInfo.SurveyDate, routeSegmentAddedEvent.MappingInfo.SurveyDate, "MappingInfo.SurveyDate"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.MappingInfo.SourceInfo, routeSegmentAddedEvent.MappingInfo.SourceInfo, "MappingInfo.SourceInfo"))
                    allTestsOk = false;
            }

            // Check lifecycle info
            if (!TestPropertyValueNoEquals(routeSegmentAddedEvent, sourceSegment.LifecycleInfo, routeSegmentAddedEvent.LifecyleInfo, "LifecycleInfo"))
            {
                allTestsOk = false;
            }
            else if (routeSegmentAddedEvent.LifecyleInfo != null)
            {
                if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.LifecycleInfo.DeploymentState, routeSegmentAddedEvent.LifecyleInfo.DeploymentState, "LifecycleInfo.DeploymentState"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.LifecycleInfo.InstallationDate, routeSegmentAddedEvent.LifecyleInfo.InstallationDate, "LifecycleInfo.InstallationDate"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.LifecycleInfo.RemovalDate, routeSegmentAddedEvent.LifecyleInfo.RemovalDate, "LifecycleInfo.RemovalDate"))
                    allTestsOk = false;
            }

            // Check safety info
            if (!TestPropertyValueNoEquals(routeSegmentAddedEvent, sourceSegment.SafetyInfo, routeSegmentAddedEvent.SafetyInfo, "SafetyInfo"))
            {
                allTestsOk = false;
            }
            else if (routeSegmentAddedEvent.SafetyInfo != null)
            {
                if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.SafetyInfo.Classification, routeSegmentAddedEvent.SafetyInfo.Classification, "SafetyInfo.Classification"))
                    allTestsOk = false;

                if (!TestPropertyValue(routeSegmentAddedEvent, sourceSegment.SafetyInfo.Remark, routeSegmentAddedEvent.SafetyInfo.Remark, "SafetyInfo.Remark"))
                    allTestsOk = false;
            }

            return allTestsOk;
        }

        private bool TestIfIdNotAlreadyUsed(RouteNetworkEvent routeNetworkEvent, Guid id, HashSet<Guid> usedIds, string propertyName)
        {
            if (usedIds.Contains(id))
            {
                Log.Error($"The property: {propertyName} in event at seq no: {routeNetworkEvent.EventSequenceNumber} contains a non unique id: {id} This is already used elsewere in the event stream.");
                return false;
            }
            else
            {
                usedIds.Add(id);
                return true;
            }
        }

        private bool TestPropertyValue(RouteNetworkEvent routeNetworkEvent, object expected, object actual, string propertyName)
        {
            if (expected == null && actual == null)
                return true;

            if ((expected != null && actual == null) || (expected == null && actual != null))
            {
                Log.Error($"The property: {propertyName} in event at seq no: {routeNetworkEvent.EventSequenceNumber} has an unexpeted property value: '{actual}' Expected: '{expected}'");
                return false;
            }

            if (expected is DateTime)
            {
                if (!expected.ToString().Equals(actual.ToString()))
                {
                    Log.Error($"The property: {propertyName} in event at seq no: {routeNetworkEvent.EventSequenceNumber} has an unexpeted property value: '{actual}' Expected: '{expected}'");
                    return false;
                }
            }
            else
            {
                if (!expected.Equals(actual))
                {
                    Log.Error($"The property: {propertyName} in event at seq no: {routeNetworkEvent.EventSequenceNumber} has an unexpeted property value: '{actual}' Expected: '{expected}'");
                    return false;
                }
            }

            return true;
        }

        private bool TestPropertyValueNoEquals(RouteNetworkEvent routeNetworkEvent, object expected, object actual, string propertyName)
        {
            if (expected == null && actual == null)
                return true;

            if ((expected != null && actual == null) || (expected == null && actual != null))
            {
                Log.Error($"The property: {propertyName} in event at seq no: {routeNetworkEvent.EventSequenceNumber} has an unexpeted property value: '{actual}' Expected: '{expected}'");
                return false;
            }

            return true;
        }
    }
}
