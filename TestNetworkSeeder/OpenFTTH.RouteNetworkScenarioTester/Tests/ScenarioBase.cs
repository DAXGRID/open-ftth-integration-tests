using Microsoft.Extensions.Logging;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.EventWatchAndFetch;
using OpenFTTH.Test.RouteNetworkDatastore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenFTTH.RouteNetworkScenarioTester.Tests
{
    public class ScenarioBase
    {
        protected RouteNetworkDatastore _routeNetworkDatastore;

        private WaitForAndFetchEvents<RouteNetworkEvent> _eventFetcher;

        private static HashSet<Guid> _idsUsed = new HashSet<Guid>();

        public ScenarioBase(ILoggerFactory loggerFactory, string postgresConnectionString, string kafkaServer, string routeNetworkTopicName)
        {
            _routeNetworkDatastore = new RouteNetworkDatastore(postgresConnectionString);

            _eventFetcher = new WaitForAndFetchEvents<RouteNetworkEvent>(loggerFactory, kafkaServer, routeNetworkTopicName);

            var startMarker = Guid.NewGuid();
            var endMarker = Guid.NewGuid();
        }

        public List<RouteNetworkEvent> WaitForEvents(Guid startMarker)
        { 
            long timeoutMs = 1000 * 60 * 1; // Wait 1 minute, before giving up recieving route network events from topic

            // First snatch cmdId
            bool timedOut = _eventFetcher.WaitForEvents(
                start => start.WorkTaskMrid.Equals(startMarker),
                stop => stop.WorkTaskMrid.Equals(startMarker),
                timeoutMs
                ).Result;

            _eventFetcher.Dispose();

            // We got a timeout, we got nothing
            if (timedOut)
            {
                Log.Error($"Timeout wating for event with TaskMrid={startMarker}");
                return new List<RouteNetworkEvent>();
            }

            if (_eventFetcher.Events.Count() != 1)
            {
                Log.Error($"We expected one event with TaskMrid={startMarker} but got " + _eventFetcher.Events.Count());
                return new List<RouteNetworkEvent>();
            }


            // Snatch cmdId from that event
            Guid cmdId = _eventFetcher.Events.First().CmdId;

            // Now fetch all events with that cmdId
            
            timedOut = _eventFetcher.WaitForEvents(
                start => start.CmdId == cmdId,
                stop => stop.IsLastEventInCmd && stop.CmdId == cmdId,
                timeoutMs
                ).Result;

            _eventFetcher.Dispose();

            // We got a timeout, we got nothing
            if (timedOut)
            {
                Log.Error($"Timeout wating for event belonging to command with id={cmdId}");
                return new List<RouteNetworkEvent>();
            }

            return _eventFetcher.Events.ToList();
        }


        protected bool CheckThatIsLastEventInCmdIsFalse(RouteNetworkEvent routeNetworkEvent)
        {
            // Check that IsLastEventInCmd is set to false
            if (routeNetworkEvent.IsLastEventInCmd != false)
                return Fail($"IsLastEventInCmd was expected to be false on event with id {routeNetworkEvent.EventId}");
            else
                return true;
        }

        protected bool CheckEventId(RouteNetworkEvent routeNetworkEvent)
        {
            if (routeNetworkEvent.EventId == null || routeNetworkEvent.EventId == Guid.Empty)
                return Fail($"EventId is null or empty on event with id: {routeNetworkEvent.EventId}");
            else
            {
                if (_idsUsed.Contains(routeNetworkEvent.EventId))
                    return Fail($"EventId is not unique on event with id: {routeNetworkEvent.EventId}");
            }

            _idsUsed.Add(routeNetworkEvent.EventId);

            return true;
        }

        protected bool CheckNodeId(RouteNodeAdded routeNodeAdded)
        {
            if (routeNodeAdded.NodeId == null || routeNodeAdded.NodeId == Guid.Empty)
                return Fail($"NodeId is null or empty on event with id: {routeNodeAdded.EventId}");
            else
            {
                if (_idsUsed.Contains(routeNodeAdded.NodeId))
                    return Fail($"NodeId is not unique on event with id: {routeNodeAdded.EventId}");
            }

            _idsUsed.Add(routeNodeAdded.NodeId);

            return true;
        }

        protected bool CheckThatWorkTaskMridIsTransferedToEvent(RouteNetworkEvent routeNetworkEvent, Guid expectedWorkTaskMrid)
        {
            if (routeNetworkEvent.WorkTaskMrid == null || routeNetworkEvent.WorkTaskMrid != expectedWorkTaskMrid)
                return Fail($"Expected WorkTaskMrid: {expectedWorkTaskMrid} to be transfered to the event with id: {routeNetworkEvent.EventId} from the original record inserted in postgres.");
            else
                return true;
        }

        protected bool CheckThatUserNameIsTransferedToEvent(RouteNetworkEvent routeNetworkEvent, string expectedUserName)
        {
            if (routeNetworkEvent.UserName == null || routeNetworkEvent.UserName != expectedUserName)
                return Fail($"Expected UserName: '{expectedUserName}' to be transfered to the event with id: {routeNetworkEvent.EventId} from the original record inserted in postgres.");
            else
                return true;
        }

        protected bool CheckEventType(RouteNetworkEvent routeNetworkEvent, string expectedEventType)
        {
            if (routeNetworkEvent.EventType == null || routeNetworkEvent.EventType != expectedEventType)
                return Fail($"Expected EventType: '{expectedEventType}' on the event with id: {routeNetworkEvent.EventId} but got: '{routeNetworkEvent.EventType}'");
            else
                return true;
        }

        protected bool CheckCmdType(RouteNetworkEvent routeNetworkEvent, string expectedCmdType)
        {
            if (routeNetworkEvent.CmdType == null || routeNetworkEvent.CmdType != expectedCmdType)
                return Fail($"Expected CmdType: '{expectedCmdType}' on the event with id: {routeNetworkEvent.EventId} but got: '{routeNetworkEvent.CmdType}'");
            else
                return true;
        }

        protected bool CheckApplicationName(RouteNetworkEvent routeNetworkEvent, string expectedApplicationName)
        {
            if (routeNetworkEvent.ApplicationName == null || routeNetworkEvent.ApplicationName != expectedApplicationName)
                return Fail($"Expected ApplicationName: '{expectedApplicationName}' on the event with id: {routeNetworkEvent.EventId} but got: '{routeNetworkEvent.ApplicationName}'");
            else
                return true;
        }

        protected bool CheckEventTimestamp(RouteNetworkEvent routeNetworkEvent, DateTime startTime)
        {
            if (routeNetworkEvent.EventTimestamp == null || routeNetworkEvent.EventTimestamp < startTime)
                return Fail($"Expected an EventTimestamp that was newer than scenario start time: {startTime} on the event with id: {routeNetworkEvent.EventId} but got: {routeNetworkEvent.EventTimestamp}");
            else
                return true;
        }


        protected bool Fail()
        {
            Log.Error($"Scenario 1 failed.");
            return false;
        }

        protected bool Fail(string errorMessage)
        {
            Log.Error(errorMessage);
            return false;
        }

    }
}
